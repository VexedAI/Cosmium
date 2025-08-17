using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Simulation.Core;
using Cosmium.Engine.Simulation.Events;

namespace Cosmium.Engine.Simulation.Engine;

/// <summary>
/// Interface for the main simulation engine that orchestrates quantum mechanical simulations.
/// Coordinates time evolution, state management, and particle interactions in quantum systems.
/// </summary>
public interface ISimulationEngine
{
    #region Basic Properties

    /// <summary>
    /// Gets the unique identifier for this simulation engine instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the simulation engine.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the simulation engine.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the current status of the simulation engine.
    /// </summary>
    SimulationEngineStatus Status { get; }

    /// <summary>
    /// Gets the simulation being executed by this engine.
    /// </summary>
    ISimulation? CurrentSimulation { get; }

    /// <summary>
    /// Gets when this engine was created.
    /// </summary>
    DateTime CreationTime { get; }

    /// <summary>
    /// Gets when the current simulation started.
    /// </summary>
    DateTime? StartTime { get; }

    #endregion

    #region Configuration and Capabilities

    /// <summary>
    /// Gets the supported simulation types by this engine.
    /// </summary>
    IReadOnlyList<string> SupportedSimulationTypes { get; }

    /// <summary>
    /// Gets whether the engine supports parallel execution.
    /// </summary>
    bool SupportsParallelExecution { get; }

    /// <summary>
    /// Gets whether the engine supports adaptive time stepping.
    /// </summary>
    bool SupportsAdaptiveTimeStep { get; }

    /// <summary>
    /// Gets whether the engine supports real-time monitoring.
    /// </summary>
    bool SupportsRealTimeMonitoring { get; }

    /// <summary>
    /// Gets the maximum number of particles this engine can handle efficiently.
    /// </summary>
    int MaxParticleCount { get; }

    /// <summary>
    /// Gets the recommended memory limit in MB for optimal performance.
    /// </summary>
    int RecommendedMemoryLimitMB { get; }

    #endregion

    #region Engine Lifecycle

    /// <summary>
    /// Initializes the simulation engine with the specified configuration.
    /// </summary>
    /// <param name="parameters">The simulation parameters to use.</param>
    Task InitializeAsync(SimulationParameters parameters);

    /// <summary>
    /// Starts execution of the specified simulation.
    /// </summary>
    /// <param name="simulation">The simulation to execute.</param>
    Task StartSimulationAsync(ISimulation simulation);

    /// <summary>
    /// Pauses the currently running simulation.
    /// </summary>
    Task PauseSimulationAsync();

    /// <summary>
    /// Resumes a paused simulation.
    /// </summary>
    Task ResumeSimulationAsync();

    /// <summary>
    /// Stops the currently running simulation.
    /// </summary>
    Task StopSimulationAsync();

    /// <summary>
    /// Resets the engine to its initial state.
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// Shuts down the engine and performs cleanup.
    /// </summary>
    Task ShutdownAsync();

    #endregion

    #region Execution Control

    /// <summary>
    /// Executes a single simulation step.
    /// </summary>
    Task ExecuteStepAsync();

    /// <summary>
    /// Executes multiple simulation steps.
    /// </summary>
    /// <param name="stepCount">The number of steps to execute.</param>
    Task ExecuteStepsAsync(long stepCount);

    /// <summary>
    /// Executes simulation for a specified time duration.
    /// </summary>
    /// <param name="duration">The time duration to simulate.</param>
    Task ExecuteForTimeAsync(double duration);

    /// <summary>
    /// Executes simulation until convergence is achieved.
    /// </summary>
    /// <param name="maxSteps">Maximum steps before timeout.</param>
    /// <param name="tolerance">Convergence tolerance.</param>
    Task ExecuteUntilConvergenceAsync(long maxSteps, double tolerance);

    #endregion

    #region Time Evolution Management

    /// <summary>
    /// Gets the time step manager used for time evolution.
    /// </summary>
    ITimeStepManager TimeStepManager { get; }

    /// <summary>
    /// Gets the state evolution engine used for quantum evolution.
    /// </summary>
    IStateEvolutionEngine StateEvolutionEngine { get; }

    /// <summary>
    /// Gets the current simulation time.
    /// </summary>
    double CurrentTime { get; }

    /// <summary>
    /// Gets the current simulation step.
    /// </summary>
    long CurrentStep { get; }

    /// <summary>
    /// Gets the current time step being used.
    /// </summary>
    double CurrentTimeStep { get; }

    /// <summary>
    /// Sets a custom time step for the next evolution.
    /// </summary>
    /// <param name="timeStep">The time step to use.</param>
    void SetCustomTimeStep(double timeStep);

    /// <summary>
    /// Resets time step to automatic determination.
    /// </summary>
    void ResetToAutomaticTimeStep();

    #endregion

    #region Performance and Monitoring

    /// <summary>
    /// Gets the current execution performance metrics.
    /// </summary>
    SimulationPerformanceMetrics PerformanceMetrics { get; }

    /// <summary>
    /// Gets the current steps per second execution rate.
    /// </summary>
    double StepsPerSecond { get; }

    /// <summary>
    /// Gets the current memory usage in MB.
    /// </summary>
    double MemoryUsageMB { get; }

    /// <summary>
    /// Gets whether the engine is operating within optimal performance ranges.
    /// </summary>
    bool IsPerformingOptimally { get; }

    /// <summary>
    /// Gets performance recommendations for optimization.
    /// </summary>
    IReadOnlyList<string> GetPerformanceRecommendations();

    #endregion

    #region Error Handling and Diagnostics

    /// <summary>
    /// Gets whether the engine is in a valid state for execution.
    /// </summary>
    bool IsHealthy { get; }

    /// <summary>
    /// Performs a comprehensive health check of the engine.
    /// </summary>
    /// <returns>Health check result with detailed diagnostics.</returns>
    Task<EngineHealthReport> PerformHealthCheckAsync();

    /// <summary>
    /// Gets any current warnings or issues with the engine state.
    /// </summary>
    IReadOnlyList<string> GetCurrentIssues();

    /// <summary>
    /// Attempts to recover from a detected error condition.
    /// </summary>
    /// <returns>True if recovery was successful.</returns>
    Task<bool> AttemptRecoveryAsync();

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the engine status changes.
    /// </summary>
    event EventHandler<EngineStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Event raised when a simulation step is completed.
    /// </summary>
    event EventHandler<SimulationStepCompletedEventArgs>? StepCompleted;

    /// <summary>
    /// Event raised when performance metrics are updated.
    /// </summary>
    event EventHandler<PerformanceMetricsUpdatedEventArgs>? PerformanceUpdated;

    /// <summary>
    /// Event raised when an error occurs in the engine.
    /// </summary>
    event EventHandler<EngineErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// Event raised when the engine detects a convergence condition.
    /// </summary>
    event EventHandler<ConvergenceDetectedEventArgs>? ConvergenceDetected;

    #endregion
}

/// <summary>
/// Enumeration of simulation engine states.
/// </summary>
public enum SimulationEngineStatus
{
    /// <summary>
    /// Engine has been created but not initialized.
    /// </summary>
    Created,

    /// <summary>
    /// Engine has been initialized and is ready for simulation.
    /// </summary>
    Initialized,

    /// <summary>
    /// Engine is actively executing a simulation.
    /// </summary>
    Running,

    /// <summary>
    /// Engine execution is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// Engine is stopping current simulation.
    /// </summary>
    Stopping,

    /// <summary>
    /// Engine is idle and ready for new simulation.
    /// </summary>
    Idle,

    /// <summary>
    /// Engine is in an error state.
    /// </summary>
    Error,

    /// <summary>
    /// Engine is shutting down.
    /// </summary>
    ShuttingDown,

    /// <summary>
    /// Engine has been shut down.
    /// </summary>
    Shutdown
}

/// <summary>
/// Performance metrics for simulation engine execution.
/// </summary>
public class SimulationPerformanceMetrics
{
    /// <summary>
    /// Gets the current steps per second rate.
    /// </summary>
    public double StepsPerSecond { get; init; }

    /// <summary>
    /// Gets the average step execution time in milliseconds.
    /// </summary>
    public double AverageStepTimeMs { get; init; }

    /// <summary>
    /// Gets the current memory usage in MB.
    /// </summary>
    public double MemoryUsageMB { get; init; }

    /// <summary>
    /// Gets the CPU usage percentage.
    /// </summary>
    public double CpuUsagePercent { get; init; }

    /// <summary>
    /// Gets the number of active threads.
    /// </summary>
    public int ActiveThreads { get; init; }

    /// <summary>
    /// Gets the number of garbage collections performed.
    /// </summary>
    public long GarbageCollections { get; init; }

    /// <summary>
    /// Gets the total execution time.
    /// </summary>
    public TimeSpan TotalExecutionTime { get; init; }

    /// <summary>
    /// Gets the efficiency rating (0.0 to 1.0).
    /// </summary>
    public double Efficiency { get; init; }
}

/// <summary>
/// Health report for simulation engine diagnostics.
/// </summary>
public class EngineHealthReport
{
    /// <summary>
    /// Gets whether the engine is healthy.
    /// </summary>
    public bool IsHealthy { get; init; }

    /// <summary>
    /// Gets the overall health score (0.0 to 1.0).
    /// </summary>
    public double HealthScore { get; init; }

    /// <summary>
    /// Gets any detected issues.
    /// </summary>
    public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets performance recommendations.
    /// </summary>
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets detailed diagnostic information.
    /// </summary>
    public Dictionary<string, object> DiagnosticData { get; init; } = new();

    /// <summary>
    /// Gets when the health check was performed.
    /// </summary>
    public DateTime CheckTimestamp { get; init; } = DateTime.UtcNow;
}
