using System;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Forces;

/// <summary>
/// Implementation of the weak nuclear force according to the Electroweak Theory.
/// Mediates flavor-changing interactions via W and Z bosons.
/// Responsible for radioactive decay, neutrino interactions, and electroweak processes.
/// </summary>
public class WeakForce : FundamentalForceBase
{
    #region Constants

    /// <summary>
    /// Typical range of weak force (W boson Compton wavelength).
    /// </summary>
    private static readonly double TypicalWeakRange = PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.SpeedOfLight / (80.4e9 * PhysicsConstants.ElementaryCharge); // W boson mass ≈ 80.4 GeV

    /// <summary>
    /// Relative strength of weak force compared to strong force.
    /// </summary>
    private const double WeakRelativeStrength = 1e-13;

    /// <summary>
    /// W boson mass in kg.
    /// </summary>
    private static readonly double WBosonMass = 80.4e9 * PhysicsConstants.ElementaryCharge / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // 80.4 GeV/c²

    /// <summary>
    /// Z boson mass in kg.
    /// </summary>
    private static readonly double ZBosonMass = 91.2e9 * PhysicsConstants.ElementaryCharge / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // 91.2 GeV/c²

    /// <summary>
    /// Weinberg angle (weak mixing angle).
    /// </summary>
    private static readonly double WeinbergAngle = Math.Asin(Math.Sqrt(PhysicsConstants.WeakMixingAngleSinSquared));

    /// <summary>
    /// Electroweak scale (W boson mass energy).
    /// </summary>
    private static readonly double ElectroweakScale = 80.4e9 * PhysicsConstants.ElementaryCharge; // 80.4 GeV

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WeakForce class.
    /// </summary>
    public WeakForce() : base(
        name: "Weak Nuclear Force",
        symbol: "F_w",
        typicalRange: TypicalWeakRange,
        relativeStrength: WeakRelativeStrength,
        character: ForceCharacter.AttractiveOrRepulsive,
        mediatingBosons: new[] { "W⁺", "W⁻", "Z⁰" })
    {
    }

    #endregion

    #region Force Implementation

    /// <summary>
    /// Calculates the weak force magnitude between two particles.
    /// Uses Yukawa-like potential with massive gauge bosons.
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
            
            // Determine interaction type (charged current or neutral current)
            if (IsChargedCurrentInteraction(particle1, particle2))
            {
                return CalculateChargedCurrentForce(particle1, particle2, distance, energy);
            }
            else if (IsNeutralCurrentInteraction(particle1, particle2))
            {
                return CalculateNeutralCurrentForce(particle1, particle2, distance, energy);
            }
            
            return 0.0;
        }
    }

    /// <summary>
    /// Calculates the weak force potential energy.
    /// Uses Yukawa potential: V(r) = -g²/(4πr) * exp(-mr/ℏ)
    /// </summary>
    public override double CalculatePotentialEnergy(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        ValidateParticles(particle1, particle2);
        ValidateDistance(distance);

        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        var coupling = GetCouplingConstant(EstimateInteractionEnergy(distance));
        
        // Determine which boson mediates the interaction
        double bosonMass;
        if (IsChargedCurrentInteraction(particle1, particle2))
        {
            bosonMass = WBosonMass;
        }
        else if (IsNeutralCurrentInteraction(particle1, particle2))
        {
            bosonMass = ZBosonMass;
        }
        else
        {
            return 0.0;
        }

        // Yukawa potential: V(r) = -g²/(4πr) * exp(-mr/ℏ)
        var yukawaMass = bosonMass * PhysicsConstants.SpeedOfLight / PhysicsConstants.ReducedPlanckConstant;
        var exponentialTerm = Math.Exp(-yukawaMass * distance);
        
        return -coupling * coupling / (4.0 * Math.PI * distance) * exponentialTerm;
    }

    /// <summary>
    /// Determines if particles can interact via the weak force.
    /// All fermions (except photons) can participate in weak interactions.
    /// </summary>
    public override bool CanParticlesInteract(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        if (particle1 == null || particle2 == null)
            return false;

        // Photons don't participate in weak interactions
        if (particle1.Name == "Photon" || particle2.Name == "Photon")
            return false;

        // All other particles can interact weakly (fermions and weak gauge bosons)
        return IsWeakInteractingParticle(particle1) && IsWeakInteractingParticle(particle2);
    }

    /// <summary>
    /// Calculates the weak coupling constant.
    /// Related to Fermi constant and electroweak parameters.
    /// </summary>
    public override double GetCouplingConstant(double energyScale)
    {
        ValidateEnergy(energyScale);

        var energyGeV = ConvertEnergyToGeV(energyScale);
        
        // At low energies, use Fermi constant relationship
        if (energyGeV < 1.0) // Below 1 GeV
        {
            // g_w = √(8√2 G_F M_W²)
            var gw = Math.Sqrt(8.0 * Math.Sqrt(2.0) * PhysicsConstants.FermiCouplingConstant * WBosonMass * WBosonMass);
            return gw / (4.0 * Math.PI); // Convert to dimensionless coupling
        }
        
        // At high energies, use electroweak theory
        var alpha = PhysicsConstants.FineStructureConstant;
        var sinSquaredTheta = PhysicsConstants.WeakMixingAngleSinSquared;
        
        // Weak coupling: g_w = e / sin(θ_W)
        var gwHigh = Math.Sqrt(4.0 * Math.PI * alpha) / Math.Sqrt(sinSquaredTheta);
        
        // Apply running coupling corrections (simplified)
        if (energyGeV > 80.0) // Above electroweak scale
        {
            var logTerm = Math.Log(energyGeV / 80.0);
            gwHigh *= (1.0 + alpha * logTerm / (12.0 * Math.PI));
        }
        
        return gwHigh / (4.0 * Math.PI);
    }

    #endregion

    #region Weak Interaction Types

    /// <summary>
    /// Determines if the interaction involves charged current (W boson exchange).
    /// Changes particle flavor/type.
    /// </summary>
    public bool IsChargedCurrentInteraction(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        // Charged current interactions change particle type
        // Examples: e + ν_e ↔ e + ν_e (via W), d ↔ u (via W)
        
        // Check for lepton interactions
        if (IsLepton(particle1) && IsLepton(particle2))
        {
            return AreChargedCurrentPartners(particle1, particle2);
        }
        
        // Check for quark interactions
        if (IsQuark(particle1) && IsQuark(particle2))
        {
            return AreChargedCurrentPartners(particle1, particle2);
        }
        
        return false;
    }

    /// <summary>
    /// Determines if the interaction involves neutral current (Z boson exchange).
    /// Preserves particle flavor/type.
    /// </summary>
    public bool IsNeutralCurrentInteraction(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        // Neutral current interactions preserve particle type
        // All fermions can interact via Z boson
        return (IsLepton(particle1) && IsLepton(particle2)) || 
               (IsQuark(particle1) && IsQuark(particle2)) ||
               (IsNeutrino(particle1) && IsNeutrino(particle2));
    }

    /// <summary>
    /// Calculates the decay rate for weak decays.
    /// Uses Fermi's golden rule with weak matrix elements.
    /// </summary>
    public double CalculateDecayRate(IQuantumParticle parent, IQuantumParticle[] products, double qValue)
    {
        if (parent == null || products == null || products.Length == 0)
            return 0.0;

        ValidateEnergy(qValue);

        var coupling = GetCouplingConstant(qValue);
        
        // Phase space factor depends on number of final particles
        var phaseSpaceFactor = CalculatePhaseSpaceFactor(products, qValue);
        
        // Matrix element squared (simplified)
        var matrixElementSquared = coupling * coupling;
        
        // Fermi's golden rule: Γ = (2π/ℏ) |M|² ρ(E)
        return (2.0 * Math.PI / PhysicsConstants.ReducedPlanckConstant) * 
               matrixElementSquared * phaseSpaceFactor;
    }

    /// <summary>
    /// Calculates beta decay probability for nuclear processes.
    /// </summary>
    public double CalculateBetaDecayProbability(double qValue, double time)
    {
        if (qValue <= 0 || time <= 0) return 0.0;
        
        var decayRate = CalculateDecayRate(null!, Array.Empty<IQuantumParticle>(), qValue);
        
        // Exponential decay: P = 1 - exp(-Γt)
        return 1.0 - Math.Exp(-decayRate * time);
    }

    /// <summary>
    /// Calculates neutrino interaction cross section.
    /// Neutrinos interact very weakly with matter.
    /// </summary>
    public double CalculateNeutrinoInteractionCrossSection(double energy)
    {
        ValidateEnergy(energy);
        
        var energyGeV = ConvertEnergyToGeV(energy);
        
        // Neutrino cross section: σ ∝ G_F² E
        var crossSection = PhysicsConstants.FermiCouplingConstant * PhysicsConstants.FermiCouplingConstant * 
                          energyGeV * 1e9 * PhysicsConstants.ElementaryCharge;
        
        // Convert to proper units (m²)
        return crossSection / (PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.SpeedOfLight);
    }

    #endregion

    #region Electroweak Unification

    /// <summary>
    /// Calculates the electroweak mixing parameter.
    /// Relates electromagnetic and weak interactions.
    /// </summary>
    public double CalculateElectroweakMixing(double energy)
    {
        var energyGeV = ConvertEnergyToGeV(energy);
        
        // Running of sin²θ_W
        var sinSquaredTheta = PhysicsConstants.WeakMixingAngleSinSquared;
        
        if (energyGeV > 80.0) // Above electroweak scale
        {
            var alpha = PhysicsConstants.FineStructureConstant;
            var logTerm = Math.Log(energyGeV / 80.0);
            
            // One-loop running (simplified)
            sinSquaredTheta += alpha * logTerm / (6.0 * Math.PI);
        }
        
        return Math.Max(0.1, Math.Min(0.5, sinSquaredTheta));
    }

    /// <summary>
    /// Determines if particles are at energies where electroweak unification is manifest.
    /// </summary>
    public bool IsElectroweakUnified(double energy)
    {
        return energy > ElectroweakScale;
    }

    /// <summary>
    /// Calculates the W boson propagator for virtual exchange.
    /// </summary>
    public double CalculateWBosonPropagator(double energy, double momentum)
    {
        var q2 = energy * energy / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight) - momentum * momentum;
        var mw2 = WBosonMass * WBosonMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
        
        // Propagator: 1/(q² - mW²)
        return 1.0 / (q2 - mw2);
    }

    /// <summary>
    /// Calculates the Z boson propagator for virtual exchange.
    /// </summary>
    public double CalculateZBosonPropagator(double energy, double momentum)
    {
        var q2 = energy * energy / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight) - momentum * momentum;
        var mz2 = ZBosonMass * ZBosonMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
        
        // Propagator: 1/(q² - mZ²)
        return 1.0 / (q2 - mz2);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Estimates interaction energy from distance.
    /// </summary>
    private static double EstimateInteractionEnergy(double distance)
    {
        return PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.SpeedOfLight / distance;
    }

    /// <summary>
    /// Calculates charged current force (W boson mediated).
    /// </summary>
    private double CalculateChargedCurrentForce(IQuantumParticle particle1, IQuantumParticle particle2, double distance, double energy)
    {
        var coupling = GetCouplingConstant(energy);
        var yukawaMass = WBosonMass * PhysicsConstants.SpeedOfLight / PhysicsConstants.ReducedPlanckConstant;
        
        // Yukawa force: F = -dV/dr
        var exponentialTerm = Math.Exp(-yukawaMass * distance);
        var force = coupling * coupling / (4.0 * Math.PI * distance * distance) * 
                   exponentialTerm * (1.0 + yukawaMass * distance);
        
        return force;
    }

    /// <summary>
    /// Calculates neutral current force (Z boson mediated).
    /// </summary>
    private double CalculateNeutralCurrentForce(IQuantumParticle particle1, IQuantumParticle particle2, double distance, double energy)
    {
        var coupling = GetCouplingConstant(energy);
        var yukawaMass = ZBosonMass * PhysicsConstants.SpeedOfLight / PhysicsConstants.ReducedPlanckConstant;
        
        // Apply weak charges (vector and axial couplings)
        var weakCharge1 = CalculateWeakCharge(particle1);
        var weakCharge2 = CalculateWeakCharge(particle2);
        
        var exponentialTerm = Math.Exp(-yukawaMass * distance);
        var force = coupling * coupling * weakCharge1 * weakCharge2 / (4.0 * Math.PI * distance * distance) * 
                   exponentialTerm * (1.0 + yukawaMass * distance);
        
        return force;
    }

    /// <summary>
    /// Determines if a particle participates in weak interactions.
    /// </summary>
    private static bool IsWeakInteractingParticle(IQuantumParticle particle)
    {
        // All fermions participate in weak interactions
        return particle.IsFermion || 
               particle.Name.Contains("W") || 
               particle.Name.Contains("Z") ||
               IsLepton(particle) || 
               IsQuark(particle);
    }

    /// <summary>
    /// Determines if a particle is a lepton.
    /// </summary>
    private static bool IsLepton(IQuantumParticle particle)
    {
        return particle.Name.Contains("Electron") || 
               particle.Name.Contains("Muon") || 
               particle.Name.Contains("Tau") || 
               particle.Name.Contains("Neutrino");
    }

    /// <summary>
    /// Determines if a particle is a quark.
    /// </summary>
    private static bool IsQuark(IQuantumParticle particle)
    {
        return particle.Name.Contains("Quark") || 
               particle.GetType().Name.Contains("Quark");
    }

    /// <summary>
    /// Determines if a particle is a neutrino.
    /// </summary>
    private static bool IsNeutrino(IQuantumParticle particle)
    {
        return particle.Name.Contains("Neutrino");
    }

    /// <summary>
    /// Determines if two particles are charged current partners.
    /// </summary>
    private static bool AreChargedCurrentPartners(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        // Simplified check - in reality this would involve detailed flavor analysis
        return (IsLepton(particle1) && IsNeutrino(particle2)) ||
               (IsNeutrino(particle1) && IsLepton(particle2)) ||
               (IsQuark(particle1) && IsQuark(particle2)); // Different flavors
    }

    /// <summary>
    /// Calculates the weak charge of a particle.
    /// </summary>
    private static double CalculateWeakCharge(IQuantumParticle particle)
    {
        // Simplified weak charge calculation
        // Real calculation involves vector and axial-vector couplings
        
        if (IsLepton(particle))
        {
            return particle.IsFermion ? -0.5 : 0.5; // T3 - Q sin²θW
        }
        else if (IsQuark(particle))
        {
            // Up-type quarks: +0.5, Down-type quarks: -0.5
            return particle.Charge.Value > 0 ? 0.5 : -0.5;
        }
        
        return 0.0;
    }

    /// <summary>
    /// Calculates phase space factor for decay processes.
    /// </summary>
    private static double CalculatePhaseSpaceFactor(IQuantumParticle[] products, double qValue)
    {
        if (products.Length == 2) // Two-body decay
        {
            return qValue * qValue / (8.0 * Math.PI);
        }
        else if (products.Length == 3) // Three-body decay
        {
            return qValue * qValue * qValue * qValue * qValue / (256.0 * Math.PI * Math.PI * Math.PI);
        }
        
        return 1.0; // Simplified for other cases
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Gets the singleton instance of the weak force.
    /// </summary>
    public static WeakForce Instance { get; } = new WeakForce();

    #endregion
}
