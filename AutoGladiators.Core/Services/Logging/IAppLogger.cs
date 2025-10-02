using System.Collections.Generic;
namespace AutoGladiators.Core.Services.Logging;

public interface IAppLogger
{
    void Log(LogLevel level, string message, Exception? ex = null, string? correlationId = null, IReadOnlyDictionary<string, object?>? props = null);
    void Trace(string m, IReadOnlyDictionary<string, object?>? p = null);
    void Debug(string m, IReadOnlyDictionary<string, object?>? p = null);
    void Info(string m, IReadOnlyDictionary<string, object?>? p = null);
    void Warn(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null);
    void Error(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null);
    void Critical(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null);
    
    // Enhanced structured logging methods
    void LogPerformance(string operationName, long durationMs, IReadOnlyDictionary<string, object?>? context = null);
    void LogBattleEvent(string eventType, string playerId, string enemyId, IReadOnlyDictionary<string, object?>? battleData = null);
    void LogUserAction(string action, string userId, IReadOnlyDictionary<string, object?>? context = null);
    void LogGameState(string stateName, string previousState, IReadOnlyDictionary<string, object?>? stateData = null);
    void LogError(string component, string errorCode, Exception? ex = null, IReadOnlyDictionary<string, object?>? context = null);
}
