using System;
using System.Collections.Generic;

namespace Cosmium.Engine.Physics.Quantum.Constants;

/// <summary>
/// Provides unit conversion utilities for quantum mechanics calculations.
/// Supports conversion between different unit systems commonly used in physics.
/// </summary>
public static class UnitConversions
{
    #region Energy Conversions

    /// <summary>
    /// Converts Joules to electron volts.
    /// </summary>
    /// <param name="joules">Energy in Joules.</param>
    /// <returns>Energy in electron volts.</returns>
    public static double JoulesToElectronVolts(double joules)
    {
        return joules / PhysicsConstants.ElementaryCharge;
    }

    /// <summary>
    /// Converts electron volts to Joules.
    /// </summary>
    /// <param name="electronVolts">Energy in electron volts.</param>
    /// <returns>Energy in Joules.</returns>
    public static double ElectronVoltsToJoules(double electronVolts)
    {
        return electronVolts * PhysicsConstants.ElementaryCharge;
    }

    /// <summary>
    /// Converts Joules to Hartree atomic units.
    /// </summary>
    /// <param name="joules">Energy in Joules.</param>
    /// <returns>Energy in Hartree atomic units.</returns>
    public static double JoulesToHartree(double joules)
    {
        return joules / PhysicsConstants.HartreeEnergy;
    }

    /// <summary>
    /// Converts Hartree atomic units to Joules.
    /// </summary>
    /// <param name="hartree">Energy in Hartree atomic units.</param>
    /// <returns>Energy in Joules.</returns>
    public static double HartreeToJoules(double hartree)
    {
        return hartree * PhysicsConstants.HartreeEnergy;
    }

    /// <summary>
    /// Converts electron volts to Hartree atomic units.
    /// </summary>
    /// <param name="electronVolts">Energy in electron volts.</param>
    /// <returns>Energy in Hartree atomic units.</returns>
    public static double ElectronVoltsToHartree(double electronVolts)
    {
        return JoulesToHartree(ElectronVoltsToJoules(electronVolts));
    }

    /// <summary>
    /// Converts Hartree atomic units to electron volts.
    /// </summary>
    /// <param name="hartree">Energy in Hartree atomic units.</param>
    /// <returns>Energy in electron volts.</returns>
    public static double HartreeToElectronVolts(double hartree)
    {
        return JoulesToElectronVolts(HartreeToJoules(hartree));
    }

    /// <summary>
    /// Converts temperature to energy using kT.
    /// </summary>
    /// <param name="temperatureKelvin">Temperature in Kelvin.</param>
    /// <returns>Thermal energy in Joules.</returns>
    public static double TemperatureToThermalEnergy(double temperatureKelvin)
    {
        return PhysicsConstants.BoltzmannConstant * temperatureKelvin;
    }

    /// <summary>
    /// Converts energy to equivalent temperature using E = kT.
    /// </summary>
    /// <param name="energyJoules">Energy in Joules.</param>
    /// <returns>Equivalent temperature in Kelvin.</returns>
    public static double EnergyToTemperature(double energyJoules)
    {
        return energyJoules / PhysicsConstants.BoltzmannConstant;
    }

    #endregion

    #region Length Conversions

    /// <summary>
    /// Converts meters to Bohr radii (atomic units of length).
    /// </summary>
    /// <param name="meters">Length in meters.</param>
    /// <returns>Length in Bohr radii.</returns>
    public static double MetersToBohrRadii(double meters)
    {
        return meters / PhysicsConstants.BohrRadius;
    }

    /// <summary>
    /// Converts Bohr radii to meters.
    /// </summary>
    /// <param name="bohrRadii">Length in Bohr radii.</param>
    /// <returns>Length in meters.</returns>
    public static double BohrRadiiToMeters(double bohrRadii)
    {
        return bohrRadii * PhysicsConstants.BohrRadius;
    }

    /// <summary>
    /// Converts meters to femtometers.
    /// </summary>
    /// <param name="meters">Length in meters.</param>
    /// <returns>Length in femtometers.</returns>
    public static double MetersToFemtometers(double meters)
    {
        return meters * 1e15;
    }

    /// <summary>
    /// Converts femtometers to meters.
    /// </summary>
    /// <param name="femtometers">Length in femtometers.</param>
    /// <returns>Length in meters.</returns>
    public static double FemtometersToMeters(double femtometers)
    {
        return femtometers * 1e-15;
    }

    /// <summary>
    /// Converts meters to angstroms.
    /// </summary>
    /// <param name="meters">Length in meters.</param>
    /// <returns>Length in angstroms.</returns>
    public static double MetersToAngstroms(double meters)
    {
        return meters * 1e10;
    }

    /// <summary>
    /// Converts angstroms to meters.
    /// </summary>
    /// <param name="angstroms">Length in angstroms.</param>
    /// <returns>Length in meters.</returns>
    public static double AngstromsToMeters(double angstroms)
    {
        return angstroms * 1e-10;
    }

    #endregion

    #region Mass Conversions

    /// <summary>
    /// Converts kilograms to atomic mass units.
    /// </summary>
    /// <param name="kilograms">Mass in kilograms.</param>
    /// <returns>Mass in atomic mass units.</returns>
    public static double KilogramsToAtomicMassUnits(double kilograms)
    {
        return kilograms / PhysicsConstants.AtomicMassUnit;
    }

    /// <summary>
    /// Converts atomic mass units to kilograms.
    /// </summary>
    /// <param name="atomicMassUnits">Mass in atomic mass units.</param>
    /// <returns>Mass in kilograms.</returns>
    public static double AtomicMassUnitsToKilograms(double atomicMassUnits)
    {
        return atomicMassUnits * PhysicsConstants.AtomicMassUnit;
    }

    /// <summary>
    /// Converts mass to energy using E = mc².
    /// </summary>
    /// <param name="massKg">Mass in kilograms.</param>
    /// <returns>Rest energy in Joules.</returns>
    public static double MassToEnergy(double massKg)
    {
        return massKg * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
    }

    /// <summary>
    /// Converts energy to equivalent mass using E = mc².
    /// </summary>
    /// <param name="energyJoules">Energy in Joules.</param>
    /// <returns>Equivalent mass in kilograms.</returns>
    public static double EnergyToMass(double energyJoules)
    {
        return energyJoules / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);
    }

    /// <summary>
    /// Converts mass in electron rest masses to kilograms.
    /// </summary>
    /// <param name="electronMasses">Mass in units of electron rest mass.</param>
    /// <returns>Mass in kilograms.</returns>
    public static double ElectronMassesToKilograms(double electronMasses)
    {
        return electronMasses * PhysicsConstants.ElectronMass;
    }

    /// <summary>
    /// Converts kilograms to electron rest masses.
    /// </summary>
    /// <param name="kilograms">Mass in kilograms.</param>
    /// <returns>Mass in units of electron rest mass.</returns>
    public static double KilogramsToElectronMasses(double kilograms)
    {
        return kilograms / PhysicsConstants.ElectronMass;
    }

    #endregion

    #region Time Conversions

    /// <summary>
    /// Converts seconds to atomic time units (ℏ/Eₕ).
    /// </summary>
    /// <param name="seconds">Time in seconds.</param>
    /// <returns>Time in atomic time units.</returns>
    public static double SecondsToAtomicTimeUnits(double seconds)
    {
        return seconds * PhysicsConstants.HartreeEnergy / PhysicsConstants.ReducedPlanckConstant;
    }

    /// <summary>
    /// Converts atomic time units to seconds.
    /// </summary>
    /// <param name="atomicTimeUnits">Time in atomic time units.</param>
    /// <returns>Time in seconds.</returns>
    public static double AtomicTimeUnitsToSeconds(double atomicTimeUnits)
    {
        return atomicTimeUnits * PhysicsConstants.ReducedPlanckConstant / PhysicsConstants.HartreeEnergy;
    }

    /// <summary>
    /// Converts seconds to femtoseconds.
    /// </summary>
    /// <param name="seconds">Time in seconds.</param>
    /// <returns>Time in femtoseconds.</returns>
    public static double SecondsToFemtoseconds(double seconds)
    {
        return seconds * 1e15;
    }

    /// <summary>
    /// Converts femtoseconds to seconds.
    /// </summary>
    /// <param name="femtoseconds">Time in femtoseconds.</param>
    /// <returns>Time in seconds.</returns>
    public static double FemtosecondsToSeconds(double femtoseconds)
    {
        return femtoseconds * 1e-15;
    }

    #endregion

    #region Frequency and Wavelength Conversions

    /// <summary>
    /// Converts frequency to wavelength using c = λν.
    /// </summary>
    /// <param name="frequencyHz">Frequency in Hz.</param>
    /// <returns>Wavelength in meters.</returns>
    public static double FrequencyToWavelength(double frequencyHz)
    {
        return PhysicsConstants.SpeedOfLight / frequencyHz;
    }

    /// <summary>
    /// Converts wavelength to frequency using c = λν.
    /// </summary>
    /// <param name="wavelengthMeters">Wavelength in meters.</param>
    /// <returns>Frequency in Hz.</returns>
    public static double WavelengthToFrequency(double wavelengthMeters)
    {
        return PhysicsConstants.SpeedOfLight / wavelengthMeters;
    }

    /// <summary>
    /// Converts frequency to photon energy using E = hν.
    /// </summary>
    /// <param name="frequencyHz">Frequency in Hz.</param>
    /// <returns>Photon energy in Joules.</returns>
    public static double FrequencyToPhotonEnergy(double frequencyHz)
    {
        return PhysicsConstants.PlanckConstant * frequencyHz;
    }

    /// <summary>
    /// Converts photon energy to frequency using E = hν.
    /// </summary>
    /// <param name="energyJoules">Photon energy in Joules.</param>
    /// <returns>Frequency in Hz.</returns>
    public static double PhotonEnergyToFrequency(double energyJoules)
    {
        return energyJoules / PhysicsConstants.PlanckConstant;
    }

    /// <summary>
    /// Converts wavelength to photon energy using E = hc/λ.
    /// </summary>
    /// <param name="wavelengthMeters">Wavelength in meters.</param>
    /// <returns>Photon energy in Joules.</returns>
    public static double WavelengthToPhotonEnergy(double wavelengthMeters)
    {
        return PhysicsConstants.PlanckConstant * PhysicsConstants.SpeedOfLight / wavelengthMeters;
    }

    /// <summary>
    /// Converts photon energy to wavelength using E = hc/λ.
    /// </summary>
    /// <param name="energyJoules">Photon energy in Joules.</param>
    /// <returns>Wavelength in meters.</returns>
    public static double PhotonEnergyToWavelength(double energyJoules)
    {
        return PhysicsConstants.PlanckConstant * PhysicsConstants.SpeedOfLight / energyJoules;
    }

    #endregion

    #region Momentum Conversions

    /// <summary>
    /// Converts momentum to de Broglie wavelength using λ = h/p.
    /// </summary>
    /// <param name="momentumKgMs">Momentum in kg⋅m/s.</param>
    /// <returns>de Broglie wavelength in meters.</returns>
    public static double MomentumToDeBroglieWavelength(double momentumKgMs)
    {
        return PhysicsConstants.PlanckConstant / momentumKgMs;
    }

    /// <summary>
    /// Converts de Broglie wavelength to momentum using λ = h/p.
    /// </summary>
    /// <param name="wavelengthMeters">de Broglie wavelength in meters.</param>
    /// <returns>Momentum in kg⋅m/s.</returns>
    public static double DeBroglieWavelengthToMomentum(double wavelengthMeters)
    {
        return PhysicsConstants.PlanckConstant / wavelengthMeters;
    }

    /// <summary>
    /// Converts kinetic energy to momentum for non-relativistic particles using p = √(2mE).
    /// </summary>
    /// <param name="kineticEnergyJoules">Kinetic energy in Joules.</param>
    /// <param name="massKg">Mass in kilograms.</param>
    /// <returns>Momentum in kg⋅m/s.</returns>
    public static double KineticEnergyToMomentumNonRelativistic(double kineticEnergyJoules, double massKg)
    {
        return Math.Sqrt(2.0 * massKg * kineticEnergyJoules);
    }

    /// <summary>
    /// Converts momentum to kinetic energy for non-relativistic particles using E = p²/(2m).
    /// </summary>
    /// <param name="momentumKgMs">Momentum in kg⋅m/s.</param>
    /// <param name="massKg">Mass in kilograms.</param>
    /// <returns>Kinetic energy in Joules.</returns>
    public static double MomentumToKineticEnergyNonRelativistic(double momentumKgMs, double massKg)
    {
        return (momentumKgMs * momentumKgMs) / (2.0 * massKg);
    }

    /// <summary>
    /// Converts total energy to momentum for relativistic particles using E² = (pc)² + (mc²)².
    /// </summary>
    /// <param name="totalEnergyJoules">Total energy in Joules.</param>
    /// <param name="restMassKg">Rest mass in kilograms.</param>
    /// <returns>Momentum in kg⋅m/s.</returns>
    public static double TotalEnergyToMomentumRelativistic(double totalEnergyJoules, double restMassKg)
    {
        double restEnergyJoules = restMassKg * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
        double momentumEnergySquared = totalEnergyJoules * totalEnergyJoules - restEnergyJoules * restEnergyJoules;
        
        if (momentumEnergySquared < 0)
            throw new ArgumentException("Total energy must be greater than rest energy for massive particles.");
        
        return Math.Sqrt(momentumEnergySquared) / PhysicsConstants.SpeedOfLight;
    }

    #endregion

    #region Quantum Number Conversions

    /// <summary>
    /// Converts angular momentum quantum number to angular momentum magnitude using L = ℏ√(l(l+1)).
    /// </summary>
    /// <param name="quantumNumber">Angular momentum quantum number l.</param>
    /// <returns>Angular momentum magnitude in J⋅s.</returns>
    public static double QuantumNumberToAngularMomentum(int quantumNumber)
    {
        return PhysicsConstants.ReducedPlanckConstant * Math.Sqrt(quantumNumber * (quantumNumber + 1));
    }

    /// <summary>
    /// Converts magnetic quantum number to z-component of angular momentum using Lz = ℏm.
    /// </summary>
    /// <param name="magneticQuantumNumber">Magnetic quantum number m.</param>
    /// <returns>Z-component of angular momentum in J⋅s.</returns>
    public static double MagneticQuantumNumberToAngularMomentumZ(int magneticQuantumNumber)
    {
        return PhysicsConstants.ReducedPlanckConstant * magneticQuantumNumber;
    }

    #endregion

    #region Temperature Scale Conversions

    /// <summary>
    /// Converts Celsius to Kelvin.
    /// </summary>
    /// <param name="celsius">Temperature in Celsius.</param>
    /// <returns>Temperature in Kelvin.</returns>
    public static double CelsiusToKelvin(double celsius)
    {
        return celsius + 273.15;
    }

    /// <summary>
    /// Converts Kelvin to Celsius.
    /// </summary>
    /// <param name="kelvin">Temperature in Kelvin.</param>
    /// <returns>Temperature in Celsius.</returns>
    public static double KelvinToCelsius(double kelvin)
    {
        return kelvin - 273.15;
    }

    /// <summary>
    /// Converts Fahrenheit to Kelvin.
    /// </summary>
    /// <param name="fahrenheit">Temperature in Fahrenheit.</param>
    /// <returns>Temperature in Kelvin.</returns>
    public static double FahrenheitToKelvin(double fahrenheit)
    {
        return (fahrenheit - 32.0) * 5.0 / 9.0 + 273.15;
    }

    /// <summary>
    /// Converts Kelvin to Fahrenheit.
    /// </summary>
    /// <param name="kelvin">Temperature in Kelvin.</param>
    /// <returns>Temperature in Fahrenheit.</returns>
    public static double KelvinToFahrenheit(double kelvin)
    {
        return (kelvin - 273.15) * 9.0 / 5.0 + 32.0;
    }

    #endregion

    #region Unit System Information

    /// <summary>
    /// Provides information about different unit systems used in physics.
    /// </summary>
    public static class UnitSystems
    {
        /// <summary>
        /// SI (International System of Units) base units.
        /// </summary>
        public static readonly Dictionary<string, string> SIUnits = new()
        {
            { "Length", "meter (m)" },
            { "Mass", "kilogram (kg)" },
            { "Time", "second (s)" },
            { "Electric Current", "ampere (A)" },
            { "Temperature", "kelvin (K)" },
            { "Amount of Substance", "mole (mol)" },
            { "Luminous Intensity", "candela (cd)" }
        };

        /// <summary>
        /// Atomic units (Hartree atomic units) commonly used in quantum chemistry.
        /// </summary>
        public static readonly Dictionary<string, string> AtomicUnits = new()
        {
            { "Length", "bohr radius (a₀)" },
            { "Mass", "electron rest mass (mₑ)" },
            { "Time", "ℏ/Eₕ" },
            { "Energy", "hartree (Eₕ)" },
            { "Charge", "elementary charge (e)" },
            { "Angular Momentum", "reduced Planck constant (ℏ)" }
        };

        /// <summary>
        /// Natural units (Planck units) where ℏ = c = G = kᵦ = 1.
        /// </summary>
        public static readonly Dictionary<string, string> NaturalUnits = new()
        {
            { "Length", "Planck length (lₚ)" },
            { "Mass", "Planck mass (mₚ)" },
            { "Time", "Planck time (tₚ)" },
            { "Energy", "Planck energy (Eₚ)" },
            { "Temperature", "Planck temperature (Tₚ)" }
        };

        /// <summary>
        /// Particle physics units where ℏ = c = 1.
        /// </summary>
        public static readonly Dictionary<string, string> ParticlePhysicsUnits = new()
        {
            { "Energy/Mass/Momentum", "electron volt (eV)" },
            { "Length/Time", "1/eV" },
            { "Cross Section", "barn (10⁻²⁴ cm²)" },
            { "Luminosity", "cm⁻²s⁻¹" }
        };
    }

    #endregion

    #region Validation and Utility Methods

    /// <summary>
    /// Validates that a conversion maintains dimensional consistency.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    /// <param name="convertedValue">Converted value.</param>
    /// <param name="conversionFactor">Expected conversion factor.</param>
    /// <param name="tolerance">Tolerance for validation.</param>
    /// <returns>True if conversion is dimensionally consistent.</returns>
    public static bool ValidateConversion(double originalValue, double convertedValue, 
                                        double conversionFactor, double tolerance = 1e-12)
    {
        double expectedValue = originalValue * conversionFactor;
        return Math.Abs(convertedValue - expectedValue) <= tolerance * Math.Abs(expectedValue);
    }

    /// <summary>
    /// Gets common conversion factors for quick reference.
    /// </summary>
    /// <returns>Dictionary of conversion factor names and values.</returns>
    public static Dictionary<string, double> GetCommonConversionFactors()
    {
        return new Dictionary<string, double>
        {
            // Energy conversions
            { "eV to Joules", PhysicsConstants.ElementaryCharge },
            { "Joules to eV", 1.0 / PhysicsConstants.ElementaryCharge },
            { "Hartree to eV", PhysicsConstants.HartreeEnergy / PhysicsConstants.ElementaryCharge },
            { "eV to Hartree", PhysicsConstants.ElementaryCharge / PhysicsConstants.HartreeEnergy },
            
            // Length conversions
            { "m to Bohr", 1.0 / PhysicsConstants.BohrRadius },
            { "Bohr to m", PhysicsConstants.BohrRadius },
            { "m to Angstrom", 1e10 },
            { "Angstrom to m", 1e-10 },
            
            // Mass conversions
            { "kg to u", 1.0 / PhysicsConstants.AtomicMassUnit },
            { "u to kg", PhysicsConstants.AtomicMassUnit },
            { "kg to me", 1.0 / PhysicsConstants.ElectronMass },
            { "me to kg", PhysicsConstants.ElectronMass },
            
            // Temperature conversions
            { "K to eV", PhysicsConstants.BoltzmannConstant / PhysicsConstants.ElementaryCharge },
            { "eV to K", PhysicsConstants.ElementaryCharge / PhysicsConstants.BoltzmannConstant }
        };
    }

    #endregion
}
