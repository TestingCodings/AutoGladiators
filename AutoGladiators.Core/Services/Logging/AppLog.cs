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
          {
              var msLevel = (Microsoft.Extensions.Logging.LogLevel)level;
              using var scope = CreateScope(correlationId, props);
              _logger.Log(msLevel, new EventId(), message, ex, (s, e) => s);
          }

          public void Trace(string m, IReadOnlyDictionary<string, object?>? p = null) 
          {
              using var scope = CreateScope(null, p);
              _logger.LogTrace(m);
          }

          public void Debug(string m, IReadOnlyDictionary<string, object?>? p = null) 
          {
              using var scope = CreateScope(null, p);
              _logger.LogDebug(m);
          }

          public void Info(string m, IReadOnlyDictionary<string, object?>? p = null) 
          {
              using var scope = CreateScope(null, p);
              _logger.LogInformation(m);
          }

          public void Warn(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) 
          {
              using var scope = CreateScope(null, p);
              _logger.LogWarning(ex, m);
          }

          public void Error(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) 
          {
              using var scope = CreateScope(null, p);
              _logger.LogError(ex, m);
          }

          public void Critical(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) 
          {
              using var scope = CreateScope(null, p);
              _logger.LogCritical(ex, m);
          }

          public void LogPerformance(string operationName, long durationMs, IReadOnlyDictionary<string, object?>? context = null)
          {
              var props = new Dictionary<string, object?>
              {
                  ["OperationName"] = operationName,
                  ["DurationMs"] = durationMs,
                  ["EventType"] = "Performance"
              };
              if (context != null)
              {
                  foreach (var kvp in context)
                      props[kvp.Key] = kvp.Value;
              }
              
              using var scope = CreateScope(null, props);
              _logger.LogInformation("Performance: {OperationName} completed in {DurationMs}ms", operationName, durationMs);
          }

          public void LogBattleEvent(string eventType, string playerId, string enemyId, IReadOnlyDictionary<string, object?>? battleData = null)
          {
              var props = new Dictionary<string, object?>
              {
                  ["EventType"] = "Battle",
                  ["BattleEventType"] = eventType,
                  ["PlayerId"] = playerId,
                  ["EnemyId"] = enemyId,
                  ["Timestamp"] = DateTime.UtcNow
              };
              if (battleData != null)
              {
                  foreach (var kvp in battleData)
                      props[kvp.Key] = kvp.Value;
              }
              
              using var scope = CreateScope(null, props);
              _logger.LogInformation("Battle Event: {EventType} - Player: {PlayerId} vs Enemy: {EnemyId}", eventType, playerId, enemyId);
          }

          public void LogUserAction(string action, string userId, IReadOnlyDictionary<string, object?>? context = null)
          {
              var props = new Dictionary<string, object?>
              {
                  ["EventType"] = "UserAction",
                  ["Action"] = action,
                  ["UserId"] = userId,
                  ["Timestamp"] = DateTime.UtcNow
              };
              if (context != null)
              {
                  foreach (var kvp in context)
                      props[kvp.Key] = kvp.Value;
              }
              
              using var scope = CreateScope(null, props);
              _logger.LogInformation("User Action: {Action} by {UserId}", action, userId);
          }

          public void LogGameState(string stateName, string previousState, IReadOnlyDictionary<string, object?>? stateData = null)
          {
              var props = new Dictionary<string, object?>
              {
                  ["EventType"] = "StateTransition",
                  ["CurrentState"] = stateName,
                  ["PreviousState"] = previousState,
                  ["Timestamp"] = DateTime.UtcNow
              };
              if (stateData != null)
              {
                  foreach (var kvp in stateData)
                      props[kvp.Key] = kvp.Value;
              }
              
              using var scope = CreateScope(null, props);
              _logger.LogInformation("State Transition: {PreviousState} → {CurrentState}", previousState, stateName);
          }

          public void LogError(string component, string errorCode, Exception? ex = null, IReadOnlyDictionary<string, object?>? context = null)
          {
              var props = new Dictionary<string, object?>
              {
                  ["EventType"] = "Error",
                  ["Component"] = component,
                  ["ErrorCode"] = errorCode,
                  ["Timestamp"] = DateTime.UtcNow
              };
              if (context != null)
              {
                  foreach (var kvp in context)
                      props[kvp.Key] = kvp.Value;
              }
              
              using var scope = CreateScope(null, props);
              _logger.LogError(ex, "Component Error: {Component} - {ErrorCode}", component, errorCode);
          }

          private IDisposable? CreateScope(string? correlationId, IReadOnlyDictionary<string, object?>? props)
          {
              var scopeDict = new Dictionary<string, object?>();
              
              if (!string.IsNullOrEmpty(correlationId))
                  scopeDict["CorrelationId"] = correlationId;
              
              if (props != null)
              {
                  foreach (var kvp in props)
                      scopeDict[kvp.Key] = kvp.Value;
              }
              
              return scopeDict.Count > 0 ? _logger.BeginScope(scopeDict) : null;
          }
    }

    }
}
