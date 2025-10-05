using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Core.Services.Analytics
{
    public interface IPerformanceMonitor
    {
        IDisposable StartOperation(string operationName, IDictionary<string, object?>? context = null);
        void RecordMetric(string metricName, double value, IDictionary<string, object?>? tags = null);
        void RecordCounter(string counterName, int increment = 1, IDictionary<string, object?>? tags = null);
    }

    public class PerformanceMonitor : IPerformanceMonitor
    {
        private static readonly IAppLogger _logger = AppLog.For<PerformanceMonitor>();
        
        public IDisposable StartOperation(string operationName, IDictionary<string, object?>? context = null)
        {
            return new OperationTimer(operationName, context);
        }

        public void RecordMetric(string metricName, double value, IDictionary<string, object?>? tags = null)
        {
            var logContext = new Dictionary<string, object?>
            {
                ["MetricName"] = metricName,
                ["MetricValue"] = value,
                ["MetricType"] = "Gauge"
            };
            
            if (tags != null)
            {
                foreach (var tag in tags)
                    logContext[$"Tag_{tag.Key}"] = tag.Value;
            }
            
            _logger.Info($"Metric: {metricName} = {value}", logContext);
        }

        public void RecordCounter(string counterName, int increment = 1, IDictionary<string, object?>? tags = null)
        {
            var logContext = new Dictionary<string, object?>
            {
                ["CounterName"] = counterName,
                ["CounterIncrement"] = increment,
                ["MetricType"] = "Counter"
            };
            
            if (tags != null)
            {
                foreach (var tag in tags)
                    logContext[$"Tag_{tag.Key}"] = tag.Value;
            }
            
            _logger.Info($"Counter: {counterName} += {increment}", logContext);
        }

        private class OperationTimer : IDisposable
        {
            private readonly string _operationName;
            private readonly IDictionary<string, object?>? _context;
            private readonly Stopwatch _stopwatch;
            private bool _disposed = false;

            public OperationTimer(string operationName, IDictionary<string, object?>? context)
            {
                _operationName = operationName;
                _context = context;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                                        var readOnlyContext = _context as IReadOnlyDictionary<string, object?> ?? new Dictionary<string, object?>(_context ?? new Dictionary<string, object?>());
                    _logger.LogPerformance(_operationName, _stopwatch.ElapsedMilliseconds, readOnlyContext);
                    _disposed = true;
                }
            }
        }
    }
}