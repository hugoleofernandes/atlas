# Modelo de Domínio

## Entidades de Identity

### Tenant

- `Id: Guid`
- `Slug: string`
- `IsActive: bool`
- `Memberships: IReadOnlyCollection<TenantMembership>`

#### Comportamentos
- `InviteUser(email, role)`
- `FindMembershipByEmail(email)`
- `FindMembershipByUser(userId)`

### TenantMembership

- `Id: Guid`
- `TenantId: Guid`
- `IdentityUserId: Guid?`
- `Email: string`
- `Role: string`
- `IsActive: bool`

#### Comportamentos
- `BindIdentityUser(identityUserId)`
- `Deactivate()`

### IdentityUser

- `Id: Guid`
- `ExternalId: string?`
- `IsActive: bool`

#### Comportamentos
- `Deactivate()`

### Relações

- `Tenant` é um agregado raiz que possui `TenantMembership`.
- `TenantMembership` pode ser associado a um `IdentityUser` após o primeiro login.
- `IdentityUser` representa o usuário do provedor externo vinculado ao tenant.

## Entidades de Staff

### StaffMember

- `Id: Guid`
- `TenantId: Guid`
- `IdentityUserId: Guid`
- `FirstName: string`
- `LastName: string`
- `Role: string`
- `IsActive: bool`
- `CreatedAt: DateTime`

#### Comportamentos
- `Deactivate()`
- `UpdateProfile(firstName, lastName)`

### StaffAuditLog

- `Id: Guid`
- `TenantId: Guid`
- `EntityName: string`
- `Action: string`
- `EntityId: string?`
- `UserId: string?`
- `ChangesJson: string`
- `OccurredAtUtc: DateTime`

## Observações de Modelo

- `IdentityUser` e `TenantMembership` pertencem ao escopo de identidade e autenticação.
- `StaffMember` pertence ao escopo de staff e está relacionado a um tenant e um usuário de identidade.
- A separação em `Domain` e `Infrastructure` reforça a regra de não expor entidades diretamente à camada de persistência.
