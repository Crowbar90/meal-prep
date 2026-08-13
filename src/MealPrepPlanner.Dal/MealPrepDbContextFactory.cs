namespace MealPrepPlanner.Dal;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Design-time factory used by <c>dotnet ef migrations ...</c> when no startup
/// project references this assembly. Reads the connection string from
/// <c>ConnectionStrings:MealPrep</c> in the current working directory's
/// <c>appsettings.json</c> (if present) and falls back to a localhost default.
/// </summary>
public sealed class MealPrepDbContextFactory : IDesignTimeDbContextFactory<MealPrepDbContext>
{
    public MealPrepDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("MealPrep")
            ?? "Host=localhost;Database=mealprep;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<MealPrepDbContext>()
            .UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new MealPrepDbContext(options);
    }
}
