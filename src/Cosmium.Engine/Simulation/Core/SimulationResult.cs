using System.Numerics;
using System.Text.Json.Serialization;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Simulation.Core;

/// <summary>
/// Comprehensive result data from quantum mechanical simulations.
/// Encapsulates all output data, measurements, and analysis from a completed simulation.
/// </summary>
public class SimulationResult
{
    #region Basic Information

    /// <summary>
    /// Gets the unique identifier for the simulation that generated this result.
    /// </summary>
    public Guid SimulationId { get; }

    /// <summary>
    /// Gets the name of the simulation.
    /// </summary>
    public string SimulationName { get; }

    /// <summary>
    /// Gets the type of simulation that was performed.
    /// </summary>
    public string SimulationType { get; }

    /// <summary>
    /// Gets when the simulation was started.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Gets when the simulation completed.
    /// </summary>
    public DateTime EndTime { get; }

    /// <summary>
    /// Gets the total duration of the simulation.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Gets whether the simulation completed successfully.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the reason for simulation termination.
    /// </summary>
    public string TerminationReason { get; }

    #endregion

    #region Simulation Progress

    /// <summary>
    /// Gets the final simulation time (physical time modeled).
    /// </summary>
    public double FinalTime { get; }

    /// <summary>
    /// Gets the total number of steps executed.
    /// </summary>
    public long TotalSteps { get; }

    /// <summary>
    /// Gets the average time step used.
    /// </summary>
    public double AverageTimeStep { get; }

    /// <summary>
    /// Gets whether the simulation converged to a stable state.
    /// </summary>
    public bool HasConverged { get; }

    /// <summary>
    /// Gets the final convergence error.
    /// </summary>
    public double ConvergenceError { get; }

    #endregion

    #region System State

    /// <summary>
    /// Gets the final total energy of the system.
    /// </summary>
    public double FinalTotalEnergy { get; }

    /// <summary>
    /// Gets the energy conservation error.
    /// </summary>
    public double EnergyConservationError { get; }

    /// <summary>
    /// Gets the final total momentum of the system.
    /// </summary>
    public Complex[] FinalTotalMomentum { get; }

    /// <summary>
    /// Gets the momentum conservation error.
    /// </summary>
    public double MomentumConservationError { get; }

    /// <summary>
    /// Gets the number of particles at the end of the simulation.
    /// </summary>
    public int FinalParticleCount { get; }

    /// <summary>
    /// Gets summary information about particle types and counts.
    /// </summary>
    public Dictionary<string, int> ParticleTypeCounts { get; }

    #endregion

    #region Observable Measurements

    /// <summary>
    /// Gets all observable measurements taken during the simulation.
    /// </summary>
    public IReadOnlyList<ObservableMeasurement> ObservableMeasurements { get; }

    /// <summary>
    /// Gets time series data for key observables.
    /// </summary>
    public Dictionary<string, TimeSeriesData> TimeSeriesData { get; }

    /// <summary>
    /// Gets statistical analysis of measured observables.
    /// </summary>
    public Dictionary<string, StatisticalSummary> ObservableStatistics { get; }

    #endregion

    #region Performance Metrics

    /// <summary>
    /// Gets performance metrics collected during the simulation.
    /// </summary>
    public IReadOnlyDictionary<string, double> PerformanceMetrics { get; }

    /// <summary>
    /// Gets the average computation time per step in milliseconds.
    /// </summary>
    public double AverageStepTime { get; }

    /// <summary>
    /// Gets the peak memory usage during simulation in MB.
    /// </summary>
    public double PeakMemoryUsage { get; }

    /// <summary>
    /// Gets the parallel efficiency achieved (for multi-threaded simulations).
    /// </summary>
    public double ParallelEfficiency { get; }

    #endregion

    #region Validation and Quality

    /// <summary>
    /// Gets validation results for the simulation.
    /// </summary>
    public ValidationSummary ValidationResults { get; }

    /// <summary>
    /// Gets any warnings generated during the simulation.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; }

    /// <summary>
    /// Gets any errors that occurred during the simulation.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Gets the overall quality score of the simulation (0-1).
    /// </summary>
    public double QualityScore { get; }

    #endregion

    #region Raw Data

    /// <summary>
    /// Gets paths to any output files generated.
    /// </summary>
    public IReadOnlyList<string> OutputFiles { get; }

    /// <summary>
    /// Gets additional metadata about the simulation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets whether raw simulation data is available for further analysis.
    /// </summary>
    public bool HasRawData { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new simulation result.
    /// </summary>
    /// <param name="simulationId">The simulation identifier.</param>
    /// <param name="simulationName">The simulation name.</param>
    /// <param name="simulationType">The type of simulation.</param>
    /// <param name="startTime">When the simulation started.</param>
    /// <param name="endTime">When the simulation ended.</param>
    /// <param name="isSuccessful">Whether the simulation completed successfully.</param>
    /// <param name="terminationReason">The reason for termination.</param>
    public SimulationResult(
        Guid simulationId,
        string simulationName,
        string simulationType,
        DateTime startTime,
        DateTime endTime,
        bool isSuccessful,
        string terminationReason)
    {
        SimulationId = simulationId;
        SimulationName = simulationName ?? throw new ArgumentNullException(nameof(simulationName));
        SimulationType = simulationType ?? throw new ArgumentNullException(nameof(simulationType));
        StartTime = startTime;
        EndTime = endTime;
        IsSuccessful = isSuccessful;
        TerminationReason = terminationReason ?? throw new ArgumentNullException(nameof(terminationReason));

        // Initialize collections
        ParticleTypeCounts = new Dictionary<string, int>();
        ObservableMeasurements = new List<ObservableMeasurement>().AsReadOnly();
        TimeSeriesData = new Dictionary<string, TimeSeriesData>();
        ObservableStatistics = new Dictionary<string, StatisticalSummary>();
        PerformanceMetrics = new Dictionary<string, double>().AsReadOnly();
        Warnings = new List<string>().AsReadOnly();
        Errors = new List<string>().AsReadOnly();
        OutputFiles = new List<string>().AsReadOnly();
        Metadata = new Dictionary<string, object>();
        
        // Initialize arrays
        FinalTotalMomentum = new Complex[3];

        // Initialize validation results
        ValidationResults = new ValidationSummary();
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a successful simulation result.
    /// </summary>
    /// <param name="simulation">The simulation that generated this result.</param>
    /// <param name="endTime">When the simulation ended.</param>
    /// <returns>A new successful simulation result.</returns>
    public static SimulationResult CreateSuccessful(ISimulation simulation, DateTime endTime)
    {
        return new SimulationResultBuilder()
            .WithBasicInfo(simulation.Id, simulation.Name, simulation.SimulationType, 
                          simulation.StartTime ?? DateTime.UtcNow, endTime, true, "Completed successfully")
            .WithProgress(simulation.CurrentTime, simulation.CurrentStep, 
                         simulation.TimeStep, simulation.HasConverged, simulation.ConvergenceTolerance)
            .WithSystemState(simulation.TotalEnergy, simulation.TotalMomentum, simulation.ParticleCount)
            .Build();
    }

    /// <summary>
    /// Creates a failed simulation result.
    /// </summary>
    /// <param name="simulation">The simulation that failed.</param>
    /// <param name="endTime">When the simulation failed.</param>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A new failed simulation result.</returns>
    public static SimulationResult CreateFailed(ISimulation simulation, DateTime endTime, string error)
    {
        return new SimulationResultBuilder()
            .WithBasicInfo(simulation.Id, simulation.Name, simulation.SimulationType,
                          simulation.StartTime ?? DateTime.UtcNow, endTime, false, $"Failed: {error}")
            .WithProgress(simulation.CurrentTime, simulation.CurrentStep,
                         simulation.TimeStep, false, double.MaxValue)
            .WithSystemState(simulation.TotalEnergy, simulation.TotalMomentum, simulation.ParticleCount)
            .WithError(error)
            .Build();
    }

    /// <summary>
    /// Creates a cancelled simulation result.
    /// </summary>
    /// <param name="simulation">The simulation that was cancelled.</param>
    /// <param name="endTime">When the simulation was cancelled.</param>
    /// <returns>A new cancelled simulation result.</returns>
    public static SimulationResult CreateCancelled(ISimulation simulation, DateTime endTime)
    {
        return new SimulationResultBuilder()
            .WithBasicInfo(simulation.Id, simulation.Name, simulation.SimulationType,
                          simulation.StartTime ?? DateTime.UtcNow, endTime, false, "Cancelled by user")
            .WithProgress(simulation.CurrentTime, simulation.CurrentStep,
                         simulation.TimeStep, false, double.MaxValue)
            .WithSystemState(simulation.TotalEnergy, simulation.TotalMomentum, simulation.ParticleCount)
            .Build();
    }

    #endregion

    #region Analysis Methods

    /// <summary>
    /// Gets a summary of the simulation performance.
    /// </summary>
    /// <returns>Performance summary string.</returns>
    public string GetPerformanceSummary()
    {
        var stepRate = TotalSteps / Duration.TotalSeconds;
        var timePerStep = Duration.TotalMilliseconds / TotalSteps;
        
        return $"Simulation Performance:\n" +
               $"  Duration: {Duration:hh\\:mm\\:ss\\.fff}\n" +
               $"  Steps: {TotalSteps:N0}\n" +
               $"  Step Rate: {stepRate:F1} steps/sec\n" +
               $"  Time per Step: {timePerStep:F3} ms\n" +
               $"  Peak Memory: {PeakMemoryUsage:F1} MB\n" +
               $"  Parallel Efficiency: {ParallelEfficiency:P1}";
    }

    /// <summary>
    /// Gets a summary of the physical results.
    /// </summary>
    /// <returns>Physics summary string.</returns>
    public string GetPhysicsSummary()
    {
        return $"Physical Results:\n" +
               $"  Final Time: {FinalTime:E3} s\n" +
               $"  Final Energy: {FinalTotalEnergy:E6} J\n" +
               $"  Energy Conservation Error: {EnergyConservationError:E3}\n" +
               $"  Momentum Conservation Error: {MomentumConservationError:E3}\n" +
               $"  Convergence: {(HasConverged ? "Yes" : "No")}\n" +
               $"  Convergence Error: {ConvergenceError:E3}\n" +
               $"  Final Particle Count: {FinalParticleCount}";
    }

    /// <summary>
    /// Gets a summary of the simulation quality.
    /// </summary>
    /// <returns>Quality summary string.</returns>
    public string GetQualitySummary()
    {
        return $"Simulation Quality:\n" +
               $"  Overall Score: {QualityScore:P1}\n" +
               $"  Successful: {IsSuccessful}\n" +
               $"  Warnings: {Warnings.Count}\n" +
               $"  Errors: {Errors.Count}\n" +
               $"  Validation Passed: {ValidationResults.IsValid}\n" +
               $"  Termination Reason: {TerminationReason}";
    }

    /// <summary>
    /// Exports the result to a formatted string.
    /// </summary>
    /// <param name="includeDetails">Whether to include detailed information.</param>
    /// <returns>Formatted result string.</returns>
    public string ExportToString(bool includeDetails = false)
    {
        var result = $"Cosmium Simulation Result\n";
        result += $"========================\n";
        result += $"Simulation: {SimulationName} ({SimulationType})\n";
        result += $"ID: {SimulationId}\n\n";
        
        result += GetPerformanceSummary() + "\n\n";
        result += GetPhysicsSummary() + "\n\n";
        result += GetQualitySummary() + "\n\n";

        if (includeDetails)
        {
            // Add detailed observable data
            if (ObservableStatistics.Any())
            {
                result += "Observable Statistics:\n";
                foreach (var kvp in ObservableStatistics)
                {
                    var stats = kvp.Value;
                    result += $"  {kvp.Key}: μ={stats.Mean:E3}, σ={stats.StandardDeviation:E3}, range=[{stats.Minimum:E3}, {stats.Maximum:E3}]\n";
                }
                result += "\n";
            }

            // Add performance metrics
            if (PerformanceMetrics.Any())
            {
                result += "Performance Metrics:\n";
                foreach (var kvp in PerformanceMetrics)
                {
                    result += $"  {kvp.Key}: {kvp.Value:F3}\n";
                }
                result += "\n";
            }

            // Add warnings and errors
            if (Warnings.Any())
            {
                result += "Warnings:\n";
                foreach (var warning in Warnings)
                {
                    result += $"  - {warning}\n";
                }
                result += "\n";
            }

            if (Errors.Any())
            {
                result += "Errors:\n";
                foreach (var error in Errors)
                {
                    result += $"  - {error}\n";
                }
                result += "\n";
            }
        }

        return result;
    }

    /// <summary>
    /// Calculates a quality score based on various simulation metrics.
    /// </summary>
    /// <returns>Quality score between 0 and 1.</returns>
    public double CalculateQualityScore()
    {
        double score = 1.0;

        // Penalize for lack of success
        if (!IsSuccessful)
            score *= 0.1;

        // Penalize for errors
        score *= Math.Max(0.1, 1.0 - (Errors.Count * 0.2));

        // Penalize for warnings
        score *= Math.Max(0.8, 1.0 - (Warnings.Count * 0.05));

        // Penalize for poor convergence
        if (!HasConverged)
            score *= 0.8;
        else if (ConvergenceError > 1e-6)
            score *= 0.9;

        // Penalize for poor conservation
        if (EnergyConservationError > 1e-6)
            score *= 0.9;
        
        if (MomentumConservationError > 1e-6)
            score *= 0.9;

        // Bonus for validation passing
        if (ValidationResults.IsValid)
            score *= 1.1;

        return Math.Min(1.0, Math.Max(0.0, score));
    }

    #endregion
}

/// <summary>
/// Builder pattern for constructing SimulationResult instances.
/// </summary>
internal class SimulationResultBuilder
{
    private Guid _simulationId;
    private string _simulationName = string.Empty;
    private string _simulationType = string.Empty;
    private DateTime _startTime;
    private DateTime _endTime;
    private bool _isSuccessful;
    private string _terminationReason = string.Empty;
    private double _finalTime;
    private long _totalSteps;
    private double _averageTimeStep;
    private bool _hasConverged;
    private double _convergenceError;
    private double _finalTotalEnergy;
    private double _energyConservationError;
    private Complex[] _finalTotalMomentum = new Complex[3];
    private double _momentumConservationError;
    private int _finalParticleCount;
    private readonly Dictionary<string, int> _particleTypeCounts = new();
    private readonly List<ObservableMeasurement> _observableMeasurements = new();
    private readonly Dictionary<string, TimeSeriesData> _timeSeriesData = new();
    private readonly Dictionary<string, StatisticalSummary> _observableStatistics = new();
    private readonly Dictionary<string, double> _performanceMetrics = new();
    private double _averageStepTime;
    private double _peakMemoryUsage;
    private double _parallelEfficiency;
    private ValidationSummary _validationResults = new();
    private readonly List<string> _warnings = new();
    private readonly List<string> _errors = new();
    private readonly List<string> _outputFiles = new();
    private readonly Dictionary<string, object> _metadata = new();

    public SimulationResultBuilder WithBasicInfo(Guid id, string name, string type, DateTime start, DateTime end, bool successful, string reason)
    {
        _simulationId = id;
        _simulationName = name;
        _simulationType = type;
        _startTime = start;
        _endTime = end;
        _isSuccessful = successful;
        _terminationReason = reason;
        return this;
    }

    public SimulationResultBuilder WithProgress(double finalTime, long totalSteps, double avgTimeStep, bool converged, double convergenceError)
    {
        _finalTime = finalTime;
        _totalSteps = totalSteps;
        _averageTimeStep = avgTimeStep;
        _hasConverged = converged;
        _convergenceError = convergenceError;
        return this;
    }

    public SimulationResultBuilder WithSystemState(double energy, Complex[] momentum, int particleCount)
    {
        _finalTotalEnergy = energy;
        _finalTotalMomentum = momentum ?? new Complex[3];
        _finalParticleCount = particleCount;
        return this;
    }

    public SimulationResultBuilder WithError(string error)
    {
        _errors.Add(error);
        return this;
    }

    public SimulationResult Build()
    {
        // Use reflection to set private fields since SimulationResult has read-only properties
        var result = new SimulationResult(_simulationId, _simulationName, _simulationType, 
                                        _startTime, _endTime, _isSuccessful, _terminationReason);
        
        // Set additional fields using reflection
        var type = typeof(SimulationResult);
        
        type.GetField("<FinalTime>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _finalTime);
        type.GetField("<TotalSteps>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _totalSteps);
        type.GetField("<AverageTimeStep>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _averageTimeStep);
        type.GetField("<HasConverged>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _hasConverged);
        type.GetField("<ConvergenceError>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _convergenceError);
        type.GetField("<FinalTotalEnergy>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _finalTotalEnergy);
        type.GetField("<FinalTotalMomentum>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _finalTotalMomentum);
        type.GetField("<FinalParticleCount>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(result, _finalParticleCount);

        return result;
    }
}

/// <summary>
/// Represents a measurement of an observable during simulation.
/// </summary>
public class ObservableMeasurement
{
    public string ObservableName { get; set; } = string.Empty;
    public double Time { get; set; }
    public double Value { get; set; }
    public double Uncertainty { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Time series data for an observable.
/// </summary>
public class TimeSeriesData
{
    public string ObservableName { get; set; } = string.Empty;
    public double[] Times { get; set; } = Array.Empty<double>();
    public double[] Values { get; set; } = Array.Empty<double>();
    public double[] Uncertainties { get; set; } = Array.Empty<double>();
    public string Units { get; set; } = string.Empty;
}

/// <summary>
/// Statistical summary for an observable.
/// </summary>
public class StatisticalSummary
{
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double Variance { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double Median { get; set; }
    public int SampleCount { get; set; }
    public double Skewness { get; set; }
    public double Kurtosis { get; set; }
}

/// <summary>
/// Validation summary for simulation results.
/// </summary>
public class ValidationSummary
{
    public bool IsValid { get; set; } = true;
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
    public Dictionary<string, bool> CheckResults { get; set; } = new();
}
