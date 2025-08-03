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
