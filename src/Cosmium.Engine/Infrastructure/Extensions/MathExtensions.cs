using System.Numerics;
using Complex = System.Numerics.Complex;

namespace Cosmium.Engine.Infrastructure.Extensions;

/// <summary>
/// Mathematical utility extensions for quantum mechanics and scientific computing.
/// Provides convenient methods for common mathematical operations.
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Tolerance for floating-point comparisons.
    /// </summary>
    public const double DefaultTolerance = 1e-12;

    #region Double Extensions

    /// <summary>
    /// Checks if a double value is approximately zero within the specified tolerance.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the value is approximately zero.</returns>
    public static bool IsApproximatelyZero(this double value, double tolerance = DefaultTolerance)
    {
        return Math.Abs(value) < tolerance;
    }

    /// <summary>
    /// Checks if two double values are approximately equal within the specified tolerance.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the values are approximately equal.</returns>
    public static bool IsApproximatelyEqual(this double value1, double value2, double tolerance = DefaultTolerance)
    {
        return Math.Abs(value1 - value2) < tolerance;
    }

    /// <summary>
    /// Clamps a value between minimum and maximum bounds.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum bound.</param>
    /// <param name="max">The maximum bound.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp(this double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">Angle in degrees.</param>
    /// <returns>Angle in radians.</returns>
    public static double ToRadians(this double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>Angle in degrees.</returns>
    public static double ToDegrees(this double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    /// <summary>
    /// Computes the square of a number.
    /// </summary>
    /// <param name="value">The value to square.</param>
    /// <returns>The square of the value.</returns>
    public static double Squared(this double value)
    {
        return value * value;
    }

    /// <summary>
    /// Computes the cube of a number.
    /// </summary>
    /// <param name="value">The value to cube.</param>
    /// <returns>The cube of the value.</returns>
    public static double Cubed(this double value)
    {
        return value * value * value;
    }

    /// <summary>
    /// Safe square root that returns 0 for negative inputs close to zero.
    /// </summary>
    /// <param name="value">The value to take the square root of.</param>
    /// <param name="tolerance">Tolerance for considering negative values as zero.</param>
    /// <returns>The square root or 0 for small negative values.</returns>
    public static double SafeSqrt(this double value, double tolerance = DefaultTolerance)
    {
        if (value >= 0)
            return Math.Sqrt(value);
        
        if (Math.Abs(value) < tolerance)
            return 0.0;
        
        throw new ArgumentException($"Cannot take square root of significantly negative value: {value}");
    }

    /// <summary>
    /// Computes the sign of a value with a dead zone around zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="tolerance">The tolerance for the dead zone.</param>
    /// <returns>-1, 0, or 1 depending on the sign.</returns>
    public static int SignWithTolerance(this double value, double tolerance = DefaultTolerance)
    {
        if (Math.Abs(value) < tolerance)
            return 0;
        return Math.Sign(value);
    }

    #endregion

    #region Complex Extensions

    /// <summary>
    /// Checks if a complex number is approximately zero.
    /// </summary>
    /// <param name="value">The complex number to check.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the complex number is approximately zero.</returns>
    public static bool IsApproximatelyZero(this Complex value, double tolerance = DefaultTolerance)
    {
        return value.Magnitude < tolerance;
    }

    /// <summary>
    /// Checks if two complex numbers are approximately equal.
    /// </summary>
    /// <param name="value1">The first complex number.</param>
    /// <param name="value2">The second complex number.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the complex numbers are approximately equal.</returns>
    public static bool IsApproximatelyEqual(this Complex value1, Complex value2, double tolerance = DefaultTolerance)
    {
        return (value1 - value2).Magnitude < tolerance;
    }

    /// <summary>
    /// Gets the phase (argument) of a complex number, handling edge cases.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The phase in radians, between -π and π.</returns>
    public static double Phase(this Complex value)
    {
        return Math.Atan2(value.Imaginary, value.Real);
    }

    /// <summary>
    /// Converts a complex number to polar form.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>A tuple containing magnitude and phase.</returns>
    public static (double Magnitude, double Phase) ToPolar(this Complex value)
    {
        return (value.Magnitude, value.Phase());
    }

    /// <summary>
    /// Creates a complex number from polar coordinates.
    /// </summary>
    /// <param name="magnitude">The magnitude.</param>
    /// <param name="phase">The phase in radians.</param>
    /// <returns>The complex number in rectangular form.</returns>
    public static Complex FromPolar(double magnitude, double phase)
    {
        return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
    }

    /// <summary>
    /// Computes the complex conjugate.
    /// </summary>
    /// <param name="value">The complex number.</param>
    /// <returns>The complex conjugate.</returns>
    public static Complex Conjugate(this Complex value)
    {
        return new Complex(value.Real, -value.Imaginary);
    }

    #endregion

    #region Vector and Matrix Extensions

    /// <summary>
    /// Normalizes a vector to unit length.
    /// </summary>
    /// <param name="vector">The vector to normalize.</param>
    /// <returns>The normalized vector.</returns>
    public static double[] Normalize(this double[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude.IsApproximatelyZero())
            throw new InvalidOperationException("Cannot normalize zero vector");
        
        return vector.Select(x => x / magnitude).ToArray();
    }

    /// <summary>
    /// Computes the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    public static double Dot(this double[] vector1, double[] vector2)
    {
        if (vector1.Length != vector2.Length)
            throw new ArgumentException("Vectors must have the same length");
        
        return vector1.Zip(vector2, (a, b) => a * b).Sum();
    }

    /// <summary>
    /// Computes the magnitude (length) of a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The magnitude.</returns>
    public static double Magnitude(this double[] vector)
    {
        return Math.Sqrt(vector.Sum(x => x * x));
    }

    /// <summary>
    /// Computes the Euclidean distance between two points.
    /// </summary>
    /// <param name="point1">The first point.</param>
    /// <param name="point2">The second point.</param>
    /// <returns>The Euclidean distance.</returns>
    public static double DistanceTo(this double[] point1, double[] point2)
    {
        if (point1.Length != point2.Length)
            throw new ArgumentException("Points must have the same dimension");
        
        return Math.Sqrt(point1.Zip(point2, (a, b) => (a - b).Squared()).Sum());
    }

    #endregion

    #region Statistical Extensions

    /// <summary>
    /// Computes the mean of a sequence of values.
    /// </summary>
    /// <param name="values">The sequence of values.</param>
    /// <returns>The mean value.</returns>
    public static double Mean(this IEnumerable<double> values)
    {
        var list = values.ToList();
        if (!list.Any())
            throw new InvalidOperationException("Cannot compute mean of empty sequence");
        
        return list.Average();
    }

    /// <summary>
    /// Computes the variance of a sequence of values.
    /// </summary>
    /// <param name="values">The sequence of values.</param>
    /// <param name="sample">Whether to use sample variance (N-1) or population variance (N).</param>
    /// <returns>The variance.</returns>
    public static double Variance(this IEnumerable<double> values, bool sample = true)
    {
        var list = values.ToList();
        if (!list.Any())
            throw new InvalidOperationException("Cannot compute variance of empty sequence");
        
        if (sample && list.Count == 1)
            throw new InvalidOperationException("Cannot compute sample variance with only one value");
        
        var mean = list.Mean();
        var sumSquaredDeviations = list.Sum(x => (x - mean).Squared());
        var denominator = sample ? list.Count - 1 : list.Count;
        
        return sumSquaredDeviations / denominator;
    }

    /// <summary>
    /// Computes the standard deviation of a sequence of values.
    /// </summary>
    /// <param name="values">The sequence of values.</param>
    /// <param name="sample">Whether to use sample standard deviation.</param>
    /// <returns>The standard deviation.</returns>
    public static double StandardDeviation(this IEnumerable<double> values, bool sample = true)
    {
        return Math.Sqrt(values.Variance(sample));
    }

    /// <summary>
    /// Computes the root mean square (RMS) of a sequence of values.
    /// </summary>
    /// <param name="values">The sequence of values.</param>
    /// <returns>The RMS value.</returns>
    public static double RootMeanSquare(this IEnumerable<double> values)
    {
        var list = values.ToList();
        if (!list.Any())
            throw new InvalidOperationException("Cannot compute RMS of empty sequence");
        
        return Math.Sqrt(list.Select(x => x.Squared()).Mean());
    }

    #endregion

    #region Quantum-Specific Extensions

    /// <summary>
    /// Normalizes a quantum state vector (array of complex amplitudes).
    /// </summary>
    /// <param name="amplitudes">The quantum amplitudes.</param>
    /// <returns>The normalized amplitudes.</returns>
    public static Complex[] NormalizeQuantumState(this Complex[] amplitudes)
    {
        var normSquared = amplitudes.Sum(a => a.Real * a.Real + a.Imaginary * a.Imaginary);
        var norm = Math.Sqrt(normSquared);
        
        if (norm.IsApproximatelyZero())
            throw new InvalidOperationException("Cannot normalize zero quantum state");
        
        return amplitudes.Select(a => a / norm).ToArray();
    }

    /// <summary>
    /// Computes the overlap (inner product) between two quantum states.
    /// </summary>
    /// <param name="state1">The first quantum state.</param>
    /// <param name="state2">The second quantum state.</param>
    /// <returns>The complex overlap.</returns>
    public static Complex QuantumOverlap(this Complex[] state1, Complex[] state2)
    {
        if (state1.Length != state2.Length)
            throw new ArgumentException("Quantum states must have the same dimension");
        
        return state1.Zip(state2, (a, b) => a.Conjugate() * b).Aggregate(Complex.Zero, (sum, term) => sum + term);
    }

    /// <summary>
    /// Computes the fidelity between two quantum states.
    /// </summary>
    /// <param name="state1">The first quantum state.</param>
    /// <param name="state2">The second quantum state.</param>
    /// <returns>The fidelity (real value between 0 and 1).</returns>
    public static double QuantumFidelity(this Complex[] state1, Complex[] state2)
    {
        var overlap = state1.QuantumOverlap(state2);
        return overlap.Magnitude;
    }

    /// <summary>
    /// Checks if a quantum state is properly normalized.
    /// </summary>
    /// <param name="state">The quantum state amplitudes.</param>
    /// <param name="tolerance">Tolerance for normalization check.</param>
    /// <returns>True if the state is normalized.</returns>
    public static bool IsNormalized(this Complex[] state, double tolerance = DefaultTolerance)
    {
        var normSquared = state.Sum(a => a.Real * a.Real + a.Imaginary * a.Imaginary);
        return Math.Abs(normSquared - 1.0) < tolerance;
    }

    #endregion

    #region Numerical Methods

    /// <summary>
    /// Performs Kahan summation for improved numerical accuracy.
    /// </summary>
    /// <param name="values">The values to sum.</param>
    /// <returns>The sum with improved accuracy.</returns>
    public static double KahanSum(this IEnumerable<double> values)
    {
        double sum = 0.0;
        double compensation = 0.0;
        
        foreach (var value in values)
        {
            var adjustedValue = value - compensation;
            var newSum = sum + adjustedValue;
            compensation = (newSum - sum) - adjustedValue;
            sum = newSum;
        }
        
        return sum;
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="start">The start value.</param>
    /// <param name="end">The end value.</param>
    /// <param name="t">Interpolation parameter (0 to 1).</param>
    /// <returns>The interpolated value.</returns>
    public static double Lerp(this double start, double end, double t)
    {
        return start + t * (end - start);
    }

    /// <summary>
    /// Performs cubic interpolation between values.
    /// </summary>
    /// <param name="t">Interpolation parameter (0 to 1).</param>
    /// <returns>Smooth interpolation value.</returns>
    public static double SmoothStep(this double t)
    {
        return t * t * (3.0 - 2.0 * t);
    }

    /// <summary>
    /// Wraps an angle to the range [-π, π].
    /// </summary>
    /// <param name="angle">The angle in radians.</param>
    /// <returns>The wrapped angle.</returns>
    public static double WrapAngle(this double angle)
    {
        while (angle > Math.PI)
            angle -= 2.0 * Math.PI;
        while (angle < -Math.PI)
            angle += 2.0 * Math.PI;
        return angle;
    }

    #endregion
}
