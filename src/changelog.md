# Changelog

## [Unreleased]
### Added

- Test suite coverage raised to 97% line coverage (from 63.7%): added `UnitOfWorkTests` (integration) covering `Dispose`, `DisposeAsync`, `CommitAsync` rollback-on-failure path and `RollbackAsync`; added `UniqueNameIdentifier` numeric-name validation test; added `PostCommandHandler` defensive-branch test for the "no author identification" guard.
- `src/coverlet.runsettings` added to exclude auto-generated code (OpenAPI source generator, regex source generator, compiler services helpers) from coverage metrics.

## [v3.0.1] - 2026-07-12
### Added

- HATEOAS links on all Post endpoints: `GET /posts/` (collection with `self` and `create` links), `GET /posts/{id}` (with `self` and `collection` links), and `POST /posts/` (with `self` and `collection` links). Collection responses now wrapped in `CollectionResponse<T>` with `items` and `links` fields.

### Changed

- `GET /posts/` now returns `200 OK` with empty collection instead of `204 No Content` when no posts exist
- `POST /posts/` now returns the created `PostResponse` (with `id`, post fields and HATEOAS links) instead of the raw `Guid`
- `IPostCommandHandler.HandleAsync` now returns `Result<Post>` instead of `Result<Guid>`
- Added `ProducesProblem` declarations to Post endpoints for OpenAPI documentation (`400` on all, `404` on `GET /{id}`)

## [v2.0.1] - 2026-07-12
### Added

- HATEOAS links on all Author endpoints: `GET /authors/` (collection with `self` and `create` links), `GET /authors/{id}` (with `self` and `collection` links), and `POST /authors/` (with `self` and `collection` links). Collection responses now wrapped in `CollectionResponse<T>` with `items` and `links` fields.

### Changed

- `GET /authors/` now returns `200 OK` with empty collection instead of `204 No Content` when no authors exist
- Added `ProducesProblem` declarations to Author endpoints for OpenAPI documentation (`400` on all, `404` on `GET /{id}`)


## [v1.2.2] - 2026-07-12
### Changed

- Post flow refactored to CQRS pattern: `IPostUseCase` split into `IPostCommandHandler` (writes) and `IPostQueryHandler` (reads), with separate query DTOs (`GetAllPostsQuery`, `GetPostByIdQuery`)

## [v1.2.1] - 2026-07-12
### Added

- Project README with full setup instructions for Windows and Linux, including prerequisites, .NET HTTPS certificate trust, and how to run the application and tests

### Changed

- Author flow refactored to CQRS pattern: `IAuthorUseCase` split into `IAuthorCommandHandler` (writes) and `IAuthorQueryHandler` (reads), with separate query DTOs (`GetAllAuthorsQuery`, `GetAuthorByIdQuery`)


## [v1.2.0] - 2026-07-12
### Added

- Blog posts can now be created via the API (`POST /api/v1/posts/`), with automatic author resolution by ID or name/surname, validation, and `201 Created` response
- Blog posts can now be retrieved via the API (`GET /api/v1/posts/`), with optional author data inclusion via `includeAuthor` query parameter, returning `200 OK` or `204 No Content` when empty
- A single blog post can now be retrieved by ID (`GET /api/v1/posts/{id}`), with optional author data inclusion via `includeAuthor` query parameter, returning `200 OK` or `404 Not Found` when not found
- Automated test coverage for post creation and retrieval flows, including unit and integration tests with a real database

## [v1.1.0] - 2026-07-12
### Added

- Authors can now be created via the API (`POST /api/v1/authors/`), with validation, duplicate detection, and `201 Created` response
- Authors can now be retrieved via the API (`GET /api/v1/authors/`), returning all registered authors or an empty response when none exist
- A single author can now be retrieved by ID (`GET /api/v1/authors/{id}`), returning the author data or `404 Not Found` when not found
- Each author is uniquely identified by a generated hash based on their name and surname, ensuring deduplication
- Architecture documentation added covering the project's structure, patterns, and tech stack
- Automated test coverage for author creation and retrieval flows, including unit and integration tests with a real database

### Changed

- CI pipeline improved with better test result reporting on pull requests

## [v1.0.0] - 2026-07-11
### Added

- **Author management:** Core entity representing blog authors with name, surname, and unique identifier
- **Blog post foundation:** Database tables created for authors and posts with performance indexes and seed data
- **REST API:** Initial API endpoints for creating and listing authors with URL-based versioning
- **Application orchestration:** Local development environment with PostgreSQL, automatic database migrations, and API orchestration via .NET Aspire
- **Architecture tests:** Automated validation enforcing hexagonal architecture, DDD principles, and domain purity

### Changed

- Solution restructured with separated layers (Domain, Application, Infrastructure, API) and orchestration projects
- API entry point refactored to use modular endpoint definitions, API versioning, and Aspire service defaults

### Removed

- Placeholder files from initial project scaffold
