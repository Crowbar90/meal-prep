// Aspire AppHost — local-development orchestrator.
//
// Today this builds and starts an empty orchestration graph (no resources
// registered). The Domain project sits on the solution graph via the .slnx
// file, not via this AppHost. Once a runnable service project lands
// (Api, Infrastructure), add it as a <ProjectReference> here and the
// Aspire SDK generates a strongly-typed Projects.<Name> reference.
//
// Run locally:
//   dotnet run --project src/MealPrepPlanner.AppHost
//
// The Aspire dashboard opens automatically; the URL is printed to stdout.

var builder = DistributedApplication.CreateBuilder(args);

builder.Build().Run();
