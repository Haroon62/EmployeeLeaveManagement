using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.Persistence;

public sealed class DatabaseInitialiser(
    ApplicationDbContext dbContext,
    DatabaseSeeder databaseSeeder)
{
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await databaseSeeder.SeedAsync(cancellationToken);
    }
}
