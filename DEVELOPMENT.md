# Development Guide

This document covers local development setup for Jordan Queue.

## Environment Setup

### .NET SDK

Install .NET 10 LTS SDK:

```bash
dotnet --version   # Should show 10.x
```

### SQL Server (Docker)

```bash
docker compose up -d sqlserver
```

Default connection string (already in `appsettings.Development.json`):

```
Server=localhost,1433;Database=JordanQueue;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### Run API

```bash
cd src/JordanQueue.Api
dotnet run
```

The API auto-migrates and seeds data on startup in Development mode.

## Seed Credentials (Development Only)

> **Warning:** These credentials are for local development only. Never use in production.

| Role | Email | Mobile | Password |
|------|-------|--------|----------|
| System Admin | admin@jordanqueue.dev | +962790000001 | Admin123! |
| Business Owner 1 | owner1@jordanqueue.dev | +962790000002 | Owner123! |
| Business Owner 2 | owner2@jordanqueue.dev | +962790000003 | Owner123! |
| Staff 1 | staff1@jordanqueue.dev | +962790000004 | Staff123! |
| Staff 2 | staff2@jordanqueue.dev | +962790000005 | Staff123! |
| Customer 1 | customer1@jordanqueue.dev | +962790000006 | Customer123! |
| Customer 2 | customer2@jordanqueue.dev | +962790000007 | Customer123! |
| Customer 3 | customer3@jordanqueue.dev | +962790000008 | Customer123! |
| Customer 4 | customer4@jordanqueue.dev | +962790000009 | Customer123! |
| Customer 5 | customer5@jordanqueue.dev | +962790000010 | Customer123! |

## Seed Businesses

| Business | Owner | Category |
|----------|-------|----------|
| Al Noor Medical Clinic | Owner 1 | Clinics |
| Amman Barber | Owner 1 | Barbers |
| Fast Auto Service | Owner 2 | Car Services |
| Jordan Car Care | Owner 2 | Car Services |
| Al Madina Dental Clinic | Owner 2 | Clinics |

Sample queue data is seeded for **Amman Barber** with tickets A001–A003.

### Run Admin Portal

```bash
cd admin/JordanQueue.Admin
npm install
npm run dev
```

- URL: http://localhost:5173
- Login as **owner1@jordanqueue.dev** / `Owner123!` or **staff1@jordanqueue.dev** / `Staff123!`
- Select **Amman Barber** to manage the seeded queue (tickets A001–A003)

Build for production:

```bash
npm run build
```

### Run Mobile App

```bash
cd mobile/JordanQueue.Mobile
npm install
npm start
```

- Use **Expo Go** on your phone or an Android/iOS emulator
- Default API URL: `http://10.0.2.2:5257` (Android emulator) or `http://localhost:5257` (iOS/web)
- On a **physical device**, create `.env` with `EXPO_PUBLIC_API_URL=http://YOUR_PC_IP:5257`
- Login: **customer1@jordanqueue.dev** / `Customer123!`
- Try **Amman Barber** → **Haircut** → Join queue

## Project Structure

```
jordan-queue/
├── src/
│   ├── JordanQueue.Api/
│   ├── JordanQueue.Application/
│   ├── JordanQueue.Domain/
│   └── JordanQueue.Infrastructure/
├── mobile/JordanQueue.Mobile/      # Phase 4
├── admin/JordanQueue.Admin/        # Phase 3
├── tests/
├── docs/
├── database/
├── .github/workflows/
├── docker-compose.yml
├── README.md
└── DEVELOPMENT.md
```

## Branch Strategy

- `main` — production-ready
- `develop` — integration branch
- `feature/*` — new features
- `bugfix/*` — bug fixes
- `hotfix/*` — production hotfixes

## Commit Convention

```
feat: implement customer queue joining
fix: prevent duplicate queue ticket numbers
refactor: extract queue position calculator
docs: update API overview
test: add queue concurrency tests
chore: update CI pipeline
```

## Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/JordanQueue.UnitTests

# Integration tests only
dotnet test tests/JordanQueue.IntegrationTests
```

## Docker (Full Stack)

```bash
docker compose up --build
```

API will be available at http://localhost:8080

## Timezone

All business dates use **Asia/Amman** timezone via `IDateTimeProvider.TodayInAmman`.

## Troubleshooting

### SQL Server connection refused

Ensure Docker is running and the container is healthy:

```bash
docker compose ps
docker compose logs sqlserver
```

### Migration errors

Reset the database (development only):

```bash
dotnet ef database drop --force \
  --project src/JordanQueue.Infrastructure \
  --startup-project src/JordanQueue.Api

dotnet ef database update \
  --project src/JordanQueue.Infrastructure \
  --startup-project src/JordanQueue.Api
```

### Port conflicts

Change the port in `src/JordanQueue.Api/Properties/launchSettings.json` or set `ASPNETCORE_URLS`.
