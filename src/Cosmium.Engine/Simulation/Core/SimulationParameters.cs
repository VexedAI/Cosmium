using System.Text.Json.Serialization;
using Cosmium.Engine.Infrastructure.Configuration;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;

namespace Cosmium.Engine.Simulation.Core;

/// <summary>
/// Parameter management system for quantum mechanical simulations.
/// Provides comprehensive configuration and validation of simulation parameters.
/// </summary>
public class SimulationParameters : ConfigurationSection
{
    #region Core Parameters

    /// <summary>
    /// Gets or sets the name of the simulation.
    /// </summary>
    public string Name { get; set; } = "Untitled Simulation";

    /// <summary>
    /// Gets or sets a detailed description of the simulation.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of simulation being performed.
    /// </summary>
    public string SimulationType { get; set; } = "Generic";

    /// <summary>
    /// Gets or sets additional metadata for the simulation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    #endregion

    #region Temporal Parameters

    /// <summary>
    /// Gets or sets the time step for discrete time evolution in seconds.
    /// </summary>
    public double TimeStep { get; set; } = 1e-18; // 1 attosecond

    /// <summary>
    /// Gets or sets the maximum simulation time in seconds.
    /// </summary>
    public double MaxTime { get; set; } = 1e-12; // 1 picosecond

    /// <summary>
    /// Gets or sets the maximum number of simulation steps.
    /// </summary>
    public long MaxSteps { get; set; } = 1_000_000;

    /// <summary>
    /// Gets or sets whether to use adaptive time stepping.
    /// </summary>
    public bool UseAdaptiveTimeStep { get; set; } = false;

    /// <summary>
    /// Gets or sets the minimum allowed time step for adaptive stepping.
    /// </summary>
    public double MinTimeStep { get; set; } = 1e-21; // 1 zeptosecond

    /// <summary>
    /// Gets or sets the maximum allowed time step for adaptive stepping.
    /// </summary>
    public double MaxTimeStep { get; set; } = 1e-15; // 1 femtosecond

    /// <summary>
    /// Gets or sets the error tolerance for adaptive time stepping.
    /// </summary>
    public double AdaptiveErrorTolerance { get; set; } = 1e-10;

    #endregion

    #region Computational Parameters

    /// <summary>
    /// Gets or sets the numerical tolerance for convergence checks.
    /// </summary>
    public double NumericalTolerance { get; set; } = 1e-12;

    /// <summary>
    /// Gets or sets the convergence tolerance for stability detection.
    /// </summary>
    public double ConvergenceTolerance { get; set; } = 1e-10;

    /// <summary>
    /// Gets or sets the maximum number of iterations for iterative solvers.
    /// </summary>
    public int MaxIterations { get; set; } = 10000;

    /// <summary>
    /// Gets or sets whether to enable parallel processing.
    /// </summary>
    public bool EnableParallelProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of threads to use (0 = auto-detect).
    /// </summary>
    public int ThreadCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the memory limit in MB (0 = no limit).
    /// </summary>
    public int MemoryLimitMB { get; set; } = 0;

    #endregion

    #region Physical Environment

    /// <summary>
    /// Gets or sets the ambient temperature in Kelvin.
    /// </summary>
    public double Temperature { get; set; } = 300.0; // Room temperature

    /// <summary>
    /// Gets or sets the pressure in Pascals.
    /// </summary>
    public double Pressure { get; set; } = 101325.0; // Standard atmospheric pressure

    /// <summary>
    /// Gets or sets external electromagnetic field parameters.
    /// </summary>
    public ElectromagneticField ExternalField { get; set; } = new();

    /// <summary>
    /// Gets or sets the boundary conditions for the simulation space.
    /// </summary>
    public BoundaryConditions Boundaries { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to include relativistic effects.
    /// </summary>
    public bool IncludeRelativisticEffects { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include quantum field effects.
    /// </summary>
    public bool IncludeQuantumFieldEffects { get; set; } = false;

    #endregion

    #region Output and Monitoring

    /// <summary>
    /// Gets or sets the frequency of progress updates (every N steps).
    /// </summary>
    public long ProgressUpdateFrequency { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the frequency of data output (every N steps).
    /// </summary>
    public long OutputFrequency { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether to enable detailed logging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to save intermediate results.
    /// </summary>
    public bool SaveIntermediateResults { get; set; } = true;

    /// <summary>
    /// Gets or sets the output directory for simulation results.
    /// </summary>
    public string OutputDirectory { get; set; } = "results";

    /// <summary>
    /// Gets or sets the format for output files.
    /// </summary>
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Json;

    #endregion

    #region Validation and Error Handling

    /// <summary>
    /// Gets or sets whether to enable strict validation.
    /// </summary>
    public bool EnableStrictValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to continue simulation on non-critical errors.
    /// </summary>
    public bool ContinueOnErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of errors before termination.
    /// </summary>
    public int MaxErrors { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to enable state validation at each step.
    /// </summary>
    public bool ValidateStateEachStep { get; set; } = false;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates default parameters suitable for atomic simulations.
    /// </summary>
    /// <returns>Parameters optimized for atomic-scale simulations.</returns>
    public static SimulationParameters CreateForAtomicSimulation()
    {
        return new SimulationParameters
        {
            SimulationType = "Atomic",
            TimeStep = 1e-18, // 1 attosecond
            MaxTime = 1e-12,   // 1 picosecond
            MaxSteps = 1_000_000,
            NumericalTolerance = 1e-12,
            ConvergenceTolerance = 1e-10,
            Temperature = 300.0,
            EnableParallelProcessing = true,
            SaveIntermediateResults = true,
            OutputFrequency = 100
        };
    }

    /// <summary>
    /// Creates default parameters suitable for molecular dynamics simulations.
    /// </summary>
    /// <returns>Parameters optimized for molecular simulations.</returns>
    public static SimulationParameters CreateForMolecularSimulation()
    {
        return new SimulationParameters
        {
            SimulationType = "Molecular",
            TimeStep = 1e-15, // 1 femtosecond
            MaxTime = 1e-9,    // 1 nanosecond
            MaxSteps = 1_000_000,
            NumericalTolerance = 1e-10,
            ConvergenceTolerance = 1e-8,
            Temperature = 300.0,
            EnableParallelProcessing = true,
            SaveIntermediateResults = true,
            OutputFrequency = 1000
        };
    }

    /// <summary>
    /// Creates default parameters suitable for particle collision simulations.
    /// </summary>
    /// <returns>Parameters optimized for high-energy particle physics.</returns>
    public static SimulationParameters CreateForParticleCollision()
    {
        return new SimulationParameters
        {
            SimulationType = "ParticleCollision",
            TimeStep = 1e-24, // 1 yoctosecond
            MaxTime = 1e-18,   // 1 attosecond
            MaxSteps = 1_000_000,
            NumericalTolerance = 1e-15,
            ConvergenceTolerance = 1e-12,
            Temperature = 0.0, // High-energy collisions
            IncludeRelativisticEffects = true,
            IncludeQuantumFieldEffects = true,
            EnableParallelProcessing = true,
            SaveIntermediateResults = false,
            OutputFrequency = 10
        };
    }

    /// <summary>
    /// Creates default parameters suitable for quantum field simulations.
    /// </summary>
    /// <returns>Parameters optimized for quantum field theory calculations.</returns>
    public static SimulationParameters CreateForQuantumField()
    {
        return new SimulationParameters
        {
            SimulationType = "QuantumField",
            TimeStep = 1e-21, // 1 zeptosecond
            MaxTime = 1e-15,   // 1 femtosecond
            MaxSteps = 100_000,
            NumericalTolerance = 1e-15,
            ConvergenceTolerance = 1e-12,
            Temperature = 0.0,
            IncludeRelativisticEffects = true,
            IncludeQuantumFieldEffects = true,
            EnableParallelProcessing = true,
            MemoryLimitMB = 8192, // 8 GB limit for QFT calculations
            SaveIntermediateResults = false,
            OutputFrequency = 100
        };
    }

    #endregion

    #region Parameter Management

    /// <summary>
    /// Gets a parameter value by name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The parameter value, or null if not found.</returns>
    public object? GetParameter(string parameterName)
    {
        var property = GetType().GetProperty(parameterName);
        return property?.GetValue(this);
    }

    /// <summary>
    /// Sets a parameter value by name.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>True if the parameter was set successfully.</returns>
    public bool SetParameter(string parameterName, object value)
    {
        try
        {
            var property = GetType().GetProperty(parameterName);
            if (property?.CanWrite == true)
            {
                property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets all parameter names and values.
    /// </summary>
    /// <returns>Dictionary of parameter names and values.</returns>
    public Dictionary<string, object?> GetAllParameters()
    {
        var result = new Dictionary<string, object?>();
        var properties = GetType().GetProperties();
        
        foreach (var property in properties)
        {
            if (property.CanRead)
            {
                result[property.Name] = property.GetValue(this);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Creates a deep copy of these parameters.
    /// </summary>
    /// <returns>A new SimulationParameters instance with the same values.</returns>
    public SimulationParameters Clone()
    {
        var clone = new SimulationParameters();
        var properties = GetType().GetProperties();
        
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(this);
                if (value != null)
                {
                    property.SetValue(clone, value);
                }
            }
        }
        
        return clone;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates all simulation parameters.
    /// </summary>
    /// <returns>True if all parameters are valid.</returns>
    public override bool Validate()
    {
        var issues = GetValidationIssues();
        return issues.Count == 0;
    }

    /// <summary>
    /// Gets detailed validation issues for the current parameters.
    /// </summary>
    /// <returns>List of validation issue descriptions.</returns>
    public List<string> GetValidationIssues()
    {
        var issues = new List<string>();

        // Validate temporal parameters
        if (TimeStep <= 0)
            issues.Add("TimeStep must be positive");
        if (MaxTime <= 0)
            issues.Add("MaxTime must be positive");
        if (MaxSteps <= 0)
            issues.Add("MaxSteps must be positive");
        if (TimeStep > MaxTime)
            issues.Add("TimeStep cannot be larger than MaxTime");

        // Validate adaptive time stepping
        if (UseAdaptiveTimeStep)
        {
            if (MinTimeStep <= 0)
                issues.Add("MinTimeStep must be positive when using adaptive time stepping");
            if (MaxTimeStep <= MinTimeStep)
                issues.Add("MaxTimeStep must be greater than MinTimeStep");
            if (AdaptiveErrorTolerance <= 0)
                issues.Add("AdaptiveErrorTolerance must be positive");
        }

        // Validate computational parameters
        if (NumericalTolerance <= 0)
            issues.Add("NumericalTolerance must be positive");
        if (ConvergenceTolerance <= 0)
            issues.Add("ConvergenceTolerance must be positive");
        if (MaxIterations <= 0)
            issues.Add("MaxIterations must be positive");
        if (ThreadCount < 0)
            issues.Add("ThreadCount cannot be negative");
        if (MemoryLimitMB < 0)
            issues.Add("MemoryLimitMB cannot be negative");

        // Validate physical parameters
        if (Temperature < 0)
            issues.Add("Temperature cannot be negative");
        if (Pressure < 0)
            issues.Add("Pressure cannot be negative");

        // Validate output parameters
        if (ProgressUpdateFrequency <= 0)
            issues.Add("ProgressUpdateFrequency must be positive");
        if (OutputFrequency <= 0)
            issues.Add("OutputFrequency must be positive");
        if (string.IsNullOrWhiteSpace(OutputDirectory))
            issues.Add("OutputDirectory cannot be empty");

        // Validate error handling parameters
        if (MaxErrors <= 0)
            issues.Add("MaxErrors must be positive");

        return issues;
    }

    /// <summary>
    /// Validates parameters using the engine's validation framework.
    /// </summary>
    /// <returns>Validation result with detailed feedback.</returns>
    public ValidationResult ValidateWithFramework()
    {
        var timeStepResult = ParameterValidator.ValidatePositive(TimeStep, nameof(TimeStep));
        if (!timeStepResult.IsValid)
            return timeStepResult;

        var maxTimeResult = ParameterValidator.ValidatePositive(MaxTime, nameof(MaxTime));
        if (!maxTimeResult.IsValid)
            return maxTimeResult;

        var maxStepsResult = ParameterValidator.ValidatePositive(MaxSteps, nameof(MaxSteps));
        if (!maxStepsResult.IsValid)
            return maxStepsResult;

        var temperatureResult = PhysicsValidator.ValidateTemperature(Temperature, nameof(Temperature));
        if (!temperatureResult.IsValid)
            return temperatureResult;

        var pressureResult = ParameterValidator.ValidatePositive(Pressure, nameof(Pressure), allowZero: true);
        if (!pressureResult.IsValid)
            return pressureResult;

        var toleranceResult = ParameterValidator.ValidateRange(NumericalTolerance, nameof(NumericalTolerance), 0.0, 1.0);
        if (!toleranceResult.IsValid)
            return toleranceResult;

        return ValidationResult.Success();
    }

    #endregion
}

/// <summary>
/// Electromagnetic field configuration for simulation environment.
/// </summary>
public class ElectromagneticField
{
    /// <summary>
    /// Gets or sets the electric field vector in V/m.
    /// </summary>
    public double[] ElectricField { get; set; } = new double[3];

    /// <summary>
    /// Gets or sets the magnetic field vector in Tesla.
    /// </summary>
    public double[] MagneticField { get; set; } = new double[3];

    /// <summary>
    /// Gets or sets whether the fields are time-dependent.
    /// </summary>
    public bool IsTimeDependent { get; set; } = false;

    /// <summary>
    /// Gets or sets the frequency for oscillating fields in Hz.
    /// </summary>
    public double Frequency { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the phase for oscillating fields in radians.
    /// </summary>
    public double Phase { get; set; } = 0.0;
}

/// <summary>
/// Boundary conditions for the simulation space.
/// </summary>
public class BoundaryConditions
{
    /// <summary>
    /// Gets or sets the type of boundary conditions.
    /// </summary>
    public BoundaryType Type { get; set; } = BoundaryType.Periodic;

    /// <summary>
    /// Gets or sets the simulation box dimensions in meters.
    /// </summary>
    public double[] BoxSize { get; set; } = new double[] { 1e-9, 1e-9, 1e-9 }; // 1 nm cube

    /// <summary>
    /// Gets or sets the center of the simulation box.
    /// </summary>
    public double[] BoxCenter { get; set; } = new double[3];

    /// <summary>
    /// Gets or sets whether to use spherical boundary geometry.
    /// </summary>
    public bool UseSphericalBoundary { get; set; } = false;

    /// <summary>
    /// Gets or sets the radius for spherical boundaries in meters.
    /// </summary>
    public double SphereRadius { get; set; } = 1e-9; // 1 nm
}

/// <summary>
/// Types of boundary conditions for simulation space.
/// </summary>
public enum BoundaryType
{
    /// <summary>
    /// Periodic boundary conditions (particles wrap around).
    /// </summary>
    Periodic,

    /// <summary>
    /// Fixed boundary conditions (particles reflect).
    /// </summary>
    Fixed,

    /// <summary>
    /// Open boundary conditions (particles can leave).
    /// </summary>
    Open,

    /// <summary>
    /// Absorbing boundary conditions (particles are absorbed).
    /// </summary>
    Absorbing
}

/// <summary>
/// Output format options for simulation results.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// JavaScript Object Notation format.
    /// </summary>
    Json,

    /// <summary>
    /// Comma-separated values format.
    /// </summary>
    Csv,

    /// <summary>
    /// Hierarchical Data Format 5.
    /// </summary>
    Hdf5,

    /// <summary>
    /// Binary format for high performance.
    /// </summary>
    Binary,

    /// <summary>
    /// XML format for structured data.
    /// </summary>
    Xml
}
