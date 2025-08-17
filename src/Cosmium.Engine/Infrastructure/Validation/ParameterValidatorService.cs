using Cosmium.Engine.Simulation.Core;

namespace Cosmium.Engine.Infrastructure.Validation;

/// <summary>
/// Concrete implementation of parameter validation services.
/// Wraps the static ParameterValidator class to provide dependency injection support.
/// </summary>
public class ParameterValidatorService : IParameterValidator
{
    /// <summary>
    /// Validates simulation parameters comprehensively.
    /// </summary>
    /// <param name="parameters">The simulation parameters to validate.</param>
    /// <returns>A task representing the validation operation with the result.</returns>
    public async Task<ValidationResult> ValidateAsync(SimulationParameters parameters)
    {
        if (parameters == null)
            return ValidationResult.Failure("Parameters cannot be null");

        var validations = new List<Func<ValidationResult>>
        {
            // Basic parameter validation
            () => ValidatePositive(parameters.MaxTime, nameof(parameters.MaxTime)),
            () => ValidateIntegerRange((int)Math.Min(parameters.MaxSteps, int.MaxValue), nameof(parameters.MaxSteps), 1, int.MaxValue),
            () => ValidatePositive(parameters.TimeStep, nameof(parameters.TimeStep)),
            () => ValidateIntegerRange((int)Math.Min(parameters.ProgressUpdateFrequency, int.MaxValue), nameof(parameters.ProgressUpdateFrequency), 1, (int)Math.Min(parameters.MaxSteps, int.MaxValue)),
            
            // Tolerance validation
            () => ValidateRange(parameters.ConvergenceTolerance, nameof(parameters.ConvergenceTolerance), 1e-15, 1e-3),
            () => ValidateRange(parameters.NumericalTolerance, nameof(parameters.NumericalTolerance), 1e-15, 1e-3),
            () => ValidateIf(parameters.UseAdaptiveTimeStep, 
                () => ValidateRange(parameters.AdaptiveErrorTolerance, nameof(parameters.AdaptiveErrorTolerance), 1e-15, 1e-3)),
            
            // Error handling validation
            () => ValidateIntegerRange(parameters.MaxErrors, nameof(parameters.MaxErrors), 1, 10000),
            () => ValidateIntegerRange(parameters.MaxIterations, nameof(parameters.MaxIterations), 1, 1000000),
            
            // Advanced settings validation
            () => ValidateIf(parameters.UseAdaptiveTimeStep, 
                () => ValidatePositive(parameters.MaxTimeStep, nameof(parameters.MaxTimeStep))),
            () => ValidateIf(parameters.UseAdaptiveTimeStep, 
                () => ValidatePositive(parameters.MinTimeStep, nameof(parameters.MinTimeStep))),
            
            // Performance settings validation
            () => ValidateIf(parameters.EnableParallelProcessing && parameters.ThreadCount > 0,
                () => ValidateIntegerRange(parameters.ThreadCount, nameof(parameters.ThreadCount), 1, Environment.ProcessorCount * 2)),
            () => ValidateIf(parameters.MemoryLimitMB > 0,
                () => ValidateIntegerRange(parameters.MemoryLimitMB, nameof(parameters.MemoryLimitMB), 128, 1024 * 1024)), // 128MB to 1TB
            
            // Physical parameters validation
            () => ValidatePositive(parameters.Temperature, nameof(parameters.Temperature), allowZero: true),
            () => ValidatePositive(parameters.Pressure, nameof(parameters.Pressure), allowZero: true),
            
            // Output validation
            () => ValidateIntegerRange((int)Math.Min(parameters.OutputFrequency, int.MaxValue), nameof(parameters.OutputFrequency), 1, (int)Math.Min(parameters.MaxSteps, int.MaxValue)),
            
            // Consistency checks
            () => ValidateIf(parameters.UseAdaptiveTimeStep,
                () => parameters.MinTimeStep <= parameters.MaxTimeStep ? 
                    ValidationResult.Success() : 
                    ValidationResult.Failure("MinTimeStep must be less than or equal to MaxTimeStep")),
            () => parameters.MaxTime >= parameters.TimeStep ?
                ValidationResult.Success() :
                ValidationResult.Failure("MaxTime must be greater than or equal to TimeStep"),
            () => parameters.MaxSteps >= parameters.ProgressUpdateFrequency ?
                ValidationResult.Success() :
                ValidationResult.Failure("MaxSteps must be greater than or equal to ProgressUpdateFrequency"),
            () => parameters.MaxSteps >= parameters.OutputFrequency ?
                ValidationResult.Success() :
                ValidationResult.Failure("MaxSteps must be greater than or equal to OutputFrequency")
        };

        return await Task.FromResult(ValidateAll(validations.ToArray()));
    }

    /// <summary>
    /// Validates a double value is finite and within acceptable bounds.
    /// </summary>
    public ValidationResult ValidateFiniteDouble(double value, string parameterName, bool allowNaN = false, bool allowInfinity = false)
    {
        return ParameterValidator.ValidateFiniteDouble(value, parameterName, allowNaN, allowInfinity);
    }

    /// <summary>
    /// Validates that a double value is within the specified range.
    /// </summary>
    public ValidationResult ValidateRange(double value, string parameterName, double min, double max, double tolerance = 1e-12)
    {
        return ParameterValidator.ValidateRange(value, parameterName, min, max, tolerance);
    }

    /// <summary>
    /// Validates that a value is positive.
    /// </summary>
    public ValidationResult ValidatePositive(double value, string parameterName, bool allowZero = false, double tolerance = 1e-12)
    {
        return ParameterValidator.ValidatePositive(value, parameterName, allowZero, tolerance);
    }

    /// <summary>
    /// Validates that an integer is within the specified range.
    /// </summary>
    public ValidationResult ValidateIntegerRange(int value, string parameterName, int min, int max)
    {
        return ParameterValidator.ValidateIntegerRange(value, parameterName, min, max);
    }

    /// <summary>
    /// Validates that a value represents a valid probability (between 0 and 1).
    /// </summary>
    public ValidationResult ValidateProbability(double probability, string parameterName, double tolerance = 1e-12)
    {
        return ParameterValidator.ValidateProbability(probability, parameterName, tolerance);
    }

    /// <summary>
    /// Validates that an array is not null and has the expected dimensions.
    /// </summary>
    public ValidationResult ValidateArray<T>(T[]? array, string parameterName, int minLength = 0, int maxLength = int.MaxValue, int? exactLength = null)
    {
        return ParameterValidator.ValidateArray(array, parameterName, minLength, maxLength, exactLength);
    }

    /// <summary>
    /// Validates that a dimension is valid for quantum calculations.
    /// </summary>
    public ValidationResult ValidateHilbertSpaceDimension(int dimension, string parameterName, bool requirePowerOfTwo = false)
    {
        return ParameterValidator.ValidateHilbertSpaceDimension(dimension, parameterName, requirePowerOfTwo);
    }

    /// <summary>
    /// Combines multiple validation results using AND logic.
    /// </summary>
    public ValidationResult ValidateAll(params Func<ValidationResult>[] validations)
    {
        return ParameterValidator.ValidateAll(validations);
    }

    /// <summary>
    /// Performs validation only if a condition is met.
    /// </summary>
    public ValidationResult ValidateIf(bool condition, Func<ValidationResult> validation)
    {
        return ParameterValidator.ValidateIf(condition, validation);
    }
}
