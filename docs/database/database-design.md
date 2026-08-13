# Database Design

## Entity Relationship Overview

```
Users ─────┬──── UserRoles ──── Roles
           │
           ├──── Businesses (OwnerUserId)
           ├──── BusinessStaff
           ├──── QueueTickets (CustomerId)
           ├──── Notifications
           └──── RefreshTokens

Businesses ──┬── Services
             ├── BusinessWorkingHours
             ├── BusinessStaff
             └── Queues

Queues ──── QueueTickets ──── Notifications

Services ──── Queues
```

## Tables

### Users
| Column | Type | Notes |
|--------|------|-------|
| Id | uniqueidentifier | PK |
| FirstName | nvarchar(100) | |
| LastName | nvarchar(100) | |
| MobileNumber | nvarchar(20) | Unique index |
| Email | nvarchar(256) | Unique index |
| PasswordHash | nvarchar(512) | |
| PreferredLanguage | int | Enum: Arabic=0, English=1 |
| IsActive | bit | |
| CreatedAt | datetime2 | |
| UpdatedAt | datetime2 | |

### Roles / UserRoles
Standard many-to-many. Role names: Customer, BusinessOwner, Staff, SystemAdmin.

### Businesses
Includes bilingual name/description/address fields and Category enum.

### Services
Per-business services with `AverageServiceMinutes` for wait time estimation.

### BusinessWorkingHours
One row per business per day of week. Unique on (BusinessId, DayOfWeek).

### Queues
One queue per business + service + date. Unique on (BusinessId, ServiceId, QueueDate).

| Index | Columns |
|-------|---------|
| IX_Queues_BusinessId_QueueDate | BusinessId, QueueDate |

### QueueTickets
| Index | Columns | Purpose |
|-------|---------|---------|
| IX_QueueTickets_QueueId_Status | QueueId, Status | Position calculation |
| IX_QueueTickets_QueueId_TicketNumber | QueueId, TicketNumber | Uniqueness / concurrency |

Ticket numbers format: `A001`, `A002`, etc.

### Notifications
In-app notifications with optional TicketId reference.

## Queue Concurrency Strategy (Phase 2)

1. **Join queue:** Transaction with row lock on Queue row; increment `CurrentTicketNumber`; insert ticket with unique constraint
2. **Call next:** Optimistic/pessimistic lock on next Waiting ticket; status transition with concurrency token

## Migrations

Initial migration: `InitialCreate` in `JordanQueue.Infrastructure/Persistence/Migrations/`

## Seed Data

Development seed creates:
- 4 roles
- 10 users (1 admin, 2 owners, 2 staff, 5 customers)
- 5 businesses with services and working hours
- 1 active queue with 3 sample tickets

See [DEVELOPMENT.md](../../DEVELOPMENT.md) for credentials.
