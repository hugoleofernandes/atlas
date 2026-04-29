# Visão Geral da Arquitetura

O Atlas é um sistema modular dividido em dois domínios principais:

- `Identity`: gerencia tenants, usuários de identidade e login multitenant.
- `Staff`: gerencia membros de staff associados a tenants e users.

A arquitetura segue um padrão em camadas:

- API (`Atlas.API`): camada de entrada HTTP, configuração, segurança e exposição de endpoints.
- Application: casos de uso, comandos e queries.
- Domain: entidades de negócio, agregados e regras de validação.
- Infrastructure: persistência, repositórios, configuração de Entity Framework e injeção de dependência.

## Fluxo principal

1. O cliente faz uma chamada para a API.
2. A API delega a mensagem a um caso de uso por `MediatR`.
3. O caso de uso aplica regras de negócio usando entidades de domínio.
4. As alterações são persistidas pela camada de infraestrutura.

## Tecnologias principais

- ASP.NET Core 10
- `MediatR` para CQRS e mensageria interna
- `Entity Framework Core` com `Npgsql`
- `Serilog` para logging
- `FluentValidation` para validação de comandos
- `OpenAPI` para documentação de API
