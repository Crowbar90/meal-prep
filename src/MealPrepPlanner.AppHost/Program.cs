// Aspire AppHost — local-development orchestrator.
//
// Today this brings up a Postgres 18 container alongside an empty service
// graph. The Domain project sits on the solution graph via the .slnx file,
// not via this AppHost. The future MealPrepPlanner.Api project will consume
// the database and be wired in here with builder.AddProject<...>().WithReference(db).
//
// Run locally:
//   dotnet run --project src/MealPrepPlanner.AppHost
//
// The Aspire dashboard opens automatically; the URL is printed to stdout.
// The Postgres 18 container also exposes its own connection details on the
// dashboard's "postgres" resource page.

var builder = DistributedApplication.CreateBuilder(args);

// Pin to PostgreSQL 18 — the latest stable major. Aspire 13.4.6's bundled
// image tag tracks whatever Aspire ships (currently 17); the explicit pin
// upgrades us to 18 per ADR 002 and matches the schema described in
// docs/architecture/data-model.md.
var postgres = builder.AddPostgres("postgres")
    .WithImageTag("18");

var mealprepDb = postgres.AddDatabase("mealprep");

// Future API service (commented today; flips on once MealPrepPlanner.Api lands):
//   var api = builder.AddProject<Projects.MealPrepPlanner_Api>("api")
//       .WithReference(mealprepDb);

builder.Build().Run();
