# Changelog

## [Unreleased] - 2026-07-11

### Added

- **Camada de Domain:**
  - Entidade `Author` com propriedades Id, Name e Surname

- **Camada de Application:**
  - Driving Port `IAuthorUseCase` com operações `CreateAuthorAsync` e `GetAuthorsAsync`
  - Driven Port `IAuthorPort` com operações `AddAsync` e `GetAllAsync`
  - Driven Port `IUnitOfWork` para gestão de transações (Begin, Commit, Rollback, Dispose)
  - Use Case `AuthorUseCase` implementando `IAuthorUseCase`
  - Referência de projeto para Domain

- **Camada de Infrastructure (Driven Adapter):**
  - `AuthorRepository` implementando `IAuthorPort`
  - `UnitOfWork` implementando `IUnitOfWork` com `NpgsqlDataSource`
  - Referências de projeto para Domain e Application

- **Camada de API (Driving Adapter):**
  - Endpoints REST para Authors (`/api/v1/authors`) com GET e POST via Carter
  - Configuração de API versioning por URL
  - Referência de projeto para Application

- **Orchestration:**
  - `DependencyInjectionExtensions` com registro de Use Cases (Driving Ports) e Driven Ports
  - Projeto `Visma.Yuki.Blog.Database` com dbup para migrations SQL:
    - `0001_Author_Table.sql` — Criação da tabela Authors
    - `0002_Posts_Table.sql` — Criação da tabela Posts
    - `0003_Indexes.sql` — Índices de performance
    - `0004_Insert_Fake_Authors.sql` — Dados seed de autores
    - `0005_Insert_Fake_Posts.sql` — Dados seed de posts
  - `AppHost` atualizado com orquestração de PostgreSQL, migration e API via Aspire

- **Testes:**
  - Projeto `Visma.Yuki.Blog.Tests.Architecture` com `NetArchTest.Rules` 1.3.2:
    - `Design/NamespaceTests.cs` — Valida namespaces corretos e dependências proibidas entre camadas
    - `Layers/LayerDependencyTests.cs` — Valida referências de assembly entre camadas
    - `Layers/HexagonalArchitectureTests.cs` — Valida ports, use cases e implementações da arquitetura hexagonal
    - `Layers/DomainPurityTests.cs` — Valida pureza do Domain (sem pacotes externos, sem ORM, sem adapters)
  - Projeto `Visma.Yuki.Blog.Tests.Unit` (estrutura pronta)
  - Projeto `Visma.Yuki.Blog.Tests.Integration` (estrutura pronta)
  - Projetos adicionados à solution sob a solution folder "Tests"

### Changed

- **`Visma.Yuki.Blog.sln`** atualizada com novos projetos de Infrastructure, API, Database, Shared e Tests
- **`Program.cs` (API)** refactorado para usar Carter, API versioning e service defaults do Aspire
- **`Extensions.cs` (Shared)** ajustado para compatibilidade com a nova estrutura
- **`AppHost.cs` (Aspire)** atualizado com orquestração de banco de dados e migrations

### Removed

- Arquivos placeholder `Class1.cs` de Domain e Infrastructure
