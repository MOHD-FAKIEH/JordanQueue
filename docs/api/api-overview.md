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

## Phase 2 — Implemented

### Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Anonymous | Register customer or business owner |
| POST | `/api/auth/login` | Anonymous | Login with email/mobile + password |
| POST | `/api/auth/refresh` | Anonymous | Refresh JWT access token |

### Businesses

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/businesses` | Anonymous | Search/list businesses |
| GET | `/api/businesses/mine` | Owner/Staff | Businesses assigned to current user |
| GET | `/api/businesses/{id}` | Anonymous | Business details + queue summary |
| POST | `/api/businesses` | BusinessOwner | Create business |
| PUT | `/api/businesses/{id}` | BusinessOwner | Update business |
| GET | `/api/businesses/{businessId}/queue` | Anonymous | Current queue for today |
| POST | `/api/businesses/{businessId}/queue/open` | Owner/Staff | Open today's queue |
| POST | `/api/businesses/{businessId}/queue/join` | Customer | Join queue |
| GET | `/api/businesses/{businessId}/stats/daily` | Owner/Staff | Daily statistics |

### Services

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/businesses/{businessId}/services` | Anonymous | List services |
| POST | `/api/businesses/{businessId}/services` | Owner/Staff | Create service |
| PUT | `/api/services/{id}` | Owner/Staff | Update service |

### Queues

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/queues/{queueId}` | Anonymous | Queue details |
| GET | `/api/queues/{queueId}/position` | Customer | Your position |
| POST | `/api/queues/{queueId}/next` | Owner/Staff | Call next customer |
| POST | `/api/queues/{queueId}/pause` | Owner/Staff | Pause queue |
| POST | `/api/queues/{queueId}/resume` | Owner/Staff | Resume queue |

### Tickets

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/tickets/mine` | Authenticated | Customer ticket history |
| GET | `/api/tickets/{id}` | Authenticated | Ticket details |
| POST | `/api/tickets/{id}/cancel` | Customer/Staff | Cancel ticket |
| POST | `/api/tickets/{id}/serve` | Owner/Staff | Mark served |
| POST | `/api/tickets/{id}/skip` | Owner/Staff | Skip customer |

### Notifications

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | Authenticated | List notifications |
| POST | `/api/notifications/{id}/read` | Authenticated | Mark as read |

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
