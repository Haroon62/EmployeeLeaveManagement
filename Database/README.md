# Database

EF Core migrations live in `src/Infrastructure/Persistence/Migrations`.

`InitialCreate.sql` is an idempotent deployment script generated from the tracked
EF Core migrations. It can safely be run against a database that may already have
some or all tracked migrations applied.

Generate a refreshed idempotent script with:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations script --idempotent `
  --project src/Infrastructure `
  --startup-project src/API `
  --output Database/InitialCreate.sql
```

The schema enforces unique users, employee numbers, department and leave-type
names, one annual balance per employee/leave type/year, nonnegative consistent
balances, valid leave date ranges, mandatory rejection reasons, and unique
idempotency keys per user. SQL Server `rowversion` columns protect leave balances
and leave requests from conflicting writes.
