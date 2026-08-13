# Contributing to Jordan Queue

Thank you for contributing to Jordan Queue.

## Getting Started

1. Fork the repository
2. Create a feature branch from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/your-feature-name
   ```
3. Make your changes
4. Run tests: `dotnet test`
5. Commit using the [commit convention](DEVELOPMENT.md#commit-convention)
6. Push and open a pull request against `develop`

## Code Standards

- Follow existing naming and project structure conventions
- Keep business logic out of controllers
- Use DTOs — never expose EF entities from API endpoints
- Use async/await with cancellation tokens in .NET code
- Add tests for business-critical logic (especially queue concurrency)
- Do not commit secrets, `.env` files, or connection strings with passwords

## Pull Request Checklist

- [ ] Code builds without errors (`dotnet build`)
- [ ] All tests pass (`dotnet test`)
- [ ] New endpoints documented in `docs/api/`
- [ ] Migrations included if schema changed
- [ ] No secrets committed

## Reporting Issues

Use GitHub Issues with the appropriate epic label. See [docs/requirements/github-issues.md](docs/requirements/github-issues.md) for the issue backlog.
