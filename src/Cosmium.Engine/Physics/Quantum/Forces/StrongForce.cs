using System;
using System.Linq;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Bosons;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

namespace Cosmium.Engine.Physics.Quantum.Forces;

/// <summary>
/// Implementation of the strong nuclear force according to Quantum Chromodynamics (QCD).
/// Mediates interactions between quarks and gluons via color charge exchange.
/// Implements confinement, asymptotic freedom, and running coupling effects.
/// </summary>
public class StrongForce : FundamentalForceBase
{
    #region Constants

    /// <summary>
    /// Typical range of strong force (confinement scale).
    /// </summary>
    private const double TypicalStrongRange = 1e-15; // 1 femtometer

    /// <summary>
    /// Relative strength of strong force (set as reference = 1).
    /// </summary>
    private const double StrongRelativeStrength = 1.0;

    /// <summary>
    /// QCD string tension (force required to separate quarks).
    /// Approximately 1 GeV/fm.
    /// </summary>
    private static readonly double StringTension = 1e9 * PhysicsConstants.ElementaryCharge / 1e-15; // 1 GeV/fm in SI

    /// <summary>
    /// QCD scale parameter (ΛQCD).
    /// Energy scale where strong coupling becomes large.
    /// </summary>
    private static readonly double QcdScale = 200e6 * PhysicsConstants.ElementaryCharge; // ~200 MeV in Joules

    /// <summary>
    /// Confinement scale - energy below which quarks are confined.
    /// </summary>
    private static readonly double ConfinementScale = 1e9 * PhysicsConstants.ElementaryCharge; // ~1 GeV in Joules

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the StrongForce class.
    /// </summary>
    public StrongForce() : base(
        name: "Strong Nuclear Force",
        symbol: "F_s",
        typicalRange: TypicalStrongRange,
        relativeStrength: StrongRelativeStrength,
        character: ForceCharacter.Complex,
        mediatingBosons: "Gluon")
    {
    }

    #endregion

    #region Force Implementation

    /// <summary>
    /// Calculates the strong force magnitude between two particles.
    /// Uses QCD-based calculations with confinement and running coupling.
    /// </summary>
    public override double CalculateForceMagnitude(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        ValidateParticles(particle1, particle2);
        ValidateDistance(distance);

        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        lock (_calculationLock)
        {
            var energy = EstimateInteractionEnergy(distance);
            var coupling = GetCouplingConstant(energy);

            // Determine force regime
            if (distance < GetPerturbativeRegimeThreshold())
            {
                // Short-distance: perturbative QCD (asymptotic freedom)
                return CalculatePerturbativeQcdForce(particle1, particle2, distance, coupling);
            }
            else
            {
                // Long-distance: confinement regime
                return CalculateConfinementForce(particle1, particle2, distance);
            }
        }
    }

    /// <summary>
    /// Calculates the strong force potential energy.
    /// Includes both Coulomb-like short-distance and linear confinement terms.
    /// </summary>
    public override double CalculatePotentialEnergy(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        ValidateParticles(particle1, particle2);
        ValidateDistance(distance);

        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        var energy = EstimateInteractionEnergy(distance);
        var coupling = GetCouplingConstant(energy);

        // QCD potential: V(r) = -4αs/(3r) + σr + constant
        // Short-distance Coulomb-like term + long-distance linear confinement
        var coulombTerm = CalculateCoulombLikePotential(particle1, particle2, distance, coupling);
        var confinementTerm = CalculateConfinementPotential(distance);

        return coulombTerm + confinementTerm;
    }

    /// <summary>
    /// Determines if particles can interact via the strong force.
    /// Only quarks and gluons participate in strong interactions.
    /// </summary>
    public override bool CanParticlesInteract(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        if (particle1 == null || particle2 == null)
            return false;

        // Strong force acts on quarks and gluons
        return IsStrongInteractingParticle(particle1) && IsStrongInteractingParticle(particle2);
    }

    /// <summary>
    /// Calculates the running strong coupling constant αs(Q²).
    /// Implements QCD beta function with asymptotic freedom.
    /// </summary>
    public override double GetCouplingConstant(double energyScale)
    {
        ValidateEnergy(energyScale);

        var energyGeV = ConvertEnergyToGeV(energyScale);
        
        // Reference values
        const double referenceEnergyGeV = 91.2; // Z boson mass
        const double referenceCoupling = 0.118; // αs(MZ)
        
        // QCD beta function (one-loop approximation)
        const int nf = 6; // Number of active quark flavors
        var beta0 = (33.0 - 2.0 * nf) / (12.0 * Math.PI); // First beta function coefficient
        
        if (energyGeV <= 0.2) // Below QCD scale
        {
            // In the confinement regime, coupling becomes large
            return Math.Min(10.0, 4.0 * Math.PI); // Cap at reasonable value
        }
        
        var logRatio = Math.Log(energyGeV * energyGeV / (referenceEnergyGeV * referenceEnergyGeV));
        var runningCoupling = referenceCoupling / (1.0 + beta0 * referenceCoupling * logRatio);
        
        // Ensure physical range
        return Math.Max(0.1, Math.Min(4.0 * Math.PI, runningCoupling));
    }

    #endregion

    #region QCD-Specific Methods

    /// <summary>
    /// Calculates the color factor for quark-quark interaction.
    /// Depends on the specific color charges of the quarks.
    /// </summary>
    public double CalculateColorFactor(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        if (particle1 is Quark quark1 && particle2 is Quark quark2)
        {
            // CF = (N²-1)/(2N) = 4/3 for fundamental representation of SU(3)
            if (quark1.Color == quark2.Color)
                return 0.0; // Same color quarks don't interact directly
            
            return 4.0 / 3.0; // Different color quarks
        }
        else if (particle1 is Gluon || particle2 is Gluon)
        {
            // CA = N = 3 for adjoint representation of SU(3)
            return 3.0;
        }
        
        return 1.0; // Default factor
    }

    /// <summary>
    /// Calculates the confinement force between quarks.
    /// Uses linear potential model: F = σ (string tension).
    /// </summary>
    public double CalculateConfinementForce(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        if (!AreQuarks(particle1, particle2))
            return 0.0;

        // Linear confinement: F = σ (constant force)
        var colorFactor = CalculateColorFactor(particle1, particle2);
        return StringTension * colorFactor;
    }

    /// <summary>
    /// Calculates the perturbative QCD force for short distances.
    /// Uses Coulomb-like potential with running coupling.
    /// </summary>
    public double CalculatePerturbativeQcdForce(IQuantumParticle particle1, IQuantumParticle particle2, 
        double distance, double coupling)
    {
        var colorFactor = CalculateColorFactor(particle1, particle2);
        
        // QCD Coulomb force: F = 4αs CF/(3r²)
        return 4.0 * coupling * colorFactor / (3.0 * distance * distance);
    }

    /// <summary>
    /// Determines if the interaction is in the perturbative regime.
    /// </summary>
    public bool IsInPerturbativeRegime(double distance, double energy)
    {
        return distance < GetPerturbativeRegimeThreshold() && energy > QcdScale;
    }

    /// <summary>
    /// Gets the threshold distance below which perturbative QCD applies.
    /// </summary>
    public double GetPerturbativeRegimeThreshold()
    {
        // Roughly 1/(ΛQCD) ≈ 1 fm
        return PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.SpeedOfLight / QcdScale;
    }

    /// <summary>
    /// Calculates gluon self-interaction effects.
    /// Unlike photons, gluons carry color charge and can interact with themselves.
    /// </summary>
    public double CalculateGluonSelfInteraction(Gluon gluon1, Gluon gluon2, Gluon gluon3, double energy)
    {
        if (gluon1 == null || gluon2 == null || gluon3 == null)
            return 0.0;

        var coupling = GetCouplingConstant(energy);
        
        // Three-gluon vertex strength
        return coupling; // Simplified - actual calculation involves color algebra
    }

    /// <summary>
    /// Estimates hadronization probability for quark pairs.
    /// Quarks cannot exist as free particles and must form color-neutral hadrons.
    /// </summary>
    public double CalculateHadronizationProbability(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        if (!AreQuarks(particle1, particle2))
            return 0.0;

        // Hadronization becomes inevitable at large distances
        var confinementDistance = GetPerturbativeRegimeThreshold();
        
        if (distance < confinementDistance)
            return 0.1; // Low probability at short distances
        
        // Exponential approach to certainty
        var exponent = (distance - confinementDistance) / confinementDistance;
        return 1.0 - Math.Exp(-exponent);
    }

    /// <summary>
    /// Calculates jet formation probability in high-energy collisions.
    /// </summary>
    public double CalculateJetFormationProbability(double energy, double angle)
    {
        var energyGeV = ConvertEnergyToGeV(energy);
        
        if (energyGeV < 1.0) return 0.0; // Too low energy for jet formation
        
        var coupling = GetCouplingConstant(energy);
        
        // Simplified jet probability based on QCD radiation patterns
        var radiationProbability = coupling * Math.Log(energyGeV) / Math.PI;
        var angularFactor = Math.Sin(angle); // Jets prefer forward/backward directions
        
        return Math.Min(1.0, radiationProbability * angularFactor);
    }

    #endregion

    #region Confinement and Deconfinement

    /// <summary>
    /// Determines if quarks are in the confinement regime.
    /// </summary>
    public bool IsInConfinementRegime(double distance, double energy)
    {
        return distance > GetPerturbativeRegimeThreshold() || energy < ConfinementScale;
    }

    /// <summary>
    /// Calculates the deconfinement temperature (QCD phase transition).
    /// </summary>
    public double GetDeconfinementTemperature()
    {
        // QCD deconfinement temperature ≈ 170 MeV
        var deconfinementEnergyGeV = 0.17; // 170 MeV
        var deconfinementEnergy = ConvertEnergyFromGeV(deconfinementEnergyGeV);
        
        // Convert energy to temperature: E = kBT
        return deconfinementEnergy / PhysicsConstants.BoltzmannConstant;
    }

    /// <summary>
    /// Determines if the system is in the quark-gluon plasma phase.
    /// </summary>
    public bool IsQuarkGluonPlasma(double temperature, double density)
    {
        var criticalTemperature = GetDeconfinementTemperature();
        var criticalDensity = CalculateCriticalDensity();
        
        return temperature > criticalTemperature || density > criticalDensity;
    }

    /// <summary>
    /// Calculates the critical density for deconfinement.
    /// </summary>
    public double CalculateCriticalDensity()
    {
        // Nuclear density ≈ 0.17 fm⁻³ ≈ 2.3 × 10¹⁷ kg/m³
        return 2.3e17; // kg/m³
    }

    #endregion

    #region Cross Sections and Scattering

    /// <summary>
    /// Calculates QCD scattering cross section.
    /// Depends on energy regime and process type.
    /// </summary>
    public override double CalculateScatteringCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        ValidateEnergy(energy);

        var energyGeV = ConvertEnergyToGeV(energy);
        
        if (energyGeV < 1.0) // Low energy: use geometric cross section
        {
            return Math.PI * TypicalRange * TypicalRange;
        }
        else // High energy: perturbative QCD
        {
            return CalculatePerturbativeQcdCrossSection(particle1, particle2, energy);
        }
    }

    /// <summary>
    /// Calculates perturbative QCD cross section for high-energy processes.
    /// </summary>
    private double CalculatePerturbativeQcdCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        var coupling = GetCouplingConstant(energy);
        var energyGeV = ConvertEnergyToGeV(energy);
        
        // Simplified QCD cross section: σ ∝ αs²/s
        var colorFactor = CalculateColorFactor(particle1, particle2);
        
        return colorFactor * coupling * coupling / energyGeV;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Determines if a particle participates in strong interactions.
    /// </summary>
    private static bool IsStrongInteractingParticle(IQuantumParticle particle)
    {
        return particle is Quark || particle is Gluon;
    }

    /// <summary>
    /// Determines if both particles are quarks.
    /// </summary>
    private static bool AreQuarks(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        return particle1 is Quark && particle2 is Quark;
    }

    /// <summary>
    /// Estimates interaction energy from distance using uncertainty principle.
    /// </summary>
    private static double EstimateInteractionEnergy(double distance)
    {
        // E ~ ℏc/r (uncertainty principle estimate)
        return PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.SpeedOfLight / distance;
    }

    /// <summary>
    /// Calculates the Coulomb-like potential term in QCD.
    /// </summary>
    private static double CalculateCoulombLikePotential(IQuantumParticle particle1, IQuantumParticle particle2, 
        double distance, double coupling)
    {
        // V_Coulomb = -4αs/(3r) for quark-quark interaction
        var colorFactor = 4.0 / 3.0; // CF for quarks
        
        if (particle1 is Gluon || particle2 is Gluon)
        {
            colorFactor = 3.0; // CA for gluons
        }
        
        return -coupling * colorFactor / distance;
    }

    /// <summary>
    /// Calculates the linear confinement potential term.
    /// </summary>
    private static double CalculateConfinementPotential(double distance)
    {
        // V_confinement = σr (linear potential)
        return StringTension * distance;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Gets the singleton instance of the strong force.
    /// </summary>
    public static StrongForce Instance { get; } = new StrongForce();

    #endregion
}
