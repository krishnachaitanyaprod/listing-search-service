using System.Text.Json;
using System.Text.Json.Serialization;
using ListingSearch.Core.Listings;
using Microsoft.Extensions.Logging;

namespace ListingSearch.Infrastructure.Listings;

// Reads listings from a file holding a JSON array. A bad record is logged with its id and reason, then skipped;
// a missing file or a file that isn't valid JSON fails the load with a clear message.
// filePath is already resolved: the Api resolves a relative Listings:FilePath against AppContext.BaseDirectory
// and uses an absolute one as it is.
public sealed class JsonFileListingSource(string filePath, ILogger<JsonFileListingSource> logger) : IListingSource
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public async Task<IReadOnlyList<Listing>> GetListingsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Listings file not found: {filePath}", filePath);

        using var document = await ParseAsync(cancellationToken);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException($"Listings file '{filePath}' must contain a JSON array of listings.");

        var listings = new List<Listing>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;

        foreach (var element in document.RootElement.EnumerateArray())
        {
            var (listing, reason) = Read(element);

            // The same source and id twice: keep the first. Cross-feed duplicates (A1/B7) have different keys and stay.
            if (listing is not null && !seenKeys.Add(listing.Key))
            {
                reason = $"duplicate key {listing.Key}; the first record with this key is kept";
                listing = null;
            }

            if (listing is not null)
                listings.Add(listing);
            else
                logger.LogWarning(
                    "Skipped listing record {Index} (id {Id}) in {FilePath}: {Reason}",
                    index, IdOf(element) ?? "unknown", filePath, reason);

            index++;
        }

        logger.LogInformation(
            "Loaded {Count} listings from {FilePath}, skipped {Skipped}", listings.Count, filePath, index - listings.Count);

        return listings;
    }

    private async Task<JsonDocument> ParseAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);

        try
        {
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Listings file '{filePath}' is not valid JSON: {ex.Message}", ex);
        }
    }

    private static (Listing? Listing, string? Reason) Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return (null, "record is not a JSON object");

        ListingRecord record;
        try
        {
            record = element.Deserialize<ListingRecord>(JsonOptions)!;
        }
        catch (JsonException ex)
        {
            // A value of the wrong type or format: text for a number, an impossible date, an unknown status.
            return (null, $"invalid value for '{FieldName(ex.Path)}'");
        }

        return record.ToListing();
    }

    // "$.listedDate" -> "listedDate"
    private static string FieldName(string? jsonPath) =>
        jsonPath is not null && jsonPath.StartsWith("$.", StringComparison.Ordinal) ? jsonPath[2..] : jsonPath ?? "unknown";

    private static string? IdOf(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, "id", StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.String)
                return property.Value.GetString();
        }

        return null;
    }
}
