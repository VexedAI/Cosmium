using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cosmium.Engine.Infrastructure.Configuration;

/// <summary>
/// Central configuration management for the Cosmium Engine.
/// Provides a unified interface for all engine settings.
/// </summary>
public class EngineConfiguration
{
    private static EngineConfiguration? _instance;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the singleton instance of the engine configuration.
    /// </summary>
    public static EngineConfiguration Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new EngineConfiguration();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Computational settings for the engine.
    /// </summary>
    public ComputationSettings Computation { get; set; } = new();

    /// <summary>
    /// Physics constants and settings.
    /// </summary>
    public PhysicsSettings Physics { get; set; } = new();

    /// <summary>
    /// Logging configuration settings.
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();

    /// <summary>
    /// Performance monitoring settings.
    /// </summary>
    public PerformanceSettings Performance { get; set; } = new();

    /// <summary>
    /// Validation settings for input parameters and physics constraints.
    /// </summary>
    public ValidationSettings Validation { get; set; } = new();

    private EngineConfiguration() { }

    /// <summary>
    /// Loads configuration from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the configuration file.</param>
    /// <returns>The loaded configuration instance.</returns>
    public static EngineConfiguration LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<EngineConfiguration>(json, GetJsonOptions()) 
                        ?? throw new InvalidOperationException("Failed to deserialize configuration");
            
            lock (_lock)
            {
                _instance = config;
            }
            
            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in configuration file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Saves the current configuration to a JSON file.
    /// </summary>
    /// <param name="filePath">Path where to save the configuration file.</param>
    public void SaveToFile(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(this, GetJsonOptions());
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration to {filePath}", ex);
        }
    }

    /// <summary>
    /// Creates a deep copy of the current configuration.
    /// </summary>
    /// <returns>A new configuration instance with the same values.</returns>
    public EngineConfiguration Clone()
    {
        var json = JsonSerializer.Serialize(this, GetJsonOptions());
        return JsonSerializer.Deserialize<EngineConfiguration>(json, GetJsonOptions())
               ?? throw new InvalidOperationException("Failed to clone configuration");
    }

    /// <summary>
    /// Validates the current configuration settings.
    /// </summary>
    /// <returns>True if all settings are valid.</returns>
    public bool Validate()
    {
        try
        {
            return Computation.Validate() &&
                   Physics.Validate() &&
                   Logging.Validate() &&
                   Performance.Validate() &&
                   Validation.Validate();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resets the configuration to default values.
    /// </summary>
    public void Reset()
    {
        Computation = new ComputationSettings();
        Physics = new PhysicsSettings();
        Logging = new LoggingSettings();
        Performance = new PerformanceSettings();
        Validation = new ValidationSettings();
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}

/// <summary>
/// Base class for configuration sections with validation support.
/// </summary>
public abstract class ConfigurationSection
{
    /// <summary>
    /// Validates the configuration section settings.
    /// </summary>
    /// <returns>True if the settings are valid.</returns>
    public abstract bool Validate();
}

/// <summary>
/// Logging configuration settings.
/// </summary>
public class LoggingSettings : ConfigurationSection
{
    /// <summary>
    /// Minimum log level to record.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Whether to log to console.
    /// </summary>
    public bool LogToConsole { get; set; } = true;

    /// <summary>
    /// Whether to log to file.
    /// </summary>
    public bool LogToFile { get; set; } = true;

    /// <summary>
    /// Log file path pattern.
    /// </summary>
    public string LogFilePath { get; set; } = "logs/cosmium-{Date}.log";

    /// <summary>
    /// Maximum log file size in MB before rotation.
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 10;

    /// <summary>
    /// Number of log files to retain.
    /// </summary>
    public int RetainedFileCount { get; set; } = 7;

    public override bool Validate()
    {
        return MaxFileSizeMB > 0 && 
               RetainedFileCount > 0 && 
               !string.IsNullOrWhiteSpace(LogFilePath);
    }
}

/// <summary>
/// Performance monitoring settings.
/// </summary>
public class PerformanceSettings : ConfigurationSection
{
    /// <summary>
    /// Whether to enable performance monitoring.
    /// </summary>
    public bool EnableMonitoring { get; set; } = true;

    /// <summary>
    /// Interval for performance metrics collection in milliseconds.
    /// </summary>
    public int MetricsIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Whether to collect memory usage statistics.
    /// </summary>
    public bool CollectMemoryStats { get; set; } = true;

    /// <summary>
    /// Whether to collect CPU usage statistics.
    /// </summary>
    public bool CollectCpuStats { get; set; } = true;

    /// <summary>
    /// Maximum number of performance samples to retain in memory.
    /// </summary>
    public int MaxSamplesInMemory { get; set; } = 1000;

    public override bool Validate()
    {
        return MetricsIntervalMs > 0 && MaxSamplesInMemory > 0;
    }
}

/// <summary>
/// Validation framework settings.
/// </summary>
public class ValidationSettings : ConfigurationSection
{
    /// <summary>
    /// Whether to enable strict parameter validation.
    /// </summary>
    public bool EnableStrictValidation { get; set; } = true;

    /// <summary>
    /// Whether to validate physics constraints.
    /// </summary>
    public bool ValidatePhysicsConstraints { get; set; } = true;

    /// <summary>
    /// Tolerance for floating-point comparisons in validation.
    /// </summary>
    public double ValidationTolerance { get; set; } = 1e-12;

    /// <summary>
    /// Whether to throw exceptions on validation failures or just log warnings.
    /// </summary>
    public bool ThrowOnValidationFailure { get; set; } = true;

    public override bool Validate()
    {
        return ValidationTolerance > 0 && ValidationTolerance < 1.0;
    }
}

/// <summary>
/// Log levels for the engine logging system.
/// </summary>
public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}
