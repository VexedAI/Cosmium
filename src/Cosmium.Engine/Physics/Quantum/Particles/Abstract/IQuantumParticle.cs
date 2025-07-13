using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;

namespace Cosmium.Engine.Physics.Quantum.Particles.Abstract;

/// <summary>
/// Core interface defining the fundamental properties and behavior of quantum particles.
/// Represents both elementary and composite particles in the quantum simulation.
/// </summary>
public interface IQuantumParticle
{
    #region Basic Properties

    /// <summary>
    /// Gets the unique identifier for this particle instance.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the name of the particle type (e.g., "electron", "proton", "photon").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the symbol commonly used to represent this particle (e.g., "e⁻", "p⁺", "γ").
    /// </summary>
    string Symbol { get; }

    /// <summary>
    /// Gets whether this is an elementary particle (true) or composite particle (false).
    /// </summary>
    bool IsElementary { get; }

    /// <summary>
    /// Gets the creation time of this particle instance.
    /// </summary>
    DateTime CreationTime { get; }

    #endregion

    #region Fundamental Quantum Properties

    /// <summary>
    /// Gets the rest mass of the particle in kg.
    /// For massless particles (like photons), this returns 0.
    /// </summary>
    IScalarMeasurable Mass { get; }

    /// <summary>
    /// Gets the electric charge of the particle in Coulombs.
    /// </summary>
    IScalarMeasurable Charge { get; }

    /// <summary>
    /// Gets the intrinsic spin angular momentum of the particle.
    /// </summary>
    IVectorMeasurable Spin { get; }

    /// <summary>
    /// Gets the spin quantum number (0, 1/2, 1, 3/2, 2, etc.).
    /// </summary>
    double SpinQuantumNumber { get; }

    /// <summary>
    /// Gets whether this particle is a fermion (half-integer spin) or boson (integer spin).
    /// </summary>
    bool IsFermion { get; }

    #endregion

    #region Quantum State

    /// <summary>
    /// Gets the current quantum state vector of the particle.
    /// This represents the complete quantum mechanical state in the particle's Hilbert space.
    /// </summary>
    Complex[] StateVector { get; }

    /// <summary>
    /// Gets the dimension of the particle's Hilbert space.
    /// </summary>
    int HilbertSpaceDimension { get; }

    /// <summary>
    /// Gets whether the particle is in a pure quantum state (true) or mixed state (false).
    /// </summary>
    bool IsPureState { get; }

    /// <summary>
    /// Gets the wave function normalization factor.
    /// </summary>
    double Normalization { get; }

    #endregion

    #region Kinematic Properties

    /// <summary>
    /// Gets the position of the particle in 3D space.
    /// For quantum particles, this may be a probability distribution.
    /// </summary>
    IVectorMeasurable Position { get; }

    /// <summary>
    /// Gets the momentum of the particle.
    /// </summary>
    IVectorMeasurable Momentum { get; }

    /// <summary>
    /// Gets the total energy of the particle (kinetic + rest energy).
    /// </summary>
    IScalarMeasurable Energy { get; }

    /// <summary>
    /// Gets the kinetic energy of the particle.
    /// </summary>
    IScalarMeasurable KineticEnergy { get; }

    /// <summary>
    /// Gets the velocity of the particle.
    /// </summary>
    IVectorMeasurable Velocity { get; }

    #endregion

    #region Quantum Mechanics Operations

    /// <summary>
    /// Evolves the particle's quantum state according to the time-dependent Schrödinger equation.
    /// </summary>
    /// <param name="hamiltonian">The Hamiltonian operator matrix.</param>
    /// <param name="timeStep">The time step for evolution.</param>
    void EvolveState(Complex[,] hamiltonian, double timeStep);

    /// <summary>
    /// Applies a quantum measurement to the particle, potentially causing state collapse.
    /// </summary>
    /// <param name="observable">The observable being measured (Hermitian matrix).</param>
    /// <returns>The measurement result.</returns>
    double MeasureObservable(Complex[,] observable);

    /// <summary>
    /// Calculates the expectation value of an observable for the current state.
    /// </summary>
    /// <param name="observable">The observable operator matrix.</param>
    /// <returns>The expectation value.</returns>
    double CalculateExpectationValue(Complex[,] observable);

    /// <summary>
    /// Calculates the probability of measuring a specific eigenvalue.
    /// </summary>
    /// <param name="observable">The observable operator.</param>
    /// <param name="eigenvalue">The eigenvalue to check probability for.</param>
    /// <returns>The probability of measuring this eigenvalue.</returns>
    double CalculateMeasurementProbability(Complex[,] observable, double eigenvalue);

    #endregion

    #region Interactions

    /// <summary>
    /// Determines if this particle can interact with another particle via electromagnetic force.
    /// </summary>
    /// <param name="other">The other particle.</param>
    /// <returns>True if electromagnetic interaction is possible.</returns>
    bool CanInteractElectromagnetically(IQuantumParticle other);

    /// <summary>
    /// Determines if this particle can interact with another particle via weak nuclear force.
    /// </summary>
    /// <param name="other">The other particle.</param>
    /// <returns>True if weak interaction is possible.</returns>
    bool CanInteractWeakly(IQuantumParticle other);

    /// <summary>
    /// Determines if this particle can interact with another particle via strong nuclear force.
    /// </summary>
    /// <param name="other">The other particle.</param>
    /// <returns>True if strong interaction is possible.</returns>
    bool CanInteractStrongly(IQuantumParticle other);

    /// <summary>
    /// Calculates the interaction strength with another particle.
    /// </summary>
    /// <param name="other">The other particle.</param>
    /// <param name="distance">The distance between particles.</param>
    /// <returns>The interaction strength.</returns>
    double CalculateInteractionStrength(IQuantumParticle other, double distance);

    #endregion

    #region Particle Statistics

    /// <summary>
    /// Gets the statistical behavior of this particle type.
    /// </summary>
    ParticleStatistics Statistics { get; }

    /// <summary>
    /// Creates a deep copy of this particle with identical quantum state.
    /// </summary>
    /// <returns>A new particle instance with the same state.</returns>
    IQuantumParticle Clone();

    /// <summary>
    /// Gets the antiparticle corresponding to this particle.
    /// </summary>
    /// <returns>The antiparticle, or null if this particle is its own antiparticle.</returns>
    IQuantumParticle? GetAntiparticle();

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the particle's quantum state changes.
    /// </summary>
    event EventHandler<QuantumStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Event raised when a measurement is performed on the particle.
    /// </summary>
    event EventHandler<MeasurementPerformedEventArgs>? MeasurementPerformed;

    /// <summary>
    /// Event raised when the particle interacts with another particle.
    /// </summary>
    event EventHandler<ParticleInteractionEventArgs>? InteractionOccurred;

    #endregion
}

/// <summary>
/// Enumeration of quantum statistical behaviors.
/// </summary>
public enum ParticleStatistics
{
    /// <summary>
    /// Fermi-Dirac statistics for fermions (particles with half-integer spin).
    /// </summary>
    FermiDirac,

    /// <summary>
    /// Bose-Einstein statistics for bosons (particles with integer spin).
    /// </summary>
    BoseEinstein,

    /// <summary>
    /// Classical Maxwell-Boltzmann statistics (approximation for high temperatures).
    /// </summary>
    MaxwellBoltzmann
}

/// <summary>
/// Event args for quantum state changes.
/// </summary>
public class QuantumStateChangedEventArgs : EventArgs
{
    public Complex[] OldState { get; set; } = Array.Empty<Complex>();
    public Complex[] NewState { get; set; } = Array.Empty<Complex>();
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event args for measurement events.
/// </summary>
public class MeasurementPerformedEventArgs : EventArgs
{
    public string ObservableName { get; set; } = string.Empty;
    public double MeasuredValue { get; set; }
    public double Uncertainty { get; set; }
    public Complex[] StateBeforeMeasurement { get; set; } = Array.Empty<Complex>();
    public Complex[] StateAfterMeasurement { get; set; } = Array.Empty<Complex>();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event args for particle interaction events.
/// </summary>
public class ParticleInteractionEventArgs : EventArgs
{
    public IQuantumParticle OtherParticle { get; set; } = null!;
    public string InteractionType { get; set; } = string.Empty;
    public double InteractionStrength { get; set; }
    public double Distance { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
