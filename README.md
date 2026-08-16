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
| Mobile | React Native + Expo + TypeScript |
| Admin | React + Vite + MUI + TypeScript |
| Auth | JWT + refresh tokens (Phase 2) |
| Deployment | Docker, GitHub Actions |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for SQL Server)
- Node.js 20+ (for admin portal and mobile)

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

- Swagger UI: http://localhost:5257/swagger (port may vary — check console output)
- Health (liveness): http://localhost:5257/health
- Health (readiness + SQL): http://localhost:5257/health/ready

### 3. Run the Admin Portal

```bash
cd admin/JordanQueue.Admin
npm install
npm run dev
```

Open http://localhost:5173 and sign in with a **BusinessOwner** or **Staff** seed account (see [DEVELOPMENT.md](DEVELOPMENT.md)).

The dev server proxies `/api` to the backend. CORS is also enabled for `localhost:5173`.

### 4. Run the Mobile App

```bash
cd mobile/JordanQueue.Mobile
npm install
npm start
```

Use Expo Go on your phone or an emulator. See [mobile/JordanQueue.Mobile/README.md](mobile/JordanQueue.Mobile/README.md) for API URL setup on physical devices.

### 5. Run Tests

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

## Current Status — Phase 4 In Progress

Phase 4 adds the React Native (Expo) customer mobile app: business search, join queue, live ticket tracking, notifications, and Arabic/English support.

## Current Status — Phase 3 Complete

Phase 3 adds the React admin portal with login, dashboard, queue management, services CRUD, staff management, and business settings. Arabic/English with RTL/LTR support.

**Next:** Phase 4 — Mobile App (React Native customer app)

## Current Status — Phase 2 Complete

Phase 2 adds JWT authentication, business/service management, the full queue engine (join, call next, serve, skip, cancel), position/wait calculation, and in-app notifications.

## License

Proprietary — Jordan Queue MVP
