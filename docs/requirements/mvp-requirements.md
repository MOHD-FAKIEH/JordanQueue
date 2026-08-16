# MVP Requirements Summary

See the full product specification for complete details. This document summarizes MVP scope and success criteria.

## MVP Users

1. **Customer** — join queues, track position, receive notifications
2. **Business Owner** — manage business, services, queue, staff
3. **Staff** — operate queue (call next, serve, skip, cancel)

## In Scope

- User registration and login
- Business CRUD with bilingual fields (Arabic/English)
- Service types with average duration
- Working hours configuration
- Virtual queue with ticket numbers (A001, A002, ...)
- Queue position and estimated wait time
- Staff queue operations
- In-app notifications
- Basic daily statistics
- RTL/LTR support (mobile and admin — Phase 3/4)

## Out of Scope (MVP)

- Payments and subscriptions
- Google Maps
- Complex appointments
- WhatsApp/SMS (architecture prepared, not required)
- Multi-country
- Microservices / Kubernetes

## Success Scenario

The MVP is complete when this end-to-end flow works:

1. Business owner registers and creates a business + service
2. Customer searches, selects service, joins queue → receives A001
3. Second customer receives A002
4. Staff sees both, calls A001 → status becomes Called
5. Customer sees updated status
6. Staff serves A001 → status becomes Served
7. Staff calls A002
8. Customer can cancel while waiting
9. Dashboard shows daily statistics

## Development Phases

| Phase | Status | Description |
|-------|--------|-------------|
| 1 — Foundation | ✅ Complete | Solution, DB, API shell, docs, CI |
| 2 — Core Queue Engine | ✅ Complete | Auth, businesses, queue operations |
| 3 — Admin Portal | ✅ Complete | React dashboard |
| 4 — Mobile App | 🚧 In Progress | React Native customer app |
| 5 — Notifications | Planned | In-app + FCM prep |
| 6 — Testing | Planned | Concurrency, auth, RTL tests |
| 7 — Deployment | Planned | Docker, staging, production |

## Localization

- Primary languages: Arabic, English
- Arabic is first-class (not post-hoc translation)
- Currency: JOD
- Timezone: Asia/Amman
