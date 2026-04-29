# Casos de Uso

## 1. Autorizar login de tenant (`AuthorizeTenantLogin`)

### Objetivo
Permitir que um usuário se autentique em um tenant usando um `ExternalOid` e um e-mail convidado.

### Entrada
- `TenantSlug`
- `ExternalOid`
- `Email`

### Fluxo principal

1. Valida os parâmetros de entrada.
2. Busca o `Tenant` ativo pelo slug com suas memberships.
3. Se o `ExternalOid` já existe em `IdentityUser`:
   - valida se o usuário está ativo.
   - encontra a membership vinculada ao usuário.
   - retorna `TenantId`, `TenantSlug`, `IdentityUserId` e `Role`.
4. Se o usuário não existe:
   - tenta encontrar um `TenantMembership` ativo pelo e-mail.
   - se não achar, lança `UnauthorizedAccessException`.
   - cria um novo `IdentityUser` com `ExternalOid`.
   - vincula o `TenantMembership` ao novo `IdentityUser`.
   - salva as alterações.

### Regras de negócio

- `TenantSlug` deve existir e estar ativo.
- `ExternalOid` deve ser único por `IdentityUser`.
- Um usuário só pode acessar um tenant se tiver uma membership ativa.
- Um convite (`TenantMembership`) deve existir para o e-mail informado no tenant.

## 2. Criar membro de staff

### Objetivo
Cadastrar um membro de staff para um tenant e um usuário de identidade.

### Entrada
- `TenantId`
- `IdentityUserId`
- `FirstName`
- `LastName`
- `Role`

### Fluxo principal

1. Verifica se já existe um `StaffMember` para o par `TenantId` + `IdentityUserId`.
2. Se já existe, retorna erro `Staff already exists for this user.`
3. Cria a entidade `StaffMember` com dados básicos e `IsActive = true`.
4. Persiste o membro via repositório.

### Regras de negócio

- Não pode haver duplicidade de staff para o mesmo usuário e tenant.
- `FirstName`, `LastName` e `Role` são atributos obrigatórios na criação.

## 3. Listar membros de staff

### Objetivo
Retornar uma página de membros de staff.

### Entrada
- `Page`
- `PageSize`

### Fluxo principal

1. Consulta paginada em `StaffMembers`.
2. Ordena por `FirstName`.
3. Retorna `PagedResult<Dto>` com `Id`, `FirstName`, `LastName`, `Role` e `IsActive`.

### Observações

- A listagem é feita com `AsNoTracking()` para performance.
- O controle de paginação é realizado pelo handler de query.
