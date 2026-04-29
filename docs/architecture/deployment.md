# Infraestrutura e Execução

## Módulos de banco de dados

O projeto usa dois `DbContext` distintos:

- `IdentityDbContext` em `Atlas.Identity.Infrastructure`
- `StaffDbContext` em `Atlas.Staff.Infrastructure`

Ambos usam `Npgsql` e compartilham a mesma string de conexão `Default`.

## Migrações

O `Makefile` define comandos úteis:

- `make migrate-identity name=InitialIdentity`
- `make migrate-staff name=InitialStaff`
- `make migrate-all name=Initial`
- `make update-identity`
- `make update-staff`
- `make update-all`

Esses comandos dependem de:

- `--project` apontando para o projeto de infraestrutura
- `--startup-project` apontando para `Atlas.API`
- `--context` especificando o `DbContext`

## Execução local

### Com Docker

Use o `docker compose` com os arquivos:

- `infrastructure/docker-compose.yml`
- `infrastructure/docker-compose.dev.yml`

Comandos úteis:

- `make up`
- `make down`
- `make logs`
- `make ps`
- `make reset`

### Sem Docker

Execute o projeto API diretamente com `dotnet run` em `source-code/src/Atlas.API`.

## Seed de desenvolvimento

Em ambiente de `Development`, o `Program.cs` executa o `SeederPipeline`:

- `GlobalIdentitySeeder`
- `IdentityDbContext`

Isso permite popular dados iniciais para testes e desenvolvimento.

## Configurações importantes

- `source-code/src/Atlas.API/appsettings.json`
- `source-code/src/Atlas.API/appsettings.Development.json`

Estas configurações controlam:

- autenticação OIDC
- tenants de desenvolvimento
- conexão com banco
- CORS e políticas de segurança

## Boas práticas

- Não misture `IdentityDbContext` com `StaffDbContext` na mesma migração.
- Use `migrate-all` somente quando precisar criar migrações sincronizadas em ambos os contextos.
- Valide o `TenantSlug` e as memberships antes de permitir login.
