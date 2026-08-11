# Architecture Overview

## Pattern: Modular Monolith

Jordan Queue is built as a single deployable application with clear module boundaries. This allows fast MVP delivery while keeping the door open for future extraction into services if needed.

## Layers

```
┌─────────────────────────────────────────┐
│           JordanQueue.Api               │
│  Controllers, Middleware, Swagger, DI   │
├─────────────────────────────────────────┤
│       JordanQueue.Application           │
│  Use Cases, DTOs, Validation, Interfaces│
├─────────────────────────────────────────┤
│         JordanQueue.Domain              │
│  Entities, Enums, Domain Constants      │
├─────────────────────────────────────────┤
│      JordanQueue.Infrastructure         │
│  EF Core, Repositories, Notifications   │
└─────────────────────────────────────────┘
         │
         ▼
    SQL Server
```

## Dependency Rules

- **Domain** has no dependencies on other projects
- **Application** depends only on Domain
- **Infrastructure** depends on Application and Domain
- **Api** depends on Application and Infrastructure

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Modular monolith | Faster MVP, simpler deployment; microservices deferred |
| SQL Server + EF Core | Relational data, strong consistency for queue tickets |
| JWT auth (Phase 2) | Stateless API suitable for mobile + web clients |
| Notification abstraction | In-app now; FCM/email/SMS/WhatsApp later without API changes |
| Asia/Amman timezone | All queue dates scoped to Jordan local time |
| Ticket number concurrency | Database transactions + unique index on (QueueId, TicketNumber) |

## API Response Format

All API responses use a consistent envelope:

```json
{
  "success": true,
  "data": {},
  "message": null,
  "errors": []
}
```

## Error Handling

Global exception middleware maps domain exceptions to HTTP status codes:

| Exception | HTTP Status |
|-----------|-------------|
| ValidationException | 400 |
| UnauthorizedException | 401 |
| NotFoundException | 404 |
| ConflictException | 409 |
| AppException | configurable |
| Unhandled | 500 |

## Health Checks

| Endpoint | Purpose |
|----------|---------|
| `GET /health` | Liveness — app is running |
| `GET /health/ready` | Readiness — includes SQL Server connectivity |
| `GET /api/health` | Application health with JSON response envelope |

## Future Extensibility

The architecture supports future additions without major rewrites:

- **Subscriptions/SaaS billing** — add `SubscriptionPlan` entity and middleware
- **Push notifications** — implement `INotificationSender` with Firebase
- **Multi-country** — extend timezone and currency configuration
- **Appointments** — add alongside queue entities in Domain layer
- **Microservices** — extract Queue Engine as first candidate service

## Deployment Topology (Target)

```
┌──────────┐     ┌──────────┐     ┌──────────────┐
│  Mobile  │────▶│   API    │────▶│  SQL Server  │
│  (Expo)  │     │ (Docker) │     │   (Docker)   │
└──────────┘     └──────────┘     └──────────────┘
┌──────────┐           │
│  Admin   │───────────┘
│  (Vite)  │
└──────────┘
```

## Security (Phase 2+)

- Password hashing (BCrypt/Argon2 planned for Phase 2; dev seed uses SHA256)
- JWT access + refresh tokens
- Role-based authorization policies
- Rate limiting on auth endpoints
- Environment-based secrets (never in source)
