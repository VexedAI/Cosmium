using System;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Forces;

/// <summary>
/// Implementation of the gravitational force according to General Relativity and classical approximations.
/// The weakest of the fundamental forces, but with infinite range.
/// At quantum scales, typically negligible compared to other forces.
/// </summary>
public class GravitationalForce : FundamentalForceBase
{
    #region Constants

    /// <summary>
    /// Typical range of gravitational force (effectively infinite).
    /// </summary>
    private const double TypicalGravitationalRange = double.PositiveInfinity;

    /// <summary>
    /// Relative strength of gravitational force compared to strong force.
    /// Extremely weak at particle physics scales.
    /// </summary>
    private const double GravitationalRelativeStrength = 6e-39;

    /// <summary>
    /// Planck mass scale where quantum gravitational effects become important.
    /// </summary>
    private static readonly double PlanckMassScale = PhysicsConstants.PlanckMass;

    /// <summary>
    /// Planck length scale where quantum gravity becomes significant.
    /// </summary>
    private static readonly double PlanckLengthScale = PhysicsConstants.PlanckLength;

    /// <summary>
    /// Planck energy scale for quantum gravitational processes.
    /// </summary>
    private static readonly double PlanckEnergyScale = PhysicsConstants.PlanckEnergy;

    /// <summary>
    /// Schwarzschild radius coefficient: rs = 2GM/c²
    /// </summary>
    private static readonly double SchwarzschildCoefficient = 2.0 * PhysicsConstants.GravitationalConstant / 
        (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the GravitationalForce class.
    /// </summary>
    public GravitationalForce() : base(
        name: "Gravitational Force",
        symbol: "F_g",
        typicalRange: TypicalGravitationalRange,
        relativeStrength: GravitationalRelativeStrength,
        character: ForceCharacter.AlwaysAttractive,
        mediatingBosons: new[] { "Graviton" })
    {
    }

    #endregion

    #region Force Implementation

    /// <summary>
    /// Calculates the gravitational force magnitude between two particles.
    /// Uses Newton's law of universal gravitation: F = G * m1 * m2 / r²
    /// Includes relativistic and quantum corrections when applicable.
    /// </summary>
    public override double CalculateForceMagnitude(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        ValidateParticles(particle1, particle2);
        ValidateDistance(distance);

        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        lock (_calculationLock)
        {
            var m1 = particle1.Mass.Value;
            var m2 = particle2.Mass.Value;

            // Handle massless particles
            if (m1 <= 0 || m2 <= 0)
                return 0.0;

            // Newton's law of universal gravitation: F = G * m1 * m2 / r²
            var newtonianForce = PhysicsConstants.GravitationalConstant * m1 * m2 / (distance * distance);

            // Apply corrections based on the regime
            var correctedForce = newtonianForce;

            // Check if relativistic corrections are needed
            if (IsRelativisticRegime(particle1, particle2, distance))
            {
                var relativisticCorrection = CalculateRelativisticCorrections(particle1, particle2, distance);
                correctedForce *= relativisticCorrection;
            }

            // Check if quantum gravitational corrections are needed
            if (IsQuantumGravityRegime(distance))
            {
                var quantumCorrection = CalculateQuantumGravitationalCorrections(particle1, particle2, distance);
                correctedForce *= quantumCorrection;
            }

            // Apply tidal effects for extended objects (simplified)
            if (IsExtendedObject(particle1) || IsExtendedObject(particle2))
            {
                var tidalCorrection = CalculateTidalCorrections(particle1, particle2, distance);
                correctedForce += tidalCorrection;
            }

            return Math.Abs(correctedForce); // Always attractive, so return positive magnitude
        }
    }

    /// <summary>
    /// Calculates the gravitational potential energy between two particles.
    /// Uses Newton's gravitational potential: U = -G * m1 * m2 / r
    /// </summary>
    public override double CalculatePotentialEnergy(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        ValidateParticles(particle1, particle2);
        ValidateDistance(distance);

        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        var m1 = particle1.Mass.Value;
        var m2 = particle2.Mass.Value;

        // Handle massless particles
        if (m1 <= 0 || m2 <= 0)
            return 0.0;

        // Newtonian gravitational potential: U = -G * m1 * m2 / r
        var newtonianPotential = -PhysicsConstants.GravitationalConstant * m1 * m2 / distance;

        // Apply post-Newtonian corrections if in relativistic regime
        if (IsRelativisticRegime(particle1, particle2, distance))
        {
            var postNewtonianCorrection = CalculatePostNewtonianPotential(particle1, particle2, distance);
            newtonianPotential += postNewtonianCorrection;
        }

        return newtonianPotential;
    }

    /// <summary>
    /// Determines if particles can interact gravitationally.
    /// All particles with mass interact gravitationally.
    /// </summary>
    public override bool CanParticlesInteract(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        if (particle1 == null || particle2 == null)
            return false;

        // All particles with mass interact gravitationally
        return particle1.Mass.Value > 0 && particle2.Mass.Value > 0;
    }

    /// <summary>
    /// Calculates the gravitational coupling constant.
    /// In quantum gravity, this is related to the Planck scale.
    /// </summary>
    public override double GetCouplingConstant(double energyScale)
    {
        ValidateEnergy(energyScale);

        // Gravitational coupling is energy-dependent in quantum gravity
        // At low energies, it's effectively zero due to weakness
        // At Planck scale, it becomes O(1)
        
        if (energyScale < PlanckEnergyScale * 1e-10) // Much below Planck scale
        {
            return GravitationalRelativeStrength;
        }
        else if (energyScale >= PlanckEnergyScale) // At or above Planck scale
        {
            return 1.0; // Quantum gravity regime
        }
        else // Intermediate regime
        {
            // Logarithmic interpolation
            var logRatio = Math.Log(energyScale / (PlanckEnergyScale * 1e-10)) / 
                          Math.Log(PlanckEnergyScale / (PlanckEnergyScale * 1e-10));
            return GravitationalRelativeStrength * Math.Pow(1.0 / GravitationalRelativeStrength, logRatio);
        }
    }

    #endregion

    #region General Relativity Effects

    /// <summary>
    /// Determines if the interaction is in the relativistic regime.
    /// This occurs when gravitational potential energy becomes comparable to rest mass energy.
    /// </summary>
    public bool IsRelativisticRegime(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        var m1 = particle1.Mass.Value;
        var m2 = particle2.Mass.Value;
        
        if (m1 <= 0 || m2 <= 0) return false;

        // Check if GM/rc² is significant (relativistic parameter)
        var totalMass = m1 + m2;
        var gravitationalRadius = PhysicsConstants.GravitationalConstant * totalMass / 
                                 (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);
        
        return distance <= 10.0 * gravitationalRadius; // Relativistic when within ~10 gravitational radii
    }

    /// <summary>
    /// Calculates relativistic corrections to Newtonian gravity.
    /// Includes post-Newtonian effects from General Relativity.
    /// </summary>
    public double CalculateRelativisticCorrections(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        var m1 = particle1.Mass.Value;
        var m2 = particle2.Mass.Value;
        var totalMass = m1 + m2;
        
        // Post-Newtonian parameter
        var epsilon = PhysicsConstants.GravitationalConstant * totalMass / 
                     (distance * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);
        
        if (epsilon < 1e-6) return 1.0; // Negligible correction
        
        // First-order post-Newtonian correction (simplified)
        // Real calculation involves orbital velocities and more complex terms
        return 1.0 + 2.5 * epsilon;
    }

    /// <summary>
    /// Calculates post-Newtonian potential corrections.
    /// </summary>
    public double CalculatePostNewtonianPotential(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        var m1 = particle1.Mass.Value;
        var m2 = particle2.Mass.Value;
        
        // Post-Newtonian potential correction (simplified)
        var correction = PhysicsConstants.GravitationalConstant * PhysicsConstants.GravitationalConstant * 
                        m1 * m2 * (m1 + m2) / 
                        (2.0 * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight * distance * distance);
        
        return correction;
    }

    /// <summary>
    /// Calculates the Schwarzschild radius for a given mass.
    /// </summary>
    public double CalculateSchwarzschildRadius(double mass)
    {
        if (mass <= 0) return 0.0;
        return SchwarzschildCoefficient * mass;
    }

    /// <summary>
    /// Determines if an object would form a black hole.
    /// </summary>
    public bool WouldFormBlackHole(IQuantumParticle particle, double characteristicSize)
    {
        var schwarzschildRadius = CalculateSchwarzschildRadius(particle.Mass.Value);
        return characteristicSize <= schwarzschildRadius;
    }

    /// <summary>
    /// Calculates gravitational time dilation factor.
    /// </summary>
    public double CalculateTimeDilation(IQuantumParticle source, double distance)
    {
        var mass = source.Mass.Value;
        if (mass <= 0) return 1.0;
        
        var gravitationalPotential = PhysicsConstants.GravitationalConstant * mass / distance;
        var timeDilationFactor = 1.0 - gravitationalPotential / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);
        
        return Math.Max(0.0, Math.Sqrt(Math.Abs(timeDilationFactor)));
    }

    #endregion

    #region Quantum Gravity Effects

    /// <summary>
    /// Determines if quantum gravitational effects are significant.
    /// This occurs at the Planck scale.
    /// </summary>
    public bool IsQuantumGravityRegime(double distance)
    {
        return distance <= PlanckLengthScale * 1000; // Significant within 1000 Planck lengths
    }

    /// <summary>
    /// Calculates quantum gravitational corrections.
    /// Highly speculative - actual quantum gravity theory is unknown.
    /// </summary>
    public double CalculateQuantumGravitationalCorrections(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        if (!IsQuantumGravityRegime(distance))
            return 1.0;

        // Speculative quantum gravity correction
        // In real quantum gravity, this would involve gravitons, extra dimensions, etc.
        var planckRatio = distance / PlanckLengthScale;
        
        if (planckRatio < 1.0)
        {
            // Deep quantum regime - large corrections
            return 1.0 + Math.Exp(-1.0 / planckRatio);
        }
        else
        {
            // Transitional regime - small corrections
            return 1.0 + PlanckLengthScale / distance;
        }
    }

    /// <summary>
    /// Calculates hypothetical graviton exchange effects.
    /// </summary>
    public double CalculateGravitonExchangeProbability(double energy)
    {
        // Graviton coupling is extremely weak
        var planckEnergyRatio = energy / PlanckEnergyScale;
        
        if (planckEnergyRatio < 1e-10)
        {
            return 0.0; // Effectively zero at low energies
        }
        else if (planckEnergyRatio > 1.0)
        {
            return 1.0; // Significant at Planck energies
        }
        else
        {
            return planckEnergyRatio * planckEnergyRatio; // Quadratic dependence
        }
    }

    #endregion

    #region Tidal Effects and Extended Objects

    /// <summary>
    /// Determines if a particle should be treated as an extended object.
    /// </summary>
    private static bool IsExtendedObject(IQuantumParticle particle)
    {
        // For this implementation, treat composite particles as potentially extended
        return !particle.IsElementary;
    }

    /// <summary>
    /// Calculates tidal force corrections for extended objects.
    /// </summary>
    public double CalculateTidalCorrections(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        // Simplified tidal force calculation
        // Real calculation would require knowledge of object size and mass distribution
        
        var m1 = particle1.Mass.Value;
        var m2 = particle2.Mass.Value;
        
        // Assume characteristic size ~ reduced Compton wavelength
        var size1 = PhysicsConstants.ReducedPlanckConstant / (m1 * PhysicsConstants.SpeedOfLight);
        var size2 = PhysicsConstants.ReducedPlanckConstant / (m2 * PhysicsConstants.SpeedOfLight);
        
        // Tidal force ~ GM * R / r³ where R is object size
        var characteristicSize = Math.Max(size1, size2);
        var tidalForce = PhysicsConstants.GravitationalConstant * m1 * m2 * characteristicSize / 
                        (distance * distance * distance);
        
        return tidalForce;
    }

    /// <summary>
    /// Calculates gravitational wave emission rate (order of magnitude).
    /// </summary>
    public double CalculateGravitationalWaveEmission(IQuantumParticle particle1, IQuantumParticle particle2, 
        double distance, double relativeVelocity)
    {
        var m1 = particle1.Mass.Value;
        var m2 = particle2.Mass.Value;
        var reducedMass = (m1 * m2) / (m1 + m2);
        var totalMass = m1 + m2;
        
        // Quadrupole formula (highly simplified)
        // P = (32/5) * (G/c⁵) * (μ²M³/r⁵) * v⁶
        var gravitationalConstantPower = Math.Pow(PhysicsConstants.GravitationalConstant, 4.0);
        var speedOfLightPower = Math.Pow(PhysicsConstants.SpeedOfLight, 5.0);
        var velocityPower = Math.Pow(relativeVelocity, 6.0);
        var distancePower = Math.Pow(distance, 5.0);
        
        var power = (32.0 / 5.0) * gravitationalConstantPower * 
                   (reducedMass * reducedMass * totalMass * totalMass * totalMass) /
                   (speedOfLightPower * distancePower) * velocityPower;
        
        return power;
    }

    #endregion

    #region Cosmological Effects

    /// <summary>
    /// Calculates the effect of dark energy (cosmological constant) on gravitational interactions.
    /// Only significant at very large scales.
    /// </summary>
    public double CalculateCosmologicalConstantEffect(double distance)
    {
        // Cosmological constant effect becomes significant at distances ~ Hubble radius
        var hubbleRadius = PhysicsConstants.SpeedOfLight / (70e3 * 3.086e22); // ~c/H₀
        
        if (distance < hubbleRadius / 1000) return 0.0; // Negligible at small scales
        
        // Simplified cosmological acceleration
        var cosmologicalAcceleration = distance / (hubbleRadius * hubbleRadius);
        return cosmologicalAcceleration;
    }

    /// <summary>
    /// Estimates dark matter contribution to gravitational interactions.
    /// </summary>
    public double EstimateDarkMatterContribution(double distance, double localDensity)
    {
        // Dark matter contributes to gravitational potential
        // Simplified model: additional mass within interaction sphere
        var interactionVolume = (4.0 / 3.0) * Math.PI * distance * distance * distance;
        var additionalMass = localDensity * interactionVolume;
        
        return PhysicsConstants.GravitationalConstant * additionalMass / (distance * distance);
    }

    #endregion

    #region Cross Sections and Scattering

    /// <summary>
    /// Calculates gravitational scattering cross section.
    /// Very small for particle interactions, but non-zero in principle.
    /// </summary>
    public override double CalculateScatteringCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        ValidateEnergy(energy);

        // Gravitational scattering is extremely weak
        var planckEnergyRatio = energy / PlanckEnergyScale;
        
        if (planckEnergyRatio < 1e-20)
        {
            return 0.0; // Effectively zero
        }
        
        // Order of magnitude estimate: σ ~ (ℏG/c³) * (E/Mₚc²)²
        var planckArea = PlanckLengthScale * PlanckLengthScale;
        return planckArea * planckEnergyRatio * planckEnergyRatio;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Gets the singleton instance of the gravitational force.
    /// </summary>
    public static GravitationalForce Instance { get; } = new GravitationalForce();

    #endregion
}
