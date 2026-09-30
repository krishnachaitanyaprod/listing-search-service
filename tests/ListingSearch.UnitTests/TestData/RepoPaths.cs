namespace ListingSearch.UnitTests.TestData;

internal static class RepoPaths
{
    // The real sample file, found by walking up from the test output folder to the repo root.
    public static string SampleListingsFile { get; } = Path.Combine(FindRepoRoot(), "data", "sample_listings.json");

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ListingSearch.slnx")))
                return dir.FullName;
        }

        throw new InvalidOperationException("Couldn't find the repo root (ListingSearch.slnx) above " + AppContext.BaseDirectory);
    }
}
