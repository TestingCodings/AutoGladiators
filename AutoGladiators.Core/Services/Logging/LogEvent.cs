using System;
using System.Collections.Generic;
namespace AutoGladiators.Core.Services.Logging;
public sealed record LogEvent(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    Exception? Exception = null,
    string? CorrelationId = null,
    IReadOnlyDictionary<string, object?>? Props = null
);
