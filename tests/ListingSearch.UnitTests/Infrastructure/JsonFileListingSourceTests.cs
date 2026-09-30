using ListingSearch.Core.Listings;
using ListingSearch.Infrastructure.Listings;
using ListingSearch.UnitTests.TestData;

namespace ListingSearch.UnitTests.Infrastructure;

public class JsonFileListingSourceTests
{
    private readonly ListLogger<JsonFileListingSource> _logger = new();

    [Fact]
    public async Task SampleFile_LoadsAllTwelveListings_WithoutWarnings()
    {
        var listings = await Load(RepoPaths.SampleListingsFile);

        Assert.Equal(12, listings.Count);
        Assert.Empty(_logger.Warnings);
    }

    [Fact]
    public async Task SampleFile_LoadsTheRightValues()
    {
        IReadOnlyDictionary<string, Listing> listings = (await Load(RepoPaths.SampleListingsFile)).ToDictionary(l => l.Key);

        var expectedA1 = new Listing
        {
            Id = "A1",
            Source = "MLS_A",
            Address = "123 Main St, Apt 4B",
            City = "Springfield",
            State = "VA",
            Zip = "22150",
            Price = 450000m,
            Bedrooms = 2,
            Bathrooms = 1.5,
            Sqft = 980,
            Latitude = 38.7893,
            Longitude = -77.1873,
            ListedDate = new DateOnly(2026, 8, 29),
            Status = ListingStatus.Active,
            Description = "Bright top-floor condo near shops and transit. Pet friendly."
        };
        Assert.Equal(expectedA1, Assert.Contains("MLS_A:A1", listings));
        Assert.Equal(ListingStatus.Pending, Assert.Contains("MLS_A:A7", listings).Status);
        Assert.Equal("22151", Assert.Contains("MLS_B:B8", listings).Zip);
    }

    [Fact]
    public async Task PropertyNamesAndStatus_AreCaseInsensitive()
    {
        using var file = new TempFile("""
            [{ "ID": "C1", "Source": "MLS_A", "ADDRESS": "1 Test St", "City": "Springfield", "state": "VA",
               "Zip": "22150", "PRICE": 450000, "Bedrooms": 3, "Bathrooms": 2.0, "Sqft": 1500,
               "Latitude": 38.8, "Longitude": -77.2, "ListedDate": "2026-09-01", "STATUS": "SOLD",
               "Description": "Test listing." }]
            """);

        var listing = Assert.Single(await Load(file.Path));

        Assert.Equal("MLS_A:C1", listing.Key);
        Assert.Equal(ListingStatus.Sold, listing.Status);
    }

    [Fact]
    public async Task OneBadRecord_IsSkipped_AndTheRestLoad()
    {
        using var file = new TempFile(FileOf(Record("G1"), Record("B1", "price", "0"), Record("G2")));

        var listings = await Load(file.Path);

        Assert.Equal(new[] { "MLS_A:G1", "MLS_A:G2" }, listings.Select(l => l.Key));
        var warning = Assert.Single(_logger.Warnings);
        Assert.Contains("B1", warning);
        Assert.Contains("price must be greater than 0", warning);
    }

    [Theory]
    [InlineData("city", null, "missing city")]
    [InlineData("price", "0", "price must be greater than 0")]
    [InlineData("price", "-1", "price must be greater than 0")]
    [InlineData("price", "\"abc\"", "invalid value for 'price'")]
    [InlineData("bedrooms", "-1", "bedrooms must be 0 or more")]
    [InlineData("listedDate", "\"2026-02-30\"", "invalid value for 'listedDate'")]
    [InlineData("status", "\"archived\"", "invalid value for 'status'")]
    [InlineData("status", "3", "invalid value for 'status'")]
    [InlineData("status", "\"Pending, Sold\"", "unknown status")]
    public async Task EachKindOfBadRecord_IsSkipped_WithItsIdAndReasonLogged(string field, string? rawValue, string reason)
    {
        using var file = new TempFile(FileOf(Record("G1"), Record("B1", field, rawValue)));

        var listing = Assert.Single(await Load(file.Path));

        Assert.Equal("MLS_A:G1", listing.Key);
        var warning = Assert.Single(_logger.Warnings);
        Assert.Contains("B1", warning);
        Assert.Contains(reason, warning);
    }

    [Fact]
    public async Task RepeatedSourceAndId_KeepsTheFirst_AndSkipsTheRepeatWithAWarning()
    {
        using var file = new TempFile(FileOf(Record("D1"), Record("D1", "price", "999999"), Record("G1")));

        var listings = await Load(file.Path);

        Assert.Equal(new[] { "MLS_A:D1", "MLS_A:G1" }, listings.Select(l => l.Key));
        Assert.Equal(450000m, listings[0].Price);
        var warning = Assert.Single(_logger.Warnings);
        Assert.Contains("duplicate key MLS_A:D1", warning);
    }

    [Fact]
    public async Task SameIdFromDifferentSources_AreDifferentKeys_AndBothLoad()
    {
        using var file = new TempFile(FileOf(Record("D1"), Record("D1", "source", "\"MLS_B\"")));

        var listings = await Load(file.Path);

        Assert.Equal(new[] { "MLS_A:D1", "MLS_B:D1" }, listings.Select(l => l.Key));
        Assert.Empty(_logger.Warnings);
    }

    [Fact]
    public async Task MissingFile_FailsWithAClearMessage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => Load(path));

        Assert.Contains("Listings file not found", ex.Message);
        Assert.Contains(path, ex.Message);
    }

    [Fact]
    public async Task InvalidJson_FailsWithAClearMessage()
    {
        using var file = new TempFile("""[{ "id": "A1", """);

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() => Load(file.Path));

        Assert.Contains("is not valid JSON", ex.Message);
        Assert.Contains(file.Path, ex.Message);
    }

    [Fact]
    public async Task JsonThatIsNotAnArray_FailsWithAClearMessage()
    {
        using var file = new TempFile(Record("A1"));

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() => Load(file.Path));

        Assert.Contains("must contain a JSON array", ex.Message);
    }

    private Task<IReadOnlyList<Listing>> Load(string path) =>
        new JsonFileListingSource(path, _logger).GetListingsAsync();

    private static string FileOf(params string[] records) => "[" + string.Join(",\n", records) + "]";

    // A valid listing record as JSON. Pass a field and raw JSON value to change it, or a null value to remove it.
    private static string Record(string id, string? field = null, string? rawValue = null)
    {
        var fields = new Dictionary<string, string>
        {
            ["id"] = $"\"{id}\"",
            ["source"] = "\"MLS_A\"",
            ["address"] = "\"1 Test St\"",
            ["city"] = "\"Springfield\"",
            ["state"] = "\"VA\"",
            ["zip"] = "\"22150\"",
            ["price"] = "450000",
            ["bedrooms"] = "3",
            ["bathrooms"] = "2.0",
            ["sqft"] = "1500",
            ["latitude"] = "38.8",
            ["longitude"] = "-77.2",
            ["listedDate"] = "\"2026-09-01\"",
            ["status"] = "\"active\"",
            ["description"] = "\"Test listing.\""
        };

        if (field is not null)
        {
            if (rawValue is null)
                fields.Remove(field);
            else
                fields[field] = rawValue;
        }

        return "{" + string.Join(", ", fields.Select(f => $"\"{f.Key}\": {f.Value}")) + "}";
    }
}
