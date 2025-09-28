// Services/Logging/AppLog.cs
using System;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services.Logging
{

    public static class AppLog
    {
        private static ILoggerFactory _factory = LoggerFactory.Create(_ => { });

        public static void Initialize(ILoggerFactory factory) => _factory = factory;

        public static IAppLogger For<T>() =>
            new AppLogger(_factory.CreateLogger(typeof(T).FullName ?? typeof(T).Name));

        public static IAppLogger For(string name) =>
            new AppLogger(_factory.CreateLogger(string.IsNullOrWhiteSpace(name) ? "App" : name));

        public static IAppLogger For(Type type) =>
            new AppLogger(_factory.CreateLogger(type?.FullName ?? type?.Name ?? "App"));

    private sealed class AppLogger : IAppLogger
    {
         private readonly ILogger _logger;
         public AppLogger(ILogger logger) => _logger = logger;

          public void Log(LogLevel level, string message, Exception? ex = null, string? correlationId = null, IReadOnlyDictionary<string, object?>? props = null)
            => _logger.Log((Microsoft.Extensions.Logging.LogLevel)level, new EventId(), message, ex, (s, e) => s);
                public void Trace(string m, IReadOnlyDictionary<string, object?>? p = null) => _logger.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new EventId(), m, null, (s, e) => s);
                public void Debug(string m, IReadOnlyDictionary<string, object?>? p = null) => _logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, new EventId(), m, null, (s, e) => s);
                public void Info(string m, IReadOnlyDictionary<string, object?>? p = null) => _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, new EventId(), m, null, (s, e) => s);
                public void Warn(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) => _logger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, new EventId(), m, ex, (s, e) => s);
                public void Error(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) => _logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, new EventId(), m, ex, (s, e) => s);
                public void Critical(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) => _logger.Log(Microsoft.Extensions.Logging.LogLevel.Critical, new EventId(), m, ex, (s, e) => s);
            }

    }
}
