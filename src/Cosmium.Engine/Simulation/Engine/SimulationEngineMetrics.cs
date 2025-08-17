namespace Cosmium.Engine.Simulation.Engine;

/// <summary>
/// Represents performance and operational metrics for the simulation engine.
/// </summary>
public class SimulationEngineMetrics
{
    /// <summary>
    /// Total number of simulation steps completed.
    /// </summary>
    public long TotalSteps { get; set; }

    /// <summary>
    /// Average time per simulation step in milliseconds.
    /// </summary>
    public double AverageStepTimeMs { get; set; }

    /// <summary>
    /// Total execution time in milliseconds.
    /// </summary>
    public double TotalExecutionTimeMs { get; set; }

    /// <summary>
    /// Current number of particles in the simulation.
    /// </summary>
    public int ParticleCount { get; set; }

    /// <summary>
    /// Current number of active components.
    /// </summary>
    public int ComponentCount { get; set; }

    /// <summary>
    /// Current memory usage in megabytes.
    /// </summary>
    public double MemoryUsageMB { get; set; }

    /// <summary>
    /// Current CPU usage percentage.
    /// </summary>
    public double CpuUsagePercent { get; set; }

    /// <summary>
    /// Simulation throughput in steps per second.
    /// </summary>
    public double ThroughputStepsPerSecond { get; set; }

    /// <summary>
    /// Overall efficiency rating (0.0 to 1.0).
    /// </summary>
    public double EfficiencyRating { get; set; }

    /// <summary>
    /// Number of errors encountered during simulation.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Number of warnings generated during simulation.
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// Peak memory usage in megabytes during this session.
    /// </summary>
    public double PeakMemoryUsageMB { get; set; }

    /// <summary>
    /// Peak CPU usage percentage during this session.
    /// </summary>
    public double PeakCpuUsagePercent { get; set; }

    /// <summary>
    /// Time spent in state evolution calculations (milliseconds).
    /// </summary>
    public double StateEvolutionTimeMs { get; set; }

    /// <summary>
    /// Time spent in time step calculations (milliseconds).
    /// </summary>
    public double TimeStepCalculationMs { get; set; }

    /// <summary>
    /// Time spent in particle interactions (milliseconds).
    /// </summary>
    public double ParticleInteractionTimeMs { get; set; }

    /// <summary>
    /// Time spent in I/O operations (milliseconds).
    /// </summary>
    public double IoTimeMs { get; set; }

    /// <summary>
    /// Number of garbage collection cycles during simulation.
    /// </summary>
    public int GarbageCollectionCount { get; set; }

    /// <summary>
    /// Total time spent in garbage collection (milliseconds).
    /// </summary>
    public double GarbageCollectionTimeMs { get; set; }

    /// <summary>
    /// Number of cache hits for frequently accessed data.
    /// </summary>
    public long CacheHits { get; set; }

    /// <summary>
    /// Number of cache misses.
    /// </summary>
    public long CacheMisses { get; set; }

    /// <summary>
    /// Cache hit ratio (0.0 to 1.0).
    /// </summary>
    public double CacheHitRatio => CacheHits + CacheMisses > 0 ? 
        (double)CacheHits / (CacheHits + CacheMisses) : 0.0;

    /// <summary>
    /// Timestamp when metrics were last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a copy of the current metrics.
    /// </summary>
    public SimulationEngineMetrics Clone()
    {
        return new SimulationEngineMetrics
        {
            TotalSteps = TotalSteps,
            AverageStepTimeMs = AverageStepTimeMs,
            TotalExecutionTimeMs = TotalExecutionTimeMs,
            ParticleCount = ParticleCount,
            ComponentCount = ComponentCount,
            MemoryUsageMB = MemoryUsageMB,
            CpuUsagePercent = CpuUsagePercent,
            ThroughputStepsPerSecond = ThroughputStepsPerSecond,
            EfficiencyRating = EfficiencyRating,
            ErrorCount = ErrorCount,
            WarningCount = WarningCount,
            PeakMemoryUsageMB = PeakMemoryUsageMB,
            PeakCpuUsagePercent = PeakCpuUsagePercent,
            StateEvolutionTimeMs = StateEvolutionTimeMs,
            TimeStepCalculationMs = TimeStepCalculationMs,
            ParticleInteractionTimeMs = ParticleInteractionTimeMs,
            IoTimeMs = IoTimeMs,
            GarbageCollectionCount = GarbageCollectionCount,
            GarbageCollectionTimeMs = GarbageCollectionTimeMs,
            CacheHits = CacheHits,
            CacheMisses = CacheMisses,
            LastUpdated = LastUpdated
        };
    }

    /// <summary>
    /// Resets all metrics to their initial values.
    /// </summary>
    public void Reset()
    {
        TotalSteps = 0;
        AverageStepTimeMs = 0.0;
        TotalExecutionTimeMs = 0.0;
        ParticleCount = 0;
        ComponentCount = 0;
        MemoryUsageMB = 0.0;
        CpuUsagePercent = 0.0;
        ThroughputStepsPerSecond = 0.0;
        EfficiencyRating = 1.0;
        ErrorCount = 0;
        WarningCount = 0;
        PeakMemoryUsageMB = 0.0;
        PeakCpuUsagePercent = 0.0;
        StateEvolutionTimeMs = 0.0;
        TimeStepCalculationMs = 0.0;
        ParticleInteractionTimeMs = 0.0;
        IoTimeMs = 0.0;
        GarbageCollectionCount = 0;
        GarbageCollectionTimeMs = 0.0;
        CacheHits = 0;
        CacheMisses = 0;
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the metrics with current values.
    /// </summary>
    public void Update(
        long totalSteps,
        double totalExecutionTimeMs,
        int particleCount,
        int componentCount,
        double memoryUsageMB,
        double cpuUsagePercent)
    {
        TotalSteps = totalSteps;
        TotalExecutionTimeMs = totalExecutionTimeMs;
        ParticleCount = particleCount;
        ComponentCount = componentCount;
        MemoryUsageMB = memoryUsageMB;
        CpuUsagePercent = cpuUsagePercent;
        
        // Calculate derived values
        AverageStepTimeMs = totalSteps > 0 ? totalExecutionTimeMs / totalSteps : 0.0;
        ThroughputStepsPerSecond = totalExecutionTimeMs > 0 ? (totalSteps * 1000.0) / totalExecutionTimeMs : 0.0;
        
        // Update peaks
        PeakMemoryUsageMB = Math.Max(PeakMemoryUsageMB, memoryUsageMB);
        PeakCpuUsagePercent = Math.Max(PeakCpuUsagePercent, cpuUsagePercent);
        
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a string representation of the key metrics.
    /// </summary>
    public override string ToString()
    {
        return $"Steps: {TotalSteps:N0}, Avg Step: {AverageStepTimeMs:F2}ms, " +
               $"Throughput: {ThroughputStepsPerSecond:F1}/s, Memory: {MemoryUsageMB:F1}MB, " +
               $"CPU: {CpuUsagePercent:F1}%, Efficiency: {EfficiencyRating:F2}";
    }
}
