using Cosmium.Engine.Physics.Mathematics;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.States;
using Cosmium.Engine.Simulation.Core;
using Cosmium.Engine.Simulation.Events;

namespace Cosmium.Engine.Simulation.Engine;

/// <summary>
/// Interface for quantum state evolution engine that handles time evolution of quantum systems.
/// Implements quantum mechanical operators, unitary evolution, and measurement collapse.
/// </summary>
public interface IStateEvolutionEngine
{
    #region Basic Properties

    /// <summary>
    /// Gets the unique identifier for this state evolution engine.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the state evolution engine.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the current status of the evolution engine.
    /// </summary>
    EvolutionEngineStatus Status { get; }

    /// <summary>
    /// Gets the current quantum state being evolved.
    /// </summary>
    QuantumState? CurrentState { get; }

    /// <summary>
    /// Gets the current time evolution operator.
    /// </summary>
    Matrix? TimeEvolutionOperator { get; }

    /// <summary>
    /// Gets the Hamiltonian operator for the current system.
    /// </summary>
    Matrix? Hamiltonian { get; }

    #endregion

    #region Configuration and Capabilities

    /// <summary>
    /// Gets whether the engine supports unitary evolution.
    /// </summary>
    bool SupportsUnitaryEvolution { get; }

    /// <summary>
    /// Gets whether the engine supports non-unitary evolution (decoherence).
    /// </summary>
    bool SupportsNonUnitaryEvolution { get; }

    /// <summary>
    /// Gets whether the engine supports measurement operations.
    /// </summary>
    bool SupportsMeasurement { get; }

    /// <summary>
    /// Gets whether the engine supports entanglement operations.
    /// </summary>
    bool SupportsEntanglement { get; }

    /// <summary>
    /// Gets the maximum number of qubits this engine can handle efficiently.
    /// </summary>
    int MaxQubitCount { get; }

    /// <summary>
    /// Gets the supported evolution methods.
    /// </summary>
    IReadOnlyList<EvolutionMethod> SupportedMethods { get; }

    #endregion

    #region Initialization and Configuration

    /// <summary>
    /// Initializes the state evolution engine with the specified parameters.
    /// </summary>
    /// <param name="parameters">The simulation parameters.</param>
    Task InitializeAsync(SimulationParameters parameters);

    /// <summary>
    /// Sets the Hamiltonian operator for time evolution.
    /// </summary>
    /// <param name="hamiltonian">The Hamiltonian matrix.</param>
    Task SetHamiltonianAsync(Matrix hamiltonian);

    /// <summary>
    /// Sets the initial quantum state for evolution.
    /// </summary>
    /// <param name="initialState">The initial quantum state.</param>
    Task SetInitialStateAsync(QuantumState initialState);

    /// <summary>
    /// Configures the evolution method to use.
    /// </summary>
    /// <param name="method">The evolution method.</param>
    /// <param name="parameters">Method-specific parameters.</param>
    Task ConfigureEvolutionMethodAsync(EvolutionMethod method, Dictionary<string, object>? parameters = null);

    #endregion

    #region State Evolution Operations

    /// <summary>
    /// Evolves the quantum state by a single time step.
    /// </summary>
    /// <param name="timeStep">The time step to evolve.</param>
    /// <returns>The evolved quantum state.</returns>
    Task<QuantumState> EvolveStateAsync(double timeStep);

    /// <summary>
    /// Evolves the quantum state by multiple time steps.
    /// </summary>
    /// <param name="timeStep">The time step size.</param>
    /// <param name="stepCount">The number of steps to evolve.</param>
    /// <returns>The final evolved quantum state.</returns>
    Task<QuantumState> EvolveStateAsync(double timeStep, int stepCount);

    /// <summary>
    /// Evolves the quantum state for a specified total time.
    /// </summary>
    /// <param name="totalTime">The total time to evolve.</param>
    /// <param name="timeStep">The time step size to use.</param>
    /// <returns>The final evolved quantum state.</returns>
    Task<QuantumState> EvolveForTimeAsync(double totalTime, double timeStep);

    /// <summary>
    /// Applies a unitary operator to the current state.
    /// </summary>
    /// <param name="unitaryOperator">The unitary operator to apply.</param>
    /// <returns>The resulting quantum state.</returns>
    Task<QuantumState> ApplyUnitaryOperatorAsync(Matrix unitaryOperator);

    /// <summary>
    /// Applies a measurement operation to the current state.
    /// </summary>
    /// <param name="measurementOperator">The measurement operator.</param>
    /// <returns>The measurement result and collapsed state.</returns>
    Task<MeasurementResult> ApplyMeasurementAsync(Matrix measurementOperator);

    #endregion

    #region Particle System Evolution

    /// <summary>
    /// Evolves a collection of quantum particles.
    /// </summary>
    /// <param name="particles">The particles to evolve.</param>
    /// <param name="timeStep">The time step for evolution.</param>
    /// <returns>The evolved particle states.</returns>
    Task<IReadOnlyList<IQuantumParticle>> EvolveParticlesAsync(
        IReadOnlyList<IQuantumParticle> particles, 
        double timeStep);

    /// <summary>
    /// Evolves particle interactions and entanglement.
    /// </summary>
    /// <param name="particles">The interacting particles.</param>
    /// <param name="interactions">The interaction terms.</param>
    /// <param name="timeStep">The time step for evolution.</param>
    /// <returns>The evolved particle system.</returns>
    Task<ParticleSystemState> EvolveInteractionsAsync(
        IReadOnlyList<IQuantumParticle> particles,
        Matrix interactions,
        double timeStep);

    /// <summary>
    /// Handles entanglement between particles during evolution.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="entanglementStrength">The strength of entanglement.</param>
    /// <returns>The entangled particle pair.</returns>
    Task<EntangledParticlePair> CreateEntanglementAsync(
        IQuantumParticle particle1,
        IQuantumParticle particle2,
        double entanglementStrength);

    #endregion

    #region Evolution Analysis and Monitoring

    /// <summary>
    /// Calculates the expectation value of an observable.
    /// </summary>
    /// <param name="observable">The observable operator.</param>
    /// <returns>The expectation value.</returns>
    Task<Complex> CalculateExpectationValueAsync(Matrix observable);

    /// <summary>
    /// Calculates the variance of an observable.
    /// </summary>
    /// <param name="observable">The observable operator.</param>
    /// <returns>The variance.</returns>
    Task<double> CalculateVarianceAsync(Matrix observable);

    /// <summary>
    /// Calculates the entropy of the current state.
    /// </summary>
    /// <returns>The von Neumann entropy.</returns>
    Task<double> CalculateEntropyAsync();

    /// <summary>
    /// Calculates the fidelity between two quantum states.
    /// </summary>
    /// <param name="state1">The first quantum state.</param>
    /// <param name="state2">The second quantum state.</param>
    /// <returns>The fidelity measure.</returns>
    Task<double> CalculateFidelityAsync(QuantumState state1, QuantumState state2);

    /// <summary>
    /// Checks if the evolution preserves unitarity.
    /// </summary>
    /// <param name="tolerance">The tolerance for unitarity check.</param>
    /// <returns>True if evolution is unitary within tolerance.</returns>
    Task<bool> CheckUnitarityAsync(double tolerance = 1e-12);

    /// <summary>
    /// Monitors the evolution for stability and convergence.
    /// </summary>
    /// <returns>Evolution stability metrics.</returns>
    Task<EvolutionStabilityMetrics> AnalyzeStabilityAsync();

    #endregion

    #region Error Handling and Validation

    /// <summary>
    /// Validates that the Hamiltonian is Hermitian.
    /// </summary>
    /// <param name="hamiltonian">The Hamiltonian to validate.</param>
    /// <param name="tolerance">The tolerance for Hermiticity check.</param>
    /// <returns>True if Hamiltonian is Hermitian within tolerance.</returns>
    Task<bool> ValidateHamiltonianAsync(Matrix hamiltonian, double tolerance = 1e-12);

    /// <summary>
    /// Validates that the quantum state is properly normalized.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <param name="tolerance">The tolerance for normalization check.</param>
    /// <returns>True if state is normalized within tolerance.</returns>
    Task<bool> ValidateStateNormalizationAsync(QuantumState state, double tolerance = 1e-12);

    /// <summary>
    /// Attempts to correct numerical errors in the quantum state.
    /// </summary>
    /// <param name="state">The state to correct.</param>
    /// <returns>The corrected quantum state.</returns>
    Task<QuantumState> CorrectNumericalErrorsAsync(QuantumState state);

    /// <summary>
    /// Gets the current numerical accuracy of the evolution.
    /// </summary>
    /// <returns>The accuracy metrics.</returns>
    Task<NumericalAccuracyMetrics> GetAccuracyMetricsAsync();

    #endregion

    #region Performance and Optimization

    /// <summary>
    /// Gets the current evolution performance metrics.
    /// </summary>
    EvolutionPerformanceMetrics PerformanceMetrics { get; }

    /// <summary>
    /// Optimizes the evolution for the current system configuration.
    /// </summary>
    Task OptimizeEvolutionAsync();

    /// <summary>
    /// Estimates the computational cost of evolution operations.
    /// </summary>
    /// <param name="timeStep">The time step size.</param>
    /// <param name="stepCount">The number of steps.</param>
    /// <returns>The estimated computational cost.</returns>
    Task<ComputationalCostEstimate> EstimateEvolutionCostAsync(double timeStep, int stepCount);

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the evolution status changes.
    /// </summary>
    event EventHandler<EvolutionStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Event raised when a state evolution step is completed.
    /// </summary>
    event EventHandler<StateEvolutionCompletedEventArgs>? EvolutionCompleted;

    /// <summary>
    /// Event raised when a measurement is performed.
    /// </summary>
    event EventHandler<Cosmium.Engine.Simulation.Events.MeasurementPerformedEventArgs>? MeasurementPerformed;

    /// <summary>
    /// Event raised when numerical errors are detected.
    /// </summary>
    event EventHandler<NumericalErrorDetectedEventArgs>? NumericalErrorDetected;

    /// <summary>
    /// Event raised when entanglement is created or modified.
    /// </summary>
    event EventHandler<EntanglementChangedEventArgs>? EntanglementChanged;

    #endregion
}

/// <summary>
/// Enumeration of evolution engine states.
/// </summary>
public enum EvolutionEngineStatus
{
    /// <summary>
    /// Engine has been created but not initialized.
    /// </summary>
    Created,

    /// <summary>
    /// Engine has been initialized and is ready for evolution.
    /// </summary>
    Initialized,

    /// <summary>
    /// Engine is actively evolving quantum states.
    /// </summary>
    Evolving,

    /// <summary>
    /// Engine is performing a measurement operation.
    /// </summary>
    Measuring,

    /// <summary>
    /// Engine is idle and ready for operations.
    /// </summary>
    Idle,

    /// <summary>
    /// Engine is optimizing evolution parameters.
    /// </summary>
    Optimizing,

    /// <summary>
    /// Engine is in an error state.
    /// </summary>
    Error,

    /// <summary>
    /// Engine is shutting down.
    /// </summary>
    ShuttingDown
}

/// <summary>
/// Enumeration of supported evolution methods.
/// </summary>
public enum EvolutionMethod
{
    /// <summary>
    /// Exact matrix exponentiation for small systems.
    /// </summary>
    ExactEvolution,

    /// <summary>
    /// Runge-Kutta integration methods.
    /// </summary>
    RungeKutta,

    /// <summary>
    /// Suzuki-Trotter decomposition for large systems.
    /// </summary>
    SuzukiTrotter,

    /// <summary>
    /// Lanczos algorithm for sparse Hamiltonians.
    /// </summary>
    Lanczos,

    /// <summary>
    /// Chebyshev polynomial expansion.
    /// </summary>
    Chebyshev,

    /// <summary>
    /// Quantum Monte Carlo methods.
    /// </summary>
    QuantumMonteCarlo,

    /// <summary>
    /// Adaptive time-step methods.
    /// </summary>
    AdaptiveEvolution
}

/// <summary>
/// Result of a quantum measurement operation.
/// </summary>
public class MeasurementResult
{
    /// <summary>
    /// Gets the measured eigenvalue.
    /// </summary>
    public Complex MeasuredValue { get; init; }

    /// <summary>
    /// Gets the probability of this measurement result.
    /// </summary>
    public double Probability { get; init; }

    /// <summary>
    /// Gets the collapsed quantum state after measurement.
    /// </summary>
    public QuantumState CollapsedState { get; init; } = null!;

    /// <summary>
    /// Gets the measurement uncertainty.
    /// </summary>
    public double Uncertainty { get; init; }

    /// <summary>
    /// Gets when the measurement was performed.
    /// </summary>
    public DateTime MeasurementTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets additional measurement metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// State of a quantum particle system after evolution.
/// </summary>
public class ParticleSystemState
{
    /// <summary>
    /// Gets the evolved particles.
    /// </summary>
    public IReadOnlyList<IQuantumParticle> Particles { get; init; } = Array.Empty<IQuantumParticle>();

    /// <summary>
    /// Gets the overall system wave function.
    /// </summary>
    public WaveFunction SystemWaveFunction { get; init; } = null!;

    /// <summary>
    /// Gets the entanglement matrix between particles.
    /// </summary>
    public Matrix EntanglementMatrix { get; init; } = null!;

    /// <summary>
    /// Gets the total system energy.
    /// </summary>
    public Complex TotalEnergy { get; init; }

    /// <summary>
    /// Gets the system entropy.
    /// </summary>
    public double SystemEntropy { get; init; }

    /// <summary>
    /// Gets the evolution time for this state.
    /// </summary>
    public double EvolutionTime { get; init; }
}

/// <summary>
/// Represents an entangled pair of quantum particles.
/// </summary>
public class EntangledParticlePair
{
    /// <summary>
    /// Gets the first entangled particle.
    /// </summary>
    public IQuantumParticle Particle1 { get; init; } = null!;

    /// <summary>
    /// Gets the second entangled particle.
    /// </summary>
    public IQuantumParticle Particle2 { get; init; } = null!;

    /// <summary>
    /// Gets the entanglement strength (0.0 to 1.0).
    /// </summary>
    public double EntanglementStrength { get; init; }

    /// <summary>
    /// Gets the Bell state type for this entanglement.
    /// </summary>
    public BellStateType BellState { get; init; }

    /// <summary>
    /// Gets when the entanglement was created.
    /// </summary>
    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the entanglement decoherence time.
    /// </summary>
    public double DecoherenceTime { get; init; }
}

/// <summary>
/// Enumeration of Bell state types for entangled particles.
/// </summary>
public enum BellStateType
{
    /// <summary>
    /// |Φ+⟩ = (|00⟩ + |11⟩)/√2
    /// </summary>
    PhiPlus,

    /// <summary>
    /// |Φ-⟩ = (|00⟩ - |11⟩)/√2
    /// </summary>
    PhiMinus,

    /// <summary>
    /// |Ψ+⟩ = (|01⟩ + |10⟩)/√2
    /// </summary>
    PsiPlus,

    /// <summary>
    /// |Ψ-⟩ = (|01⟩ - |10⟩)/√2
    /// </summary>
    PsiMinus
}

/// <summary>
/// Metrics for evolution stability analysis.
/// </summary>
public class EvolutionStabilityMetrics
{
    /// <summary>
    /// Gets whether the evolution is stable.
    /// </summary>
    public bool IsStable { get; init; }

    /// <summary>
    /// Gets the stability factor (higher is more stable).
    /// </summary>
    public double StabilityFactor { get; init; }

    /// <summary>
    /// Gets the norm preservation error.
    /// </summary>
    public double NormError { get; init; }

    /// <summary>
    /// Gets the energy conservation error.
    /// </summary>
    public double EnergyError { get; init; }

    /// <summary>
    /// Gets the maximum eigenvalue of the evolution operator.
    /// </summary>
    public double MaxEigenvalue { get; init; }

    /// <summary>
    /// Gets the condition number of the Hamiltonian.
    /// </summary>
    public double ConditionNumber { get; init; }

    /// <summary>
    /// Gets recommendations for improving stability.
    /// </summary>
    public IReadOnlyList<string> StabilityRecommendations { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Metrics for numerical accuracy of evolution.
/// </summary>
public class NumericalAccuracyMetrics
{
    /// <summary>
    /// Gets the overall accuracy score (0.0 to 1.0).
    /// </summary>
    public double AccuracyScore { get; init; }

    /// <summary>
    /// Gets the machine precision being used.
    /// </summary>
    public double MachinePrecision { get; init; }

    /// <summary>
    /// Gets the accumulated numerical error.
    /// </summary>
    public double AccumulatedError { get; init; }

    /// <summary>
    /// Gets the relative error in state normalization.
    /// </summary>
    public double NormalizationError { get; init; }

    /// <summary>
    /// Gets the relative error in unitary evolution.
    /// </summary>
    public double UnitarityError { get; init; }

    /// <summary>
    /// Gets the number of numerical corrections applied.
    /// </summary>
    public long CorrectionsApplied { get; init; }
}

/// <summary>
/// Performance metrics for state evolution operations.
/// </summary>
public class EvolutionPerformanceMetrics
{
    /// <summary>
    /// Gets the average time per evolution step in milliseconds.
    /// </summary>
    public double AverageStepTimeMs { get; init; }

    /// <summary>
    /// Gets the evolution steps per second.
    /// </summary>
    public double StepsPerSecond { get; init; }

    /// <summary>
    /// Gets the memory usage for state storage in MB.
    /// </summary>
    public double StateMemoryUsageMB { get; init; }

    /// <summary>
    /// Gets the memory usage for operators in MB.
    /// </summary>
    public double OperatorMemoryUsageMB { get; init; }

    /// <summary>
    /// Gets the FLOPS (floating point operations per second).
    /// </summary>
    public double FLOPS { get; init; }

    /// <summary>
    /// Gets the efficiency relative to theoretical maximum.
    /// </summary>
    public double Efficiency { get; init; }

    /// <summary>
    /// Gets the parallel processing utilization (0.0 to 1.0).
    /// </summary>
    public double ParallelUtilization { get; init; }
}

/// <summary>
/// Estimate of computational cost for evolution operations.
/// </summary>
public class ComputationalCostEstimate
{
    /// <summary>
    /// Gets the estimated execution time.
    /// </summary>
    public TimeSpan EstimatedTime { get; init; }

    /// <summary>
    /// Gets the estimated memory usage in MB.
    /// </summary>
    public double EstimatedMemoryMB { get; init; }

    /// <summary>
    /// Gets the estimated number of floating point operations.
    /// </summary>
    public long EstimatedFLOPS { get; init; }

    /// <summary>
    /// Gets the computational complexity estimate.
    /// </summary>
    public string ComplexityEstimate { get; init; } = string.Empty;

    /// <summary>
    /// Gets the confidence level of the estimate (0.0 to 1.0).
    /// </summary>
    public double ConfidenceLevel { get; init; }

    /// <summary>
    /// Gets recommendations for optimization.
    /// </summary>
    public IReadOnlyList<string> OptimizationRecommendations { get; init; } = Array.Empty<string>();
}
