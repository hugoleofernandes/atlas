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

Toda query segue as camadas: `Endpoint → IQueryHandler → QueryHandler → IReader → Reader (EF)`.

Mesmo para dados estáticos/in-memory, o reader existe e fica em `Atlas.Identity.Infrastructure`. Não há exceções à estrutura de camadas.

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
