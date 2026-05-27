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

### Ao adicionar um recurso novo

1. Crie as constantes no `PermissionCatalog` (`read`, `create`, `update`, `delete`, `manage`)
2. Adicione as traduções em `PermissionLabels.resx` (EN) e `PermissionLabels.pt.resx` (PT)
3. Os `PermissionCatalogTranslationTests` vão garantir que nenhuma tradução ficou faltando
4. Decore os endpoints com os verbos corretos

---

## Padrão de Query Handler

Toda query segue as camadas: `Controller → IQueryHandler → QueryHandler → IReader → Reader (EF)`.

Mesmo para dados estáticos/in-memory, o reader existe e fica em `Atlas.Identity.Infrastructure`. Não há exceções à estrutura de camadas.
