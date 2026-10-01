using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace ListingSearch.IntegrationTests;

public class GeneratedListingsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task ByDefault_OnlyTheFileIsSearched()
    {
        Assert.Equal(12, await TotalCountAsync(factory));
    }

    [Fact]
    public async Task GeneratedListings_AreSearchedAlongsideTheFile()
    {
        using var app = factory.WithWebHostBuilder(builder => builder.UseSetting("GeneratedListings:Count", "50"));

        Assert.Equal(62, await TotalCountAsync(app));
    }

    private static async Task<int> TotalCountAsync(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> app)
    {
        var response = await app.CreateClient().GetAsync("/api/v1/listings/search?pageSize=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("totalCount").GetInt32();
    }
}
