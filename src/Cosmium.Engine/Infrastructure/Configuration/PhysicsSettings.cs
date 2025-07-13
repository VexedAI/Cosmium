using Cosmium.Engine.Infrastructure.Configuration;

namespace Cosmium.Engine.Infrastructure.Configuration;

/// <summary>
/// Physics constants and settings for the Cosmium Engine.
/// Manages fundamental constants, unit systems, and physics-specific parameters.
/// </summary>
public class PhysicsSettings : ConfigurationSection
{
    /// <summary>
    /// The unit system to use for calculations and output.
    /// </summary>
    public UnitSystem UnitSystem { get; set; } = UnitSystem.SI;

    /// <summary>
    /// Whether to use natural units (ℏ = c = 1) for calculations.
    /// </summary>
    public bool UseNaturalUnits { get; set; } = false;

    /// <summary>
    /// Temperature for thermal equilibrium calculations in Kelvin.
    /// </summary>
    public double DefaultTemperature { get; set; } = 300.0; // Room temperature

    /// <summary>
    /// Default time step for time evolution calculations in seconds.
    /// </summary>
    public double DefaultTimeStep { get; set; } = 1e-15; // Femtosecond scale

    /// <summary>
    /// Maximum time for time evolution simulations in seconds.
    /// </summary>
    public double MaxSimulationTime { get; set; } = 1e-9; // Nanosecond scale

    /// <summary>
    /// Quantum mechanics specific settings.
    /// </summary>
    public QuantumSettings Quantum { get; set; } = new();

    /// <summary>
    /// Statistical mechanics settings.
    /// </summary>
    public StatisticalSettings Statistical { get; set; } = new();

    /// <summary>
    /// Electromagnetic field settings.
    /// </summary>
    public ElectromagneticSettings Electromagnetic { get; set; } = new();

    /// <summary>
    /// Constants validation and precision settings.
    /// </summary>
    public ConstantsSettings Constants { get; set; } = new();

    public override bool Validate()
    {
        return DefaultTemperature > 0 &&
               DefaultTimeStep > 0 &&
               MaxSimulationTime > 0 &&
               DefaultTimeStep < MaxSimulationTime &&
               Enum.IsDefined(typeof(UnitSystem), UnitSystem) &&
               Quantum.Validate() &&
               Statistical.Validate() &&
               Electromagnetic.Validate() &&
               Constants.Validate();
    }

    /// <summary>
    /// Gets the Boltzmann factor for the default temperature.
    /// </summary>
    /// <returns>kT in Joules.</returns>
    public double GetThermalEnergy()
    {
        const double kB = 1.380649e-23; // Boltzmann constant in J/K
        return kB * DefaultTemperature;
    }

    /// <summary>
    /// Gets the number of time steps for the maximum simulation time.
    /// </summary>
    /// <returns>Number of time steps.</returns>
    public long GetMaxTimeSteps()
    {
        return (long)(MaxSimulationTime / DefaultTimeStep);
    }
}

/// <summary>
/// Quantum mechanics specific settings.
/// </summary>
public class QuantumSettings : ConfigurationSection
{
    /// <summary>
    /// Default Hilbert space dimension for quantum systems.
    /// </summary>
    public int DefaultHilbertSpaceDimension { get; set; } = 64;

    /// <summary>
    /// Maximum number of particles to simulate simultaneously.
    /// </summary>
    public int MaxParticles { get; set; } = 10;

    /// <summary>
    /// Whether to enforce normalization of quantum states.
    /// </summary>
    public bool EnforceNormalization { get; set; } = true;

    /// <summary>
    /// Whether to check unitarity of time evolution operators.
    /// </summary>
    public bool ValidateUnitarity { get; set; } = true;

    /// <summary>
    /// Default basis set for quantum calculations.
    /// </summary>
    public BasisSet DefaultBasisSet { get; set; } = BasisSet.Position;

    /// <summary>
    /// Entanglement detection threshold.
    /// </summary>
    public double EntanglementThreshold { get; set; } = 1e-10;

    /// <summary>
    /// Whether to track quantum coherence during calculations.
    /// </summary>
    public bool TrackCoherence { get; set; } = true;

    public override bool Validate()
    {
        return DefaultHilbertSpaceDimension > 0 &&
               MaxParticles > 0 &&
               EntanglementThreshold > 0 &&
               EntanglementThreshold < 1.0 &&
               Enum.IsDefined(typeof(BasisSet), DefaultBasisSet);
    }
}

/// <summary>
/// Statistical mechanics settings.
/// </summary>
public class StatisticalSettings : ConfigurationSection
{
    /// <summary>
    /// Default ensemble type for statistical calculations.
    /// </summary>
    public EnsembleType DefaultEnsemble { get; set; } = EnsembleType.Canonical;

    /// <summary>
    /// Number of Monte Carlo samples for statistical averaging.
    /// </summary>
    public int MonteCarloSamples { get; set; } = 100000;

    /// <summary>
    /// Whether to use importance sampling for Monte Carlo methods.
    /// </summary>
    public bool UseImportanceSampling { get; set; } = true;

    /// <summary>
    /// Convergence tolerance for statistical averages.
    /// </summary>
    public double ConvergenceTolerance { get; set; } = 1e-6;

    /// <summary>
    /// Random seed for reproducible statistical calculations (0 = random).
    /// </summary>
    public int RandomSeed { get; set; } = 0;

    public override bool Validate()
    {
        return MonteCarloSamples > 0 &&
               ConvergenceTolerance > 0 &&
               ConvergenceTolerance < 1.0 &&
               Enum.IsDefined(typeof(EnsembleType), DefaultEnsemble);
    }
}

/// <summary>
/// Electromagnetic field settings.
/// </summary>
public class ElectromagneticSettings : ConfigurationSection
{
    /// <summary>
    /// Whether to include magnetic field effects.
    /// </summary>
    public bool IncludeMagneticFields { get; set; } = true;

    /// <summary>
    /// Whether to use relativistic corrections.
    /// </summary>
    public bool UseRelativisticCorrections { get; set; } = false;

    /// <summary>
    /// Maximum electric field strength in V/m.
    /// </summary>
    public double MaxElectricField { get; set; } = 1e12;

    /// <summary>
    /// Maximum magnetic field strength in Tesla.
    /// </summary>
    public double MaxMagneticField { get; set; } = 100.0;

    /// <summary>
    /// Gauge choice for electromagnetic potentials.
    /// </summary>
    public GaugeChoice Gauge { get; set; } = GaugeChoice.Coulomb;

    public override bool Validate()
    {
        return MaxElectricField > 0 &&
               MaxMagneticField > 0 &&
               Enum.IsDefined(typeof(GaugeChoice), Gauge);
    }
}

/// <summary>
/// Physical constants validation and precision settings.
/// </summary>
public class ConstantsSettings : ConfigurationSection
{
    /// <summary>
    /// Source of fundamental constants.
    /// </summary>
    public ConstantsSource Source { get; set; } = ConstantsSource.CODATA2018;

    /// <summary>
    /// Whether to validate constant relationships during initialization.
    /// </summary>
    public bool ValidateRelationships { get; set; } = true;

    /// <summary>
    /// Tolerance for constant relationship validation.
    /// </summary>
    public double ValidationTolerance { get; set; } = 1e-12;

    /// <summary>
    /// Whether to use experimental uncertainties in calculations.
    /// </summary>
    public bool IncludeUncertainties { get; set; } = false;

    public override bool Validate()
    {
        return ValidationTolerance > 0 &&
               ValidationTolerance < 1.0 &&
               Enum.IsDefined(typeof(ConstantsSource), Source);
    }
}

/// <summary>
/// Available unit systems.
/// </summary>
public enum UnitSystem
{
    /// <summary>
    /// International System of Units.
    /// </summary>
    SI,

    /// <summary>
    /// Gaussian CGS units.
    /// </summary>
    CGS,

    /// <summary>
    /// Atomic units (ℏ = me = e = 1).
    /// </summary>
    Atomic,

    /// <summary>
    /// Natural units (ℏ = c = 1).
    /// </summary>
    Natural,

    /// <summary>
    /// Planck units.
    /// </summary>
    Planck
}

/// <summary>
/// Quantum basis sets.
/// </summary>
public enum BasisSet
{
    Position,
    Momentum,
    Energy,
    AngularMomentum,
    Spin,
    Custom
}

/// <summary>
/// Statistical ensemble types.
/// </summary>
public enum EnsembleType
{
    Microcanonical,
    Canonical,
    GrandCanonical,
    IsothermalIsobaric
}

/// <summary>
/// Electromagnetic gauge choices.
/// </summary>
public enum GaugeChoice
{
    Coulomb,
    Lorenz,
    Temporal,
    Radiation
}

/// <summary>
/// Sources for fundamental constants.
/// </summary>
public enum ConstantsSource
{
    CODATA2018,
    CODATA2014,
    NIST,
    Custom
}
