# API Overview

Base URL: `/api`

All responses use the standard envelope:

```json
{
  "success": true,
  "data": { },
  "message": null,
  "errors": []
}
```

## Phase 1 — Implemented

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health` | Application health check |
| GET | `/health` | Liveness probe |
| GET | `/health/ready` | Readiness probe (includes SQL Server) |

## Phase 2 — Planned

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login, returns JWT |
| POST | `/api/auth/refresh` | Refresh access token |

### Businesses

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/businesses` | List/search businesses |
| GET | `/api/businesses/{id}` | Business details |
| POST | `/api/businesses` | Create business (owner) |
| PUT | `/api/businesses/{id}` | Update business (owner) |

### Services

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/businesses/{businessId}/services` | List services |
| POST | `/api/businesses/{businessId}/services` | Create service |
| PUT | `/api/services/{id}` | Update service |

### Queues

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/businesses/{businessId}/queue` | Current queue status |
| POST | `/api/businesses/{businessId}/queue/join` | Join queue |
| GET | `/api/queues/{queueId}` | Queue details |
| GET | `/api/queues/{queueId}/position` | Customer position |

### Tickets

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tickets/{id}` | Ticket details |
| POST | `/api/tickets/{id}/cancel` | Cancel ticket |

### Staff Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/queues/{queueId}/next` | Call next customer |
| POST | `/api/tickets/{id}/serve` | Mark served |
| POST | `/api/tickets/{id}/skip` | Skip customer |
| POST | `/api/tickets/{id}/cancel` | Staff cancel |

### Notifications

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/notifications` | List user notifications |
| POST | `/api/notifications/{id}/read` | Mark as read |

## Error Codes (Planned)

| Code | HTTP | Description |
|------|------|-------------|
| QUEUE_CLOSED | 400 | Queue is not accepting customers |
| QUEUE_PAUSED | 400 | Queue is paused |
| DUPLICATE_TICKET | 409 | Concurrent join conflict |
| NOT_FOUND | 404 | Resource not found |
| UNAUTHORIZED | 401 | Invalid or missing token |
| VALIDATION_ERROR | 400 | Input validation failed |

## Swagger

Interactive API documentation available at `/swagger` in Development mode.
