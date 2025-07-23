using System;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Forces;

/// <summary>
/// Implementation of the electromagnetic force according to Quantum Electrodynamics (QED).
/// Mediates interactions between electrically charged particles via photon exchange.
/// Implements both classical Coulomb interactions and quantum electrodynamic effects.
/// </summary>
public class ElectromagneticForce : FundamentalForceBase
{
    #region Constants

    /// <summary>
    /// Typical range of electromagnetic force (effectively infinite, but use a practical scale).
    /// </summary>
    private const double TypicalElectromagneticRange = 1e10; // 10 billion meters

    /// <summary>
    /// Relative strength of electromagnetic force compared to strong force.
    /// </summary>
    private const double ElectromagneticRelativeStrength = 1.0 / 137.0; // ≈ α

    /// <summary>
    /// Classical electron radius used in QED calculations.
    /// </summary>
    private static readonly double ClassicalElectronRadius = PhysicsConstants.ClassicalElectronRadius;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the ElectromagneticForce class.
    /// </summary>
    public ElectromagneticForce() : base(
        name: "Electromagnetic Force",
        symbol: "F_em",
        typicalRange: TypicalElectromagneticRange,
        relativeStrength: ElectromagneticRelativeStrength,
        character: ForceCharacter.AttractiveOrRepulsive,
        mediatingBosons: "Photon")
    {
    }

    #endregion

    #region Force Implementation

    /// <summary>
    /// Calculates the electromagnetic force magnitude between two charged particles.
    /// Uses Coulomb's law: F = k * |q1 * q2| / r²
    /// </summary>
    public override double CalculateForceMagnitude(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        ValidateParticles(particle1, particle2);
        ValidateDistance(distance);

        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        lock (_calculationLock)
        {
            var q1 = particle1.Charge.Value;
            var q2 = particle2.Charge.Value;

            // Coulomb's law: F = k * q1 * q2 / r²
            var forceMagnitude = PhysicsConstants.CoulombConstant * q1 * q2 / (distance * distance);

            // Apply quantum corrections if significant
            var energy = EstimateInteractionEnergy(particle1, particle2, distance);
            if (AreQuantumEffectsSignificant(distance, energy))
            {
                var quantumCorrection = CalculateQuantumCorrections(particle1, particle2, distance, energy);
                forceMagnitude *= quantumCorrection;
            }

            return forceMagnitude;
        }
    }

    /// <summary>
    /// Calculates the electromagnetic potential energy between two charged particles.
    /// Uses Coulomb potential: U = k * q1 * q2 / r
    /// </summary>
    public override double CalculatePotentialEnergy(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        ValidateParticles(particle1, particle2);
        ValidateDistance(distance);

        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        var q1 = particle1.Charge.Value;
        var q2 = particle2.Charge.Value;

        // Coulomb potential: U = k * q1 * q2 / r
        return PhysicsConstants.CoulombConstant * q1 * q2 / distance;
    }

    /// <summary>
    /// Determines if particles can interact electromagnetically.
    /// Requires at least one particle to have non-zero electric charge.
    /// </summary>
    public override bool CanParticlesInteract(IQuantumParticle particle1, IQuantumParticle particle2)
    {
        if (particle1 == null || particle2 == null)
            return false;

        var q1 = Math.Abs(particle1.Charge.Value);
        var q2 = Math.Abs(particle2.Charge.Value);

        // Both particles must have non-zero charge for electromagnetic interaction
        return q1 > double.Epsilon && q2 > double.Epsilon;
    }

    /// <summary>
    /// Gets the electromagnetic coupling constant (fine structure constant).
    /// In QED, this runs logarithmically with energy scale.
    /// </summary>
    public override double GetCouplingConstant(double energyScale)
    {
        ValidateEnergy(energyScale);

        // Running of the fine structure constant in QED
        var energyGeV = ConvertEnergyToGeV(energyScale);
        
        // Reference energy scale (electron mass)
        var referenceEnergyGeV = 0.511e-3; // 0.511 MeV
        
        if (energyGeV <= referenceEnergyGeV)
        {
            // Low energy: use constant fine structure constant
            return PhysicsConstants.FineStructureConstant;
        }

        // One-loop running of α in QED
        // α(E) ≈ α(0) / (1 - (α(0)/3π) * ln(E/m_e))
        var logRatio = Math.Log(energyGeV / referenceEnergyGeV);
        var runningCorrection = PhysicsConstants.FineStructureConstant * logRatio / (3.0 * Math.PI);
        
        var runningAlpha = PhysicsConstants.FineStructureConstant / (1.0 - runningCorrection);
        
        // Ensure physical range
        return Math.Max(PhysicsConstants.FineStructureConstant, Math.Min(1.0, runningAlpha));
    }

    #endregion

    #region Quantum Electrodynamics Effects

    /// <summary>
    /// Calculates quantum corrections to the classical Coulomb force.
    /// Includes vacuum polarization and other QED effects.
    /// </summary>
    public override double CalculateQuantumCorrections(IQuantumParticle particle1, IQuantumParticle particle2, 
        double distance, double energy)
    {
        ValidateParticles(particle1, particle2);
        ValidateEnergyAndDistance(energy, distance);

        // QED corrections become significant at distances comparable to Compton wavelength
        var comptonWavelength = PhysicsConstants.ElectronReducedComptonWavelength;
        
        if (distance > 10.0 * comptonWavelength)
        {
            // Classical regime: no significant corrections
            return 1.0;
        }

        // Vacuum polarization correction (Uehling potential)
        var vacuumPolarizationCorrection = CalculateVacuumPolarizationCorrection(distance);
        
        // Radiative corrections (simplified)
        var radiativeCorrection = CalculateRadiativeCorrections(particle1, particle2, energy);
        
        return (1.0 + vacuumPolarizationCorrection) * (1.0 + radiativeCorrection);
    }

    /// <summary>
    /// Calculates the scattering cross section for electromagnetic interactions.
    /// Uses appropriate formulas depending on energy regime.
    /// </summary>
    public override double CalculateScatteringCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        if (!CanParticlesInteract(particle1, particle2))
            return 0.0;

        ValidateEnergy(energy);

        var energyGeV = ConvertEnergyToGeV(energy);
        
        // Determine scattering regime
        if (energyGeV < 1e-3) // Low energy: use classical cross section
        {
            return CalculateClassicalScatteringCrossSection(particle1, particle2, energy);
        }
        else if (energyGeV < 1.0) // Intermediate energy: use Klein-Nishina
        {
            return CalculateKleinNishinaCrossSection(particle1, particle2, energy);
        }
        else // High energy: use relativistic formulas
        {
            return CalculateRelativisticScatteringCrossSection(particle1, particle2, energy);
        }
    }

    /// <summary>
    /// Calculates the magnetic force component for moving charged particles.
    /// F = q(v × B) where B is the magnetic field created by the other particle.
    /// </summary>
    public Vector3D CalculateMagneticForce(IQuantumParticle particle1, IQuantumParticle particle2,
        Vector3D position1, Vector3D position2, Vector3D velocity1, Vector3D velocity2)
    {
        ValidateParticles(particle1, particle2);
        ValidatePositions(position1, position2);

        if (!CanParticlesInteract(particle1, particle2))
            return Vector3D.Zero;

        var displacement = position2 - position1;
        var distance = displacement.Magnitude;

        if (distance < double.Epsilon)
            return Vector3D.Zero;

        var q1 = particle1.Charge.Value;
        var q2 = particle2.Charge.Value;

        // Magnetic field at position1 due to moving charge at position2
        // B = (μ₀/4π) * q2 * (v2 × r̂) / r²
        var rHat = displacement.Normalized;
        var magneticFieldConstant = PhysicsConstants.VacuumPermeability / (4.0 * Math.PI);
        var magneticField = magneticFieldConstant * q2 * velocity2.Cross(rHat) / (distance * distance);

        // Magnetic force on particle1: F = q1 * (v1 × B)
        return q1 * velocity1.Cross(magneticField);
    }

    /// <summary>
    /// Calculates the total electromagnetic force including both electric and magnetic components.
    /// This is the complete Lorentz force.
    /// </summary>
    public Vector3D CalculateLorentzForce(IQuantumParticle particle1, IQuantumParticle particle2,
        Vector3D position1, Vector3D position2, Vector3D velocity1, Vector3D velocity2)
    {
        // Electric force (Coulomb force)
        var electricForce = CalculateForceVector(particle1, particle2, position1, position2);
        
        // Magnetic force
        var magneticForce = CalculateMagneticForce(particle1, particle2, position1, position2, velocity1, velocity2);
        
        return electricForce + magneticForce;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Estimates the interaction energy based on the Coulomb potential.
    /// </summary>
    private double EstimateInteractionEnergy(IQuantumParticle particle1, IQuantumParticle particle2, double distance)
    {
        return Math.Abs(CalculatePotentialEnergy(particle1, particle2, distance));
    }

    /// <summary>
    /// Calculates vacuum polarization corrections to the Coulomb potential.
    /// </summary>
    private static double CalculateVacuumPolarizationCorrection(double distance)
    {
        var comptonWavelength = PhysicsConstants.ElectronReducedComptonWavelength;
        var x = distance / comptonWavelength;
        
        if (x > 10.0) return 0.0; // Negligible at large distances
        
        // Simplified vacuum polarization correction
        // Full calculation involves modified Bessel functions
        var alpha = PhysicsConstants.FineStructureConstant;
        return -(2.0 * alpha / (3.0 * Math.PI)) * Math.Exp(-2.0 * x) / x;
    }

    /// <summary>
    /// Calculates radiative corrections (vertex corrections, self-energy, etc.).
    /// </summary>
    private static double CalculateRadiativeCorrections(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        var energyGeV = ConvertEnergyToGeV(energy);
        var electronMassGeV = 0.511e-3; // 0.511 MeV
        
        if (energyGeV < electronMassGeV) return 0.0;
        
        // Simplified radiative correction
        var alpha = PhysicsConstants.FineStructureConstant;
        var logTerm = Math.Log(energyGeV / electronMassGeV);
        
        return alpha * logTerm / Math.PI;
    }

    /// <summary>
    /// Calculates classical scattering cross section (Rutherford scattering).
    /// </summary>
    private static double CalculateClassicalScatteringCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        var q1 = particle1.Charge.Value;
        var q2 = particle2.Charge.Value;
        var reducedMass = CalculateReducedMass(particle1, particle2);
        
        if (reducedMass <= 0) return 0.0;
        
        // Rutherford scattering cross section
        var kineticEnergy = energy;
        var factor = PhysicsConstants.CoulombConstant * q1 * q2;
        
        return Math.PI * (factor / (2.0 * kineticEnergy)) * (factor / (2.0 * kineticEnergy));
    }

    /// <summary>
    /// Calculates Klein-Nishina cross section for Compton scattering.
    /// </summary>
    private static double CalculateKleinNishinaCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        // Simplified Klein-Nishina formula for electron-photon scattering
        var energyGeV = ConvertEnergyToGeV(energy);
        var electronMassGeV = 0.511e-3;
        var x = energyGeV / electronMassGeV;
        
        if (x < 0.01) // Non-relativistic limit
        {
            return PhysicsConstants.ClassicalElectronRadius * PhysicsConstants.ClassicalElectronRadius * 8.0 * Math.PI / 3.0;
        }
        
        // Klein-Nishina cross section (simplified)
        var factor = PhysicsConstants.ClassicalElectronRadius * PhysicsConstants.ClassicalElectronRadius * Math.PI * 2.0;
        var logTerm = Math.Log(1.0 + 2.0 * x);
        
        return factor * ((1.0 + x) / (x * x * x)) * (2.0 * x * (1.0 + x) / (1.0 + 2.0 * x) - logTerm) + logTerm / (2.0 * x);
    }

    /// <summary>
    /// Calculates relativistic scattering cross section for high-energy interactions.
    /// </summary>
    private static double CalculateRelativisticScatteringCrossSection(IQuantumParticle particle1, IQuantumParticle particle2, double energy)
    {
        // High-energy QED cross section (simplified)
        var alpha = PhysicsConstants.FineStructureConstant;
        var energyGeV = ConvertEnergyToGeV(energy);
        
        // Asymptotic behavior: σ ∝ α² ln(s) / s
        var logTerm = Math.Log(energyGeV);
        return alpha * alpha * logTerm / energyGeV;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Gets the singleton instance of the electromagnetic force.
    /// </summary>
    public static ElectromagneticForce Instance { get; } = new ElectromagneticForce();

    #endregion
}
