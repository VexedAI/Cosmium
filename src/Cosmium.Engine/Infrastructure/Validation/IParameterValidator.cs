using Cosmium.Engine.Simulation.Core;

namespace Cosmium.Engine.Infrastructure.Validation;

/// <summary>
/// Interface for parameter validation services.
/// Provides validation capabilities for simulation parameters and quantum mechanics calculations.
/// </summary>
public interface IParameterValidator
{
    /// <summary>
    /// Validates simulation parameters comprehensively.
    /// </summary>
    /// <param name="parameters">The simulation parameters to validate.</param>
    /// <returns>A task representing the validation operation with the result.</returns>
    Task<ValidationResult> ValidateAsync(SimulationParameters parameters);

    /// <summary>
    /// Validates a double value is finite and within acceptable bounds.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowNaN">Whether NaN values are allowed.</param>
    /// <param name="allowInfinity">Whether infinite values are allowed.</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidateFiniteDouble(double value, string parameterName, bool allowNaN = false, bool allowInfinity = false);

    /// <summary>
    /// Validates that a double value is within the specified range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="tolerance">Tolerance for floating-point comparisons.</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidateRange(double value, string parameterName, double min, double max, double tolerance = 1e-12);

    /// <summary>
    /// Validates that a value is positive.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowZero">Whether zero is considered valid.</param>
    /// <param name="tolerance">Tolerance for zero comparison.</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidatePositive(double value, string parameterName, bool allowZero = false, double tolerance = 1e-12);

    /// <summary>
    /// Validates that an integer is within the specified range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidateIntegerRange(int value, string parameterName, int min, int max);

    /// <summary>
    /// Validates that a value represents a valid probability (between 0 and 1).
    /// </summary>
    /// <param name="probability">The probability value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for boundary checks.</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidateProbability(double probability, string parameterName, double tolerance = 1e-12);

    /// <summary>
    /// Validates that an array is not null and has the expected dimensions.
    /// </summary>
    /// <typeparam name="T">The type of array elements.</typeparam>
    /// <param name="array">The array to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="minLength">The minimum allowed length.</param>
    /// <param name="maxLength">The maximum allowed length.</param>
    /// <param name="exactLength">The exact required length (if specified).</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidateArray<T>(T[]? array, string parameterName, int minLength = 0, int maxLength = int.MaxValue, int? exactLength = null);

    /// <summary>
    /// Validates that a dimension is valid for quantum calculations.
    /// </summary>
    /// <param name="dimension">The Hilbert space dimension to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="requirePowerOfTwo">Whether the dimension must be a power of 2.</param>
    /// <returns>The validation result.</returns>
    ValidationResult ValidateHilbertSpaceDimension(int dimension, string parameterName, bool requirePowerOfTwo = false);

    /// <summary>
    /// Combines multiple validation results using AND logic.
    /// </summary>
    /// <param name="validations">The validation functions to combine.</param>
    /// <returns>A combined validation result.</returns>
    ValidationResult ValidateAll(params Func<ValidationResult>[] validations);

    /// <summary>
    /// Performs validation only if a condition is met.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="validation">The validation function to run if condition is true.</param>
    /// <returns>The validation result or success if condition is false.</returns>
    ValidationResult ValidateIf(bool condition, Func<ValidationResult> validation);
}
