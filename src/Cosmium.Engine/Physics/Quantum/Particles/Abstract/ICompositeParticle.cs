using System.Numerics;

namespace Cosmium.Engine.Physics.Quantum.Particles.Abstract;

/// <summary>
/// Interface for composite particles made up of constituent particles.
/// Examples include protons (made of quarks), nuclei (made of protons and neutrons), 
/// atoms (made of nucleus and electrons), and molecules.
/// </summary>
public interface ICompositeParticle : IQuantumParticle
{
    #region Composition

    /// <summary>
    /// Gets the constituent particles that make up this composite particle.
    /// </summary>
    IReadOnlyList<IQuantumParticle> Constituents { get; }

    /// <summary>
    /// Gets the number of constituent particles.
    /// </summary>
    int ConstituentCount { get; }

    /// <summary>
    /// Gets the binding energy that holds the constituents together.
    /// This is the energy required to separate all constituents to infinity.
    /// </summary>
    IScalarMeasurable BindingEnergy { get; }

    /// <summary>
    /// Gets the excitation energy above the ground state.
    /// </summary>
    IScalarMeasurable ExcitationEnergy { get; }

    #endregion

    #region Constituent Management

    /// <summary>
    /// Adds a constituent particle to this composite particle.
    /// </summary>
    /// <param name="particle">The particle to add as a constituent.</param>
    /// <param name="bindingEnergy">The binding energy for this constituent.</param>
    void AddConstituent(IQuantumParticle particle, double bindingEnergy);

    /// <summary>
    /// Removes a constituent particle from this composite particle.
    /// </summary>
    /// <param name="particle">The particle to remove.</param>
    /// <returns>True if the particle was successfully removed.</returns>
    bool RemoveConstituent(IQuantumParticle particle);

    /// <summary>
    /// Removes a constituent particle at the specified index.
    /// </summary>
    /// <param name="index">The index of the constituent to remove.</param>
    /// <returns>The removed particle.</returns>
    IQuantumParticle RemoveConstituentAt(int index);

    /// <summary>
    /// Gets a constituent particle by its index.
    /// </summary>
    /// <param name="index">The index of the constituent.</param>
    /// <returns>The constituent particle.</returns>
    IQuantumParticle GetConstituent(int index);

    /// <summary>
    /// Finds all constituents of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of particle to find.</typeparam>
    /// <returns>A collection of constituents of the specified type.</returns>
    IEnumerable<T> GetConstituentsOfType<T>() where T : class, IQuantumParticle;

    #endregion

    #region Internal Structure

    /// <summary>
    /// Gets the center of mass of the composite particle.
    /// </summary>
    IVectorMeasurable CenterOfMass { get; }

    /// <summary>
    /// Gets the moment of inertia tensor for rotational dynamics.
    /// </summary>
    double[,] MomentOfInertiaTensor { get; }

    /// <summary>
    /// Gets the internal vibrational modes of the composite particle.
    /// </summary>
    IReadOnlyList<VibrationalMode> VibrationalModes { get; }

    /// <summary>
    /// Gets the internal rotational modes of the composite particle.
    /// </summary>
    IReadOnlyList<RotationalMode> RotationalModes { get; }

    /// <summary>
    /// Gets the characteristic size (radius) of the composite particle.
    /// </summary>
    IScalarMeasurable Size { get; }

    #endregion

    #region Quantum States

    /// <summary>
    /// Gets the total angular momentum quantum number of the composite system.
    /// </summary>
    double TotalAngularMomentumQuantumNumber { get; }

    /// <summary>
    /// Gets the ground state energy of the composite particle.
    /// </summary>
    double GroundStateEnergy { get; }

    /// <summary>
    /// Gets all possible excited states of the composite particle.
    /// </summary>
    IReadOnlyList<ExcitedState> ExcitedStates { get; }

    /// <summary>
    /// Gets the current excitation level (0 = ground state, 1 = first excited state, etc.).
    /// </summary>
    int ExcitationLevel { get; }

    #endregion

    #region Composite Operations

    /// <summary>
    /// Calculates the total quantum state of the composite system from constituent states.
    /// This involves tensor products of individual particle states.
    /// </summary>
    /// <returns>The composite quantum state vector.</returns>
    Complex[] CalculateCompositeState();

    /// <summary>
    /// Updates the composite particle properties based on current constituent states.
    /// </summary>
    void UpdateCompositeProperties();

    /// <summary>
    /// Decomposes the composite particle into its constituent particles.
    /// This represents particle decay or dissociation.
    /// </summary>
    /// <returns>The constituent particles after decomposition.</returns>
    IEnumerable<IQuantumParticle> Decompose();

    /// <summary>
    /// Checks if the composite particle is stable against decay.
    /// </summary>
    /// <returns>True if the particle is stable.</returns>
    bool IsStable();

    /// <summary>
    /// Calculates the decay rate (inverse of mean lifetime) if the particle is unstable.
    /// </summary>
    /// <returns>The decay rate in inverse seconds.</returns>
    double CalculateDecayRate();

    /// <summary>
    /// Simulates the time evolution of the composite particle and its constituents.
    /// </summary>
    /// <param name="timeStep">The time step for evolution.</param>
    void EvolveCompositeState(double timeStep);

    #endregion

    #region Internal Interactions

    /// <summary>
    /// Gets the interaction matrix between all pairs of constituents.
    /// </summary>
    double[,] ConstituentInteractionMatrix { get; }

    /// <summary>
    /// Calculates the internal interaction energy between constituents.
    /// </summary>
    /// <returns>The total internal interaction energy.</returns>
    double CalculateInternalInteractionEnergy();

    /// <summary>
    /// Updates the interactions between constituent particles.
    /// </summary>
    void UpdateInternalInteractions();

    #endregion

    #region Events

    /// <summary>
    /// Event raised when a constituent is added to the composite particle.
    /// </summary>
    event EventHandler<ConstituentChangedEventArgs>? ConstituentAdded;

    /// <summary>
    /// Event raised when a constituent is removed from the composite particle.
    /// </summary>
    event EventHandler<ConstituentChangedEventArgs>? ConstituentRemoved;

    /// <summary>
    /// Event raised when the composite particle decays.
    /// </summary>
    event EventHandler<ParticleDecayEventArgs>? ParticleDecayed;

    /// <summary>
    /// Event raised when the excitation level changes.
    /// </summary>
    event EventHandler<ExcitationChangedEventArgs>? ExcitationChanged;

    #endregion
}

/// <summary>
/// Represents a vibrational mode in a composite particle.
/// </summary>
public class VibrationalMode
{
    /// <summary>
    /// Gets the frequency of the vibrational mode.
    /// </summary>
    public double Frequency { get; set; }

    /// <summary>
    /// Gets the quantum number for this vibrational mode.
    /// </summary>
    public int QuantumNumber { get; set; }

    /// <summary>
    /// Gets the amplitude of vibration.
    /// </summary>
    public double Amplitude { get; set; }

    /// <summary>
    /// Gets the reduced mass for this mode.
    /// </summary>
    public double ReducedMass { get; set; }
}

/// <summary>
/// Represents a rotational mode in a composite particle.
/// </summary>
public class RotationalMode
{
    /// <summary>
    /// Gets the rotational quantum number.
    /// </summary>
    public int QuantumNumber { get; set; }

    /// <summary>
    /// Gets the rotational energy for this mode.
    /// </summary>
    public double Energy { get; set; }

    /// <summary>
    /// Gets the angular velocity vector.
    /// </summary>
    public double[] AngularVelocity { get; set; } = new double[3];

    /// <summary>
    /// Gets the rotational constant for this mode.
    /// </summary>
    public double RotationalConstant { get; set; }
}

/// <summary>
/// Represents an excited state of a composite particle.
/// </summary>
public class ExcitedState
{
    /// <summary>
    /// Gets the energy level of this excited state.
    /// </summary>
    public double Energy { get; set; }

    /// <summary>
    /// Gets the quantum numbers characterizing this state.
    /// </summary>
    public Dictionary<string, double> QuantumNumbers { get; set; } = new();

    /// <summary>
    /// Gets the lifetime of this excited state.
    /// </summary>
    public double Lifetime { get; set; }

    /// <summary>
    /// Gets the decay channels from this excited state.
    /// </summary>
    public List<DecayChannel> DecayChannels { get; set; } = new();

    /// <summary>
    /// Gets the wave function for this excited state.
    /// </summary>
    public Complex[] WaveFunction { get; set; } = Array.Empty<Complex>();
}

/// <summary>
/// Represents a decay channel for an excited state.
/// </summary>
public class DecayChannel
{
    /// <summary>
    /// Gets the products of this decay channel.
    /// </summary>
    public List<Type> DecayProducts { get; set; } = new();

    /// <summary>
    /// Gets the branching ratio (probability) for this decay channel.
    /// </summary>
    public double BranchingRatio { get; set; }

    /// <summary>
    /// Gets the energy release in this decay.
    /// </summary>
    public double EnergyRelease { get; set; }
}

/// <summary>
/// Event args for constituent changes.
/// </summary>
public class ConstituentChangedEventArgs : EventArgs
{
    public IQuantumParticle Constituent { get; set; } = null!;
    public double BindingEnergy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event args for particle decay events.
/// </summary>
public class ParticleDecayEventArgs : EventArgs
{
    public IEnumerable<IQuantumParticle> DecayProducts { get; set; } = Enumerable.Empty<IQuantumParticle>();
    public string DecayMode { get; set; } = string.Empty;
    public double EnergyRelease { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event args for excitation level changes.
/// </summary>
public class ExcitationChangedEventArgs : EventArgs
{
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
    public double EnergyChange { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
