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

## Database

This API uses **SQLite** and **EF Core**. The initial schema has 4 tables: `Users`, `Samples`, `Tests`, `Results`.

## Roles

Authorization is permission-based. At login, the API issues a JWT that includes the user’s role and derived permissions.

| Role | Permissions (what they can do) |
| --- | --- |
| `Scientist` | Read + create/update samples (`Samples:Read`, `Samples:Write`) |
| `Technician` | Read samples, create/update tests, assign tests, create results (`Samples:Read`, `Tests:Write`, `Tests:Assign`, `Results:Write`) |
| `Supervisor` | Everything **except** manage users (all permissions minus `Users:Manage`) |
| `Admin` | Everything, including user management (all permissions) |

### Example placeholder data

#### `Users`

| Id | Email | FullName | Role |
| --- | --- | --- | --- |
| 1 | `admin@example.com` | `Alex Admin` | `Admin` |
| 2 | `tech@example.com` | `Taylor Tech` | `Tech` |

#### `Samples`

| Id | Name | Type | Origin | Status | SubmittedById |
| --- | --- | --- | --- | --- | --- |
| 100 | `River Water A` | `Water` | `Site 12` | `Submitted` | 2 |
| 101 | `Soil Core B` | `Soil` | `Field 3` | `InProgress` | 2 |

#### `Tests`

| Id | Method | Status | SampleId | AssignedToId |
| --- | --- | --- | --- | --- |
| 500 | `pH (Electrode)` | `Done` | 100 | 2 |
| 501 | `Nitrate (IC)` | `Running` | 100 | 2 |

#### `Results`

| Id | TestId | Value | Unit | IsPublished | RecordedById | PublishedById |
| --- | --- | --- | --- | --- | --- | --- |
| 900 | 500 | `7.12` | `pH` | true | 2 | 1 |
| 901 | 501 | `14.8` | `mg/L` | false | 2 |  |

## Solution layout

| Path                        | Purpose                                 |
| --------------------------- | --------------------------------------- |
| `LabLedger.sln`             | Solution entry point                    |
| `src/LabLedger.Api`         | HTTP API and controllers                |
| `src/LabLedger.Application` | Application services and DTOs           |
| `src/LabLedger.Core`        | Domain interfaces                       |
| `src/LabLedger.DataModel`   | EF Core entities, DbContext, migrations |
| `tests/LabLedger.Tests`     | Unit and integration tests (xUnit)      |
