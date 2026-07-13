# Visma.Yuki.Blog

## About This Repository

This repository is part of the **Ateliware | Visma | Yuki** hiring process — a technical assessment designed to evaluate architectural decision-making, clean code practices, testing discipline, and API design. The project is a **blog RESTFull API** built with **.NET 10**, following **Hexagonal Architecture (Ports and Adapters)** and **Domain-Driven Design (DDD)** principles.

The API provides endpoints for managing blog **authors** and **posts**, with features like automatic author resolution (by ID or by name/surname), duplicate detection via hashed unique identifiers, optional author data inclusion in post queries, and full transaction management with rollback on failure.

---

## Running the Project Locally

### Prerequisites

You can run the project in two ways — choose one:

#### Option A: .NET Aspire (full local development)

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 10.0 | Application framework |
| .NET Aspire workload | 13.4+ | Local orchestration (PostgreSQL, API, migrations) |
| Docker | Latest | Required by Aspire to spin up PostgreSQL container |

#### Option B: Docker Compose (minimal — no .NET SDK needed)

| Tool | Version | Purpose |
|---|---|---|
| Docker | Latest | Builds and runs all services (PostgreSQL, migrations, API) |

### Installing Prerequisites

#### Windows

**1. Install Docker Desktop**

Download and install from [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/). Start Docker Desktop and ensure it is running.

**2. Install .NET 10 SDK**

Download and install from [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0).

Verify the installation:

```powershell
dotnet --version
```

**3. Install the Aspire workload**

```powershell
dotnet workload install aspire
```

#### Linux

**1. Install Docker**

```bash
# Ubuntu / Debian
sudo apt-get update
sudo apt-get install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Add your user to the docker group (so you don't need sudo)
sudo usermod -aG docker $USER
newgrp docker
```

Verify the installation:

```bash
docker --version
```

**2. Install .NET 10 SDK**

```bash
# Ubuntu 24.04 (or adjust for your distro)
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft.deb
sudo dpkg -i packages-microsoft.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

Verify the installation:

```bash
dotnet --version
```

**3. Install the Aspire workload**

```bash
dotnet workload install aspire
```

### Trusting the .NET HTTPS Certificate

The API runs over HTTPS using the .NET developer certificate. You need to trust it once to avoid browser warnings or SSL errors when accessing the API locally.

#### Windows

The certificate is usually trusted automatically on Windows. If you encounter SSL errors, run:

```powershell
dotnet dev-certs https --trust
```

#### Linux

On Linux, the certificate must be trusted manually. The steps vary by distribution:

**Ubuntu / Debian (using `update-ca-certificates`):**

```bash
# Generate and export the certificate to a PEM file
dotnet dev-certs https -ep ${HOME}/.dotnet/https/aspnetcore.pem --format PEM

# Copy it to the system certificate store and set permissions
sudo cp ${HOME}/.dotnet/https/aspnetcore.pem /usr/local/share/ca-certificates/aspnetcore.crt
sudo chmod 644 /usr/local/share/ca-certificates/aspnetcore.crt

# Update the system certificate store
sudo update-ca-certificates
```

**Fedora / RHEL (using `update-ca-trust`):**

```bash
dotnet dev-certs https -ep ${HOME}/.dotnet/https/aspnetcore.pem --format PEM
sudo cp ${HOME}/.dotnet/https/aspnetcore.pem /etc/pki/ca-trust/source/anchors/aspnetcore.pem
sudo update-ca-trust
```

For browsers like Firefox or Chrome on Linux, you may also need to import the certificate manually into the browser's certificate store if the system trust is not picked up automatically.

### Running via .NET Aspire

Once all prerequisites (Option A) are installed, clone the repository and run the project:

```bash
git clone https://github.com/6a8i/Yuki.Blog.git
cd visma.yuki.blog/src
dotnet run --project Orchestration/Visma.Yuki.Blog.Aspire.Orchestration
```

.NET Aspire will automatically:
1. Start a **PostgreSQL** container via Docker
2. Run **database migrations** (SQL scripts via dbup)
3. Launch the **REST API**

The **Aspire Dashboard** will be available at `https://localhost:17102`, where you can monitor services, logs, and traces.

The **API** will be available at `https://localhost:7054/` (or the port shown in the Aspire Dashboard).

The **Scalar API reference** (interactive OpenAPI docs) is available at the root URL: `https://localhost:7054/`

### Running via Docker Compose

As an alternative to .NET Aspire, the project can be run directly with Docker Compose — no .NET SDK or Aspire workload required, only Docker (Option B).

```bash
git clone https://github.com/6a8i/Yuki.Blog.git
cd visma.yuki.blog
docker compose up --build
```

This will:
1. Start a **PostgreSQL** container (with health check)
2. Build and run the **database-migrator** image — applies SQL migrations via dbup, waits for PostgreSQL to be healthy, then exits
3. Build and run the **api** image — waits for the migrations to complete successfully before starting

The **API** will be available at `http://localhost:8080/`, with the **Scalar API reference** (interactive OpenAPI docs) at the root URL.

To stop and remove the containers:

```bash
docker compose down
```

To also remove the PostgreSQL data volume:

```bash
docker compose down -v
```

To rebuild the images after code changes:

```bash
docker compose up --build -d
```

### Accessing the API

Regardless of which method you use, the API exposes the same endpoints. The only difference is the base URL:

| Method | Base URL | Scalar API Reference |
|---|---|---|
| .NET Aspire | `https://localhost:7054` | `https://localhost:7054/` |
| Docker Compose | `http://localhost:8080` | `http://localhost:8080/` |

All endpoints are versioned under `/api/v1/`. See the [Endpoints](#endpoints) section below for the full list.

### Running the Tests

The project includes three test suites:

**Unit Tests** (98 tests) — Use cases with mocked dependencies via NSubstitute:

```bash
dotnet test src/Tests/Visma.Yuki.Blog.Tests.Unit/Visma.Yuki.Blog.Tests.Unit.csproj
```

**Integration Tests** (82 tests) — API endpoints and repositories with a real PostgreSQL database via Testcontainers (requires Docker):

```bash
dotnet test src/Tests/Visma.Yuki.Blog.Tests.Integration/Visma.Yuki.Blog.Tests.Integration.csproj
```

**Architecture Tests** (25 tests) — Enforce hexagonal architecture, DDD principles, and domain purity using NetArchTest:

```bash
dotnet test src/Tests/Visma.Yuki.Blog.Tests.Architecture/Visma.Yuki.Blog.Tests.Architecture.csproj
```

**Run all tests at once:**

```bash
dotnet test src/Visma.Yuki.Blog.sln
```

---

## Architecture and Technologies

### Hexagonal Architecture (Ports and Adapters)

The application is structured around the **Hexagonal Architecture** pattern, which isolates the business logic (Core) from external infrastructure (Adapters) through **Ports** — interfaces that define communication contracts.

```
┌─────────────────────────────────────────────────────────────┐
│                        Adapters                             │
│  ┌────────────────────┐    ┌──────────────────────────────┐ │
│  │   Driving (API)    │    │    Driven (Infrastructure)   │ │
│  │  REST Endpoints    │    │  Repositories, UnitOfWork    │ │
│  └────────┬───────────┘    └────────────┬─────────────────┘ │
│           │                             │                   │
│           ▼                             ▼                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                     Core                             │   │
│  │  ┌──────────────┐    ┌───────────────────────────┐   │   │
│  │  │   Domain     │◄── │      Application          │   │   │
│  │  │  Entities    │    │  Ports, UseCases          │   │   │
│  │  └──────────────┘    └───────────────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

**Request flow:**
1. A **Driving Adapter** (API endpoint) receives the HTTP request
2. The endpoint calls a **Driving Port** (interface in the Application layer)
3. The **Use Case** (Driving Port implementation) executes business logic
4. The Use Case calls a **Driven Port** (infrastructure interface) when persistence is needed
5. The **Driven Adapter** (Infrastructure) implements the Driven Port and executes the database operation

### Project Structure

```
src/
├── Core/                                       # Pure business logic
│   ├── Visma.Yuki.Blog.Domain/                 # Domain layer
│   │   ├── Entities/                           # Domain entities (Author, Post)
│   │   └── ValueObjects/                       # Value Objects (UniqueNameIdentifier)
│   │
│   └── Visma.Yuki.Blog.Application/            # Application layer
│       ├── Ports/
│       │   ├── Driving/                        # Inbound interfaces (IAuthorUseCase, IPostUseCase)
│       │   └── Driven/                         # Outbound interfaces (IAuthorPorts, IPostPorts, IUnitOfWork)
│       ├── UseCases/                           # Driving Port implementations
│       └── Commands/                           # Commands and validators
│
├── Adapters/                                   # External adapters
│   ├── Driving/
│   │   └── Visma.Yuki.Blog.Api/                # REST API
│   │       ├── Endpoints/V1/                   # Versioned endpoints
│   │       ├── Program.cs                      # Application entry point
│   │       └── Dockerfile                      # API container image
│   │
│   └── Driven/
│       └── Visma.Yuki.Blog.Infrastructure/     # Infrastructure
│           └── Repositories/                   # Driven Port implementations (Dapper + Npgsql)
│
├── Orchestration/                              # Orchestration and configuration
│   ├── Visma.Yuki.Blog.Aspire.Orchestration/   # Aspire AppHost
│   ├── Visma.Yuki.Blog.Database/               # SQL migrations (dbup)
│   │   └── Dockerfile                          # Database migrator container image
│   └── Visma.Yuki.Blog.Shared/                # Shared DI configuration
│
├── Tests/
│   ├── Visma.Yuki.Blog.Tests.Architecture/     # Architecture tests (NetArchTest)
│   ├── Visma.Yuki.Blog.Tests.Unit/             # Unit tests (xUnit + NSubstitute)
│   └── Visma.Yuki.Blog.Tests.Integration/      # Integration tests (Testcontainers + PostgreSQL)
│
└── Visma.Yuki.Blog.sln

docker-compose.yml                              # Runs postgres + database-migrator + api without Aspire
```

### Technology Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET | 10.0 | Application framework |
| ASP.NET Core | 10.0 | REST API |
| .NET Aspire | 13.4 | Local orchestration and service defaults |
| PostgreSQL | — | Relational database |
| Npgsql | 10.0.3 | PostgreSQL driver for .NET |
| Dapper | 2.1.79 | Lightweight micro-ORM |
| Carter | 10.0.0 | Minimal API endpoint modules |
| Asp.Versioning.Http | 10.0.0 | URL-based API versioning |
| dbup | 5.0.41 | SQL migration runner |
| FluentResults | 4.0.0 | Functional error handling (Result pattern) |
| FluentValidation | 12.1.1 | Command/request validation |
| Scalar.AspNetCore | 2.16.11 | API documentation UI |
| xUnit | 2.9.3 | Test framework |
| NSubstitute | 5.3.0 | Mocking library for unit tests |
| NetArchTest.Rules | 1.3.2 | Architecture test rules |
| Testcontainers.PostgreSql | 4.6.0 | Real PostgreSQL for integration tests |
| coverlet | 6.0.4 | Code coverage |
| OpenTelemetry | — | Observability (via Aspire) |

---

## Project Details

### Endpoints

The API exposes the following endpoints under `/api/v1/`. All responses include **HATEOAS links** (`self`, `collection`, `create`) for navigation. Collection responses are wrapped in `CollectionResponse<T>` with `items` and `links` fields.

#### Authors

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/authors/` | Create a new author with name and surname. Validates input, detects duplicates via `UniqueNameIdentifier` (a hash of name + surname). Returns `201 Created` with the author resource and `self`/`collection` links |
| `GET` | `/api/v1/authors/` | Retrieve all registered authors. Returns `200 OK` with a `CollectionResponse` (always, even when empty — `items` will be an empty array) |
| `GET` | `/api/v1/authors/{id}` | Retrieve a single author by ID. Returns `200 OK` with `self`/`collection` links, or `404 Not Found` |

#### Posts

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/posts/` | Create a new blog post. Automatically resolves the author by `AuthorId` or by name/surname (creating the author if not found). Validates fields, manages transactions with rollback on failure. Returns `201 Created` with the post resource and `self`/`collection` links |
| `GET` | `/api/v1/posts/` | Retrieve all blog posts. Supports optional `?includeAuthor=true` query parameter to include author data. Returns `200 OK` with a `CollectionResponse` (always, even when empty) |
| `GET` | `/api/v1/posts/{id}` | Retrieve a single blog post by ID. Supports optional `?includeAuthor=true` query parameter. Returns `200 OK` with `self`/`collection` links, or `404 Not Found` |

#### Example Responses

**GET `/api/v1/authors/`** — collection with HATEOAS links:

```json
{
  "items": [
    {
      "id": "b5a17382-9263-4c61-b8d1-ba204c5ebf69",
      "fullName": "John Doe",
      "links": [
        { "rel": "self", "method": "GET", "href": "/api/v1/authors/b5a17382-9263-4c61-b8d1-ba204c5ebf69" }
      ]
    }
  ],
  "links": [
    { "rel": "self", "method": "GET", "href": "/api/v1/authors/" },
    { "rel": "create", "method": "POST", "href": "/api/v1/authors/" }
  ]
}
```

**POST `/api/v1/posts/`** — request body:

```json
{
  "title": "My Blog Post",
  "description": "A short description",
  "content": "The full content of the post",
  "authorId": "b5a17382-9263-4c61-b8d1-ba204c5ebf69"
}
```

Alternatively, provide `authorName` and `authorSurname` instead of `authorId` — the API will resolve or create the author automatically:

```json
{
  "title": "My Blog Post",
  "description": "A short description",
  "content": "The full content of the post",
  "authorName": "John",
  "authorSurname": "Doe"
}
```

### Key Design Decisions

- **Hexagonal Architecture** — Business logic is isolated from infrastructure through Ports, making the Core testable and swappable without touching external concerns
- **Domain Purity** — The Domain layer has zero external dependencies (no ORM, no database drivers), enforced by automated architecture tests
- **Result Pattern** — `FluentResults` is used for functional error handling instead of throwing exceptions for expected business failures
- **Unit of Work** — Transaction management is abstracted through `IUnitOfWork`, ensuring commit/rollback semantics across repositories
- **UniqueNameIdentifier** — Authors are deduplicated by a hash of their name and surname, preventing duplicate author records
- **Architecture Tests** — 25 automated tests using `NetArchTest.Rules` enforce layer dependencies, namespace conventions, hexagonal contracts, and domain purity at compile time
- **Integration Tests with Real Database** — Integration tests use `Testcontainers` to spin up a real PostgreSQL container, ensuring tests validate actual database behavior including constraints, foreign keys, and SQL queries

### CI/CD

The GitHub Actions workflow (`.github/workflows/pr-validation.yml`) runs on every pull request to `master` or `development`:

1. **Build** — Compiles the solution in Release mode
2. **Tests** — Executes all test suites and publishes results as PR annotations
3. **Changelog Check** — Verifies that `changelog.md` has been updated with new content under `## [Unreleased]`
