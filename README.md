# Vendix

A global-ready, scalable e-commerce platform built with Clean Architecture and DDD.

## Technology Stack

- .NET 10 LTS
- Blazor Web App (Interactive Auto)
- Entity Framework Core 10 with PostgreSQL
- MediatR for CQRS
- FluentValidation
- Mapster

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 16+
- Node.js (for Tailwind CSS in production)

### Running the Application

```bash
# Clone the repository
git clone <repo-url>

# Navigate to the solution directory
cd Vendix

# Restore dependencies
dotnet restore

# Run database migrations (when available)
dotnet ef database update -p src/Vendix.Infrastructure -s src/Vendix.Web

# Run the web application
dotnet run --project src/Vendix.Web

# Run tests
dotnet test
```

## Project Structure

See [ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed documentation.

```
Vendix/
├── src/
│   ├── Vendix.Domain/         # Domain entities, value objects, interfaces
│   ├── Vendix.Application/    # CQRS commands, queries, DTOs, behaviors
│   ├── Vendix.Infrastructure/ # EF Core, repositories, external services
│   ├── Vendix.Web/            # Blazor Web App
│   └── Vendix.Api/            # REST API
├── tests/
│   ├── Vendix.Domain.Tests/
│   ├── Vendix.Application.Tests/
│   └── Vendix.Integration.Tests/
└── docs/
    ├── ARCHITECTURE.md
    └── CHANGELOG.md
```

## Key Features

- Multi-language support (EN/FA)
- Configurable payment and shipping providers
- Physical and digital product types
- Full audit trail
- Soft delete support
- Optimistic concurrency with RowVersion

## Architecture

The solution follows Clean Architecture principles:

- **Domain Layer**: Zero dependencies, pure business logic
- **Application Layer**: CQRS with MediatR, validation, DTOs
- **Infrastructure Layer**: EF Core, repositories, external services
- **Presentation Layer**: Blazor Web App and REST API

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Vendix.Domain.Tests
dotnet test tests/Vendix.Application.Tests
```

### Code Style

- File-scoped namespaces
- Primary constructors where appropriate
- XML documentation on public members
- Record types for Commands, Queries, and DTOs

## License

MIT
