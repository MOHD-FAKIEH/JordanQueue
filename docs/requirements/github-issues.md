# GitHub Issues Backlog

Create these issues in the repository to track MVP development.

## Epic 1: Project Foundation

- [x] Create repository
- [x] Create .NET solution
- [ ] Create React admin (Phase 3)
- [ ] Create React Native mobile (Phase 4)
- [x] Configure database
- [x] Configure EF Core
- [x] Configure CI pipeline

## Epic 2: Authentication

- [ ] User registration
- [ ] Login
- [ ] JWT access tokens
- [ ] Refresh tokens
- [ ] Role-based authorization
- [ ] Authorization policies per endpoint

## Epic 3: Business Management

- [ ] Business CRUD
- [ ] Services CRUD
- [ ] Working hours configuration
- [ ] Staff management

## Epic 4: Queue Engine

- [ ] Create/open daily queue
- [ ] Join queue with ticket generation
- [ ] Calculate queue position
- [ ] Calculate estimated wait time
- [ ] Call next customer (concurrency-safe)
- [ ] Serve customer
- [ ] Skip customer
- [ ] Cancel ticket
- [ ] Pause/resume queue

## Epic 5: Customer Mobile

- [ ] Home screen with search
- [ ] Business list and details
- [ ] Service selection
- [ ] Join queue flow
- [ ] Ticket screen (live updates)
- [ ] Notifications
- [ ] Ticket history

## Epic 6: Admin Portal

- [ ] Login
- [ ] Dashboard with statistics
- [ ] Queue management screen
- [ ] Services management
- [ ] Staff management
- [ ] Business settings

## Epic 7: Testing

- [ ] Unit tests for queue logic
- [ ] Integration tests for API endpoints
- [ ] Queue concurrency tests (duplicate ticket prevention)
- [ ] Authentication and authorization tests

## Epic 8: Deployment

- [ ] Docker images for API
- [ ] GitHub Actions deploy pipeline
- [ ] Development environment
- [ ] Staging environment
- [ ] Production preparation

## Recommended Next Issue

**Epic 2 / User registration and login with JWT**

This unblocks all authenticated endpoints in Phase 2 (Core Queue Engine).
