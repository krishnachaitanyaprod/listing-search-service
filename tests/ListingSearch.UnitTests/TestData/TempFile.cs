namespace ListingSearch.UnitTests.TestData;

// A file in the temp folder that's deleted when the test disposes it.
internal sealed class TempFile : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"listings-{Guid.NewGuid():N}.json");

    public TempFile(string content) => File.WriteAllText(Path, content);

    public void Dispose() => File.Delete(Path);
}
