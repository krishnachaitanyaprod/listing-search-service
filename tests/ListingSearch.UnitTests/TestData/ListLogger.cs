using Microsoft.Extensions.Logging;

namespace ListingSearch.UnitTests.TestData;

// Keeps every log entry so tests can check what was logged.
internal sealed class ListLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IEnumerable<string> Warnings =>
        Entries.Where(e => e.Level == LogLevel.Warning).Select(e => e.Message);

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entries.Add((logLevel, formatter(state, exception)));

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
