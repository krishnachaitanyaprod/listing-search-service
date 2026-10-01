using ListingSearch.Api;
using ListingSearch.Core;
using ListingSearch.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddListingSearchCore()
    .AddListingSearchInfrastructure(builder.Configuration)
    .AddListingSearchApi(builder.Configuration);

var app = builder.Build();

// Any unexpected error becomes a 500 ProblemDetails with a traceId and no exception details.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapControllers();

app.Run();
