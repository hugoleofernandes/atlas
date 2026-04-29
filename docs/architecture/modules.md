# Módulos e Responsabilidades

## Atlas.API

Responsável por:

- Configuração de aplicação e de serviços.
- Registro de `DbContext` para `IdentityDbContext` e `StaffDbContext`.
- Configuração de autenticação multi-tenant com OIDC.
- Middleware de segurança, CORS, rate limiting e tratamento de erros.
- Endpoints HTTP expostos por controladores.

### Principais componentes

- `Program.cs`
- `Controllers/Staff/StaffController.cs`
- `Security/*`
- `OpenApi` e `ProblemDetails`

## Atlas.Identity.Application

Responsável por:

- Regras de orquestração para login de tenant.
- Casos de uso como `AuthorizeTenantLogin`.
- Interfaces de repositório e abstrações de aplicação.

### Principais componentes

- `UseCases/AuthorizeTenantLogin`
- `IAuthorizeTenantLoginUseCase`
- `AuthorizeTenantLoginUseCase`

## Atlas.Identity.Domain

Responsável por:

- Modelo de domínio de identidade e tenant.
- Agregados e invariantes de negócio.
- Entidades de domínio centrais.

### Entidades principais

- `Tenant`
- `TenantMembership`
- `IdentityUser`
- `IdentityAuditLog`

## Atlas.Identity.Infrastructure

Responsável por:

- Persistência com Entity Framework Core.
- Configuração de `IdentityDbContext`.
- Repositórios e UoW para o domínio de Identity.
- Seed de dados em desenvolvimento.

### Principais componentes

- `Persistence/IdentityDbContext.cs`
- `Persistence/TenantConfig/*`
- `Persistence/Seed/GlobalIdentitySeeder.cs`
- `DI/DependencyInjection.cs`

## Atlas.Staff.Application

Responsável por:

- Casos de uso de staff, como criação e listagem de membros.
- Definição de comandos, queries e DTOs.

### Componentes principais

- `StaffMembers/Commands/Create`
- `StaffMembers/Queries/List`

## Atlas.Staff.Domain

Responsável por:

- Modelo de domínio para `StaffMember` e `StaffAuditLog`.
- Regras de negócio do staff.

## Atlas.Staff.Infrastructure

Responsável por:

- Persistência com `StaffDbContext`.
- Implementação de repositórios e consultas.
- Mapeamento EF Core com `IEntityTypeConfiguration`.
- Configuração de DI do módulo de staff.
