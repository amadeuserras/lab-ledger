# Lab Ledger

Laboratory sample and test results API (.NET 8).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

Restore local CLI tools (Entity Framework) once after cloning:

```bash
dotnet tool restore
```

## Commands

Run these from the repository root.

### Build

```bash
dotnet build
```

### Tests

```bash
# All tests
dotnet test

# Single test class or method (adjust the filter)
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

### Run the API

```bash
dotnet run --project src/LabLedger.Api
```

In Development, Swagger is at [http://localhost:5143/swagger](http://localhost:5143/swagger) (see `src/LabLedger.Api/Properties/launchSettings.json` for URLs and profiles).

Apply migrations before the first local run (SQLite file `labledger.db` in the API project directory):

```bash
dotnet ef database update --project src/LabLedger.DataModel --startup-project src/LabLedger.Api
```

### Database migrations

```bash
# Add a migration
dotnet ef migrations add <MigrationName> --project src/LabLedger.DataModel --startup-project src/LabLedger.Api

# Apply migrations
dotnet ef database update --project src/LabLedger.DataModel --startup-project src/LabLedger.Api
```

## Solution layout

| Path                        | Purpose                                 |
| --------------------------- | --------------------------------------- |
| `LabLedger.sln`             | Solution entry point                    |
| `src/LabLedger.Api`         | HTTP API and controllers                |
| `src/LabLedger.Application` | Application services and DTOs           |
| `src/LabLedger.Core`        | Domain interfaces                       |
| `src/LabLedger.DataModel`   | EF Core entities, DbContext, migrations |
| `tests/LabLedger.Tests`     | Unit and integration tests (xUnit)      |
