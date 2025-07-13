using System.Numerics;
using Cosmium.Engine.Infrastructure.Configuration;
using Cosmium.Engine.Infrastructure.Logging;

namespace Cosmium.Engine.Infrastructure.Validation;

/// <summary>
/// Input parameter validation for quantum mechanics calculations.
/// Provides comprehensive validation of numerical inputs and ranges.
/// </summary>
public static class ParameterValidator
{
    private static readonly SimulationLogger Logger = SimulationLogger.Instance;

    #region Basic Type Validation

    /// <summary>
    /// Validates that a double value is finite and within acceptable bounds.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowNaN">Whether NaN values are allowed.</param>
    /// <param name="allowInfinity">Whether infinite values are allowed.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateFiniteDouble(double value, string parameterName, bool allowNaN = false, bool allowInfinity = false)
    {
        var context = new { Parameter = parameterName, Value = value, AllowNaN = allowNaN, AllowInfinity = allowInfinity };
        
        if (double.IsNaN(value) && !allowNaN)
        {
            var error = $"Parameter '{parameterName}' cannot be NaN";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (double.IsInfinity(value) && !allowInfinity)
        {
            var error = $"Parameter '{parameterName}' cannot be infinite";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (!double.IsFinite(value) && !allowNaN && !allowInfinity)
        {
            var error = $"Parameter '{parameterName}' must be a finite number";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a double value is within the specified range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="tolerance">Tolerance for floating-point comparisons.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateRange(double value, string parameterName, double min, double max, double tolerance = 1e-12)
    {
        var finiteResult = ValidateFiniteDouble(value, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        var context = new { Parameter = parameterName, Value = value, Min = min, Max = max, Tolerance = tolerance };

        if (value < min - tolerance)
        {
            var error = $"Parameter '{parameterName}' ({value}) is below minimum allowed value ({min})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (value > max + tolerance)
        {
            var error = $"Parameter '{parameterName}' ({value}) is above maximum allowed value ({max})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a value is positive.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="allowZero">Whether zero is considered valid.</param>
    /// <param name="tolerance">Tolerance for zero comparison.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidatePositive(double value, string parameterName, bool allowZero = false, double tolerance = 1e-12)
    {
        var finiteResult = ValidateFiniteDouble(value, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        var context = new { Parameter = parameterName, Value = value, AllowZero = allowZero, Tolerance = tolerance };

        if (!allowZero && Math.Abs(value) < tolerance)
        {
            var error = $"Parameter '{parameterName}' must be positive (cannot be zero)";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (value < (allowZero ? -tolerance : tolerance))
        {
            var error = $"Parameter '{parameterName}' ({value}) must be {(allowZero ? "non-negative" : "positive")}";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that an integer is within the specified range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateIntegerRange(int value, string parameterName, int min, int max)
    {
        var context = new { Parameter = parameterName, Value = value, Min = min, Max = max };

        if (value < min)
        {
            var error = $"Parameter '{parameterName}' ({value}) is below minimum allowed value ({min})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (value > max)
        {
            var error = $"Parameter '{parameterName}' ({value}) is above maximum allowed value ({max})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Complex Number Validation

    /// <summary>
    /// Validates that a complex number has finite real and imaginary parts.
    /// </summary>
    /// <param name="value">The complex number to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateFiniteComplex(Complex value, string parameterName)
    {
        var realResult = ValidateFiniteDouble(value.Real, $"{parameterName}.Real");
        if (!realResult.IsValid)
            return realResult;

        var imagResult = ValidateFiniteDouble(value.Imaginary, $"{parameterName}.Imaginary");
        if (!imagResult.IsValid)
            return imagResult;

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a complex number has magnitude within the specified range.
    /// </summary>
    /// <param name="value">The complex number to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="minMagnitude">The minimum allowed magnitude.</param>
    /// <param name="maxMagnitude">The maximum allowed magnitude.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateComplexMagnitude(Complex value, string parameterName, double minMagnitude = 0.0, double maxMagnitude = double.MaxValue)
    {
        var finiteResult = ValidateFiniteComplex(value, parameterName);
        if (!finiteResult.IsValid)
            return finiteResult;

        var magnitude = value.Magnitude;
        return ValidateRange(magnitude, $"{parameterName}.Magnitude", minMagnitude, maxMagnitude);
    }

    #endregion

    #region Array and Collection Validation

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
    public static ValidationResult ValidateArray<T>(T[]? array, string parameterName, int minLength = 0, int maxLength = int.MaxValue, int? exactLength = null)
    {
        if (array == null)
        {
            var error = $"Parameter '{parameterName}' cannot be null";
            Logger.Warning(error);
            return ValidationResult.Failure(error);
        }

        var context = new { Parameter = parameterName, Length = array.Length, MinLength = minLength, MaxLength = maxLength, ExactLength = exactLength };

        if (exactLength.HasValue && array.Length != exactLength.Value)
        {
            var error = $"Parameter '{parameterName}' must have exactly {exactLength.Value} elements (found {array.Length})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (array.Length < minLength)
        {
            var error = $"Parameter '{parameterName}' must have at least {minLength} elements (found {array.Length})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (array.Length > maxLength)
        {
            var error = $"Parameter '{parameterName}' must have at most {maxLength} elements (found {array.Length})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that all elements in a double array are finite.
    /// </summary>
    /// <param name="array">The array to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateFiniteDoubleArray(double[]? array, string parameterName)
    {
        var arrayResult = ValidateArray(array, parameterName, minLength: 1);
        if (!arrayResult.IsValid)
            return arrayResult;

        for (int i = 0; i < array!.Length; i++)
        {
            var elementResult = ValidateFiniteDouble(array[i], $"{parameterName}[{i}]");
            if (!elementResult.IsValid)
                return elementResult;
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that all elements in a complex array are finite.
    /// </summary>
    /// <param name="array">The array to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateFiniteComplexArray(Complex[]? array, string parameterName)
    {
        var arrayResult = ValidateArray(array, parameterName, minLength: 1);
        if (!arrayResult.IsValid)
            return arrayResult;

        for (int i = 0; i < array!.Length; i++)
        {
            var elementResult = ValidateFiniteComplex(array[i], $"{parameterName}[{i}]");
            if (!elementResult.IsValid)
                return elementResult;
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Matrix Validation

    /// <summary>
    /// Validates that a 2D array represents a valid matrix.
    /// </summary>
    /// <typeparam name="T">The type of matrix elements.</typeparam>
    /// <param name="matrix">The matrix to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="minRows">The minimum number of rows.</param>
    /// <param name="maxRows">The maximum number of rows.</param>
    /// <param name="minColumns">The minimum number of columns.</param>
    /// <param name="maxColumns">The maximum number of columns.</param>
    /// <param name="requireSquare">Whether the matrix must be square.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateMatrix<T>(T[,]? matrix, string parameterName, 
        int minRows = 1, int maxRows = int.MaxValue, 
        int minColumns = 1, int maxColumns = int.MaxValue, 
        bool requireSquare = false)
    {
        if (matrix == null)
        {
            var error = $"Parameter '{parameterName}' cannot be null";
            Logger.Warning(error);
            return ValidationResult.Failure(error);
        }

        var rows = matrix.GetLength(0);
        var columns = matrix.GetLength(1);
        var context = new { Parameter = parameterName, Rows = rows, Columns = columns, RequireSquare = requireSquare };

        if (rows < minRows)
        {
            var error = $"Matrix '{parameterName}' must have at least {minRows} rows (found {rows})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (rows > maxRows)
        {
            var error = $"Matrix '{parameterName}' must have at most {maxRows} rows (found {rows})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (columns < minColumns)
        {
            var error = $"Matrix '{parameterName}' must have at least {minColumns} columns (found {columns})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (columns > maxColumns)
        {
            var error = $"Matrix '{parameterName}' must have at most {maxColumns} columns (found {columns})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        if (requireSquare && rows != columns)
        {
            var error = $"Matrix '{parameterName}' must be square (found {rows}x{columns})";
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a complex matrix has finite elements.
    /// </summary>
    /// <param name="matrix">The matrix to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateFiniteComplexMatrix(Complex[,]? matrix, string parameterName)
    {
        var matrixResult = ValidateMatrix(matrix, parameterName);
        if (!matrixResult.IsValid)
            return matrixResult;

        var rows = matrix!.GetLength(0);
        var columns = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                var elementResult = ValidateFiniteComplex(matrix[i, j], $"{parameterName}[{i},{j}]");
                if (!elementResult.IsValid)
                    return elementResult;
            }
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Probability Validation

    /// <summary>
    /// Validates that a value represents a valid probability (between 0 and 1).
    /// </summary>
    /// <param name="probability">The probability value to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for boundary checks.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateProbability(double probability, string parameterName, double tolerance = 1e-12)
    {
        return ValidateRange(probability, parameterName, 0.0, 1.0, tolerance);
    }

    /// <summary>
    /// Validates that an array represents valid probabilities that sum to 1.
    /// </summary>
    /// <param name="probabilities">The probability array to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for normalization check.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateProbabilityDistribution(double[]? probabilities, string parameterName, double tolerance = 1e-12)
    {
        var arrayResult = ValidateFiniteDoubleArray(probabilities, parameterName);
        if (!arrayResult.IsValid)
            return arrayResult;

        // Check each probability is valid
        for (int i = 0; i < probabilities!.Length; i++)
        {
            var probResult = ValidateProbability(probabilities[i], $"{parameterName}[{i}]", tolerance);
            if (!probResult.IsValid)
                return probResult;
        }

        // Check normalization
        var sum = probabilities.Sum();
        if (Math.Abs(sum - 1.0) > tolerance)
        {
            var error = $"Probability distribution '{parameterName}' must sum to 1.0 (found {sum})";
            var context = new { Parameter = parameterName, Sum = sum, Tolerance = tolerance };
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Quantum-Specific Validation

    /// <summary>
    /// Validates that a complex array represents a normalized quantum state.
    /// </summary>
    /// <param name="state">The quantum state to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="tolerance">Tolerance for normalization check.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateQuantumState(Complex[]? state, string parameterName, double tolerance = 1e-12)
    {
        var arrayResult = ValidateFiniteComplexArray(state, parameterName);
        if (!arrayResult.IsValid)
            return arrayResult;

        // Check normalization
        var normSquared = state!.Sum(amplitude => amplitude.Real * amplitude.Real + amplitude.Imaginary * amplitude.Imaginary);
        
        if (Math.Abs(normSquared - 1.0) > tolerance)
        {
            var error = $"Quantum state '{parameterName}' must be normalized (||ψ||² = {normSquared})";
            var context = new { Parameter = parameterName, NormSquared = normSquared, Tolerance = tolerance };
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates that a dimension is valid for quantum calculations.
    /// </summary>
    /// <param name="dimension">The Hilbert space dimension to validate.</param>
    /// <param name="parameterName">The name of the parameter for error reporting.</param>
    /// <param name="requirePowerOfTwo">Whether the dimension must be a power of 2.</param>
    /// <returns>The validation result.</returns>
    public static ValidationResult ValidateHilbertSpaceDimension(int dimension, string parameterName, bool requirePowerOfTwo = false)
    {
        var rangeResult = ValidateIntegerRange(dimension, parameterName, 2, 1 << 20); // Up to 2^20
        if (!rangeResult.IsValid)
            return rangeResult;

        if (requirePowerOfTwo && (dimension & (dimension - 1)) != 0)
        {
            var error = $"Hilbert space dimension '{parameterName}' must be a power of 2 (found {dimension})";
            var context = new { Parameter = parameterName, Dimension = dimension };
            Logger.Warning(error, context);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    #endregion

    #region Conditional Validation

    /// <summary>
    /// Performs validation only if a condition is met.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="validation">The validation function to run if condition is true.</param>
    /// <returns>The validation result or success if condition is false.</returns>
    public static ValidationResult ValidateIf(bool condition, Func<ValidationResult> validation)
    {
        return condition ? validation() : ValidationResult.Success();
    }

    /// <summary>
    /// Combines multiple validation results using AND logic.
    /// </summary>
    /// <param name="validations">The validation functions to combine.</param>
    /// <returns>A combined validation result.</returns>
    public static ValidationResult ValidateAll(params Func<ValidationResult>[] validations)
    {
        var errors = new List<string>();
        
        foreach (var validation in validations)
        {
            var result = validation();
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    #endregion
}

/// <summary>
/// Represents the result of a parameter validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; private set; }

    /// <summary>
    /// Gets the first error message, or null if validation was successful.
    /// </summary>
    public string? FirstError => Errors.FirstOrDefault();

    private ValidationResult(bool isValid, IEnumerable<string> errors)
    {
        IsValid = isValid;
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result.</returns>
    public static ValidationResult Success()
    {
        return new ValidationResult(true, Enumerable.Empty<string>());
    }

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(string error)
    {
        return new ValidationResult(false, new[] { error });
    }

    /// <summary>
    /// Creates a failed validation result with multiple errors.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult(false, errors);
    }

    /// <summary>
    /// Throws an exception if the validation failed.
    /// </summary>
    /// <param name="exceptionType">The type of exception to throw.</param>
    public void ThrowIfInvalid(Type? exceptionType = null)
    {
        if (!IsValid)
        {
            var message = string.Join(Environment.NewLine, Errors);
            
            if (exceptionType == typeof(ArgumentOutOfRangeException))
                throw new ArgumentOutOfRangeException(message);
            else if (exceptionType == typeof(ArgumentNullException))
                throw new ArgumentNullException(message);
            else
                throw new ArgumentException(message);
        }
    }

    /// <summary>
    /// Returns a string representation of the validation result.
    /// </summary>
    /// <returns>A string describing the validation result.</returns>
    public override string ToString()
    {
        return IsValid ? "Valid" : $"Invalid: {string.Join(", ", Errors)}";
    }
}
