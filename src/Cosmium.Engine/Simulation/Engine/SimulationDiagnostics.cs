namespace Cosmium.Engine.Simulation.Engine;

/// <summary>
/// Represents diagnostic information about the simulation engine's health and status.
/// </summary>
public class SimulationDiagnostics
{
    /// <summary>
    /// Unique identifier of the simulation engine.
    /// </summary>
    public Guid EngineId { get; set; }

    /// <summary>
    /// Name of the simulation engine.
    /// </summary>
    public string EngineName { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the simulation engine.
    /// </summary>
    public SimulationEngineStatus Status { get; set; }

    /// <summary>
    /// Overall health status of the engine.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Timestamp when diagnostics were collected.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status of individual components within the engine.
    /// </summary>
    public Dictionary<string, object> ComponentStatus { get; set; } = new();

    /// <summary>
    /// Current performance metrics.
    /// </summary>
    public SimulationEngineMetrics? PerformanceMetrics { get; set; }

    /// <summary>
    /// Last error that occurred, if any.
    /// </summary>
    public Exception? LastError { get; set; }

    /// <summary>
    /// List of current warnings or issues.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// List of performance recommendations.
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// System resource information.
    /// </summary>
    public SystemResourceInfo SystemResources { get; set; } = new();

    /// <summary>
    /// Configuration validation results.
    /// </summary>
    public Dictionary<string, bool> ConfigurationValidation { get; set; } = new();

    /// <summary>
    /// Network connectivity status (if applicable).
    /// </summary>
    public Dictionary<string, bool> NetworkStatus { get; set; } = new();

    /// <summary>
    /// Disk space and I/O information.
    /// </summary>
    public DiskInfo DiskInfo { get; set; } = new();

    /// <summary>
    /// Thread and concurrency information.
    /// </summary>
    public ConcurrencyInfo ConcurrencyInfo { get; set; } = new();

    /// <summary>
    /// Memory allocation and garbage collection information.
    /// </summary>
    public MemoryInfo MemoryInfo { get; set; } = new();

    /// <summary>
    /// Validates the overall health of the simulation engine.
    /// </summary>
    public HealthStatus GetOverallHealth()
    {
        if (!IsHealthy || LastError != null)
            return HealthStatus.Critical;

        if (Warnings.Count > 10)
            return HealthStatus.Warning;

        if (PerformanceMetrics != null)
        {
            if (PerformanceMetrics.EfficiencyRating < 0.5)
                return HealthStatus.Warning;

            if (PerformanceMetrics.ErrorCount > 0)
                return HealthStatus.Warning;
        }

        return HealthStatus.Healthy;
    }

    /// <summary>
    /// Gets a summary of the most critical issues.
    /// </summary>
    public List<string> GetCriticalIssues()
    {
        var issues = new List<string>();

        if (!IsHealthy)
            issues.Add("Engine is not healthy");

        if (LastError != null)
            issues.Add($"Last error: {LastError.Message}");

        if (PerformanceMetrics != null)
        {
            if (PerformanceMetrics.MemoryUsageMB > SystemResources.TotalMemoryMB * 0.9)
                issues.Add("Memory usage critically high");

            if (PerformanceMetrics.CpuUsagePercent > 95.0)
                issues.Add("CPU usage critically high");

            if (PerformanceMetrics.EfficiencyRating < 0.3)
                issues.Add("Performance efficiency critically low");
        }

        if (DiskInfo.AvailableSpaceGB < 1.0)
            issues.Add("Disk space critically low");

        return issues;
    }

    /// <summary>
    /// Creates a copy of the diagnostics.
    /// </summary>
    public SimulationDiagnostics Clone()
    {
        return new SimulationDiagnostics
        {
            EngineId = EngineId,
            EngineName = EngineName,
            Status = Status,
            IsHealthy = IsHealthy,
            Timestamp = Timestamp,
            ComponentStatus = new Dictionary<string, object>(ComponentStatus),
            PerformanceMetrics = PerformanceMetrics?.Clone(),
            LastError = LastError,
            Warnings = new List<string>(Warnings),
            Recommendations = new List<string>(Recommendations),
            SystemResources = SystemResources.Clone(),
            ConfigurationValidation = new Dictionary<string, bool>(ConfigurationValidation),
            NetworkStatus = new Dictionary<string, bool>(NetworkStatus),
            DiskInfo = DiskInfo.Clone(),
            ConcurrencyInfo = ConcurrencyInfo.Clone(),
            MemoryInfo = MemoryInfo.Clone()
        };
    }

    /// <summary>
    /// Returns a string representation of the diagnostics.
    /// </summary>
    public override string ToString()
    {
        var health = GetOverallHealth();
        return $"Engine {EngineName} ({EngineId:D}): {Status}, Health: {health}, " +
               $"Warnings: {Warnings.Count}, Critical Issues: {GetCriticalIssues().Count}";
    }
}

/// <summary>
/// Overall health status levels.
/// </summary>
public enum HealthStatus
{
    Healthy,
    Warning,
    Critical,
    Unknown
}

/// <summary>
/// System resource information.
/// </summary>
public class SystemResourceInfo
{
    public double TotalMemoryMB { get; set; }
    public double AvailableMemoryMB { get; set; }
    public int CpuCoreCount { get; set; }
    public double CpuUsagePercent { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;

    public SystemResourceInfo Clone()
    {
        return new SystemResourceInfo
        {
            TotalMemoryMB = TotalMemoryMB,
            AvailableMemoryMB = AvailableMemoryMB,
            CpuCoreCount = CpuCoreCount,
            CpuUsagePercent = CpuUsagePercent,
            OperatingSystem = OperatingSystem,
            RuntimeVersion = RuntimeVersion
        };
    }
}

/// <summary>
/// Disk space and I/O information.
/// </summary>
public class DiskInfo
{
    public double TotalSpaceGB { get; set; }
    public double AvailableSpaceGB { get; set; }
    public double UsedSpaceGB { get; set; }
    public double IoReadMBps { get; set; }
    public double IoWriteMBps { get; set; }

    public DiskInfo Clone()
    {
        return new DiskInfo
        {
            TotalSpaceGB = TotalSpaceGB,
            AvailableSpaceGB = AvailableSpaceGB,
            UsedSpaceGB = UsedSpaceGB,
            IoReadMBps = IoReadMBps,
            IoWriteMBps = IoWriteMBps
        };
    }
}

/// <summary>
/// Thread and concurrency information.
/// </summary>
public class ConcurrencyInfo
{
    public int ActiveThreadCount { get; set; }
    public int ThreadPoolWorkerThreads { get; set; }
    public int ThreadPoolCompletionPortThreads { get; set; }
    public int TaskSchedulerTaskCount { get; set; }
    public bool IsThreadSafe { get; set; } = true;
    public List<string> DeadlockWarnings { get; set; } = new();

    public ConcurrencyInfo Clone()
    {
        return new ConcurrencyInfo
        {
            ActiveThreadCount = ActiveThreadCount,
            ThreadPoolWorkerThreads = ThreadPoolWorkerThreads,
            ThreadPoolCompletionPortThreads = ThreadPoolCompletionPortThreads,
            TaskSchedulerTaskCount = TaskSchedulerTaskCount,
            IsThreadSafe = IsThreadSafe,
            DeadlockWarnings = new List<string>(DeadlockWarnings)
        };
    }
}

/// <summary>
/// Memory allocation and garbage collection information.
/// </summary>
public class MemoryInfo
{
    public long TotalAllocatedBytes { get; set; }
    public long Gen0Collections { get; set; }
    public long Gen1Collections { get; set; }
    public long Gen2Collections { get; set; }
    public double LastGcDurationMs { get; set; }
    public bool IsLowMemory { get; set; }
    public List<string> MemoryWarnings { get; set; } = new();

    public MemoryInfo Clone()
    {
        return new MemoryInfo
        {
            TotalAllocatedBytes = TotalAllocatedBytes,
            Gen0Collections = Gen0Collections,
            Gen1Collections = Gen1Collections,
            Gen2Collections = Gen2Collections,
            LastGcDurationMs = LastGcDurationMs,
            IsLowMemory = IsLowMemory,
            MemoryWarnings = new List<string>(MemoryWarnings)
        };
    }
}
