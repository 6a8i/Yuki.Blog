# Changelog

## [Unreleased]
<!-- Add new features here before they go to development -->
### Added

- Blog posts can now be created via the API (`POST /api/v1/posts/`), with automatic author resolution by ID or name/surname, validation, and `201 Created` response
- Automated test coverage for the post creation flow, including unit and integration tests with a real database

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
