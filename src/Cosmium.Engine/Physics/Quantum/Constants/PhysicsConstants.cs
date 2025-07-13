using System;

namespace Cosmium.Engine.Physics.Quantum.Constants;

/// <summary>
/// Provides fundamental physical constants used in quantum mechanics and particle physics calculations.
/// All values are given in SI units with maximum available precision from CODATA 2018.
/// </summary>
public static class PhysicsConstants
{
    #region Fundamental Constants

    /// <summary>
    /// Speed of light in vacuum (c) in m/s.
    /// Exact value by definition.
    /// </summary>
    public const double SpeedOfLight = 299_792_458.0;

    /// <summary>
    /// Reduced Planck constant (ℏ = h/2π) in J·s.
    /// CODATA 2018 value with zero uncertainty.
    /// </summary>
    public const double ReducedPlanckConstant = 1.054_571_817e-34;

    /// <summary>
    /// Planck constant (h) in J·s.
    /// CODATA 2018 value with zero uncertainty.
    /// </summary>
    public const double PlanckConstant = 6.626_070_15e-34;

    /// <summary>
    /// Elementary charge (e) in C.
    /// CODATA 2018 value with zero uncertainty.
    /// </summary>
    public const double ElementaryCharge = 1.602_176_634e-19;

    /// <summary>
    /// Electron rest mass (mₑ) in kg.
    /// CODATA 2018 value.
    /// </summary>
    public const double ElectronMass = 9.109_383_7015e-31;

    /// <summary>
    /// Proton rest mass (mₚ) in kg.
    /// CODATA 2018 value.
    /// </summary>
    public const double ProtonMass = 1.672_621_923_69e-27;

    /// <summary>
    /// Neutron rest mass (mₙ) in kg.
    /// CODATA 2018 value.
    /// </summary>
    public const double NeutronMass = 1.674_927_498_04e-27;

    /// <summary>
    /// Atomic mass unit (u) in kg.
    /// CODATA 2018 value with zero uncertainty.
    /// </summary>
    public const double AtomicMassUnit = 1.660_538_921e-27;

    /// <summary>
    /// Avogadro constant (Nₐ) in mol⁻¹.
    /// CODATA 2018 value with zero uncertainty.
    /// </summary>
    public const double AvogadroConstant = 6.022_140_76e23;

    /// <summary>
    /// Boltzmann constant (kᵦ) in J/K.
    /// CODATA 2018 value with zero uncertainty.
    /// </summary>
    public const double BoltzmannConstant = 1.380_649e-23;

    /// <summary>
    /// Gas constant (R) in J/(mol·K).
    /// Derived from Avogadro and Boltzmann constants.
    /// </summary>
    public static readonly double GasConstant = AvogadroConstant * BoltzmannConstant;

    #endregion

    #region Electromagnetic Constants

    /// <summary>
    /// Vacuum permeability (μ₀) in H/m.
    /// CODATA 2018 value.
    /// </summary>
    public const double VacuumPermeability = 1.256_637_062_12e-6;

    /// <summary>
    /// Vacuum permittivity (ε₀) in F/m.
    /// Derived from speed of light and vacuum permeability.
    /// </summary>
    public static readonly double VacuumPermittivity = 1.0 / (VacuumPermeability * SpeedOfLight * SpeedOfLight);

    /// <summary>
    /// Coulomb constant (kₑ = 1/(4πε₀)) in N·m²/C².
    /// </summary>
    public static readonly double CoulombConstant = 1.0 / (4.0 * Math.PI * VacuumPermittivity);

    /// <summary>
    /// Fine structure constant (α = e²/(4πε₀ℏc)).
    /// CODATA 2018 value.
    /// </summary>
    public const double FineStructureConstant = 7.297_352_566_4e-3;

    /// <summary>
    /// Impedance of free space (Z₀ = √(μ₀/ε₀)) in Ω.
    /// </summary>
    public static readonly double ImpedanceOfFreeSpace = Math.Sqrt(VacuumPermeability / VacuumPermittivity);

    #endregion

    #region Quantum Mechanical Constants

    /// <summary>
    /// Bohr radius (a₀ = 4πε₀ℏ²/(mₑe²)) in m.
    /// CODATA 2018 value.
    /// </summary>
    public const double BohrRadius = 5.291_772_109_03e-11;

    /// <summary>
    /// Classical electron radius (rₑ = e²/(4πε₀mₑc²)) in m.
    /// CODATA 2018 value.
    /// </summary>
    public const double ClassicalElectronRadius = 2.817_940_326_2e-15;

    /// <summary>
    /// Compton wavelength of electron (λₑ = h/(mₑc)) in m.
    /// CODATA 2018 value.
    /// </summary>
    public const double ElectronComptonWavelength = 2.426_310_217_5e-12;

    /// <summary>
    /// Reduced Compton wavelength of electron (λ̄ₑ = ℏ/(mₑc)) in m.
    /// </summary>
    public static readonly double ElectronReducedComptonWavelength = ReducedPlanckConstant / (ElectronMass * SpeedOfLight);

    /// <summary>
    /// Rydberg constant (R∞) in m⁻¹.
    /// CODATA 2018 value.
    /// </summary>
    public const double RydbergConstant = 1.097_373_156_816_0e7;

    /// <summary>
    /// Hartree energy (Eₕ = 2R∞hc) in J.
    /// CODATA 2018 value.
    /// </summary>
    public const double HartreeEnergy = 4.359_744_722_207_1e-18;

    /// <summary>
    /// Quantum of magnetic flux (Φ₀ = h/(2e)) in Wb.
    /// CODATA 2018 value.
    /// </summary>
    public const double MagneticFluxQuantum = 2.067_833_848e-15;

    /// <summary>
    /// Conductance quantum (G₀ = 2e²/h) in S.
    /// CODATA 2018 value.
    /// </summary>
    public const double ConductanceQuantum = 7.748_091_729e-5;

    #endregion

    #region Particle Physics Constants

    /// <summary>
    /// Weak mixing angle (sin²θw).
    /// PDG 2020 value.
    /// </summary>
    public const double WeakMixingAngleSinSquared = 0.23121;

    /// <summary>
    /// Strong coupling constant (αₛ) at Z boson mass scale.
    /// PDG 2020 value.
    /// </summary>
    public const double StrongCouplingConstant = 0.1179;

    /// <summary>
    /// Fermi coupling constant (Gf) in GeV⁻².
    /// PDG 2020 value.
    /// </summary>
    public const double FermiCouplingConstant = 1.1663787e-5;

    #endregion

    #region Cosmological Constants

    /// <summary>
    /// Gravitational constant (G) in m³/(kg·s²).
    /// CODATA 2018 value.
    /// </summary>
    public const double GravitationalConstant = 6.674_30e-11;

    /// <summary>
    /// Solar mass (M☉) in kg.
    /// IAU 2015 nominal value.
    /// </summary>
    public const double SolarMass = 1.988_47e30;

    /// <summary>
    /// Earth mass (M⊕) in kg.
    /// IAU 2015 nominal value.
    /// </summary>
    public const double EarthMass = 5.972_168e24;

    /// <summary>
    /// Astronomical unit (au) in m.
    /// IAU 2012 exact value.
    /// </summary>
    public const double AstronomicalUnit = 149_597_870_700.0;

    /// <summary>
    /// Parsec (pc) in m.
    /// Calculated value based on astronomical unit and angular conversion.
    /// </summary>
    public static readonly double Parsec = AstronomicalUnit / Math.Tan(Math.PI / (180.0 * 3600.0));

    /// <summary>
    /// Light year (ly) in m.
    /// Calculated value based on speed of light and Julian year.
    /// </summary>
    public static readonly double LightYear = SpeedOfLight * 365.25 * 24.0 * 3600.0;

    #endregion

    #region Derived Quantum Constants

    /// <summary>
    /// Planck length (lₚ = √(ℏG/c³)) in m.
    /// </summary>
    public static readonly double PlanckLength = Math.Sqrt(ReducedPlanckConstant * GravitationalConstant / Math.Pow(SpeedOfLight, 3));

    /// <summary>
    /// Planck mass (mₚ = √(ℏc/G)) in kg.
    /// </summary>
    public static readonly double PlanckMass = Math.Sqrt(ReducedPlanckConstant * SpeedOfLight / GravitationalConstant);

    /// <summary>
    /// Planck time (tₚ = √(ℏG/c⁵)) in s.
    /// </summary>
    public static readonly double PlanckTime = Math.Sqrt(ReducedPlanckConstant * GravitationalConstant / Math.Pow(SpeedOfLight, 5));

    /// <summary>
    /// Planck energy (Eₚ = √(ℏc⁵/G)) in J.
    /// </summary>
    public static readonly double PlanckEnergy = Math.Sqrt(ReducedPlanckConstant * Math.Pow(SpeedOfLight, 5) / GravitationalConstant);

    /// <summary>
    /// Planck temperature (Tₚ = √(ℏc⁵/Gk²)) in K.
    /// </summary>
    public static readonly double PlanckTemperature = Math.Sqrt(ReducedPlanckConstant * Math.Pow(SpeedOfLight, 5) / (GravitationalConstant * BoltzmannConstant * BoltzmannConstant));

    #endregion

    #region Commonly Used Combinations

    /// <summary>
    /// Energy equivalent of electron mass (mₑc²) in J.
    /// </summary>
    public static readonly double ElectronRestEnergyJoules = ElectronMass * SpeedOfLight * SpeedOfLight;

    /// <summary>
    /// Energy equivalent of proton mass (mₚc²) in J.
    /// </summary>
    public static readonly double ProtonRestEnergyJoules = ProtonMass * SpeedOfLight * SpeedOfLight;

    /// <summary>
    /// Energy equivalent of neutron mass (mₙc²) in J.
    /// </summary>
    public static readonly double NeutronRestEnergyJoules = NeutronMass * SpeedOfLight * SpeedOfLight;

    /// <summary>
    /// Thermal de Broglie wavelength scale factor (h/√(2πmkT)).
    /// Multiply by √(m/T) to get actual wavelength in m.
    /// </summary>
    public static readonly double ThermalDeBroglieScale = PlanckConstant / Math.Sqrt(2.0 * Math.PI);

    /// <summary>
    /// Stefan-Boltzmann constant (σ = 2π⁵k⁴/(15h³c²)) in W/(m²·K⁴).
    /// CODATA 2018 value.
    /// </summary>
    public const double StefanBoltzmannConstant = 5.670_374_419e-8;

    /// <summary>
    /// Wien wavelength displacement constant (b = hc/(4.965kB)) in m·K.
    /// CODATA 2018 value.
    /// </summary>
    public const double WienDisplacementConstant = 2.897_771_955e-3;

    #endregion

    #region Uncertainty and Precision Information

    /// <summary>
    /// Provides information about the relative standard uncertainty of fundamental constants.
    /// </summary>
    public static class Uncertainties
    {
        /// <summary>
        /// Relative standard uncertainty of the electron mass.
        /// CODATA 2018 value.
        /// </summary>
        public const double ElectronMassUncertainty = 3.0e-10;

        /// <summary>
        /// Relative standard uncertainty of the proton mass.
        /// CODATA 2018 value.
        /// </summary>
        public const double ProtonMassUncertainty = 3.1e-10;

        /// <summary>
        /// Relative standard uncertainty of the neutron mass.
        /// CODATA 2018 value.
        /// </summary>
        public const double NeutronMassUncertainty = 5.7e-10;

        /// <summary>
        /// Relative standard uncertainty of the fine structure constant.
        /// CODATA 2018 value.
        /// </summary>
        public const double FineStructureConstantUncertainty = 1.5e-10;

        /// <summary>
        /// Relative standard uncertainty of the Rydberg constant.
        /// CODATA 2018 value.
        /// </summary>
        public const double RydbergConstantUncertainty = 1.9e-12;

        /// <summary>
        /// Relative standard uncertainty of the gravitational constant.
        /// CODATA 2018 value.
        /// </summary>
        public const double GravitationalConstantUncertainty = 2.2e-5;
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates fundamental relationships between constants.
    /// Useful for testing precision and detecting calculation errors.
    /// </summary>
    /// <param name="tolerance">The tolerance for validation checks.</param>
    /// <returns>True if all validations pass.</returns>
    public static bool ValidateConstantRelationships(double tolerance = 1e-10)
    {
        try
        {
            // Check c = 1/√(μ₀ε₀)
            double computedSpeedOfLight = 1.0 / Math.Sqrt(VacuumPermeability * VacuumPermittivity);
            if (Math.Abs(computedSpeedOfLight - SpeedOfLight) / SpeedOfLight > tolerance)
                return false;

            // Check α = e²/(4πε₀ℏc)
            double computedAlpha = (ElementaryCharge * ElementaryCharge) / 
                                 (4.0 * Math.PI * VacuumPermittivity * ReducedPlanckConstant * SpeedOfLight);
            if (Math.Abs(computedAlpha - FineStructureConstant) / FineStructureConstant > tolerance)
                return false;

            // Check a₀ = 4πε₀ℏ²/(mₑe²)
            double computedBohrRadius = (4.0 * Math.PI * VacuumPermittivity * ReducedPlanckConstant * ReducedPlanckConstant) /
                                      (ElectronMass * ElementaryCharge * ElementaryCharge);
            if (Math.Abs(computedBohrRadius - BohrRadius) / BohrRadius > tolerance)
                return false;

            // Check h = 2πℏ
            double computedPlanckConstant = 2.0 * Math.PI * ReducedPlanckConstant;
            if (Math.Abs(computedPlanckConstant - PlanckConstant) / PlanckConstant > tolerance)
                return false;

            // Check R = NA × kB
            double computedGasConstant = AvogadroConstant * BoltzmannConstant;
            if (Math.Abs(computedGasConstant - GasConstant) / GasConstant > tolerance)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the most precisely known fundamental constants for high-precision calculations.
    /// </summary>
    /// <returns>A dictionary of constant names and their relative uncertainties.</returns>
    public static Dictionary<string, double> GetHighPrecisionConstants()
    {
        return new Dictionary<string, double>
        {
            { nameof(SpeedOfLight), 0.0 }, // Exact by definition
            { nameof(PlanckConstant), 0.0 }, // Exact by definition
            { nameof(ElementaryCharge), 0.0 }, // Exact by definition
            { nameof(AvogadroConstant), 0.0 }, // Exact by definition
            { nameof(BoltzmannConstant), 0.0 }, // Exact by definition
            { nameof(RydbergConstant), Uncertainties.RydbergConstantUncertainty },
            { nameof(FineStructureConstant), Uncertainties.FineStructureConstantUncertainty },
            { nameof(ElectronMass), Uncertainties.ElectronMassUncertainty },
            { nameof(ProtonMass), Uncertainties.ProtonMassUncertainty }
        };
    }

    #endregion
}
