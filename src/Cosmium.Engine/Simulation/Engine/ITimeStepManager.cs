using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Simulation.Core;

namespace Cosmium.Engine.Simulation.Engine;

/// <summary>
/// Interface for managing time evolution and time stepping in quantum mechanical simulations.
/// Handles both fixed and adaptive time stepping algorithms with stability analysis.
/// </summary>
public interface ITimeStepManager
{
    #region Properties

    /// <summary>
    /// Gets the current time step being used.
    /// </summary>
    double CurrentTimeStep { get; }

    /// <summary>
    /// Gets the minimum allowed time step.
    /// </summary>
    double MinTimeStep { get; }

    /// <summary>
    /// Gets the maximum allowed time step.
    /// </summary>
    double MaxTimeStep { get; }

    /// <summary>
    /// Gets whether adaptive time stepping is enabled.
    /// </summary>
    bool IsAdaptiveSteppingEnabled { get; }

    /// <summary>
    /// Gets the current error tolerance for adaptive stepping.
    /// </summary>
    double ErrorTolerance { get; }

    /// <summary>
    /// Gets the stability factor for the current time step.
    /// </summary>
    double StabilityFactor { get; }

    /// <summary>
    /// Gets whether the current time step is stable.
    /// </summary>
    bool IsCurrentStepStable { get; }

    /// <summary>
    /// Gets the total number of time step adjustments made.
    /// </summary>
    long TotalAdjustments { get; }

    /// <summary>
    /// Gets the number of time step increases.
    /// </summary>
    long StepIncreases { get; }

    /// <summary>
    /// Gets the number of time step decreases.
    /// </summary>
    long StepDecreases { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the time step manager with simulation parameters.
    /// </summary>
    /// <param name="parameters">The simulation parameters.</param>
    void Initialize(SimulationParameters parameters);

    /// <summary>
    /// Configures adaptive time stepping.
    /// </summary>
    /// <param name="enable">Whether to enable adaptive stepping.</param>
    /// <param name="errorTolerance">The error tolerance for adaptation.</param>
    /// <param name="minTimeStep">The minimum allowed time step.</param>
    /// <param name="maxTimeStep">The maximum allowed time step.</param>
    void ConfigureAdaptiveStepping(bool enable, double errorTolerance, double minTimeStep, double maxTimeStep);

    /// <summary>
    /// Sets a fixed time step value.
    /// </summary>
    /// <param name="timeStep">The fixed time step to use.</param>
    void SetFixedTimeStep(double timeStep);

    #endregion

    #region Time Step Calculation

    /// <summary>
    /// Calculates the optimal time step for the current system state.
    /// </summary>
    /// <param name="particles">The particles in the system.</param>
    /// <param name="currentTime">The current simulation time.</param>
    /// <returns>The recommended time step.</returns>
    double CalculateOptimalTimeStep(IReadOnlyList<IQuantumParticle> particles, double currentTime);

    /// <summary>
    /// Determines the time step based on stability criteria.
    /// </summary>
    /// <param name="particles">The particles in the system.</param>
    /// <param name="maxEigenvalue">The maximum eigenvalue of the system Hamiltonian.</param>
    /// <returns>The stable time step.</returns>
    double CalculateStableTimeStep(IReadOnlyList<IQuantumParticle> particles, double maxEigenvalue);

    /// <summary>
    /// Calculates time step based on CFL (Courant-Friedrichs-Lewy) condition.
    /// </summary>
    /// <param name="maxVelocity">The maximum velocity in the system.</param>
    /// <param name="minSpatialStep">The minimum spatial discretization step.</param>
    /// <returns>The CFL-limited time step.</returns>
    double CalculateCFLTimeStep(double maxVelocity, double minSpatialStep);

    /// <summary>
    /// Estimates the local error for adaptive stepping.
    /// </summary>
    /// <param name="particles">The particles in the system.</param>
    /// <param name="proposedTimeStep">The proposed time step.</param>
    /// <returns>The estimated local truncation error.</returns>
    double EstimateLocalError(IReadOnlyList<IQuantumParticle> particles, double proposedTimeStep);

    #endregion

    #region Time Step Adaptation

    /// <summary>
    /// Updates the time step based on the current system state and error analysis.
    /// </summary>
    /// <param name="particles">The particles in the system.</param>
    /// <param name="localError">The local error from the previous step.</param>
    /// <param name="currentTime">The current simulation time.</param>
    /// <returns>True if the time step was adjusted.</returns>
    bool UpdateTimeStep(IReadOnlyList<IQuantumParticle> particles, double localError, double currentTime);

    /// <summary>
    /// Suggests a new time step based on error analysis.
    /// </summary>
    /// <param name="currentTimeStep">The current time step.</param>
    /// <param name="localError">The local error.</param>
    /// <param name="targetError">The target error tolerance.</param>
    /// <returns>The suggested new time step.</returns>
    double SuggestNewTimeStep(double currentTimeStep, double localError, double targetError);

    /// <summary>
    /// Forces a time step increase if conditions allow.
    /// </summary>
    /// <param name="factor">The factor by which to increase (e.g., 1.5).</param>
    /// <returns>True if the increase was applied.</returns>
    bool IncreaseTimeStep(double factor = 1.5);

    /// <summary>
    /// Forces a time step decrease for stability.
    /// </summary>
    /// <param name="factor">The factor by which to decrease (e.g., 0.5).</param>
    /// <returns>True if the decrease was applied.</returns>
    bool DecreaseTimeStep(double factor = 0.5);

    #endregion

    #region Stability Analysis

    /// <summary>
    /// Performs stability analysis for the current time step.
    /// </summary>
    /// <param name="particles">The particles in the system.</param>
    /// <returns>Stability analysis result.</returns>
    TimeStepStabilityResult AnalyzeStability(IReadOnlyList<IQuantumParticle> particles);

    /// <summary>
    /// Checks if the proposed time step satisfies all stability criteria.
    /// </summary>
    /// <param name="particles">The particles in the system.</param>
    /// <param name="proposedTimeStep">The proposed time step.</param>
    /// <returns>True if the time step is stable.</returns>
    bool IsTimeStepStable(IReadOnlyList<IQuantumParticle> particles, double proposedTimeStep);

    /// <summary>
    /// Gets the maximum stable time step for the current system.
    /// </summary>
    /// <param name="particles">The particles in the system.</param>
    /// <returns>The maximum stable time step.</returns>
    double GetMaxStableTimeStep(IReadOnlyList<IQuantumParticle> particles);

    #endregion

    #region Validation

    /// <summary>
    /// Validates the current time step configuration.
    /// </summary>
    /// <returns>Validation result.</returns>
    ValidationResult ValidateConfiguration();

    /// <summary>
    /// Validates a proposed time step value.
    /// </summary>
    /// <param name="timeStep">The time step to validate.</param>
    /// <returns>Validation result.</returns>
    ValidationResult ValidateTimeStep(double timeStep);

    #endregion

    #region Statistics and Diagnostics

    /// <summary>
    /// Gets statistics about time step usage and adaptation.
    /// </summary>
    /// <returns>Time step statistics.</returns>
    TimeStepStatistics GetStatistics();

    /// <summary>
    /// Resets all time step statistics.
    /// </summary>
    void ResetStatistics();

    /// <summary>
    /// Gets diagnostic information about the current time stepping state.
    /// </summary>
    /// <returns>Diagnostic information.</returns>
    Dictionary<string, object> GetDiagnostics();

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the time step is adjusted.
    /// </summary>
    event EventHandler<TimeStepAdjustedEventArgs>? TimeStepAdjusted;

    /// <summary>
    /// Event raised when stability issues are detected.
    /// </summary>
    event EventHandler<StabilityIssueDetectedEventArgs>? StabilityIssueDetected;

    #endregion
}

/// <summary>
/// Result of time step stability analysis.
/// </summary>
public class TimeStepStabilityResult
{
    /// <summary>
    /// Gets whether the time step is stable.
    /// </summary>
    public bool IsStable { get; init; }

    /// <summary>
    /// Gets the stability factor (>1.0 indicates stability margin).
    /// </summary>
    public double StabilityFactor { get; init; }

    /// <summary>
    /// Gets the maximum eigenvalue that determines stability.
    /// </summary>
    public double MaxEigenvalue { get; init; }

    /// <summary>
    /// Gets the CFL number for the current configuration.
    /// </summary>
    public double CFLNumber { get; init; }

    /// <summary>
    /// Gets any stability warnings or recommendations.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the recommended time step for stability.
    /// </summary>
    public double RecommendedTimeStep { get; init; }
}

/// <summary>
/// Statistics about time step usage and adaptation.
/// </summary>
public class TimeStepStatistics
{
    /// <summary>
    /// Gets the total number of time steps taken.
    /// </summary>
    public long TotalSteps { get; init; }

    /// <summary>
    /// Gets the average time step used.
    /// </summary>
    public double AverageTimeStep { get; init; }

    /// <summary>
    /// Gets the minimum time step used.
    /// </summary>
    public double MinTimeStepUsed { get; init; }

    /// <summary>
    /// Gets the maximum time step used.
    /// </summary>
    public double MaxTimeStepUsed { get; init; }

    /// <summary>
    /// Gets the total number of time step adjustments.
    /// </summary>
    public long TotalAdjustments { get; init; }

    /// <summary>
    /// Gets the number of increases.
    /// </summary>
    public long Increases { get; init; }

    /// <summary>
    /// Gets the number of decreases.
    /// </summary>
    public long Decreases { get; init; }

    /// <summary>
    /// Gets the number of rejected steps due to instability.
    /// </summary>
    public long RejectedSteps { get; init; }

    /// <summary>
    /// Gets the efficiency ratio (accepted/total steps).
    /// </summary>
    public double Efficiency { get; init; }

    /// <summary>
    /// Gets the time spent in time step calculations.
    /// </summary>
    public TimeSpan TimeStepCalculationTime { get; init; }
}

/// <summary>
/// Event arguments for time step adjustments.
/// </summary>
public class TimeStepAdjustedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous time step.
    /// </summary>
    public double PreviousTimeStep { get; }

    /// <summary>
    /// Gets the new time step.
    /// </summary>
    public double NewTimeStep { get; }

    /// <summary>
    /// Gets the reason for the adjustment.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the local error that triggered the adjustment.
    /// </summary>
    public double LocalError { get; }

    /// <summary>
    /// Gets the simulation time when the adjustment occurred.
    /// </summary>
    public double SimulationTime { get; }

    /// <summary>
    /// Initializes a new instance of the TimeStepAdjustedEventArgs class.
    /// </summary>
    /// <param name="previousTimeStep">The previous time step.</param>
    /// <param name="newTimeStep">The new time step.</param>
    /// <param name="reason">The reason for adjustment.</param>
    /// <param name="localError">The local error.</param>
    /// <param name="simulationTime">The simulation time.</param>
    public TimeStepAdjustedEventArgs(double previousTimeStep, double newTimeStep, string reason, double localError, double simulationTime)
    {
        PreviousTimeStep = previousTimeStep;
        NewTimeStep = newTimeStep;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        LocalError = localError;
        SimulationTime = simulationTime;
    }
}

/// <summary>
/// Event arguments for stability issue detection.
/// </summary>
public class StabilityIssueDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the severity of the stability issue.
    /// </summary>
    public StabilityIssueSeverity Severity { get; }

    /// <summary>
    /// Gets the description of the stability issue.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the current time step causing the issue.
    /// </summary>
    public double CurrentTimeStep { get; }

    /// <summary>
    /// Gets the recommended time step for stability.
    /// </summary>
    public double RecommendedTimeStep { get; }

    /// <summary>
    /// Gets the stability factor.
    /// </summary>
    public double StabilityFactor { get; }

    /// <summary>
    /// Gets the simulation time when the issue was detected.
    /// </summary>
    public double SimulationTime { get; }

    /// <summary>
    /// Initializes a new instance of the StabilityIssueDetectedEventArgs class.
    /// </summary>
    /// <param name="severity">The issue severity.</param>
    /// <param name="description">The issue description.</param>
    /// <param name="currentTimeStep">The current time step.</param>
    /// <param name="recommendedTimeStep">The recommended time step.</param>
    /// <param name="stabilityFactor">The stability factor.</param>
    /// <param name="simulationTime">The simulation time.</param>
    public StabilityIssueDetectedEventArgs(StabilityIssueSeverity severity, string description, double currentTimeStep, double recommendedTimeStep, double stabilityFactor, double simulationTime)
    {
        Severity = severity;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        CurrentTimeStep = currentTimeStep;
        RecommendedTimeStep = recommendedTimeStep;
        StabilityFactor = stabilityFactor;
        SimulationTime = simulationTime;
    }
}

/// <summary>
/// Severity levels for stability issues.
/// </summary>
public enum StabilityIssueSeverity
{
    /// <summary>
    /// Minor stability concern.
    /// </summary>
    Minor,

    /// <summary>
    /// Moderate stability issue.
    /// </summary>
    Moderate,

    /// <summary>
    /// Serious stability problem.
    /// </summary>
    Serious,

    /// <summary>
    /// Critical stability failure.
    /// </summary>
    Critical
}
