# CodingAgentStandardHelper

**Status**: Phase 1 Implementation Complete ✅

A comprehensive system for managing and retrieving coding standards to help AI assistants follow consistent development patterns. Uses vector embeddings for semantic search and Entity Framework Core for persistent storage.

## Quick Start

### Prerequisites
- .NET 9 SDK
- SQLite (included in EF Core)
- Chroma (optional, running on port 8601 for Phase 2+)

### Build & Test
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run API (Phase 2+)
cd src/CodingAgentHelper.Api
dotnet run

# Run Web UI (Phase 2+)
cd src/CodingAgentHelper.Web
dotnet run
```

## Architecture

### Core Layers
1. **Domain Layer** (`CodingAgentHelper.Core`)
   - Entities: `Standard`, `Category`
   - Repositories: `IStandardRepository`, `ICategoryRepository`
   - Services: `IStandardService`, `ICategoryService`
   - Vector Store: `IChromaClient`, `IEmbeddingService`

2. **API Layer** (`CodingAgentHelper.Api`)
   - REST endpoints (40+ planned for Phase 2)
   - Swagger/OpenAPI documentation
   - ASP.NET Core 9

3. **Web Layer** (`CodingAgentHelper.Web`)
   - Razor Pages UI
   - Admin dashboard
   - Standard browsing and management

### Database
- **SQLite** (default, file-based): `./data/cah_standards.db`
- **Entity Framework Core** for ORM
- Migration path to PostgreSQL post-release

### Vector Store
- **Chroma** (Phase 1 uses mock, Phase 2+ HTTP client)
- Port: 8601 (isolated from DevOps port 8600)
- Collections: `cah_standards`, `cah_categories`

## Features

### Phase 1 (Current) ✅
- ✅ Domain models with validation
- ✅ CRUD operations for standards and categories
- ✅ Full-text search (title/description)
- ✅ Category filtering
- ✅ Priority and status management
- ✅ Tag-based organization
- ✅ Unit test infrastructure
- ✅ Mock vector store for testing

### Phase 2 (Planned)
- 🔄 REST API endpoints (40+ operations)
- 🔄 Actual Chroma HTTP client integration
- 🔄 ML-based embeddings (Sentence Transformers)
- 🔄 Razor Pages admin UI
- 🔄 Integration tests
- 🔄 CI/CD pipeline (GitHub Actions)
- 🔄 Advanced search filters

### Phase 3+ (Future)
- 📋 AI suggestion engine
- 📋 Compliance validation
- 📋 Category hierarchy
- 📋 Version control for standards
- 📋 Audit logging
- 📋 Multi-tenant support

## Project Structure

```
src/
├── CodingAgentHelper.Core/           # Domain & Business Logic
│   ├── Domain/
│   │   ├── Entities/                 # Standard, Category
│   │   ├── Repositories/             # Repository interfaces
│   │   └── Exceptions/               # Domain exceptions
│   ├── Application/
│   │   └── Services/                 # Business services
│   └── Infrastructure/
│       ├── Data/                     # EF Core DbContext & Repositories
│       └── VectorStore/              # Chroma integration
├── CodingAgentHelper.Api/            # REST API
├── CodingAgentHelper.Web/            # Razor Pages UI
└── CodingAgentHelper.Utilities/      # Utilities

tests/
├── CodingAgentHelper.Core.Tests/     # Unit & Integration tests
├── CodingAgentHelper.Api.Tests/      # API tests
└── CodingAgentHelper.Web.Tests/      # UI tests
```

## Development

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/CodingAgentHelper.Core.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Adding New Standards
```csharp
// Via service
var standard = await standardService.CreateStandardAsync(
    title: "Use async/await for I/O",
    description: "Always use async patterns for I/O operations",
    category: "Performance"
);
```

### Searching Standards
```csharp
// Full-text search (Phase 1)
var results = await standardService.SearchStandardsAsync("performance async");

// Vector search (Phase 2+)
var vectorResults = await standardService.SearchStandardsAsync("optimize I/O");
```

## Configuration

### appsettings.json
```json
{
  "Chroma": {
    "Host": "http://localhost",
    "Port": 8601
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./data/cah_standards.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Database Migrations

```bash
# Create initial migration (if needed)
dotnet ef migrations add InitialCreate -p src/CodingAgentHelper.Core

# Apply migrations
dotnet ef database update -p src/CodingAgentHelper.Core
```

## Performance Notes

- SQLite is suitable for MVP (file-based, zero infrastructure)
- Mock vector store in Phase 1 (in-memory, for testing)
- Chroma supports up to millions of vectors (Phase 2+)
- Consider PostgreSQL for production (post-release migration)

## Contributing

1. Follow Domain-Driven Design principles
2. Write unit tests for new features
3. Use meaningful commit messages (conventional commits)
4. Ensure all tests pass before committing

## Troubleshooting

### Build Errors
- Ensure .NET 9 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

### Test Failures
- Check database fixture initialization
- Verify EF Core in-memory database is available
- Run with verbose output: `dotnet test -v detailed`

### SQLite Issues
- Ensure `./data/` directory exists and is writable
- SQLite file will be created automatically
- Clear data folder to reset: `rm -r ./data/`

## License

Internal project for AI assistance development.

## References

- [CodingAgentStandardHelper Implementation Plan](../../AIDocumentation/docs/projects/CodingAgentStandardHelper_IMPLEMENTATION_PLAN.md)
- [Phase 1 Session Notes](../../AIDocumentation/docs/session_notes/SESSION_2025-01-14_CodingAgentStandardHelper_Setup.md)
- [Architecture Overview](../../AIDocumentation/docs/projects/CodingAgentStandardHelper_ARCHITECTURE_DIAGRAMS.md)
