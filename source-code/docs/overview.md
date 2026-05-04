# Atlas Overview

## Why this project exists

Atlas is a modular multi-tenant system designed with:
- Domain-Driven Design (DDD)
- Clear bounded contexts (Identity, Staff)
- Clean Architecture principles

## How the system is structured

- **Domain** → business rules
- **Application** → use cases
- **Infrastructure** → persistence / external systems
- **API** → entry points

## Documentation structure

- Modules → human understanding of each bounded context (Identity, Staff ...)
- API Reference → generated code documentation (C# structure)
- Guides → practical instructions on how to use and extend the system
- Guidelines → coding standards and architectural rules

## How to use this documentation

1. Start with a module (Identity / Staff)
2. Check the domain rules
3. Use API reference for implementation details
4. Follow guidelines when extending the system