using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MealPrepPlanner.ServiceDefaults;

/// <summary>
/// Shared Aspire service defaults (telemetry, health checks, service discovery,
/// standard middleware). Wired into every backend service via
/// <see cref="AddServiceDefaults"/> and <see cref="MapDefaultEndpoints"/>.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers the cross-cutting defaults: OpenTelemetry, default health checks,
    /// service discovery, and standard HTTP resilience.
    /// </summary>
    public static void AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default.
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default.
            http.AddServiceDiscovery();
        });
    }

    /// <summary>
    /// Configures OpenTelemetry for logs, metrics, and traces. The Aspire
    /// dashboard OTLP endpoint is wired automatically when running under the
    /// AppHost.
    /// </summary>
    public static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });
    }

    /// <summary>
    /// Adds a baseline <c>live</c> health check. Services should add their own
    /// dependency-specific checks on top.
    /// </summary>
    public static void AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }

    /// <summary>
    /// Maps <c>/health</c> (all checks) and <c>/alive</c> (only <c>live</c> tag).
    /// <c>/alive</c> returns 503 if the process is not ready, which Aspire uses
    /// to gate resource startup.
    /// </summary>
    public static void MapDefaultEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Health checks pass through when no checks are registered. Live is
        // gated by a single "self" check registered in AddDefaultHealthChecks.
        app.MapHealthChecks("/health");

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
        });
    }
}
