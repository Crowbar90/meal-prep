namespace MealPrepPlanner.Dal.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers <see cref="MealPrepDbContext"/> with the DI container. The DbContext
/// uses the snake-case naming convention, the Npgsql provider, and the default
/// migrations history table.
///
/// This extension does NOT call <c>EnrichNpgsqlDbContext&lt;T&gt;()</c>; that adds
/// retries, OpenTelemetry instrumentation, and health checks and is a
/// service-host concern. The future <c>MealPrepPlanner.Api</c> project will call
/// it from its <c>Program.cs</c> after this registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    public const string DefaultConnectionName = "mealprep";

    public static IServiceCollection AddMealPrepDataLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName = DefaultConnectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException(
                $"Missing connection string '{connectionName}' in configuration. " +
                "Set it under ConnectionStrings:mealprep or pass --connection.");

        services.AddDbContext<MealPrepDbContext>(opt =>
            opt.UseNpgsql(connectionString, npg => npg.MigrationsHistoryTable("__ef_migrations_history"))
               .UseSnakeCaseNamingConvention());

        return services;
    }
}
