using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Forces;

/// <summary>
/// Interface defining the contract for fundamental forces in the Standard Model.
/// Represents the four fundamental interactions: strong, electromagnetic, weak, and gravitational.
/// </summary>
public interface IFundamentalForce
{
    #region Basic Properties

    /// <summary>
    /// Gets the name of the fundamental force.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the symbol commonly used to represent this force.
    /// </summary>
    string Symbol { get; }

    /// <summary>
    /// Gets the typical range of the force in meters.
    /// </summary>
    double TypicalRange { get; }

    /// <summary>
    /// Gets the relative strength of the force compared to the strong force.
    /// Strong force = 1, Electromagnetic ≈ 10^-2, Weak ≈ 10^-13, Gravitational ≈ 10^-39
    /// </summary>
    double RelativeStrength { get; }

    /// <summary>
    /// Gets whether this force is attractive only, repulsive only, or both.
    /// </summary>
    ForceCharacter Character { get; }

    /// <summary>
    /// Gets the gauge bosons that mediate this force.
    /// </summary>
    IReadOnlyList<string> MediatingBosons { get; }

    #endregion

    #region Force Calculations

    /// <summary>
    /// Calculates the force magnitude between two particles at a given distance.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="distance">The distance between particles in meters.</param>
    /// <returns>The force magnitude in Newtons (positive for repulsive, negative for attractive).</returns>
    double CalculateForceMagnitude(IQuantumParticle particle1, IQuantumParticle particle2, double distance);

    /// <summary>
    /// Calculates the force vector between two particles.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="position1">Position of the first particle.</param>
    /// <param name="position2">Position of the second particle.</param>
    /// <returns>The force vector on particle1 due to particle2.</returns>
    Vector3D CalculateForceVector(IQuantumParticle particle1, IQuantumParticle particle2, 
        Vector3D position1, Vector3D position2);

    /// <summary>
    /// Calculates the potential energy between two particles at a given distance.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="distance">The distance between particles in meters.</param>
    /// <returns>The potential energy in Joules.</returns>
    double CalculatePotentialEnergy(IQuantumParticle particle1, IQuantumParticle particle2, double distance);

    /// <summary>
    /// Calculates the coupling constant for the interaction at a given energy scale.
    /// </summary>
    /// <param name="energyScale">The energy scale in Joules.</param>
    /// <returns>The coupling constant at this energy scale.</returns>
    double GetCouplingConstant(double energyScale);

    #endregion

    #region Interaction Validation

    /// <summary>
    /// Determines if two particles can interact via this fundamental force.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <returns>True if the particles can interact via this force.</returns>
    bool CanParticlesInteract(IQuantumParticle particle1, IQuantumParticle particle2);

    /// <summary>
    /// Gets the interaction probability at a given energy and distance.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="energy">The interaction energy in Joules.</param>
    /// <param name="distance">The distance between particles in meters.</param>
    /// <returns>The interaction probability (0 to 1).</returns>
    double GetInteractionProbability(IQuantumParticle particle1, IQuantumParticle particle2, 
        double energy, double distance);

    /// <summary>
    /// Determines the effective range of the force for given particles.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <returns>The effective interaction range in meters.</returns>
    double GetEffectiveRange(IQuantumParticle particle1, IQuantumParticle particle2);

    #endregion

    #region Quantum Effects

    /// <summary>
    /// Calculates quantum corrections to the classical force.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="distance">The distance between particles.</param>
    /// <param name="energy">The interaction energy.</param>
    /// <returns>The quantum correction factor.</returns>
    double CalculateQuantumCorrections(IQuantumParticle particle1, IQuantumParticle particle2, 
        double distance, double energy);

    /// <summary>
    /// Determines if quantum effects are significant at the given scale.
    /// </summary>
    /// <param name="distance">The interaction distance.</param>
    /// <param name="energy">The interaction energy.</param>
    /// <returns>True if quantum effects are significant.</returns>
    bool AreQuantumEffectsSignificant(double distance, double energy);

    #endregion

    #region Cross Sections and Scattering

    /// <summary>
    /// Calculates the scattering cross section for particle interaction.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="energy">The center-of-mass energy.</param>
    /// <returns>The cross section in square meters.</returns>
    double CalculateScatteringCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy);

    /// <summary>
    /// Calculates the differential cross section for scattering at a given angle.
    /// </summary>
    /// <param name="particle1">The first particle.</param>
    /// <param name="particle2">The second particle.</param>
    /// <param name="energy">The center-of-mass energy.</param>
    /// <param name="scatteringAngle">The scattering angle in radians.</param>
    /// <returns>The differential cross section.</returns>
    double CalculateDifferentialCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, 
        double energy, double scatteringAngle);

    #endregion
}

/// <summary>
/// Enumeration describing the character of a fundamental force.
/// </summary>
public enum ForceCharacter
{
    /// <summary>
    /// Force is always attractive (e.g., gravity for positive masses).
    /// </summary>
    AlwaysAttractive,

    /// <summary>
    /// Force is always repulsive.
    /// </summary>
    AlwaysRepulsive,

    /// <summary>
    /// Force can be either attractive or repulsive depending on charges/properties.
    /// </summary>
    AttractiveOrRepulsive,

    /// <summary>
    /// Force character depends on complex quantum properties (e.g., strong force).
    /// </summary>
    Complex
}

/// <summary>
/// Event arguments for force interaction events.
/// </summary>
public class ForceInteractionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the first particle in the interaction.
    /// </summary>
    public IQuantumParticle Particle1 { get; init; } = null!;

    /// <summary>
    /// Gets the second particle in the interaction.
    /// </summary>
    public IQuantumParticle Particle2 { get; init; } = null!;

    /// <summary>
    /// Gets the force that mediated the interaction.
    /// </summary>
    public IFundamentalForce Force { get; init; } = null!;

    /// <summary>
    /// Gets the calculated force magnitude.
    /// </summary>
    public double ForceMagnitude { get; init; }

    /// <summary>
    /// Gets the interaction distance.
    /// </summary>
    public double Distance { get; init; }

    /// <summary>
    /// Gets the interaction energy.
    /// </summary>
    public double Energy { get; init; }

    /// <summary>
    /// Gets the timestamp when the interaction occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
