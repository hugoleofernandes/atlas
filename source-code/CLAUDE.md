# Atlas — Guia para Claude

## Padrão de Permissões

### Nomenclatura: `{module}.{resource}.{verb}`

Todo endpoint protegido deve ter `[HasPermission]` com um código do `PermissionCatalog`.

**Verbos disponíveis:** `read` | `create` | `update` | `delete` | verbos de negócio excepcionais (`deactivate`)

**`manage`** é um atalho de atribuição — engloba todos os verbos do recurso. O authorization handler aceita `{prefix}.manage` como satisfazendo qualquer check `{prefix}.{verb}`. **Nunca use `manage` em `[HasPermission]` em endpoints** — use sempre o verbo mais específico.

### Regra de ouro

> Antes de colocar `[HasPermission]` num endpoint, pergunte: *"que capacidade de negócio o usuário precisa ter?"* — não *"qual permissão já existe que parece próxima?"*

**Se a capacidade não está no `PermissionCatalog`, adicione lá primeiro** — antes de decorar o endpoint.

### Catálogo atual

```
tenant.roles.read / create / update / delete / manage
tenant.invitations.read / create / update / delete / manage
staff.read / create / update / deactivate / manage
system.root  ← só para roles de sistema
```

### Exemplo de uso correto

```csharp
// Endpoint de leitura: verbo específico
[HasPermission(PermissionCatalog.Tenant.Roles.Read)]
public async Task<IActionResult> List(...) { }

// Endpoint de escrita: verbo específico
[HasPermission(PermissionCatalog.Tenant.Roles.Delete)]
public async Task<IActionResult> Remove(...) { }

// Ao atribuir permissão a uma role de admin no frontend/seed:
// use Manage para dar tudo — o handler resolve para todos os verbos
```

### Arquitetura modular de permissões

Cada módulo define suas próprias permissões — Identity não conhece as constantes de Staff.

| Módulo | Catálogo | Registro |
|---|---|---|
| Identity | `PermissionCatalog` (tenant.*) | `IdentityModulePermissions` |
| Staff | `StaffPermissionCatalog` (staff.*) | `StaffModulePermissions` |

O `IPermissionPolicy` (singleton em DI) agrega todos os `IModulePermissions` registrados.  
O `PermissionPolicyService` faz a agregação em `IdentityDependencyInjection`.

### Ao adicionar um recurso novo

**Recurso dentro de um módulo existente:**
1. Crie as constantes no catálogo do módulo (`{ModulePermissionCatalog}`)
2. Atualize o `{Module}ModulePermissions` (adicione ao `Permissions` set e ao `Groups` list)
3. Adicione as traduções em `PermissionLabels.resx` (EN) e `PermissionLabels.pt.resx` (PT)
4. Os `PermissionCatalogTranslationTests` vão garantir que nenhuma tradução ficou faltando
5. Decore os endpoints com os verbos corretos

**Novo módulo:**
1. Crie `{Module}PermissionCatalog.cs` em `Atlas.{Module}.Domain/Permissions/`
2. Crie `{Module}ModulePermissions.cs` implementando `IModulePermissions`
3. Registre em `IdentityDependencyInjection`: `services.AddSingleton<IModulePermissions, {Module}ModulePermissions>()`
4. Siga os passos acima para os recursos do módulo

### Métodos de domínio que validam permissões

`Tenant.AddCustomRole` e `Tenant.UpdateRole` recebem `IReadOnlySet<string> validCodes` como parâmetro.  
Os command handlers injetam `IPermissionPolicy` e passam `_permissionPolicy.All`.

---

## Padrão de Query Handler

Toda query segue as camadas: `Endpoint → IQueryHandler → QueryHandler → IReader → Reader (Dapper)`.

Mesmo para dados estáticos/in-memory, o reader existe e fica em `Atlas.{Module}.Infrastructure`. Não há exceções à estrutura de camadas.

### Reader é exclusivo do query handler

Cada query handler tem seu próprio reader — relação 1:1. O reader fica co-localizado na pasta do handler.

```
Invitations/Handlers/Queries/ListInvitations/
├── IListInvitationsQueryHandler.cs
├── IListInvitationsReader.cs        ← exclusivo deste handler
├── InvitationSummary.cs             ← DTO exclusivo deste handler
├── ListInvitationsQuery.cs
└── ListInvitationsQueryHandler.cs
```

O reader retorna DTOs — nunca objetos de domínio. Command handlers **não chamam readers**; eles chamam repositórios que retornam domain objects.

### DTO exclusivo por query handler

Cada query handler define seu próprio tipo de retorno. Nunca reutilize um DTO entre handlers — cada projeção serve um caso de uso específico e vai divergir.

```csharp
ListInvitationsQueryHandler  → IReadOnlyList<InvitationSummary>  // visão de lista
GetInvitationQueryHandler    → InvitationDetail                  // visão de detalhe
```

---

## Padrão de Reader (Dapper)

Todos os readers usam **Dapper com SQL raw** — não EF Core LINQ. A única exceção é `ListPermissionsReader`, que é in-memory (dados estáticos do catálogo de permissões).

### Nomenclatura de colunas

O projeto usa `UseSnakeCaseNamingConvention()` em todos os DbContexts. As colunas no banco são snake_case (`tenant_id`, `created_at`, `role_id`). SQL raw deve usar snake_case **sem aspas**.

```sql
-- ✅ correto
SELECT id, tenant_id, created_at FROM atlas_identity.roles

-- ❌ errado — aspas não são necessárias com snake_case
SELECT "Id", "TenantId", "CreatedAt" FROM atlas_identity.roles
```

Dapper faz matching case-insensitive, mas **não converte underscore para PascalCase**. Colunas multi-palavra precisam de alias explícito:

```sql
-- ❌ Dapper não mapeia is_system → IsSystem
SELECT id, name, is_system FROM atlas_identity.roles

-- ✅ alias explícito para colunas com underscore
SELECT id, name, is_system AS IsSystem FROM atlas_identity.roles
```

Colunas de uma só palavra (`id`, `name`, `code`) funcionam sem alias — Dapper resolve `id` → `Id` pelo case-insensitive.

### Mapeamento: quando usar DTO direto vs record intermediário

**DTO direto** — quando as colunas do SQL casam 1:1 com o DTO:
```csharp
var results = await conn.QueryAsync<RoleLookupDto>(Sql, new { TenantId = tenantId });
```

**Record intermediário** — quando o resultado precisa de reshaping em C# (ex: 1:N):
```csharp
// RoleDto tem IReadOnlyList<string> PermissionCodes — SQL retorna linhas achatadas
// Usa record intermediário + agrupa em C#
private sealed record RoleRow(Guid Id, string Name, bool IsSystem);
private sealed record PermissionRow(Guid RoleId, string Code);
```

### Parâmetros — sempre `new { }` anônimo

```csharp
// ✅ padrão Dapper — objeto anônimo é o bag de parâmetros
conn.QueryAsync<Dto>(Sql, new { TenantId = tenantId, IsActive = isActive });
```

### Argumentos nomeados na construção manual

Sempre usar argumentos nomeados ao construir DTOs ou records manualmente:

```csharp
// ✅
return new RoleDto(
    RoleId:          role.Id,
    Name:            role.Name,
    IsSystem:        role.IsSystem,
    PermissionCodes: permissions);

// ❌ — depende da ordem, frágil
return new RoleDto(role.Id, role.Name, role.IsSystem, permissions);
```

### Constantes para predicados SQL reutilizados

Quando a mesma expressão booleana aparece mais de uma vez no SQL (ex: no SELECT e no WHERE), extrair como constante evita dessincronização:

```csharp
// A mesma lógica aparece no SELECT (para projetar) e no WHERE (para filtrar)
private const string IsActivePredicate = "NOT i.is_used AND i.expires_at >= @Now";

private const string Sql = $"""
    SELECT ({IsActivePredicate}) AS IsActive, ...
    WHERE (@IsActive AND ({IsActivePredicate}))
       OR (!@IsActive AND NOT ({IsActivePredicate}))
    """;
```

### Paginação com 1:N — duas queries separadas

Nunca fazer JOIN em query paginada quando o lado N pode multiplicar linhas. Usar duas queries:

```csharp
// Query 1: roles paginados
var roles = await conn.QueryAsync<RoleRow>(RolesSql, ...);

// Query 2: permissões só dos IDs retornados
var permissions = await conn.QueryAsync<PermissionRow>(
    PermissionsSql, new { RoleIds = roles.Select(r => r.Id).ToArray() });

// Agrupa em C#
var lookup = permissions.ToLookup(p => p.RoleId);
```

---

## Multi-tenancy

### Global query filter

Todas as entidades que implementam `IMultiTenantEntity` têm um global query filter aplicado automaticamente em `DbContextBase`. O filter garante que queries retornem apenas dados do tenant do contexto atual.

```csharp
// Entidade multi-tenant
public sealed class Invitation : AggregateRoot, IMultiTenantEntity { ... }

// Entidade que não é multi-tenant (ex: Tenant em si)
public sealed class Tenant : AggregateRoot, INotMultiTenant { ... }
```

### Suspender o filter — bootstrap e seeders

Situações onde o `TenantId` ainda não está no contexto (ex: `ResolveTenantAccess`, seeders) exigem suspensão explícita do filter:

```csharp
// ResolveTenantAccessCommandHandler — roda antes do TenantId ser populado
using (_contextSetter.SuspendTenantFilter())
{
    var user       = await _userRepository.FindActiveByEmailAsync(tenant.Id, email, ct);
    var invitation = await _invitationRepository.FindByEmailAsync(tenant.Id, email, ct);
}
// filter reativado automaticamente ao sair do using
```

O filter é suspenso apenas dentro do `using` — nunca vaza para outras operações.

### Seeders

Seeders usam `IIdentityUnitOfWork` (não `db.SaveChangesAsync()` diretamente) para acionar o pipeline de auditoria, e populam o contexto com `SystemIdentity` antes de salvar:

```csharp
var uow    = services.GetRequiredService<IIdentityUnitOfWork>();
var setter = services.GetRequiredService<IRequestContextSetter>();

// Leitura cross-tenant: IgnoreQueryFilters()
var exists = await db.Tenants.IgnoreQueryFilters().AnyAsync(ct);

// Antes de salvar: popula contexto para o stamper de auditoria
setter.Set(tenant.Id, tenant.Name, SystemIdentity.UserId, SystemIdentity.Email);
await uow.SaveChangesAsync(ct);
```

`SystemIdentity.UserId = Guid.Empty` e `SystemIdentity.Email = "system@atlas"` — constantes em `Atlas.SharedKernel`.

---

## Padrão de Endpoint (FastEndpoints)

O projeto usa **FastEndpoints** — não MVC controllers para novos endpoints.

### Estrutura de pastas

```
src/Atlas.API/Endpoints/{Module}/{Resource}/{Operation}/
    ├── {Operation}Request.cs
    ├── {Operation}Response.cs   (apenas se o endpoint retorna body)
    └── {Operation}Endpoint.cs
```

Exemplo:
```
Endpoints/Identity/Invitations/
├── CreateInvitation/
│   ├── CreateInvitationRequest.cs
│   ├── CreateInvitationResponse.cs
│   └── CreateInvitationEndpoint.cs
└── ListInvitations/
    ├── ListInvitationsRequest.cs
    └── ListInvitationsEndpoint.cs
```

### Nomenclatura

Endpoints usam **nome do recurso HTTP**, não nome do comando de domínio.

```
✅ CreateInvitationEndpoint   (recurso: Invitation)
❌ InviteUserEndpoint         (intenção de domínio)

✅ CreateRoleEndpoint
✅ ListInvitationsEndpoint
✅ RevokeInvitationEndpoint   (verbo de negócio excepcional é aceitável)
```

O comando (`InviteUserCommand`) mantém o nome de domínio — são camadas diferentes.

### Classe base: AtlasEndpoint

Todo endpoint herda de `AtlasEndpoint<TReq, TRes>` (em `Atlas.BuildingBlocks.AspNetCore`), não de `Endpoint<TReq, TRes>` diretamente.

A base expõe helpers que tratam o caminho de erro automaticamente:

```csharp
// Comandos
await CreatedFromResultAsync(result, CreateInvitationResponse.From, ct);   // 201
await UpdatedFromResultAsync(result, RoleResponse.From, ct);               // 200
await UpdatedNoContentFromResultAsync(result, ct);                         // 204
await DeletedFromResultAsync(result, ct);                                  // 204

// Queries
await OkFromResultAsync(result, ct);                                       // 200 (sem mapeamento)
await OkFromResultAsync(result, MyResponse.From, ct);                      // 200 (com mapeamento)
```

Se `result.IsSuccess == false`, a base retorna Problem Details com o status correto automaticamente — o endpoint não precisa tratar o caminho de erro.

### Autorização

Use o string literal `"permission:"` — não `HasPermissionAttribute.PolicyPrefix` (conflito de namespace com FastEndpoints):

```csharp
Policies($"permission:{IdentityPermissions.Tenant.Invitations.Create}");
```

### Documentação OpenAPI

O `ProblemDetailsOperationTransformer` adiciona 400/401/404/409/422/500 em **todos** os endpoints automaticamente. Cada endpoint só declara o que é específico: o código e tipo de resposta de sucesso.

```csharp
// ✅ correto — transformer cobre os erros
Description(d => d.Produces<CreateInvitationResponse>(201));

// ❌ redundante
Description(d => d
    .Produces<CreateInvitationResponse>(201)
    .ProducesProblem(404)
    .ProducesProblem(409));
```

### Request vs Command

Request e Command são tipos separados mesmo quando têm o mesmo shape.

- **Request** — contrato HTTP: o que o cliente manda. Pertence à camada API. Pode ter anotações de binding e validação de input.
- **Command** — input da aplicação. Pertence à camada Application. Pode ter campos que não vêm do body (ex: `TenantId` resolvido da sessão).

```csharp
// Request: só o que o cliente envia
public sealed record CreateInvitationRequest(string Email, Guid RoleId);

// Command: o que o handler precisa (pode divergir)
public sealed record InviteUserCommand(string Email, Guid RoleId);

// Endpoint faz a ponte — e pode enriquecer com dados da sessão
var cmd = new InviteUserCommand(req.Email, req.RoleId);
```

---

## Formatação de Código

O projeto usa **CSharpier** como formatador — equivalente ao Prettier para C#.

- Extensão instalada no Visual Studio com **Format on Save** ativado
- Configuração em `.csharpierrc.json` na raiz do repositório (`printWidth: 120`)
- `.editorconfig` complementa com regras de estilo (indentação, line endings, `var`, modificadores)

Nunca ajuste formatação manualmente — salvar o arquivo já formata. Se o código parecer "mal formatado" antes de salvar, é normal.

---

## Regras de Git

**Nunca se coloque como co-autor nos commits.** O autor do repositório é exclusivamente Hugo.

```
// ❌ Proibido — jamais incluir isso em mensagens de commit
Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
```

Ao criar commits, use apenas a mensagem descritiva — sem nenhuma linha `Co-Authored-By`.

---

## Mapeamento ErrorCategory → HTTP Status

A fonte única é `ErrorCategoryExtensions.ToHttpStatus()` em `Atlas.BuildingBlocks.AspNetCore.HttpErrors`.

Todos os consumidores usam a mesma extensão: `GlobalExceptionMiddleware`, `HttpResultMapper`, e os endpoints via `AtlasEndpoint`. Nunca duplique o switch `ErrorCategory → int` em outro lugar.
