# Employee Leave Management

ASP.NET Core Web API assessment project for managing employees, departments,
leave types, leave balances, and leave requests.

This project provides authentication, role-based authorization, employee and
department administration, leave workflows, balance tracking, concurrency protection,
and reporting.

## Prerequisites

- .NET 8 SDK
- SQL Server Express or Developer Edition

## Build and test

```powershell
dotnet restore
dotnet tool restore
dotnet build --configuration Release
dotnet test --configuration Release
```

The suite currently contains 10 unit tests and 5 API contract/integration tests.

## Submission package

Create the required archive from the repository root:

```powershell
Compress-Archive -Path EmployeeLeaveManagement.sln,src,tests,Database,README.md,API_TESTING.md,.gitignore -DestinationPath Haroon_DotNet_Assessment.zip -Force
```

The archive should contain source and test projects only; build output folders are not required.

## Run the API

```powershell
dotnet run --project src/API
```

The health endpoint is available at `GET /health`. In Development, Swagger UI is
available at `/swagger`.

## Database setup

Both the application and EF Core migration commands use `DefaultConnection` from
`src/API/appsettings.json`. There is no fallback database connection. For secrets
outside this assessment, override the value with user secrets or an environment
variable rather than committing production credentials.

### SQL Server connection string

Set the connection string in `src/API/appsettings.json` to match your SQL Server
instance before running the migration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=;Database=EmployeeLeaveManagement;Trusted_Connection=True;TrustServerCertificate=True;User ID=;Password=;"
  }
}
```

Must Add Below Field in Connection String:

- Server/source: ``
- User ID: ``
- Password: ``

After saving the connection string, run:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Infrastructure --startup-project src/API
```

The same connection string is used by the API at runtime. Do not use this sample
password outside the local assessment database; store real credentials in user
secrets or environment variables.

Restore the repository-local EF tool and apply the migration:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Infrastructure --startup-project src/API
```

From Visual Studio Package Manager Console, select `EmployeeLeaveManagement.Infrastructure`
as the Default project, set `EmployeeLeaveManagement.API` as the Startup Project,
and run `Update-Database`.

To apply migrations and insert development seed data at startup:

```powershell
$env:Database__ApplyMigrationsOnStartup = "true"
dotnet run --project src/API
```

Leave `Database:ApplyMigrationsOnStartup` disabled in production and run migrations
as a controlled deployment step.

### Development seed accounts

| Role | Email | Password |
|---|---|---|
| Admin | `admin@leave.local` | `Admin123!` |
| Manager | `manager@leave.local` | `Manager123!` |
| Employee | `employee@leave.local` | `Employee123!` |

These credentials are for local assessment use only. Passwords are stored as
ASP.NET Core password hashes, never as plaintext.

The seed also creates the Engineering department, Annual Leave and Sick Leave
types, two employee profiles, and current-year balances.

## Authentication

Obtain an access token with `POST /api/auth/login`:

```json
{
  "email": "admin@leave.local",
  "password": "Admin123!"
}
```

Send the returned token in protected requests:

```text
Authorization: Bearer <access-token>
```

Swagger's **Authorize** button accepts the access token. Logout uses
`POST /api/auth/logout`; the token identifier is persisted in `RevokedTokens`, so
the same JWT is rejected immediately even if it has not expired.

Public registration is available at `POST /api/auth/register`. It always creates
an Employee account; callers cannot grant themselves Manager or Admin access.
Registration also creates the employee's current-year balances for every active
leave type.

## Administration endpoints

| Method | Route | Access |
|---|---|---|
| GET | `/api/employees` | Admin, Manager (manager is department-scoped) |
| GET | `/api/employees/{id}` | Admin, Manager (manager is department-scoped) |
| POST | `/api/employees` | Admin |
| PUT | `/api/employees/{id}` | Admin |
| DELETE | `/api/employees/{id}` | Admin; soft deactivation |
| GET | `/api/departments` | Admin, Manager |
| GET | `/api/departments/{id}` | Admin, Manager |
| POST | `/api/departments` | Admin |
| PUT | `/api/departments/{id}` | Admin |
| DELETE | `/api/departments/{id}` | Admin; soft deactivation |
| GET | `/api/leave-types` | Authenticated users |
| GET | `/api/leave-types/{id}` | Authenticated users |
| POST | `/api/leave-types` | Admin |
| PUT | `/api/leave-types/{id}` | Admin |
| DELETE | `/api/leave-types/{id}` | Admin; soft deactivation |

Admin-created employees are limited to Employee and Manager roles. A selected
manager must be active and belong to the same department. Historical records are
preserved through soft deactivation rather than physical deletion.

## Solution structure

- `src/Domain`: Domain entities and rules.
- `src/Application`: Use cases, DTOs, and abstractions.
- `src/Infrastructure`: Persistence and external services.
- `src/API`: HTTP API and composition root.
- `tests/UnitTests`: Isolated business-rule tests.
- `tests/IntegrationTests`: End-to-end API tests.
- `Database`: Database-related scripts or supporting files.

## Leave request workflow

Employees submit pending requests. Managers can approve or reject requests from
their department. Approval deducts working days from the employee balance; rejected
and cancelled requests do not affect the balance. Duplicate submissions are prevented
with the `Idempotency-Key` header, and concurrent approvals are protected by a
serializable transaction and conditional balance update.

See `API_TESTING.md` for request examples and expected responses.
