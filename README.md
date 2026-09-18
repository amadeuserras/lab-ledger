# Lab Ledger 🧪

Lab Ledger is a system for tracking **lab samples**, the **tests** you run on them, and the **results** you record — with **role-based access**, so different users can do different things.

If you’ve ever had sample info spread across emails, spreadsheets, and sticky notes, this is meant to replace that with one clear source of truth.

## What you can do with it

- **Track samples**: what they are, where they came from, and their current status.
- **Run tests**: attach one or more tests to a sample and track progress.
- **Record results**: store the measured value, unit, and whether the result is published.
- **Control access**: different roles can do different things (see below).

## Data model

- **`Users`** – `email`, `fullName`, `role`
- **`Samples`** – `name`, `type`, `origin`, `status`, `submittedById`
- **`Tests`** – `method`, `status`, `sampleId`, `assignedToId`
- **`Results`** – `testId`, `value`, `unit`, `isPublished`, `recordedById`, `publishedById`

## Roles

| Role         | What they can do                               |
| ------------ | ---------------------------------------------- |
| `Scientist`  | Work with samples (view, create, update)       |
| `Technician` | Work with tests and results (and view samples) |
| `Supervisor` | Do almost everything (but not manage users)    |
| `Admin`      | Do everything, including user management       |

## Endpoints

Most endpoints require you to be logged in. You log in once, then send the token with each request like so: `Authorization: Bearer <your-token>`

- Register a user: `POST /api/auth/register`
- Log in (get a token): `POST /api/auth/login`
- List samples: `GET /api/samples` (optional `?status=InProgress`)
- Get one sample: `GET /api/samples/{id}`
- Create a sample: `POST /api/samples`
- Update a sample’s status: `PATCH /api/samples/{id}/status`
- Create a test for a sample: `POST /api/samples/{id}/tests`
- Assign a test to someone: `PATCH /api/tests/{id}/assign`
- Update a test’s status: `PATCH /api/tests/{id}/status`
- Record a result for a test: `POST /api/tests/{id}/result`
- View a result: `GET /api/results/{id}`
- Publish a result: `PATCH /api/results/{id}/publish`

## Tests

This project has **28 automated tests** (unit + integration).

- **How integration tests work**: the API is spun up in-memory using `Microsoft.AspNetCore.Mvc.Testing`, then tests call the real HTTP endpoints like a client would.
- **What’s covered**: authentication (`/api/auth/*`), samples (`/api/samples/*`), tests + results (`/api/tests/*`, `/api/results/*`), and global error handling.
- **Coverage**: code coverage is collected with Coverlet.

## Tech stack

| Area              | Technology                                                               |
| ----------------- | ------------------------------------------------------------------------ |
| Runtime           | .NET 8 (`net8.0`)                                                        |
| Web API           | ASP.NET Core                                                             |
| Auth              | JWT Bearer auth (`Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.11) |
| Database          | SQLite                                                                   |
| ORM / data access | Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite` 8.0.11)    |
| Validation        | FluentValidation (API: 11.3.1, Application: 12.1.1)                      |
| API docs          | Swagger / Swashbuckle 6.6.2                                              |
| Mapping           | Mapster 10.0.7                                                           |
| Password hashing  | BCrypt.Net-Next 4.2.0                                                    |
| Testing           | xUnit 2.5.3, FluentAssertions 8.2.0                                      |
| Test hosting      | `Microsoft.AspNetCore.Mvc.Testing` 8.0.11                                |
| Coverage          | `coverlet.collector` 6.0.0                                               |

## Project layout

| Path                        | What it is                      |
| --------------------------- | ------------------------------- |
| `LabLedger.sln`             | Main solution file              |
| `src/LabLedger.Api`         | The web API (the thing you run) |
| `src/LabLedger.Application` | Application logic               |
| `src/LabLedger.DataModel`   | Database model + migrations     |
| `tests/LabLedger.Tests`     | Automated tests                 |

## Quickstart

Run these commands from the repository root.

### Prerequisites

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
dotnet build
```

### Create/update the local database

Do this on first run, and again after schema changes:

```bash
dotnet ef database update --project src/LabLedger.DataModel --startup-project src/LabLedger.Api
```

### Run the API

```bash
dotnet run --project src/LabLedger.Api
```

### Visit the API docs (Swagger)

Once the API is running, open:

- `http://localhost:5143/swagger`

### Run the tests

Run everything:

```bash
dotnet test
```

Run a single test class or method (adjust the filter):

```bash
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

### Create a migration (when you change the data model)

```bash
dotnet ef migrations add <MigrationName> --project src/LabLedger.DataModel --startup-project src/LabLedger.Api
```
