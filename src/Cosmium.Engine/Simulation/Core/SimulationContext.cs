using System.Collections.Concurrent;
using Cosmium.Engine.Infrastructure.Configuration;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Simulation.Core;

/// <summary>
/// Execution context for quantum mechanical simulations.
/// Manages runtime state, resources, and coordination between simulation components.
/// </summary>
public class SimulationContext
{
    private readonly object _lock = new();
    private readonly Dictionary<string, object> _data = new();
    private readonly List<string> _messages = new();
    private readonly PerformanceMetrics _performanceMetrics = new();

    #region Properties

    /// <summary>
    /// Gets the unique identifier for this simulation context.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets when this context was created.
    /// </summary>
    public DateTime CreationTime { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the simulation parameters associated with this context.
    /// </summary>
    public SimulationParameters Parameters { get; private set; } = new();

    /// <summary>
    /// Gets the engine configuration being used.
    /// </summary>
    public EngineConfiguration Configuration { get; } = EngineConfiguration.Instance;

    /// <summary>
    /// Gets the random number generator for this simulation context.
    /// </summary>
    public Random RandomGenerator { get; private set; } = new();

    /// <summary>
    /// Gets the current thread count being used for parallel operations.
    /// </summary>
    public int ActiveThreadCount { get; private set; } = 1;

    /// <summary>
    /// Gets the current memory usage in bytes.
    /// </summary>
    public long MemoryUsage => GC.GetTotalMemory(false);

    /// <summary>
    /// Gets whether the context is configured for parallel execution.
    /// </summary>
    public bool IsParallelExecutionEnabled => Parameters.EnableParallelProcessing && ActiveThreadCount > 1;

    /// <summary>
    /// Gets the simulation logger instance.
    /// </summary>
    public SimulationLogger Logger { get; } = SimulationLogger.Instance;

    /// <summary>
    /// Gets performance metrics for the simulation.
    /// </summary>
    public IReadOnlyDictionary<string, double> PerformanceMetrics => _performanceMetrics.GetMetrics();

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the context with the specified simulation parameters.
    /// </summary>
    /// <param name="parameters">The simulation parameters.</param>
    /// <param name="seed">Optional random seed for reproducible results.</param>
    public void Initialize(SimulationParameters parameters, int? seed = null)
    {
        lock (_lock)
        {
            Parameters = parameters.Clone();
            
            // Initialize random number generator
            RandomGenerator = seed.HasValue ? new Random(seed.Value) : new Random();
            
            // Configure threading
            ActiveThreadCount = DetermineOptimalThreadCount();
            
            // Configure parallel options
            ConfigureParallelExecution();
            
            // Log initialization
            Logger.Information("Simulation context initialized", new
            {
                ContextId = Id,
                SimulationType = Parameters.SimulationType,
                ThreadCount = ActiveThreadCount,
                MemoryLimitMB = Parameters.MemoryLimitMB,
                Seed = seed
            });
        }
    }

    /// <summary>
    /// Determines the optimal number of threads for the current system and parameters.
    /// </summary>
    /// <returns>The optimal thread count.</returns>
    private int DetermineOptimalThreadCount()
    {
        if (!Parameters.EnableParallelProcessing)
            return 1;

        if (Parameters.ThreadCount > 0)
            return Math.Min(Parameters.ThreadCount, Environment.ProcessorCount);

        // Auto-detect based on system capabilities
        var availableCores = Environment.ProcessorCount;
        var optimalThreads = Math.Max(1, availableCores - 1); // Leave one core for OS

        // Limit based on memory constraints if specified
        if (Parameters.MemoryLimitMB > 0)
        {
            var availableMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
            var memoryPerThread = Math.Max(100, availableMemoryMB / optimalThreads);
            if (memoryPerThread < 100) // Minimum 100MB per thread
            {
                optimalThreads = Math.Max(1, (int)(availableMemoryMB / 100));
            }
        }

        return optimalThreads;
    }

    /// <summary>
    /// Configures parallel execution settings.
    /// </summary>
    private void ConfigureParallelExecution()
    {
        if (IsParallelExecutionEnabled)
        {
            // Configure ThreadPool if needed
            ThreadPool.SetMinThreads(ActiveThreadCount, ActiveThreadCount);
            
            // Set parallel LINQ degree of parallelism
            Environment.SetEnvironmentVariable("PLINQ_MAX_DEGREE_OF_PARALLELISM", ActiveThreadCount.ToString());
        }
    }

    #endregion

    #region Data Management

    /// <summary>
    /// Sets a value in the context data store.
    /// </summary>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    public void SetData(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        lock (_lock)
        {
            _data[key] = value;
        }
    }

    /// <summary>
    /// Gets a value from the context data store.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value if found, or default(T) if not found.</returns>
    public T? GetData<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        lock (_lock)
        {
            if (_data.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return default;
        }
    }

    /// <summary>
    /// Checks if a key exists in the context data store.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists.</returns>
    public bool HasData(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        lock (_lock)
        {
            return _data.ContainsKey(key);
        }
    }

    /// <summary>
    /// Removes a value from the context data store.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>True if the key was found and removed.</returns>
    public bool RemoveData(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        lock (_lock)
        {
            return _data.Remove(key);
        }
    }

    /// <summary>
    /// Gets all keys in the context data store.
    /// </summary>
    /// <returns>Collection of all keys.</returns>
    public IReadOnlyCollection<string> GetDataKeys()
    {
        lock (_lock)
        {
            return _data.Keys.ToList().AsReadOnly();
        }
    }

    #endregion

    #region Message Management

    /// <summary>
    /// Adds a message to the context message log.
    /// </summary>
    /// <param name="message">The message to add.</param>
    public void AddMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        
        lock (_lock)
        {
            _messages.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}: {message}");
        }
    }

    /// <summary>
    /// Gets all messages from the context message log.
    /// </summary>
    /// <returns>Read-only list of messages.</returns>
    public IReadOnlyList<string> GetMessages()
    {
        lock (_lock)
        {
            return _messages.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Clears all messages from the context message log.
    /// </summary>
    public void ClearMessages()
    {
        lock (_lock)
        {
            _messages.Clear();
        }
    }

    #endregion

    #region Performance Monitoring

    /// <summary>
    /// Records a performance metric.
    /// </summary>
    /// <param name="name">The name of the metric.</param>
    /// <param name="value">The metric value.</param>
    public void RecordMetric(string name, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _performanceMetrics.RecordMetric(name, value);
    }

    /// <summary>
    /// Records the duration of an operation.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="duration">The duration of the operation.</param>
    public void RecordOperationTime(string operationName, TimeSpan duration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        _performanceMetrics.RecordMetric($"{operationName}_duration_ms", duration.TotalMilliseconds);
    }

    /// <summary>
    /// Starts timing an operation.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <returns>A disposable timer that records the duration when disposed.</returns>
    public IDisposable StartTimer(string operationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        return new OperationTimer(this, operationName);
    }

    /// <summary>
    /// Updates performance metrics with current system state.
    /// </summary>
    public void UpdateSystemMetrics()
    {
        _performanceMetrics.RecordMetric("memory_usage_mb", MemoryUsage / (1024.0 * 1024.0));
        _performanceMetrics.RecordMetric("active_threads", ActiveThreadCount);
        
        // Record GC information
        _performanceMetrics.RecordMetric("gc_gen0_collections", GC.CollectionCount(0));
        _performanceMetrics.RecordMetric("gc_gen1_collections", GC.CollectionCount(1));
        _performanceMetrics.RecordMetric("gc_gen2_collections", GC.CollectionCount(2));
    }

    #endregion

    #region Resource Management

    /// <summary>
    /// Checks if the simulation is approaching memory limits.
    /// </summary>
    /// <returns>True if memory usage is high.</returns>
    public bool IsMemoryUsageHigh()
    {
        if (Parameters.MemoryLimitMB <= 0)
            return false;

        var usageMB = MemoryUsage / (1024.0 * 1024.0);
        return usageMB > (Parameters.MemoryLimitMB * 0.8); // 80% threshold
    }

    /// <summary>
    /// Forces garbage collection if memory usage is high.
    /// </summary>
    public void TryForceGarbageCollection()
    {
        if (IsMemoryUsageHigh())
        {
            Logger.Warning("High memory usage detected, forcing garbage collection", new { MemoryUsageMB = MemoryUsage / (1024.0 * 1024.0) });
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    /// <summary>
    /// Validates that the context is in a consistent state.
    /// </summary>
    /// <returns>True if the context is valid.</returns>
    public bool ValidateState()
    {
        try
        {
            // Check basic validity
            if (Parameters == null)
                return false;

            if (ActiveThreadCount <= 0)
                return false;

            // Check memory constraints
            if (Parameters.MemoryLimitMB > 0 && MemoryUsage > Parameters.MemoryLimitMB * 1024 * 1024)
                return false;

            // Validate parameters
            return Parameters.Validate();
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Performs cleanup operations for the context.
    /// </summary>
    public void Cleanup()
    {
        lock (_lock)
        {
            // Clear data
            _data.Clear();
            _messages.Clear();
            
            // Force final garbage collection
            TryForceGarbageCollection();
            
            Logger.Information("Simulation context cleaned up", new { ContextId = Id });
        }
    }

    #endregion
}

/// <summary>
/// Performance metrics collection for simulation context.
/// </summary>
internal class PerformanceMetrics
{
    private readonly ConcurrentDictionary<string, List<double>> _metrics = new();
    private readonly object _lock = new();

    /// <summary>
    /// Records a metric value.
    /// </summary>
    /// <param name="name">The metric name.</param>
    /// <param name="value">The metric value.</param>
    public void RecordMetric(string name, double value)
    {
        _metrics.AddOrUpdate(name, 
            new List<double> { value },
            (key, existing) =>
            {
                lock (_lock)
                {
                    existing.Add(value);
                    // Keep only the last 1000 values to prevent memory bloat
                    if (existing.Count > 1000)
                    {
                        existing.RemoveAt(0);
                    }
                    return existing;
                }
            });
    }

    /// <summary>
    /// Gets aggregated metrics.
    /// </summary>
    /// <returns>Dictionary of metric names to average values.</returns>
    public IReadOnlyDictionary<string, double> GetMetrics()
    {
        var result = new Dictionary<string, double>();
        
        foreach (var kvp in _metrics)
        {
            lock (_lock)
            {
                if (kvp.Value.Count > 0)
                {
                    result[kvp.Key] = kvp.Value.Average();
                    result[$"{kvp.Key}_min"] = kvp.Value.Min();
                    result[$"{kvp.Key}_max"] = kvp.Value.Max();
                    result[$"{kvp.Key}_count"] = kvp.Value.Count;
                }
            }
        }
        
        return result.AsReadOnly();
    }
}

/// <summary>
/// Timer for measuring operation durations.
/// </summary>
internal class OperationTimer : IDisposable
{
    private readonly SimulationContext _context;
    private readonly string _operationName;
    private readonly DateTime _startTime;
    private bool _disposed;

    public OperationTimer(SimulationContext context, string operationName)
    {
        _context = context;
        _operationName = operationName;
        _startTime = DateTime.UtcNow;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            var duration = DateTime.UtcNow - _startTime;
            _context.RecordOperationTime(_operationName, duration);
            _disposed = true;
        }
    }
}
