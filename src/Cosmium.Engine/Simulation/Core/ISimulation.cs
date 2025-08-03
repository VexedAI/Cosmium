using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Simulation.Events;

namespace Cosmium.Engine.Simulation.Core;

/// <summary>
/// Core interface defining the contract for all quantum mechanical simulations in Cosmium.
/// Represents the fundamental operations and properties that every simulation must implement.
/// </summary>
public interface ISimulation
{
    #region Basic Properties

    /// <summary>
    /// Gets the unique identifier for this simulation instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the simulation.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a detailed description of what this simulation models.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the type of simulation (e.g., "Atomic", "Molecular", "Particle Collision").
    /// </summary>
    string SimulationType { get; }

    /// <summary>
    /// Gets the current status of the simulation.
    /// </summary>
    SimulationStatus Status { get; }

    /// <summary>
    /// Gets when this simulation was created.
    /// </summary>
    DateTime CreationTime { get; }

    /// <summary>
    /// Gets when this simulation was started, if it has been started.
    /// </summary>
    DateTime? StartTime { get; }

    /// <summary>
    /// Gets when this simulation completed, if it has completed.
    /// </summary>
    DateTime? EndTime { get; }

    /// <summary>
    /// Gets the total elapsed time of the simulation.
    /// </summary>
    TimeSpan ElapsedTime { get; }

    #endregion

    #region Simulation Parameters

    /// <summary>
    /// Gets the simulation parameters that control the behavior and execution.
    /// </summary>
    SimulationParameters Parameters { get; }

    /// <summary>
    /// Gets the execution context containing runtime information and state.
    /// </summary>
    SimulationContext Context { get; }

    /// <summary>
    /// Gets the current simulation time (physical time being modeled).
    /// </summary>
    double CurrentTime { get; }

    /// <summary>
    /// Gets the time step used for discrete time evolution.
    /// </summary>
    double TimeStep { get; }

    /// <summary>
    /// Gets the maximum simulation time before automatic termination.
    /// </summary>
    double MaxTime { get; }

    /// <summary>
    /// Gets the current step number in the simulation.
    /// </summary>
    long CurrentStep { get; }

    /// <summary>
    /// Gets the maximum number of steps before termination.
    /// </summary>
    long MaxSteps { get; }

    #endregion

    #region Simulation State

    /// <summary>
    /// Gets all quantum particles participating in this simulation.
    /// </summary>
    IReadOnlyList<IQuantumParticle> Particles { get; }

    /// <summary>
    /// Gets the number of particles in the simulation.
    /// </summary>
    int ParticleCount { get; }

    /// <summary>
    /// Gets the total energy of the system.
    /// </summary>
    double TotalEnergy { get; }

    /// <summary>
    /// Gets the total momentum of the system.
    /// </summary>
    Complex[] TotalMomentum { get; }

    /// <summary>
    /// Gets whether the simulation has converged to a stable state.
    /// </summary>
    bool HasConverged { get; }

    /// <summary>
    /// Gets the convergence criteria used to determine stability.
    /// </summary>
    double ConvergenceTolerance { get; }

    #endregion

    #region Execution Control

    /// <summary>
    /// Initializes the simulation with the specified parameters.
    /// Must be called before starting the simulation.
    /// </summary>
    /// <param name="parameters">The simulation parameters.</param>
    Task InitializeAsync(SimulationParameters parameters);

    /// <summary>
    /// Starts the simulation execution.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Pauses the simulation if it is currently running.
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// Resumes a paused simulation.
    /// </summary>
    Task ResumeAsync();

    /// <summary>
    /// Stops the simulation and performs cleanup.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Resets the simulation to its initial state.
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// Executes a single simulation step.
    /// </summary>
    Task StepAsync();

    /// <summary>
    /// Runs the simulation for a specified number of steps.
    /// </summary>
    /// <param name="steps">Number of steps to execute.</param>
    Task RunStepsAsync(long steps);

    /// <summary>
    /// Runs the simulation for a specified time duration.
    /// </summary>
    /// <param name="duration">Time duration to simulate.</param>
    Task RunForTimeAsync(double duration);

    #endregion

    #region Particle Management

    /// <summary>
    /// Adds a particle to the simulation.
    /// </summary>
    /// <param name="particle">The particle to add.</param>
    void AddParticle(IQuantumParticle particle);

    /// <summary>
    /// Removes a particle from the simulation.
    /// </summary>
    /// <param name="particle">The particle to remove.</param>
    /// <returns>True if the particle was removed successfully.</returns>
    bool RemoveParticle(IQuantumParticle particle);

    /// <summary>
    /// Removes a particle by its unique identifier.
    /// </summary>
    /// <param name="particleId">The ID of the particle to remove.</param>
    /// <returns>True if the particle was removed successfully.</returns>
    bool RemoveParticle(Guid particleId);

    /// <summary>
    /// Gets a particle by its unique identifier.
    /// </summary>
    /// <param name="particleId">The particle ID.</param>
    /// <returns>The particle if found, null otherwise.</returns>
    IQuantumParticle? GetParticle(Guid particleId);

    /// <summary>
    /// Gets all particles of a specific type.
    /// </summary>
    /// <typeparam name="T">The particle type to filter by.</typeparam>
    /// <returns>All particles of the specified type.</returns>
    IEnumerable<T> GetParticlesOfType<T>() where T : class, IQuantumParticle;

    /// <summary>
    /// Clears all particles from the simulation.
    /// </summary>
    void ClearParticles();

    #endregion

    #region Results and Observables

    /// <summary>
    /// Gets the current simulation results.
    /// </summary>
    SimulationResult GetResults();

    /// <summary>
    /// Calculates the expectation value of an observable over all particles.
    /// </summary>
    /// <param name="observable">The observable operator matrix.</param>
    /// <returns>The expectation value.</returns>
    double CalculateObservableExpectation(Complex[,] observable);

    /// <summary>
    /// Measures an observable across all particles in the simulation.
    /// </summary>
    /// <param name="observable">The observable to measure.</param>
    /// <returns>Array of measurement results for each particle.</returns>
    double[] MeasureObservable(Complex[,] observable);

    /// <summary>
    /// Gets the probability distribution for a measurement outcome.
    /// </summary>
    /// <param name="observable">The observable operator.</param>
    /// <returns>Probability distribution over eigenvalues.</returns>
    Dictionary<double, double> GetMeasurementProbabilities(Complex[,] observable);

    #endregion

    #region Validation and Error Handling

    /// <summary>
    /// Validates that the simulation is in a consistent state.
    /// </summary>
    /// <returns>True if the simulation state is valid.</returns>
    bool ValidateState();

    /// <summary>
    /// Gets any validation errors or warnings about the current simulation state.
    /// </summary>
    /// <returns>List of validation issues.</returns>
    IReadOnlyList<string> GetValidationIssues();

    /// <summary>
    /// Checks if the simulation can be safely started.
    /// </summary>
    /// <returns>True if the simulation can start.</returns>
    bool CanStart();

    /// <summary>
    /// Gets the reason why the simulation cannot start, if applicable.
    /// </summary>
    /// <returns>Reason string, or null if the simulation can start.</returns>
    string? GetStartBlockingReason();

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the simulation status changes.
    /// </summary>
    event EventHandler<SimulationStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Event raised when the simulation progresses through steps.
    /// </summary>
    event EventHandler<SimulationProgressEventArgs>? ProgressUpdated;

    /// <summary>
    /// Event raised when the simulation completes.
    /// </summary>
    event EventHandler<SimulationCompletedEventArgs>? Completed;

    /// <summary>
    /// Event raised when an error occurs during simulation.
    /// </summary>
    event EventHandler<SimulationErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// Event raised when a particle is added to the simulation.
    /// </summary>
    event EventHandler<ParticleAddedEventArgs>? ParticleAdded;

    /// <summary>
    /// Event raised when a particle is removed from the simulation.
    /// </summary>
    event EventHandler<ParticleRemovedEventArgs>? ParticleRemoved;

    /// <summary>
    /// Event raised when an observable is measured.
    /// </summary>
    event EventHandler<ObservableMeasuredEventArgs>? ObservableMeasured;

    #endregion
}

/// <summary>
/// Enumeration of possible simulation states.
/// </summary>
public enum SimulationStatus
{
    /// <summary>
    /// Simulation has been created but not yet initialized.
    /// </summary>
    Created,

    /// <summary>
    /// Simulation has been initialized with parameters.
    /// </summary>
    Initialized,

    /// <summary>
    /// Simulation is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// Simulation is paused and can be resumed.
    /// </summary>
    Paused,

    /// <summary>
    /// Simulation has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Simulation has been stopped by user request.
    /// </summary>
    Stopped,

    /// <summary>
    /// Simulation has failed due to an error.
    /// </summary>
    Failed,

    /// <summary>
    /// Simulation is being reset to initial state.
    /// </summary>
    Resetting
}
