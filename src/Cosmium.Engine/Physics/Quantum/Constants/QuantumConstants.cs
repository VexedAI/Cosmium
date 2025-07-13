namespace Cosmium.Engine.Physics.Quantum.Constants;

/// <summary>
/// Fundamental physical constants used in quantum mechanics.
/// </summary>
public static class QuantumConstants
{
    /// <summary>
    /// Planck constant (h) in Joule-seconds.
    /// </summary>
    public const double PlanckConstant = 6.62607015e-34;
    
    /// <summary>
    /// Reduced Planck constant (ℏ = h/2π) in Joule-seconds.
    /// </summary>
    public const double ReducedPlanckConstant = PlanckConstant / (2.0 * Math.PI);
    
    /// <summary>
    /// Speed of light in vacuum (c) in meters per second.
    /// </summary>
    public const double SpeedOfLight = 299792458.0;
    
    /// <summary>
    /// Elementary charge (e) in Coulombs.
    /// </summary>
    public const double ElementaryCharge = 1.602176634e-19;
    
    /// <summary>
    /// Electron rest mass in kilograms.
    /// </summary>
    public const double ElectronMass = 9.1093837015e-31;
    
    /// <summary>
    /// Proton rest mass in kilograms.
    /// </summary>
    public const double ProtonMass = 1.67262192369e-27;
    
    /// <summary>
    /// Neutron rest mass in kilograms.
    /// </summary>
    public const double NeutronMass = 1.67492749804e-27;
    
    /// <summary>
    /// Boltzmann constant (k_B) in Joules per Kelvin.
    /// </summary>
    public const double BoltzmannConstant = 1.380649e-23;
    
    /// <summary>
    /// Fine structure constant (α ≈ 1/137).
    /// </summary>
    public const double FineStructureConstant = 7.2973525693e-3;
    
    /// <summary>
    /// Bohr radius (a₀) in meters.
    /// </summary>
    public const double BohrRadius = 5.29177210903e-11;
    
    /// <summary>
    /// Rydberg constant (R∞) in per meter.
    /// </summary>
    public const double RydbergConstant = 10973731.568160;
}

/// <summary>
/// Helper methods for quantum calculations.
/// </summary>
public static class QuantumMath
{
    /// <summary>
    /// Converts energy from Joules to electron volts.
    /// </summary>
    /// <param name="joules">Energy in Joules.</param>
    /// <returns>Energy in electron volts.</returns>
    public static double JoulesToElectronVolts(double joules)
    {
        return joules / QuantumConstants.ElementaryCharge;
    }
    
    /// <summary>
    /// Converts energy from electron volts to Joules.
    /// </summary>
    /// <param name="electronVolts">Energy in electron volts.</param>
    /// <returns>Energy in Joules.</returns>
    public static double ElectronVoltsToJoules(double electronVolts)
    {
        return electronVolts * QuantumConstants.ElementaryCharge;
    }
    
    /// <summary>
    /// Calculates the de Broglie wavelength for a particle.
    /// </summary>
    /// <param name="momentum">Momentum in kg⋅m/s.</param>
    /// <returns>Wavelength in meters.</returns>
    public static double DeBroglieWavelength(double momentum)
    {
        return QuantumConstants.PlanckConstant / momentum;
    }
    
    /// <summary>
    /// Calculates the Compton wavelength for a particle.
    /// </summary>
    /// <param name="mass">Mass in kilograms.</param>
    /// <returns>Compton wavelength in meters.</returns>
    public static double ComptonWavelength(double mass)
    {
        return QuantumConstants.PlanckConstant / (mass * QuantumConstants.SpeedOfLight);
    }
    
    /// <summary>
    /// Calculates the thermal de Broglie wavelength at a given temperature.
    /// </summary>
    /// <param name="mass">Particle mass in kilograms.</param>
    /// <param name="temperature">Temperature in Kelvin.</param>
    /// <returns>Thermal wavelength in meters.</returns>
    public static double ThermalDeBroglieWavelength(double mass, double temperature)
    {
        var momentum = Math.Sqrt(2.0 * Math.PI * mass * QuantumConstants.BoltzmannConstant * temperature);
        return QuantumConstants.PlanckConstant / momentum;
    }
}
