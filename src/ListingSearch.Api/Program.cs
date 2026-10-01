using ListingSearch.Api;
using ListingSearch.Api.Health;
using ListingSearch.Core;
using ListingSearch.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddListingSearchCore()
    .AddListingSearchInfrastructure(builder.Configuration)
    .AddListingSearchApi(builder.Configuration);

var app = builder.Build();

// Any unexpected error becomes a 500 ProblemDetails with a traceId and no exception details.
app.UseExceptionHandler();

// Live: the process is up (runs no checks). Ready: the listings are loaded and searches can be served.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(HealthTags.Ready)
});

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();

app.Run();
