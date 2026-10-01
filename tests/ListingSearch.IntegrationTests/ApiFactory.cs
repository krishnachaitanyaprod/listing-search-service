using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace ListingSearch.IntegrationTests;

// The real app over data/sample_listings.json, with "today" pinned to 2026-09-30 (UTC).
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private static readonly DateTimeOffset Today = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(new FakeTimeProvider(Today)));
}
