using System;
using System.Collections.Generic;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Forces;

/// <summary>
/// Abstract base class providing common functionality for fundamental forces.
/// Implements shared behavior and validation for all fundamental interactions.
/// </summary>
public abstract class FundamentalForceBase : IFundamentalForce
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    protected readonly object _calculationLock = new();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the FundamentalForceBase class.
    /// </summary>
    /// <param name="name">The name of the force.</param>
    /// <param name="symbol">The symbol representing the force.</param>
    /// <param name="typicalRange">The typical range of the force in meters.</param>
    /// <param name="relativeStrength">The relative strength compared to the strong force.</param>
    /// <param name="character">The character of the force (attractive/repulsive).</param>
    /// <param name="mediatingBosons">The gauge bosons that mediate this force.</param>
    protected FundamentalForceBase(string name, string symbol, double typicalRange, 
        double relativeStrength, ForceCharacter character, params string[] mediatingBosons)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Force name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Force symbol cannot be null or empty", nameof(symbol));
        
        // Allow positive infinity for forces with infinite range (like gravity)
        if (typicalRange <= 0 && !double.IsPositiveInfinity(typicalRange))
            throw new ArgumentException("Typical range must be positive or positive infinity", nameof(typicalRange));

        Name = name;
        Symbol = symbol;
        TypicalRange = typicalRange;
        RelativeStrength = relativeStrength;
        Character = character;
        MediatingBosons = Array.AsReadOnly(mediatingBosons ?? Array.Empty<string>());

        Logger.Debug($"Initialized fundamental force: {name} ({symbol})", 
            new { Range = typicalRange, Strength = relativeStrength });
    }

    #endregion

    #region Basic Properties

    public string Name { get; }
    public string Symbol { get; }
    public double TypicalRange { get; }
    public double RelativeStrength { get; }
    public ForceCharacter Character { get; }
    public IReadOnlyList<string> MediatingBosons { get; }

    #endregion

    #region Abstract Methods (Must be implemented by derived classes)

    /// <summary>
    /// Calculates the force magnitude between two particles at a given distance.
    /// Must be implemented by derived classes to provide force-specific calculations.
    /// </summary>
    public abstract double CalculateForceMagnitude(IQuantumParticle particle1, IQuantumParticle particle2, double distance);

    /// <summary>
    /// Calculates the potential energy between two particles at a given distance.
    /// Must be implemented by derived classes.
    /// </summary>
    public abstract double CalculatePotentialEnergy(IQuantumParticle particle1, IQuantumParticle particle2, double distance);

    /// <summary>
    /// Determines if two particles can interact via this fundamental force.
    /// Must be implemented by derived classes based on force-specific rules.
    /// </summary>
    public abstract bool CanParticlesInteract(IQuantumParticle particle1, IQuantumParticle particle2);

    /// <summary>
    /// Calculates the coupling constant for the interaction at a given energy scale.
    /// Must be implemented by derived classes.
    /// </summary>
    public abstract double GetCouplingConstant(double energyScale);

    #endregion

    #region Virtual Methods (Can be overridden by derived classes)

    /// <summary>
    /// Calculates the force vector between two particles.
    /// Default implementation uses the force magnitude and direction vector.
    /// </summary>
    public virtual Vector3D CalculateForceVector(IQuantumParticle particle1, IQuantumParticle particle2, 
        Vector3D position1, Vector3D position2)
    {
        ValidateParticles(particle1, particle2);
        ValidatePositions(position1, position2);

        var displacement = position2 - position1;
        var distance = displacement.Magnitude;

        if (distance < double.Epsilon)
        {
            Logger.Warning("Force calculation at zero distance, returning zero force", 
                new { Particle1 = particle1.Name, Particle2 = particle2.Name });
            return Vector3D.Zero;
        }

        lock (_calculationLock)
        {
            var forceMagnitude = CalculateForceMagnitude(particle1, particle2, distance);
            var forceDirection = displacement.Normalized;
            
            // Force on particle1 due to particle2
            var forceVector = forceDirection * forceMagnitude;
            
            OnForceCalculated(particle1, particle2, forceMagnitude, distance);
            
            return forceVector;
        }
    }

    /// <summary>
    /// Gets the interaction probability at a given energy and distance.
    /// Default implementation based on force strength and range.
    /// </summary>
    public virtual double GetInteractionProbability(IQuantumParticle particle1, IQuantumParticle particle2, 
        double energy, double distance)
    {
        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        ValidateEnergyAndDistance(energy, distance);

        // Simple model: probability decreases with distance and increases with energy
        var rangeFactor = Math.Exp(-distance / TypicalRange);
        var energyFactor = Math.Min(1.0, energy / (PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.SpeedOfLight / TypicalRange));
        
        return Math.Min(1.0, RelativeStrength * rangeFactor * energyFactor);
    }

    /// <summary>
    /// Determines the effective range of the force for given particles.
    /// Default implementation returns the typical range.
    /// </summary>
    public virtual double GetEffectiveRange(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        return TypicalRange;
    }

    /// <summary>
    /// Calculates quantum corrections to the classical force.
    /// Default implementation returns unity (no corrections).
    /// </summary>
    public virtual double CalculateQuantumCorrections(IQuantumParticle particle1, IQuantumParticle particle2, 
        double distance, double energy)
    {
        ValidateParticles(particle1, particle2);
        ValidateEnergyAndDistance(energy, distance);

        // Default: no quantum corrections (classical approximation)
        return 1.0;
    }

    /// <summary>
    /// Determines if quantum effects are significant at the given scale.
    /// Default implementation based on de Broglie wavelength.
    /// </summary>
    public virtual bool AreQuantumEffectsSignificant(double distance, double energy)
    {
        ValidateEnergyAndDistance(energy, distance);

        // Compare distance to reduced Compton wavelength scale
        var momentumScale = energy / PhysicsConstants.SpeedOfLight;
        if (momentumScale > 0)
        {
            var quantumScale = PhysicsConstants.ReducedPlanckConstant / momentumScale;
            return distance <= quantumScale;
        }

        return false;
    }

    /// <summary>
    /// Calculates the scattering cross section for particle interaction.
    /// Default implementation uses geometric cross section.
    /// </summary>
    public virtual double CalculateScatteringCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        ValidateEnergy(energy);

        // Geometric cross section based on interaction range
        var effectiveRange = GetEffectiveRange(particle1, particle2);
        return Math.PI * effectiveRange * effectiveRange;
    }

    /// <summary>
    /// Calculates the differential cross section for scattering at a given angle.
    /// Default implementation assumes isotropic scattering.
    /// </summary>
    public virtual double CalculateDifferentialCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, 
        double energy, double scatteringAngle)
    {
        var totalCrossSection = CalculateScatteringCrossSection(particle1, particle2, energy);
        
        // Isotropic scattering: dσ/dΩ = σ_total / 4π
        return totalCrossSection / (4.0 * Math.PI);
    }

    #endregion

    #region Protected Helper Methods

    /// <summary>
    /// Validates that particles are not null and are valid for calculations.
    /// </summary>
    protected static void ValidateParticles(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        if (particle1 == null)
            throw new ArgumentNullException(nameof(particle1));
        if (particle2 == null)
            throw new ArgumentNullException(nameof(particle2));
        if (particle1.Id == particle2.Id)
            throw new ArgumentException("Cannot calculate force between a particle and itself");
    }

    /// <summary>
    /// Validates position vectors for force calculations.
    /// </summary>
    protected static void ValidatePositions(Vector3D position1, Vector3D position2)
    {
        if (!position1.IsValidPhysicalVector())
            throw new ArgumentException("Invalid position vector for particle 1", nameof(position1));
        if (!position2.IsValidPhysicalVector())
            throw new ArgumentException("Invalid position vector for particle 2", nameof(position2));
    }

    /// <summary>
    /// Validates energy and distance parameters.
    /// </summary>
    protected static void ValidateEnergyAndDistance(double energy, double distance)
    {
        ValidateEnergy(energy);
        ValidateDistance(distance);
    }

    /// <summary>
    /// Validates energy parameter.
    /// </summary>
    protected static void ValidateEnergy(double energy)
    {
        // For quantum physics, allow very small but positive energies
        if (energy <= 0.0)
            throw new ArgumentException("Energy must be positive", nameof(energy));
        if (double.IsNaN(energy) || double.IsInfinity(energy))
            throw new ArgumentException("Energy must be a finite positive value", nameof(energy));
    }

    /// <summary>
    /// Validates distance parameter.
    /// </summary>
    protected static void ValidateDistance(double distance)
    {
        // For quantum physics, allow very small but positive distances
        if (distance <= 0.0)
            throw new ArgumentException("Distance must be positive", nameof(distance));
        if (double.IsNaN(distance) || double.IsInfinity(distance))
            throw new ArgumentException("Distance must be a finite positive value", nameof(distance));
    }

    /// <summary>
    /// Calculates the reduced mass of two particles.
    /// </summary>
    protected static double CalculateReducedMass(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        var m1 = particle1.Mass.Value;
        var m2 = particle2.Mass.Value;
        
        // Handle massless particles
        if (m1 <= 0 && m2 <= 0) return 0.0;
        if (m1 <= 0) return m2;
        if (m2 <= 0) return m1;
        
        return (m1 * m2) / (m1 + m2);
    }

    /// <summary>
    /// Converts energy from Joules to natural units (GeV).
    /// </summary>
    protected static double ConvertEnergyToGeV(double energyJoules)
    {
        return energyJoules / (1e9 * PhysicsConstants.ElementaryCharge);
    }

    /// <summary>
    /// Converts energy from natural units (GeV) to Joules.
    /// </summary>
    protected static double ConvertEnergyFromGeV(double energyGeV)
    {
        return energyGeV * 1e9 * PhysicsConstants.ElementaryCharge;
    }

    /// <summary>
    /// Calculates the relativistic factor γ = 1/√(1 - v²/c²).
    /// </summary>
    protected static double CalculateLorentzFactor(double velocity)
    {
        var beta = velocity / PhysicsConstants.SpeedOfLight;
        var betaSquared = beta * beta;
        
        if (betaSquared >= 1.0)
            throw new ArgumentException("Velocity cannot exceed speed of light", nameof(velocity));
        
        return 1.0 / Math.Sqrt(1.0 - betaSquared);
    }

    #endregion

    #region Events

    /// <summary>
    /// Event raised when a force calculation is completed.
    /// </summary>
    public event EventHandler<ForceInteractionEventArgs>? ForceCalculated;

    /// <summary>
    /// Raises the ForceCalculated event.
    /// </summary>
    protected virtual void OnForceCalculated(IQuantumParticle particle1, IQuantumParticle particle2, 
        double forceMagnitude, double distance)
    {
        ForceCalculated?.Invoke(this, new ForceInteractionEventArgs
        {
            Particle1 = particle1,
            Particle2 = particle2,
            Force = this,
            ForceMagnitude = forceMagnitude,
            Distance = distance,
            Energy = Math.Abs(forceMagnitude * distance), // Rough energy estimate
            Timestamp = DateTime.UtcNow
        });
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the force.
    /// </summary>
    public override string ToString()
    {
        return $"{Name} ({Symbol}) - Range: {TypicalRange:E2} m, Strength: {RelativeStrength:E2}";
    }

    #endregion
}
