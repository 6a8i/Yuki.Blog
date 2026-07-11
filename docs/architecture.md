# Visma.Yuki.Blog — Documentação de Arquitetura

## Visão Geral

O **Visma.Yuki.Blog** é um projeto de blog construído com **Arquitetura Hexagonal** (Ports and Adapters), **Domain-Driven Design (DDD)** e **Repository Pattern**. O projeto utiliza **.NET 10**, **.NET Aspire** para orquestração local e **PostgreSQL** como banco de dados.

---

## Arquitetura Hexagonal

A arquitetura hexagonal isola a lógica de negócio (Core) de infraestruturas externas (Adapters) através de **Ports** — interfaces que definem contratos de comunicação.

```
┌─────────────────────────────────────────────────────────┐
│                     Adapters                            │
│  ┌──────────────────┐    ┌───────────────────────────┐  │
│  │   Driving (API)  │    │    Driven (Infrastructure)│  │
│  │  REST Endpoints  │    │  Repositories, UnitOfWork │  │
│  └────────┬─────────┘    └───────────┬───────────────┘  │
│           │                          │                  │
│           ▼                          ▼                  │
│  ┌──────────────────────────────────────────────────┐   │
│  │                    Core                          │   │
│  │  ┌─────────────┐    ┌────────────────────────┐   │   │
│  │  │   Domain    │◄── │     Application        │   │   │
│  │  │  Entities   │    │  Ports, UseCases       │   │   │
│  │  └─────────────┘    └────────────────────────┘   │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

### Fluxo de uma requisição

1. **Driving Adapter** (API) recebe a requisição HTTP
2. O endpoint chama um **Driving Port** (interface da Application)
3. O **Use Case** (implementação do Driving Port) executa a lógica de negócio
4. O Use Case chama um **Driven Port** (interface de infraestrutura) quando precisa persistir dados
5. O **Driven Adapter** (Infrastructure) implementa o Driven Port e executa a operação no banco

---

## Organização do Projeto

```
src/
├── Core/                                    # Lógica de negócio pura
│   ├── Visma.Yuki.Blog.Domain/              # Camada de domínio
│   │   ├── Entities/                        # Entidades de domínio
│   │   ├── Enums/                           # Enumerações
│   │   ├── Events/                          # Eventos de domínio
│   │   ├── Exceptions/                      # Exceções de domínio
│   │   └── ValueObjects/                    # Value Objects
│   │
│   └── Visma.Yuki.Blog.Application/         # Camada de aplicação
│       ├── Ports/
│       │   ├── Driving/                     # Ports de entrada (interfaces)
│       │   └── Driven/                      # Ports de saída (interfaces)
│       ├── UseCases/                        # Implementações dos Driving Ports
│       └── Commands/                        # Commands / DTOs
│
├── Adapters/                                # Adaptadores externos
│   ├── Driving/                             # Adaptadores de entrada
│   │   └── Visma.Yuki.Blog.Api/             # API REST
│   │       ├── Endpoints/V1/                # Endpoints versionados
│   │       └── Program.cs                   # Entry point da aplicação
│   │
│   └── Driven/                              # Adaptadores de saída
│       └── Visma.Yuki.Blog.Infrastructure/  # Infraestrutura
│           ├── Repositories/                # Implementações dos Driven Ports
│           └── DbContext/                   # Contexto do banco (futuro)
│
├── Orchestration/                           # Orquestração e configuração
│   ├── Visma.Yuki.Blog.Aspire.Orchestration/# AppHost do Aspire
│   ├── Visma.Yuki.Blog.Database/            # Migrations SQL (dbup)
│   │   └── Migrations/                      # Scripts de migration
│   └── Visma.Yuki.Blog.Shared/             # Configurações compartilhadas
│       ├── DependencyInjectionExtensions.cs # Registro de DI
│       └── Extensions.cs                    # Extensões do Aspire (telemetry, health)
│
├── Tests/                                   # Projetos de teste
│   ├── Visma.Yuki.Blog.Tests.Architecture/  # Testes de arquitetura
│   │   ├── Design/                          # Testes de namespace e design
│   │   └── Layers/                          # Testes de camadas e hexagonal
│   ├── Visma.Yuki.Blog.Tests.Unit/          # Testes unitários
│   └── Visma.Yuki.Blog.Tests.Integration/   # Testes de integração
│
└── Visma.Yuki.Blog.sln                      # Solution file
```

---

## Camadas

### Domain (`Visma.Yuki.Blog.Domain`)

Camada mais interna, contém as regras de negócio puras.

- **Entities** — Objetos de domínio com identidade (ex: `Author`)
- **Value Objects** — Objetos imutáveis sem identidade
- **Enums** — Enumerações de domínio
- **Events** — Eventos de domínio
- **Exceptions** — Exceções específicas de domínio

**Restrições:**
- Não referencia nenhuma outra camada
- Não referencia pacotes externos (EntityFramework, Dapper, etc.)
- Não tem dependência de Adapters

### Application (`Visma.Yuki.Blog.Application`)

Camada de aplicação que orquestra a lógica de negócio através de Ports e Use Cases.

- **Driving Ports** — Interfaces que definem as operações de entrada (ex: `IAuthorUseCase`)
- **Driven Ports** — Interfaces que definem as operações de saída (ex: `IAuthorPort`, `IUnitOfWork`)
- **Use Cases** — Implementações dos Driving Ports (ex: `AuthorUseCase`)
- **Commands** — DTOs e commands para operações

**Restrições:**
- Referencia apenas Domain
- Não referencia Infrastructure, API ou Shared

### Infrastructure (`Visma.Yuki.Blog.Infrastructure`) — Driven Adapter

Implementa os Driven Ports da Application, conectando com infraestrutura externa.

- **Repositories** — Implementações dos Driven Ports (ex: `AuthorRepository` implementa `IAuthorPort`)
- **UnitOfWork** — Implementação de `IUnitOfWork` com `NpgsqlDataSource`

**Restrições:**
- Referencia Domain e Application
- Não referencia API ou Shared

### API (`Visma.Yuki.Blog.Api`) — Driving Adapter

Expõe a aplicação via REST API.

- **Endpoints** — Definidos com Carter, versionados por URL (ex: `/api/v1/authors`)
- **Program.cs** — Configuração da aplicação (DI, API versioning, Aspire service defaults)

**Restrições:**
- Referencia Application (via Driving Ports)
- Não referencia Infrastructure diretamente

### Shared (`Visma.Yuki.Blog.Shared`)

Configurações compartilhadas entre os adaptadores.

- **DependencyInjectionExtensions** — Registro de Use Cases (Driving Ports) e Driven Ports
- **Extensions** — Configurações do Aspire (OpenTelemetry, health checks, service discovery)

### Aspire Orchestration (`Visma.Yuki.Blog.Aspire.Orchestration`)

Orquestra a aplicação localmente com .NET Aspire.

- **AppHost** — Define PostgreSQL, API e projeto de migrations
- Gerencia dependências entre serviços (API aguarda migrations completarem)

### Database (`Visma.Yuki.Blog.Database`)

Projeto de migrations SQL usando dbup.

- **Migrations** — Scripts SQL versionados executados em ordem
- **program.cs** — Runner do dbup que aplica as migrations no startup

---

## Domain-Driven Design (DDD)

O projeto aplica princípios de DDD:

- **Domain isolado** — A camada de domínio não tem dependências externas
- **Entidades ricas** — Entities contêm both data e behavior
- **Ports como contratos** — Interfaces definem limites claros entre camadas
- **Use Cases como orquestradores** — Aplicam regras de negócio coordenando entities e ports

---

## Repository Pattern

O Repository Pattern é implementado através dos Driven Ports:

- **Driven Port** (`IAuthorPort`) — Define o contrato de acesso a dados na Application
- **Repository** (`AuthorRepository`) — Implementa o contrato na Infrastructure
- **Unit of Work** (`IUnitOfWork` / `UnitOfWork`) — Gerencia transações

A injeção de dependência é configurada em `DependencyInjectionExtensions.cs`:

```csharp
// Driving Ports → Use Cases
services.AddTransient<IAuthorUseCase, AuthorUseCase>();

// Driven Ports → Repositories
services.AddTransient<IAuthorPort, AuthorRepository>();
```

---

## Tecnologias

| Tecnologia | Versão | Propósito |
|---|---|---|
| .NET | 10.0 | Framework principal |
| ASP.NET Core | 10.0 | API REST |
| .NET Aspire | 9.x | Orquestração local e service defaults |
| PostgreSQL | — | Banco de dados |
| Npgsql | — | Driver PostgreSQL |
| Carter | — | Minimal API endpoints |
| Asp.Versioning.Mvc | — | API versioning por URL |
| dbup | — | Migrations SQL |
| FluentResults | — | Result pattern para tratamento de erros |
| xUnit | 2.9.3 | Framework de testes |
| NetArchTest.Rules | 1.3.2 | Testes de arquitetura |
| coverlet | 6.0.4 | Code coverage |
| OpenTelemetry | — | Observabilidade (via Aspire) |

---

## Testes

### Arquitetura (`Visma.Yuki.Blog.Tests.Architecture`)

Testes que validam a estrutura e arquitetura do projeto usando `NetArchTest.Rules`:

- **Design/NamespaceTests.cs** — Valida namespaces corretos por camada e dependências proibidas
- **Layers/LayerDependencyTests.cs** — Valida referências de assembly entre camadas
- **Layers/HexagonalArchitectureTests.cs** — Valida ports, use cases e implementações hexagonais
- **Layers/DomainPurityTests.cs** — Valida pureza do Domain (sem pacotes externos, sem ORM)

### Unit (`Visma.Yuki.Blog.Tests.Unit`)

Projeto preparado para testes unitários de Use Cases e entidades de domínio.

### Integration (`Visma.Yuki.Blog.Tests.Integration`)

Projeto preparado para testes de integração com banco de dados e API.

---

## CI/CD

O workflow `.github/workflows/pr-validation.yml` executa em cada PR para `master` ou `development`:

1. **Build** — Compila a solution em Release
2. **Tests** — Executa todos os testes e publica resultados como annotations no PR
3. **Changelog Check** — Verifica se o `changelog.md` foi atualizado e tem conteúdo sob `## [Unreleased]`

---

## Como Executar Localmente

### Pré-requisitos

- .NET 10 SDK (preview)
- .NET Aspire workload (`dotnet workload install aspire`)
- Docker (para PostgreSQL via Aspire)

### Comando

```bash
cd src
dotnet run --project Orchestration/Visma.Yuki.Blog.Aspire.Orchestration
```

O Aspire iniciará PostgreSQL, aplicará as migrations e subir a API. O Aspire Dashboard estará disponível em `http://localhost:18888`.
