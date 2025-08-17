using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Simulation.Core;

namespace Cosmium.Engine.Simulation.Events;

/// <summary>
/// Base class for all simulation-related event arguments.
/// Provides common properties and functionality for simulation events.
/// </summary>
public abstract class SimulationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the unique identifier of the simulation that raised this event.
    /// </summary>
    public Guid SimulationId { get; }

    /// <summary>
    /// Gets when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Gets the simulation time when the event occurred (physical time being modeled).
    /// </summary>
    public double SimulationTime { get; }

    /// <summary>
    /// Gets the simulation step when the event occurred.
    /// </summary>
    public long SimulationStep { get; }

    /// <summary>
    /// Gets additional metadata associated with this event.
    /// </summary>
    public Dictionary<string, object> Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the SimulationEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    protected SimulationEventArgs(Guid simulationId, double simulationTime, long simulationStep)
    {
        SimulationId = simulationId;
        Timestamp = DateTime.UtcNow;
        SimulationTime = simulationTime;
        SimulationStep = simulationStep;
        Metadata = new Dictionary<string, object>();
    }
}

/// <summary>
/// Event arguments for simulation status changes.
/// </summary>
public class SimulationStatusChangedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the previous status of the simulation.
    /// </summary>
    public SimulationStatus PreviousStatus { get; }

    /// <summary>
    /// Gets the new status of the simulation.
    /// </summary>
    public SimulationStatus NewStatus { get; }

    /// <summary>
    /// Gets the reason for the status change.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Initializes a new instance of the SimulationStatusChangedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="previousStatus">The previous status.</param>
    /// <param name="newStatus">The new status.</param>
    /// <param name="reason">The reason for the change.</param>
    public SimulationStatusChangedEventArgs(
        Guid simulationId, 
        double simulationTime, 
        long simulationStep,
        SimulationStatus previousStatus, 
        SimulationStatus newStatus, 
        string reason) 
        : base(simulationId, simulationTime, simulationStep)
    {
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }
}

/// <summary>
/// Event arguments for simulation progress updates.
/// </summary>
public class SimulationProgressEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the progress percentage (0-100).
    /// </summary>
    public double ProgressPercentage { get; }

    /// <summary>
    /// Gets the estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; }

    /// <summary>
    /// Gets the current steps per second rate.
    /// </summary>
    public double StepsPerSecond { get; }

    /// <summary>
    /// Gets the current memory usage in MB.
    /// </summary>
    public double MemoryUsageMB { get; }

    /// <summary>
    /// Gets whether the simulation has converged.
    /// </summary>
    public bool HasConverged { get; }

    /// <summary>
    /// Gets the current convergence error.
    /// </summary>
    public double ConvergenceError { get; }

    /// <summary>
    /// Gets the current total energy of the system.
    /// </summary>
    public double TotalEnergy { get; }

    /// <summary>
    /// Gets additional progress information.
    /// </summary>
    public string StatusMessage { get; }

    /// <summary>
    /// Initializes a new instance of the SimulationProgressEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="progressPercentage">The progress percentage.</param>
    /// <param name="stepsPerSecond">The steps per second rate.</param>
    /// <param name="memoryUsageMB">The memory usage in MB.</param>
    /// <param name="hasConverged">Whether the simulation has converged.</param>
    /// <param name="convergenceError">The convergence error.</param>
    /// <param name="totalEnergy">The total energy.</param>
    /// <param name="statusMessage">Additional status message.</param>
    public SimulationProgressEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        double progressPercentage,
        double stepsPerSecond,
        double memoryUsageMB,
        bool hasConverged,
        double convergenceError,
        double totalEnergy,
        string statusMessage = "")
        : base(simulationId, simulationTime, simulationStep)
    {
        ProgressPercentage = Math.Max(0, Math.Min(100, progressPercentage));
        StepsPerSecond = stepsPerSecond;
        MemoryUsageMB = memoryUsageMB;
        HasConverged = hasConverged;
        ConvergenceError = convergenceError;
        TotalEnergy = totalEnergy;
        StatusMessage = statusMessage ?? string.Empty;

        // Calculate estimated time remaining if possible
        if (progressPercentage > 0 && progressPercentage < 100 && stepsPerSecond > 0)
        {
            var remainingPercentage = 100 - progressPercentage;
            var stepsCompleted = simulationStep;
            var totalStepsEstimate = stepsCompleted * 100 / progressPercentage;
            var stepsRemaining = totalStepsEstimate - stepsCompleted;
            var secondsRemaining = stepsRemaining / stepsPerSecond;
            EstimatedTimeRemaining = TimeSpan.FromSeconds(secondsRemaining);
        }
    }
}

/// <summary>
/// Event arguments for simulation completion.
/// </summary>
public class SimulationCompletedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets whether the simulation completed successfully.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the simulation result.
    /// </summary>
    public SimulationResult Result { get; }

    /// <summary>
    /// Gets the reason for completion.
    /// </summary>
    public string CompletionReason { get; }

    /// <summary>
    /// Gets the final status of the simulation.
    /// </summary>
    public SimulationStatus FinalStatus { get; }

    /// <summary>
    /// Initializes a new instance of the SimulationCompletedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="isSuccessful">Whether the simulation was successful.</param>
    /// <param name="result">The simulation result.</param>
    /// <param name="completionReason">The reason for completion.</param>
    /// <param name="finalStatus">The final status.</param>
    public SimulationCompletedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        bool isSuccessful,
        SimulationResult result,
        string completionReason,
        SimulationStatus finalStatus)
        : base(simulationId, simulationTime, simulationStep)
    {
        IsSuccessful = isSuccessful;
        Result = result ?? throw new ArgumentNullException(nameof(result));
        CompletionReason = completionReason ?? throw new ArgumentNullException(nameof(completionReason));
        FinalStatus = finalStatus;
    }
}

/// <summary>
/// Event arguments for simulation errors.
/// </summary>
public class SimulationErrorEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the error that occurred.
    /// </summary>
    public Exception Error { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the severity of the error.
    /// </summary>
    public ErrorSeverity Severity { get; }

    /// <summary>
    /// Gets whether the error is recoverable.
    /// </summary>
    public bool IsRecoverable { get; }

    /// <summary>
    /// Gets the component that generated the error.
    /// </summary>
    public string Component { get; }

    /// <summary>
    /// Gets additional error context.
    /// </summary>
    public Dictionary<string, object> ErrorContext { get; }

    /// <summary>
    /// Initializes a new instance of the SimulationErrorEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="error">The error that occurred.</param>
    /// <param name="severity">The error severity.</param>
    /// <param name="isRecoverable">Whether the error is recoverable.</param>
    /// <param name="component">The component that generated the error.</param>
    public SimulationErrorEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        Exception error,
        ErrorSeverity severity,
        bool isRecoverable,
        string component = "")
        : base(simulationId, simulationTime, simulationStep)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        ErrorMessage = error.Message;
        Severity = severity;
        IsRecoverable = isRecoverable;
        Component = component ?? string.Empty;
        ErrorContext = new Dictionary<string, object>();
    }
}

/// <summary>
/// Event arguments for particle addition to simulation.
/// </summary>
public class ParticleAddedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the particle that was added.
    /// </summary>
    public IQuantumParticle Particle { get; }

    /// <summary>
    /// Gets the reason the particle was added.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the new total particle count.
    /// </summary>
    public int NewParticleCount { get; }

    /// <summary>
    /// Initializes a new instance of the ParticleAddedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="particle">The particle that was added.</param>
    /// <param name="reason">The reason for addition.</param>
    /// <param name="newParticleCount">The new particle count.</param>
    public ParticleAddedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        IQuantumParticle particle,
        string reason,
        int newParticleCount)
        : base(simulationId, simulationTime, simulationStep)
    {
        Particle = particle ?? throw new ArgumentNullException(nameof(particle));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        NewParticleCount = newParticleCount;
    }
}

/// <summary>
/// Event arguments for particle removal from simulation.
/// </summary>
public class ParticleRemovedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the identifier of the particle that was removed.
    /// </summary>
    public Guid ParticleId { get; }

    /// <summary>
    /// Gets the name of the particle type that was removed.
    /// </summary>
    public string ParticleType { get; }

    /// <summary>
    /// Gets the reason the particle was removed.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the new total particle count.
    /// </summary>
    public int NewParticleCount { get; }

    /// <summary>
    /// Gets whether the particle was removed due to decay.
    /// </summary>
    public bool WasDecay { get; }

    /// <summary>
    /// Gets whether the particle was removed due to interaction.
    /// </summary>
    public bool WasInteraction { get; }

    /// <summary>
    /// Initializes a new instance of the ParticleRemovedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="particleId">The particle identifier.</param>
    /// <param name="particleType">The particle type.</param>
    /// <param name="reason">The reason for removal.</param>
    /// <param name="newParticleCount">The new particle count.</param>
    /// <param name="wasDecay">Whether this was due to decay.</param>
    /// <param name="wasInteraction">Whether this was due to interaction.</param>
    public ParticleRemovedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        Guid particleId,
        string particleType,
        string reason,
        int newParticleCount,
        bool wasDecay = false,
        bool wasInteraction = false)
        : base(simulationId, simulationTime, simulationStep)
    {
        ParticleId = particleId;
        ParticleType = particleType ?? throw new ArgumentNullException(nameof(particleType));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        NewParticleCount = newParticleCount;
        WasDecay = wasDecay;
        WasInteraction = wasInteraction;
    }
}

/// <summary>
/// Event arguments for observable measurements.
/// </summary>
public class ObservableMeasuredEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the name of the observable that was measured.
    /// </summary>
    public string ObservableName { get; }

    /// <summary>
    /// Gets the measured value.
    /// </summary>
    public double MeasuredValue { get; }

    /// <summary>
    /// Gets the uncertainty in the measurement.
    /// </summary>
    public double Uncertainty { get; }

    /// <summary>
    /// Gets the expectation value before measurement.
    /// </summary>
    public double ExpectationValueBefore { get; }

    /// <summary>
    /// Gets the expectation value after measurement.
    /// </summary>
    public double ExpectationValueAfter { get; }

    /// <summary>
    /// Gets the particles involved in the measurement.
    /// </summary>
    public IReadOnlyList<Guid> ParticleIds { get; }

    /// <summary>
    /// Gets whether the measurement caused state collapse.
    /// </summary>
    public bool CausedStateCollapse { get; }

    /// <summary>
    /// Gets the probability of obtaining this measurement result.
    /// </summary>
    public double MeasurementProbability { get; }

    /// <summary>
    /// Gets the measurement operator used.
    /// </summary>
    public string OperatorDescription { get; }

    /// <summary>
    /// Initializes a new instance of the ObservableMeasuredEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="observableName">The observable name.</param>
    /// <param name="measuredValue">The measured value.</param>
    /// <param name="uncertainty">The measurement uncertainty.</param>
    /// <param name="expectationValueBefore">The expectation value before measurement.</param>
    /// <param name="expectationValueAfter">The expectation value after measurement.</param>
    /// <param name="particleIds">The particle IDs involved.</param>
    /// <param name="causedStateCollapse">Whether state collapse occurred.</param>
    /// <param name="measurementProbability">The measurement probability.</param>
    /// <param name="operatorDescription">Description of the measurement operator.</param>
    public ObservableMeasuredEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        string observableName,
        double measuredValue,
        double uncertainty,
        double expectationValueBefore,
        double expectationValueAfter,
        IEnumerable<Guid> particleIds,
        bool causedStateCollapse,
        double measurementProbability,
        string operatorDescription = "")
        : base(simulationId, simulationTime, simulationStep)
    {
        ObservableName = observableName ?? throw new ArgumentNullException(nameof(observableName));
        MeasuredValue = measuredValue;
        Uncertainty = uncertainty;
        ExpectationValueBefore = expectationValueBefore;
        ExpectationValueAfter = expectationValueAfter;
        ParticleIds = (particleIds ?? throw new ArgumentNullException(nameof(particleIds))).ToList().AsReadOnly();
        CausedStateCollapse = causedStateCollapse;
        MeasurementProbability = measurementProbability;
        OperatorDescription = operatorDescription ?? string.Empty;
    }
}

/// <summary>
/// Enumeration of error severity levels.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational message (not really an error).
    /// </summary>
    Info,

    /// <summary>
    /// Warning that doesn't prevent simulation from continuing.
    /// </summary>
    Warning,

    /// <summary>
    /// Minor error that may affect results but allows continuation.
    /// </summary>
    Minor,

    /// <summary>
    /// Major error that significantly affects results.
    /// </summary>
    Major,

    /// <summary>
    /// Critical error that prevents simulation from continuing.
    /// </summary>
    Critical,

    /// <summary>
    /// Fatal error that causes immediate termination.
    /// </summary>
    Fatal
}

#region Engine Event Arguments

/// <summary>
/// Event arguments for simulation engine status changes.
/// </summary>
public class EngineStatusChangedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the unique identifier of the engine.
    /// </summary>
    public Guid EngineId { get; }

    /// <summary>
    /// Gets the previous status of the engine.
    /// </summary>
    public object PreviousStatus { get; }

    /// <summary>
    /// Gets the new status of the engine.
    /// </summary>
    public object NewStatus { get; }

    /// <summary>
    /// Gets the reason for the status change.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the engine name.
    /// </summary>
    public string EngineName { get; }

    /// <summary>
    /// Initializes a new instance of the EngineStatusChangedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="engineId">The engine identifier.</param>
    /// <param name="engineName">The engine name.</param>
    /// <param name="previousStatus">The previous status.</param>
    /// <param name="newStatus">The new status.</param>
    /// <param name="reason">The reason for the change.</param>
    public EngineStatusChangedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        Guid engineId,
        string engineName,
        object previousStatus,
        object newStatus,
        string reason)
        : base(simulationId, simulationTime, simulationStep)
    {
        EngineId = engineId;
        EngineName = engineName ?? throw new ArgumentNullException(nameof(engineName));
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }
}

/// <summary>
/// Event arguments for simulation step completion.
/// </summary>
public class SimulationStepCompletedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the time step that was executed.
    /// </summary>
    public double TimeStep { get; }

    /// <summary>
    /// Gets the execution time for this step in milliseconds.
    /// </summary>
    public double ExecutionTimeMs { get; }

    /// <summary>
    /// Gets the number of particles evolved in this step.
    /// </summary>
    public int ParticleCount { get; }

    /// <summary>
    /// Gets the total energy after this step.
    /// </summary>
    public double TotalEnergy { get; }

    /// <summary>
    /// Gets the energy conservation error for this step.
    /// </summary>
    public double EnergyError { get; }

    /// <summary>
    /// Gets whether this step was successful.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets any warnings generated during this step.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>
    /// Gets performance metrics for this step.
    /// </summary>
    public Dictionary<string, double> PerformanceMetrics { get; }

    /// <summary>
    /// Initializes a new instance of the SimulationStepCompletedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="timeStep">The time step executed.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="particleCount">The particle count.</param>
    /// <param name="totalEnergy">The total energy.</param>
    /// <param name="energyError">The energy conservation error.</param>
    /// <param name="isSuccessful">Whether the step was successful.</param>
    /// <param name="warnings">Any warnings generated.</param>
    /// <param name="performanceMetrics">Performance metrics.</param>
    public SimulationStepCompletedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        double timeStep,
        double executionTimeMs,
        int particleCount,
        double totalEnergy,
        double energyError,
        bool isSuccessful,
        IEnumerable<string>? warnings = null,
        Dictionary<string, double>? performanceMetrics = null)
        : base(simulationId, simulationTime, simulationStep)
    {
        TimeStep = timeStep;
        ExecutionTimeMs = executionTimeMs;
        ParticleCount = particleCount;
        TotalEnergy = totalEnergy;
        EnergyError = energyError;
        IsSuccessful = isSuccessful;
        Warnings = warnings?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        PerformanceMetrics = performanceMetrics ?? new Dictionary<string, double>();
    }
}

/// <summary>
/// Event arguments for performance metrics updates.
/// </summary>
public class PerformanceMetricsUpdatedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the current steps per second rate.
    /// </summary>
    public double StepsPerSecond { get; }

    /// <summary>
    /// Gets the average step execution time in milliseconds.
    /// </summary>
    public double AverageStepTimeMs { get; }

    /// <summary>
    /// Gets the current memory usage in MB.
    /// </summary>
    public double MemoryUsageMB { get; }

    /// <summary>
    /// Gets the CPU usage percentage.
    /// </summary>
    public double CpuUsagePercent { get; }

    /// <summary>
    /// Gets the efficiency rating (0.0 to 1.0).
    /// </summary>
    public double Efficiency { get; }

    /// <summary>
    /// Gets the number of active threads.
    /// </summary>
    public int ActiveThreads { get; }

    /// <summary>
    /// Gets the FLOPS (floating point operations per second).
    /// </summary>
    public double FLOPS { get; }

    /// <summary>
    /// Gets additional performance metrics.
    /// </summary>
    public Dictionary<string, double> AdditionalMetrics { get; }

    /// <summary>
    /// Gets performance recommendations.
    /// </summary>
    public IReadOnlyList<string> Recommendations { get; }

    /// <summary>
    /// Initializes a new instance of the PerformanceMetricsUpdatedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="stepsPerSecond">The steps per second rate.</param>
    /// <param name="averageStepTimeMs">The average step time in milliseconds.</param>
    /// <param name="memoryUsageMB">The memory usage in MB.</param>
    /// <param name="cpuUsagePercent">The CPU usage percentage.</param>
    /// <param name="efficiency">The efficiency rating.</param>
    /// <param name="activeThreads">The number of active threads.</param>
    /// <param name="flops">The FLOPS rate.</param>
    /// <param name="additionalMetrics">Additional metrics.</param>
    /// <param name="recommendations">Performance recommendations.</param>
    public PerformanceMetricsUpdatedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        double stepsPerSecond,
        double averageStepTimeMs,
        double memoryUsageMB,
        double cpuUsagePercent,
        double efficiency,
        int activeThreads,
        double flops,
        Dictionary<string, double>? additionalMetrics = null,
        IEnumerable<string>? recommendations = null)
        : base(simulationId, simulationTime, simulationStep)
    {
        StepsPerSecond = stepsPerSecond;
        AverageStepTimeMs = averageStepTimeMs;
        MemoryUsageMB = memoryUsageMB;
        CpuUsagePercent = cpuUsagePercent;
        Efficiency = Math.Max(0, Math.Min(1, efficiency));
        ActiveThreads = activeThreads;
        FLOPS = flops;
        AdditionalMetrics = additionalMetrics ?? new Dictionary<string, double>();
        Recommendations = recommendations?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
    }
}

/// <summary>
/// Event arguments for engine errors.
/// </summary>
public class EngineErrorEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the engine identifier where the error occurred.
    /// </summary>
    public Guid EngineId { get; }

    /// <summary>
    /// Gets the engine name where the error occurred.
    /// </summary>
    public string EngineName { get; }

    /// <summary>
    /// Gets the error that occurred.
    /// </summary>
    public Exception Error { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the severity of the error.
    /// </summary>
    public ErrorSeverity Severity { get; }

    /// <summary>
    /// Gets whether the error is recoverable.
    /// </summary>
    public bool IsRecoverable { get; }

    /// <summary>
    /// Gets the component that generated the error.
    /// </summary>
    public string Component { get; }

    /// <summary>
    /// Gets additional error context.
    /// </summary>
    public Dictionary<string, object> ErrorContext { get; }

    /// <summary>
    /// Gets whether recovery was attempted.
    /// </summary>
    public bool RecoveryAttempted { get; }

    /// <summary>
    /// Gets whether recovery was successful.
    /// </summary>
    public bool RecoverySuccessful { get; }

    /// <summary>
    /// Initializes a new instance of the EngineErrorEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="engineId">The engine identifier.</param>
    /// <param name="engineName">The engine name.</param>
    /// <param name="error">The error that occurred.</param>
    /// <param name="severity">The error severity.</param>
    /// <param name="isRecoverable">Whether the error is recoverable.</param>
    /// <param name="component">The component that generated the error.</param>
    /// <param name="recoveryAttempted">Whether recovery was attempted.</param>
    /// <param name="recoverySuccessful">Whether recovery was successful.</param>
    public EngineErrorEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        Guid engineId,
        string engineName,
        Exception error,
        ErrorSeverity severity,
        bool isRecoverable,
        string component = "",
        bool recoveryAttempted = false,
        bool recoverySuccessful = false)
        : base(simulationId, simulationTime, simulationStep)
    {
        EngineId = engineId;
        EngineName = engineName ?? throw new ArgumentNullException(nameof(engineName));
        Error = error ?? throw new ArgumentNullException(nameof(error));
        ErrorMessage = error.Message;
        Severity = severity;
        IsRecoverable = isRecoverable;
        Component = component ?? string.Empty;
        ErrorContext = new Dictionary<string, object>();
        RecoveryAttempted = recoveryAttempted;
        RecoverySuccessful = recoverySuccessful;
    }
}

/// <summary>
/// Event arguments for convergence detection.
/// </summary>
public class ConvergenceDetectedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the type of convergence detected.
    /// </summary>
    public ConvergenceType ConvergenceType { get; }

    /// <summary>
    /// Gets the convergence tolerance used.
    /// </summary>
    public double Tolerance { get; }

    /// <summary>
    /// Gets the actual convergence error achieved.
    /// </summary>
    public double ActualError { get; }

    /// <summary>
    /// Gets the convergence criteria that was met.
    /// </summary>
    public string ConvergenceCriteria { get; }

    /// <summary>
    /// Gets the number of steps taken to reach convergence.
    /// </summary>
    public long StepsToConvergence { get; }

    /// <summary>
    /// Gets the time taken to reach convergence.
    /// </summary>
    public TimeSpan TimeToConvergence { get; }

    /// <summary>
    /// Gets the final converged value.
    /// </summary>
    public double ConvergedValue { get; }

    /// <summary>
    /// Gets additional convergence metrics.
    /// </summary>
    public Dictionary<string, double> ConvergenceMetrics { get; }

    /// <summary>
    /// Initializes a new instance of the ConvergenceDetectedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="convergenceType">The type of convergence.</param>
    /// <param name="tolerance">The convergence tolerance.</param>
    /// <param name="actualError">The actual error achieved.</param>
    /// <param name="convergenceCriteria">The convergence criteria.</param>
    /// <param name="stepsToConvergence">The steps to convergence.</param>
    /// <param name="timeToConvergence">The time to convergence.</param>
    /// <param name="convergedValue">The final converged value.</param>
    /// <param name="convergenceMetrics">Additional convergence metrics.</param>
    public ConvergenceDetectedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        ConvergenceType convergenceType,
        double tolerance,
        double actualError,
        string convergenceCriteria,
        long stepsToConvergence,
        TimeSpan timeToConvergence,
        double convergedValue,
        Dictionary<string, double>? convergenceMetrics = null)
        : base(simulationId, simulationTime, simulationStep)
    {
        ConvergenceType = convergenceType;
        Tolerance = tolerance;
        ActualError = actualError;
        ConvergenceCriteria = convergenceCriteria ?? throw new ArgumentNullException(nameof(convergenceCriteria));
        StepsToConvergence = stepsToConvergence;
        TimeToConvergence = timeToConvergence;
        ConvergedValue = convergedValue;
        ConvergenceMetrics = convergenceMetrics ?? new Dictionary<string, double>();
    }
}

/// <summary>
/// Enumeration of convergence types.
/// </summary>
public enum ConvergenceType
{
    /// <summary>
    /// Energy convergence.
    /// </summary>
    Energy,

    /// <summary>
    /// State convergence.
    /// </summary>
    State,

    /// <summary>
    /// Observable convergence.
    /// </summary>
    Observable,

    /// <summary>
    /// Force convergence.
    /// </summary>
    Force,

    /// <summary>
    /// Gradient convergence.
    /// </summary>
    Gradient,

    /// <summary>
    /// Density convergence.
    /// </summary>
    Density,

    /// <summary>
    /// Custom convergence criteria.
    /// </summary>
    Custom
}

#endregion

#region State Evolution Event Arguments

/// <summary>
/// Event arguments for evolution engine status changes.
/// </summary>
public class EvolutionStatusChangedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the unique identifier of the evolution engine.
    /// </summary>
    public Guid EngineId { get; }

    /// <summary>
    /// Gets the engine name.
    /// </summary>
    public string EngineName { get; }

    /// <summary>
    /// Gets the previous status of the evolution engine.
    /// </summary>
    public object PreviousStatus { get; }

    /// <summary>
    /// Gets the new status of the evolution engine.
    /// </summary>
    public object NewStatus { get; }

    /// <summary>
    /// Gets the reason for the status change.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the current evolution method being used.
    /// </summary>
    public string EvolutionMethod { get; }

    /// <summary>
    /// Initializes a new instance of the EvolutionStatusChangedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="engineId">The engine identifier.</param>
    /// <param name="engineName">The engine name.</param>
    /// <param name="previousStatus">The previous status.</param>
    /// <param name="newStatus">The new status.</param>
    /// <param name="reason">The reason for the change.</param>
    /// <param name="evolutionMethod">The current evolution method.</param>
    public EvolutionStatusChangedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        Guid engineId,
        string engineName,
        object previousStatus,
        object newStatus,
        string reason,
        string evolutionMethod = "")
        : base(simulationId, simulationTime, simulationStep)
    {
        EngineId = engineId;
        EngineName = engineName ?? throw new ArgumentNullException(nameof(engineName));
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        EvolutionMethod = evolutionMethod ?? string.Empty;
    }
}

/// <summary>
/// Event arguments for state evolution completion.
/// </summary>
public class StateEvolutionCompletedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the time step that was evolved.
    /// </summary>
    public double TimeStep { get; }

    /// <summary>
    /// Gets the evolution method used.
    /// </summary>
    public string EvolutionMethod { get; }

    /// <summary>
    /// Gets the execution time for the evolution in milliseconds.
    /// </summary>
    public double ExecutionTimeMs { get; }

    /// <summary>
    /// Gets whether the evolution was successful.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the numerical error in the evolution.
    /// </summary>
    public double NumericalError { get; }

    /// <summary>
    /// Gets the unitarity error after evolution.
    /// </summary>
    public double UnitarityError { get; }

    /// <summary>
    /// Gets the norm preservation error.
    /// </summary>
    public double NormError { get; }

    /// <summary>
    /// Gets the energy conservation error.
    /// </summary>
    public double EnergyError { get; }

    /// <summary>
    /// Gets the number of particles evolved.
    /// </summary>
    public int ParticleCount { get; }

    /// <summary>
    /// Gets any warnings generated during evolution.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>
    /// Gets performance metrics for the evolution.
    /// </summary>
    public Dictionary<string, double> PerformanceMetrics { get; }

    /// <summary>
    /// Initializes a new instance of the StateEvolutionCompletedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="timeStep">The time step evolved.</param>
    /// <param name="evolutionMethod">The evolution method used.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="isSuccessful">Whether the evolution was successful.</param>
    /// <param name="numericalError">The numerical error.</param>
    /// <param name="unitarityError">The unitarity error.</param>
    /// <param name="normError">The norm preservation error.</param>
    /// <param name="energyError">The energy conservation error.</param>
    /// <param name="particleCount">The particle count.</param>
    /// <param name="warnings">Any warnings generated.</param>
    /// <param name="performanceMetrics">Performance metrics.</param>
    public StateEvolutionCompletedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        double timeStep,
        string evolutionMethod,
        double executionTimeMs,
        bool isSuccessful,
        double numericalError,
        double unitarityError,
        double normError,
        double energyError,
        int particleCount,
        IEnumerable<string>? warnings = null,
        Dictionary<string, double>? performanceMetrics = null)
        : base(simulationId, simulationTime, simulationStep)
    {
        TimeStep = timeStep;
        EvolutionMethod = evolutionMethod ?? throw new ArgumentNullException(nameof(evolutionMethod));
        ExecutionTimeMs = executionTimeMs;
        IsSuccessful = isSuccessful;
        NumericalError = numericalError;
        UnitarityError = unitarityError;
        NormError = normError;
        EnergyError = energyError;
        ParticleCount = particleCount;
        Warnings = warnings?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        PerformanceMetrics = performanceMetrics ?? new Dictionary<string, double>();
    }
}

/// <summary>
/// Event arguments for measurement operations.
/// </summary>
public class MeasurementPerformedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the measurement operator used.
    /// </summary>
    public string MeasurementOperator { get; }

    /// <summary>
    /// Gets the measured eigenvalue.
    /// </summary>
    public Complex MeasuredValue { get; }

    /// <summary>
    /// Gets the probability of this measurement result.
    /// </summary>
    public double Probability { get; }

    /// <summary>
    /// Gets the measurement uncertainty.
    /// </summary>
    public double Uncertainty { get; }

    /// <summary>
    /// Gets whether the measurement caused state collapse.
    /// </summary>
    public bool CausedStateCollapse { get; }

    /// <summary>
    /// Gets the particles involved in the measurement.
    /// </summary>
    public IReadOnlyList<Guid> ParticleIds { get; }

    /// <summary>
    /// Gets the entropy before measurement.
    /// </summary>
    public double EntropyBefore { get; }

    /// <summary>
    /// Gets the entropy after measurement.
    /// </summary>
    public double EntropyAfter { get; }

    /// <summary>
    /// Gets additional measurement metadata.
    /// </summary>
    public Dictionary<string, object> MeasurementMetadata { get; }

    /// <summary>
    /// Initializes a new instance of the MeasurementPerformedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="measurementOperator">The measurement operator.</param>
    /// <param name="measuredValue">The measured value.</param>
    /// <param name="probability">The measurement probability.</param>
    /// <param name="uncertainty">The measurement uncertainty.</param>
    /// <param name="causedStateCollapse">Whether state collapse occurred.</param>
    /// <param name="particleIds">The particle IDs involved.</param>
    /// <param name="entropyBefore">The entropy before measurement.</param>
    /// <param name="entropyAfter">The entropy after measurement.</param>
    /// <param name="measurementMetadata">Additional measurement metadata.</param>
    public MeasurementPerformedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        string measurementOperator,
        Complex measuredValue,
        double probability,
        double uncertainty,
        bool causedStateCollapse,
        IEnumerable<Guid> particleIds,
        double entropyBefore,
        double entropyAfter,
        Dictionary<string, object>? measurementMetadata = null)
        : base(simulationId, simulationTime, simulationStep)
    {
        MeasurementOperator = measurementOperator ?? throw new ArgumentNullException(nameof(measurementOperator));
        MeasuredValue = measuredValue;
        Probability = probability;
        Uncertainty = uncertainty;
        CausedStateCollapse = causedStateCollapse;
        ParticleIds = (particleIds ?? throw new ArgumentNullException(nameof(particleIds))).ToList().AsReadOnly();
        EntropyBefore = entropyBefore;
        EntropyAfter = entropyAfter;
        MeasurementMetadata = measurementMetadata ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Event arguments for numerical error detection.
/// </summary>
public class NumericalErrorDetectedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the type of numerical error detected.
    /// </summary>
    public NumericalErrorType ErrorType { get; }

    /// <summary>
    /// Gets the magnitude of the error.
    /// </summary>
    public double ErrorMagnitude { get; }

    /// <summary>
    /// Gets the threshold that was exceeded.
    /// </summary>
    public double Threshold { get; }

    /// <summary>
    /// Gets the component where the error was detected.
    /// </summary>
    public string Component { get; }

    /// <summary>
    /// Gets whether correction was attempted.
    /// </summary>
    public bool CorrectionAttempted { get; }

    /// <summary>
    /// Gets whether correction was successful.
    /// </summary>
    public bool CorrectionSuccessful { get; }

    /// <summary>
    /// Gets the error description.
    /// </summary>
    public string ErrorDescription { get; }

    /// <summary>
    /// Gets recommendations for addressing the error.
    /// </summary>
    public IReadOnlyList<string> Recommendations { get; }

    /// <summary>
    /// Gets additional error details.
    /// </summary>
    public Dictionary<string, object> ErrorDetails { get; }

    /// <summary>
    /// Initializes a new instance of the NumericalErrorDetectedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="errorType">The type of error.</param>
    /// <param name="errorMagnitude">The error magnitude.</param>
    /// <param name="threshold">The threshold exceeded.</param>
    /// <param name="component">The component where error occurred.</param>
    /// <param name="correctionAttempted">Whether correction was attempted.</param>
    /// <param name="correctionSuccessful">Whether correction was successful.</param>
    /// <param name="errorDescription">The error description.</param>
    /// <param name="recommendations">Recommendations for addressing the error.</param>
    /// <param name="errorDetails">Additional error details.</param>
    public NumericalErrorDetectedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        NumericalErrorType errorType,
        double errorMagnitude,
        double threshold,
        string component,
        bool correctionAttempted,
        bool correctionSuccessful,
        string errorDescription,
        IEnumerable<string>? recommendations = null,
        Dictionary<string, object>? errorDetails = null)
        : base(simulationId, simulationTime, simulationStep)
    {
        ErrorType = errorType;
        ErrorMagnitude = errorMagnitude;
        Threshold = threshold;
        Component = component ?? throw new ArgumentNullException(nameof(component));
        CorrectionAttempted = correctionAttempted;
        CorrectionSuccessful = correctionSuccessful;
        ErrorDescription = errorDescription ?? throw new ArgumentNullException(nameof(errorDescription));
        Recommendations = recommendations?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        ErrorDetails = errorDetails ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Event arguments for entanglement changes.
/// </summary>
public class EntanglementChangedEventArgs : SimulationEventArgs
{
    /// <summary>
    /// Gets the type of entanglement change.
    /// </summary>
    public EntanglementChangeType ChangeType { get; }

    /// <summary>
    /// Gets the first particle involved.
    /// </summary>
    public Guid Particle1Id { get; }

    /// <summary>
    /// Gets the second particle involved.
    /// </summary>
    public Guid Particle2Id { get; }

    /// <summary>
    /// Gets the entanglement strength before the change.
    /// </summary>
    public double EntanglementBefore { get; }

    /// <summary>
    /// Gets the entanglement strength after the change.
    /// </summary>
    public double EntanglementAfter { get; }

    /// <summary>
    /// Gets the Bell state type for the entanglement.
    /// </summary>
    public string BellStateType { get; }

    /// <summary>
    /// Gets the decoherence time for the entanglement.
    /// </summary>
    public double DecoherenceTime { get; }

    /// <summary>
    /// Gets the reason for the entanglement change.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets additional entanglement metadata.
    /// </summary>
    public Dictionary<string, object> EntanglementMetadata { get; }

    /// <summary>
    /// Initializes a new instance of the EntanglementChangedEventArgs class.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationTime">The simulation time.</param>
    /// <param name="simulationStep">The simulation step.</param>
    /// <param name="changeType">The type of change.</param>
    /// <param name="particle1Id">The first particle ID.</param>
    /// <param name="particle2Id">The second particle ID.</param>
    /// <param name="entanglementBefore">The entanglement strength before.</param>
    /// <param name="entanglementAfter">The entanglement strength after.</param>
    /// <param name="bellStateType">The Bell state type.</param>
    /// <param name="decoherenceTime">The decoherence time.</param>
    /// <param name="reason">The reason for the change.</param>
    /// <param name="entanglementMetadata">Additional metadata.</param>
    public EntanglementChangedEventArgs(
        Guid simulationId,
        double simulationTime,
        long simulationStep,
        EntanglementChangeType changeType,
        Guid particle1Id,
        Guid particle2Id,
        double entanglementBefore,
        double entanglementAfter,
        string bellStateType,
        double decoherenceTime,
        string reason,
        Dictionary<string, object>? entanglementMetadata = null)
        : base(simulationId, simulationTime, simulationStep)
    {
        ChangeType = changeType;
        Particle1Id = particle1Id;
        Particle2Id = particle2Id;
        EntanglementBefore = entanglementBefore;
        EntanglementAfter = entanglementAfter;
        BellStateType = bellStateType ?? throw new ArgumentNullException(nameof(bellStateType));
        DecoherenceTime = decoherenceTime;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        EntanglementMetadata = entanglementMetadata ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Enumeration of numerical error types.
/// </summary>
public enum NumericalErrorType
{
    /// <summary>
    /// State normalization error.
    /// </summary>
    NormalizationError,

    /// <summary>
    /// Unitarity preservation error.
    /// </summary>
    UnitarityError,

    /// <summary>
    /// Energy conservation error.
    /// </summary>
    EnergyConservationError,

    /// <summary>
    /// Matrix conditioning error.
    /// </summary>
    MatrixConditioningError,

    /// <summary>
    /// Numerical overflow.
    /// </summary>
    NumericalOverflow,

    /// <summary>
    /// Numerical underflow.
    /// </summary>
    NumericalUnderflow,

    /// <summary>
    /// Convergence failure.
    /// </summary>
    ConvergenceFailure,

    /// <summary>
    /// Precision loss.
    /// </summary>
    PrecisionLoss,

    /// <summary>
    /// Accumulated rounding error.
    /// </summary>
    AccumulatedRoundingError
}

/// <summary>
/// Enumeration of entanglement change types.
/// </summary>
public enum EntanglementChangeType
{
    /// <summary>
    /// New entanglement created.
    /// </summary>
    Created,

    /// <summary>
    /// Existing entanglement strengthened.
    /// </summary>
    Strengthened,

    /// <summary>
    /// Existing entanglement weakened.
    /// </summary>
    Weakened,

    /// <summary>
    /// Entanglement broken/destroyed.
    /// </summary>
    Broken,

    /// <summary>
    /// Entanglement underwent decoherence.
    /// </summary>
    Decoherence,

    /// <summary>
    /// Entanglement measured and collapsed.
    /// </summary>
    Measured
}

#endregion
