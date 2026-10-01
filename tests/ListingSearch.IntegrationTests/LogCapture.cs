using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ListingSearch.IntegrationTests;

public sealed record LogEntry(
    string Category, LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Properties);

// Keeps every log entry with its structured properties, so tests can check what the app logged.
public sealed class LogCapture : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public IReadOnlyList<LogEntry> For(string category) => _entries.Where(e => e.Category == category).ToList();

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _entries);

    public void Dispose()
    {
    }

    private sealed class Logger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var properties = state as IEnumerable<KeyValuePair<string, object?>> ?? [];
            entries.Enqueue(new LogEntry(
                category, logLevel, formatter(state, exception), properties.ToDictionary(p => p.Key, p => p.Value)));
        }
    }
}
