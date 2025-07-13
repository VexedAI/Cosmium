using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Cosmium.Engine.Infrastructure.Configuration;

namespace Cosmium.Engine.Infrastructure.Logging;

/// <summary>
/// Performance metrics logging system for the Cosmium Engine.
/// Tracks system resources, execution times, and computational efficiency.
/// </summary>
public class PerformanceLogger
{
    private static PerformanceLogger? _instance;
    private static readonly object _lock = new();
    private readonly ConcurrentQueue<PerformanceMetric> _metricsQueue = new();
    private readonly Timer _metricsTimer;
    // CPU counter disabled for compatibility
    private readonly Process _currentProcess;
    private readonly ConcurrentDictionary<string, OperationTracker> _activeOperations = new();
    private readonly List<IPerformanceTarget> _targets = new();

    /// <summary>
    /// Gets the singleton instance of the performance logger.
    /// </summary>
    public static PerformanceLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new PerformanceLogger();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Whether performance monitoring is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Collection interval for system metrics in milliseconds.
    /// </summary>
    public int MetricsIntervalMs { get; set; } = 1000;

    private PerformanceLogger()
    {
        var config = EngineConfiguration.Instance.Performance;
        IsEnabled = config.EnableMonitoring;
        MetricsIntervalMs = config.MetricsIntervalMs;

        _currentProcess = Process.GetCurrentProcess();

        // CPU counter disabled for compatibility

        // Add default targets
        _targets.Add(new InMemoryPerformanceTarget());

        // Start metrics collection timer
        _metricsTimer = new Timer(CollectSystemMetrics, null, 
            TimeSpan.FromMilliseconds(MetricsIntervalMs), 
            TimeSpan.FromMilliseconds(MetricsIntervalMs));
    }

    /// <summary>
    /// Starts tracking a named operation.
    /// </summary>
    /// <param name="operationName">Name of the operation to track.</param>
    /// <returns>A disposable tracker that will automatically log completion metrics.</returns>
    public IDisposable BeginOperation(string operationName)
    {
        var operationId = Guid.NewGuid().ToString();
        var tracker = new OperationTracker(this, operationId, operationName);
        _activeOperations.TryAdd(operationId, tracker);
        
        LogMetric(new PerformanceMetric
        {
            Type = MetricType.OperationStart,
            Name = operationName,
            Value = 0,
            Timestamp = DateTime.UtcNow,
            Properties = new Dictionary<string, object> { ["OperationId"] = operationId }
        });

        return tracker;
    }

    /// <summary>
    /// Logs a custom performance metric.
    /// </summary>
    public void LogMetric(string name, double value, MetricType type = MetricType.Custom, Dictionary<string, object>? properties = null)
    {
        LogMetric(new PerformanceMetric
        {
            Type = type,
            Name = name,
            Value = value,
            Timestamp = DateTime.UtcNow,
            Properties = properties ?? new Dictionary<string, object>()
        });
    }

    /// <summary>
    /// Logs memory usage metrics.
    /// </summary>
    public void LogMemoryUsage(string context = "General")
    {
        var workingSet = _currentProcess.WorkingSet64;
        var privateMemory = _currentProcess.PrivateMemorySize64;
        var gcMemory = GC.GetTotalMemory(false);
        
        LogMetric("WorkingSet", workingSet / 1024.0 / 1024.0, MetricType.Memory, 
            new Dictionary<string, object> { ["Context"] = context, ["Unit"] = "MB" });
        
        LogMetric("PrivateMemory", privateMemory / 1024.0 / 1024.0, MetricType.Memory, 
            new Dictionary<string, object> { ["Context"] = context, ["Unit"] = "MB" });
        
        LogMetric("GCMemory", gcMemory / 1024.0 / 1024.0, MetricType.Memory, 
            new Dictionary<string, object> { ["Context"] = context, ["Unit"] = "MB" });
    }

    /// <summary>
    /// Logs quantum operation specific metrics.
    /// </summary>
    public void LogQuantumMetrics(string operation, int hilbertSpaceDimension, double executionTimeMs, 
        double? fidelity = null, double? entanglement = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["HilbertSpaceDimension"] = hilbertSpaceDimension,
            ["ExecutionTimeMs"] = executionTimeMs
        };

        if (fidelity.HasValue)
            properties["Fidelity"] = fidelity.Value;
        
        if (entanglement.HasValue)
            properties["Entanglement"] = entanglement.Value;

        LogMetric($"Quantum.{operation}", executionTimeMs, MetricType.QuantumOperation, properties);
    }

    /// <summary>
    /// Logs matrix operation performance metrics.
    /// </summary>
    public void LogMatrixOperation(string operation, int rows, int columns, double executionTimeMs)
    {
        var properties = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["Rows"] = rows,
            ["Columns"] = columns,
            ["Size"] = rows * columns,
            ["ExecutionTimeMs"] = executionTimeMs
        };

        LogMetric($"Matrix.{operation}", executionTimeMs, MetricType.MatrixOperation, properties);
    }

    /// <summary>
    /// Gets performance statistics for a specific metric type.
    /// </summary>
    public PerformanceStatistics GetStatistics(MetricType type, TimeSpan? timeWindow = null)
    {
        var target = _targets.OfType<InMemoryPerformanceTarget>().FirstOrDefault();
        if (target == null)
            return new PerformanceStatistics();

        return target.GetStatistics(type, timeWindow);
    }

    /// <summary>
    /// Gets all recent metrics within the specified time window.
    /// </summary>
    public IEnumerable<PerformanceMetric> GetRecentMetrics(TimeSpan timeWindow)
    {
        var target = _targets.OfType<InMemoryPerformanceTarget>().FirstOrDefault();
        return target?.GetRecentMetrics(timeWindow) ?? Enumerable.Empty<PerformanceMetric>();
    }

    /// <summary>
    /// Adds a custom performance target.
    /// </summary>
    public void AddTarget(IPerformanceTarget target)
    {
        _targets.Add(target);
    }

    /// <summary>
    /// Exports performance data to JSON format.
    /// </summary>
    public string ExportToJson(TimeSpan? timeWindow = null)
    {
        var metrics = GetRecentMetrics(timeWindow ?? TimeSpan.FromHours(1));
        return JsonSerializer.Serialize(metrics, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    internal void EndOperation(string operationId, string operationName, TimeSpan duration, Dictionary<string, object>? additionalProperties = null)
    {
        _activeOperations.TryRemove(operationId, out _);
        
        var properties = new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["DurationMs"] = duration.TotalMilliseconds
        };

        if (additionalProperties != null)
        {
            foreach (var prop in additionalProperties)
            {
                properties[prop.Key] = prop.Value;
            }
        }

        LogMetric(new PerformanceMetric
        {
            Type = MetricType.OperationEnd,
            Name = operationName,
            Value = duration.TotalMilliseconds,
            Timestamp = DateTime.UtcNow,
            Properties = properties
        });
    }

    private void LogMetric(PerformanceMetric metric)
    {
        if (!IsEnabled)
            return;

        _metricsQueue.Enqueue(metric);

        // Process metrics in background
        Task.Run(() =>
        {
            while (_metricsQueue.TryDequeue(out var queuedMetric))
            {
                foreach (var target in _targets)
                {
                    try
                    {
                        target.WriteMetric(queuedMetric);
                    }
                    catch (Exception ex)
                    {
                        // Log target failed - use simulation logger if available
                        Console.WriteLine($"Performance target failed: {ex.Message}");
                    }
                }
            }
        });
    }

    private void CollectSystemMetrics(object? state)
    {
        if (!IsEnabled)
            return;

        try
        {
            var config = EngineConfiguration.Instance.Performance;
            
            if (config.CollectMemoryStats)
            {
                LogMemoryUsage("System");
            }

            if (config.CollectCpuStats)
            {
                LogCpuUsage();
            }

            LogActiveOperationsCount();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"System metrics collection failed: {ex.Message}");
        }
    }

    private void LogCpuUsage()
    {
        try
        {
            // Alternative CPU measurement for all platforms
            double cpuUsage = _currentProcess.TotalProcessorTime.TotalMilliseconds;

            LogMetric("CPU.Usage", cpuUsage, MetricType.System, 
                new Dictionary<string, object> { ["Unit"] = "Milliseconds" });
        }
        catch
        {
            // CPU measurement failed - skip this cycle
        }
    }

    private void LogActiveOperationsCount()
    {
        LogMetric("Operations.Active", _activeOperations.Count, MetricType.System);
    }

    /// <summary>
    /// Disposes the performance logger and flushes remaining metrics.
    /// </summary>
    public void Dispose()
    {
        _metricsTimer?.Dispose();
        // _cpuCounter is disabled for compatibility
        
        foreach (var target in _targets)
        {
            target.Dispose();
        }
    }
}

/// <summary>
/// Represents a performance metric with contextual information.
/// </summary>
public class PerformanceMetric
{
    public MetricType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Types of performance metrics.
/// </summary>
public enum MetricType
{
    Custom,
    System,
    Memory,
    OperationStart,
    OperationEnd,
    QuantumOperation,
    MatrixOperation,
    NetworkIO,
    DiskIO
}

/// <summary>
/// Performance statistics for a metric type.
/// </summary>
public class PerformanceStatistics
{
    public int Count { get; set; }
    public double Average { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double StandardDeviation { get; set; }
    public double Sum { get; set; }
    public DateTime FirstTimestamp { get; set; }
    public DateTime LastTimestamp { get; set; }
}

/// <summary>
/// Tracks the duration and context of an operation.
/// </summary>
internal class OperationTracker : IDisposable
{
    private readonly Cosmium.Engine.Infrastructure.Logging.PerformanceLogger _logger;
    private readonly string _operationId;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private readonly Dictionary<string, object> _properties = new();

    public OperationTracker(Cosmium.Engine.Infrastructure.Logging.PerformanceLogger logger, string operationId, string operationName)
    {
        _logger = logger;
        _operationId = operationId;
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
    }

    public void AddProperty(string key, object value)
    {
        _properties[key] = value;
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.EndOperation(_operationId, _operationName, _stopwatch.Elapsed, _properties);
    }
}

/// <summary>
/// Interface for performance metric output targets.
/// </summary>
public interface IPerformanceTarget : IDisposable
{
    void WriteMetric(PerformanceMetric metric);
}

/// <summary>
/// In-memory performance target for quick statistics and querying.
/// </summary>
public class InMemoryPerformanceTarget : IPerformanceTarget
{
    private readonly ConcurrentQueue<PerformanceMetric> _metrics = new();
    private readonly int _maxMetrics;

    public InMemoryPerformanceTarget(int maxMetrics = 10000)
    {
        _maxMetrics = maxMetrics;
    }

    public void WriteMetric(PerformanceMetric metric)
    {
        _metrics.Enqueue(metric);

        // Trim old metrics if we exceed the limit
        while (_metrics.Count > _maxMetrics)
        {
            _metrics.TryDequeue(out _);
        }
    }

    public PerformanceStatistics GetStatistics(MetricType type, TimeSpan? timeWindow = null)
    {
        var cutoff = timeWindow.HasValue ? DateTime.UtcNow - timeWindow.Value : DateTime.MinValue;
        var relevantMetrics = _metrics
            .Where(m => m.Type == type && m.Timestamp >= cutoff)
            .Select(m => m.Value)
            .ToList();

        if (!relevantMetrics.Any())
            return new PerformanceStatistics();

        var average = relevantMetrics.Average();
        var variance = relevantMetrics.Select(v => Math.Pow(v - average, 2)).Average();

        return new PerformanceStatistics
        {
            Count = relevantMetrics.Count,
            Average = average,
            Minimum = relevantMetrics.Min(),
            Maximum = relevantMetrics.Max(),
            StandardDeviation = Math.Sqrt(variance),
            Sum = relevantMetrics.Sum(),
            FirstTimestamp = _metrics.Where(m => m.Type == type && m.Timestamp >= cutoff)
                                   .Min(m => m.Timestamp),
            LastTimestamp = _metrics.Where(m => m.Type == type && m.Timestamp >= cutoff)
                                  .Max(m => m.Timestamp)
        };
    }

    public IEnumerable<PerformanceMetric> GetRecentMetrics(TimeSpan timeWindow)
    {
        var cutoff = DateTime.UtcNow - timeWindow;
        return _metrics.Where(m => m.Timestamp >= cutoff).OrderBy(m => m.Timestamp);
    }

    public void Dispose()
    {
        // In-memory target doesn't need disposal
    }
}
