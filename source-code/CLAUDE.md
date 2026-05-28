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

Toda query segue as camadas: `Controller → IQueryHandler → QueryHandler → IReader → Reader (EF)`.

Mesmo para dados estáticos/in-memory, o reader existe e fica em `Atlas.Identity.Infrastructure`. Não há exceções à estrutura de camadas.
