# Jordan Queue

Jordan Queue is a virtual queue and appointment management platform for businesses and service providers in Jordan. Customers can join a queue remotely, receive a ticket number, track their position, and get notified when their turn is approaching — without physically waiting in line.

## Architecture

This project uses a **modular monolith** architecture:

```
src/
├── JordanQueue.Api/           # ASP.NET Core Web API (REST, Swagger, health checks)
├── JordanQueue.Application/   # Business logic, DTOs, validation, interfaces
├── JordanQueue.Domain/        # Domain entities, enums, constants
└── JordanQueue.Infrastructure/# EF Core, repositories, notifications, seed data

mobile/JordanQueue.Mobile/     # React Native + Expo (Phase 4)
admin/JordanQueue.Admin/       # React + Vite admin portal (Phase 3)

tests/
├── JordanQueue.UnitTests/
└── JordanQueue.IntegrationTests/
```

## Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core 10 (LTS), C#, EF Core |
| Database | SQL Server |
| Mobile | React Native + Expo + TypeScript (planned) |
| Admin | React + Vite + TypeScript (planned) |
| Auth | JWT + refresh tokens (Phase 2) |
| Deployment | Docker, GitHub Actions |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for SQL Server)
- Node.js 20+ (for mobile/admin — future phases)

## Quick Start

### 1. Start SQL Server

```bash
docker compose up -d sqlserver
```

Wait until the container is healthy (~30 seconds on first run).

### 2. Run the API

```bash
cd src/JordanQueue.Api
dotnet run
```

On first run in Development, the API automatically applies migrations and seeds sample data.

- Swagger UI: http://localhost:5080/swagger (port may vary — check console output)
- Health (liveness): http://localhost:5080/health
- Health (readiness + SQL): http://localhost:5080/health/ready
- API health: http://localhost:5080/api/health

### 3. Run Tests

```bash
dotnet test
```

### 4. Database Migrations

```bash
# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project src/JordanQueue.Infrastructure \
  --startup-project src/JordanQueue.Api \
  --output-dir Persistence/Migrations

# Apply migrations manually
dotnet ef database update \
  --project src/JordanQueue.Infrastructure \
  --startup-project src/JordanQueue.Api
```

## Configuration

Connection strings and secrets are configured via environment variables or `appsettings.Development.json`. Never commit real secrets.

| Setting | Environment Variable |
|---------|---------------------|
| SQL connection | `ConnectionStrings__DefaultConnection` |
| JWT secret | `Jwt__SecretKey` |

See [DEVELOPMENT.md](DEVELOPMENT.md) for seed credentials and local setup details.

## Documentation

- [DEVELOPMENT.md](DEVELOPMENT.md) — local development guide
- [CONTRIBUTING.md](CONTRIBUTING.md) — contribution guidelines
- [docs/architecture/architecture.md](docs/architecture/architecture.md)
- [docs/database/database-design.md](docs/database/database-design.md)
- [docs/api/api-overview.md](docs/api/api-overview.md)
- [docs/requirements/mvp-requirements.md](docs/requirements/mvp-requirements.md)

## Current Status — Phase 1 Complete

Phase 1 (Foundation) includes:

- .NET solution with Domain, Application, Infrastructure, and API projects
- Domain entities and EF Core configurations
- Initial database migration
- Swagger/OpenAPI
- Global exception handling with consistent API responses
- Health checks (liveness + readiness)
- Development seed data
- Unit and integration tests
- Docker Compose for SQL Server
- GitHub Actions CI pipeline

**Next:** Phase 2 — Core Queue Engine (authentication, businesses, queue operations)

## License

Proprietary — Jordan Queue MVP
