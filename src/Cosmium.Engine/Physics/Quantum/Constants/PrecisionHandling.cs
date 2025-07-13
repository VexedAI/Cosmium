using System;
using System.Collections.Generic;
using Cosmium.Engine.Physics.Mathematics;

namespace Cosmium.Engine.Physics.Quantum.Constants;

/// <summary>
/// Provides precision handling and numerical stability utilities for quantum mechanical calculations.
/// Handles common numerical issues in quantum simulations and provides high-precision alternatives.
/// </summary>
public static class PrecisionHandling
{
    #region Precision Constants

    /// <summary>
    /// Machine epsilon for double precision floating point arithmetic.
    /// </summary>
    public const double DoublePrecisionEpsilon = 2.220446049250313e-16;

    /// <summary>
    /// Recommended tolerance for quantum mechanical calculations.
    /// Based on typical precision requirements in quantum chemistry and physics.
    /// </summary>
    public const double QuantumCalculationTolerance = 1e-12;

    /// <summary>
    /// Tolerance for probability normalization checks.
    /// Stricter than general calculations due to physical constraints.
    /// </summary>
    public const double ProbabilityNormalizationTolerance = 1e-10;

    /// <summary>
    /// Tolerance for energy eigenvalue calculations.
    /// Relaxed due to typical convergence criteria in quantum simulations.
    /// </summary>
    public const double EigenvalueTolerance = 1e-8;

    /// <summary>
    /// Tolerance for wavefunction orthogonality checks.
    /// </summary>
    public const double OrthogonalityTolerance = 1e-10;

    /// <summary>
    /// Minimum magnitude for considering a quantum amplitude as non-zero.
    /// </summary>
    public const double MinimumAmplitudeMagnitude = 1e-14;

    /// <summary>
    /// Maximum condition number for stable matrix operations.
    /// Beyond this, special numerical techniques should be used.
    /// </summary>
    public const double MaximumConditionNumber = 1e12;

    #endregion

    #region Numerical Stability Checks

    /// <summary>
    /// Checks if a floating-point number is numerically zero within quantum calculation tolerance.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="tolerance">Optional custom tolerance.</param>
    /// <returns>True if the value is considered numerically zero.</returns>
    public static bool IsNumericallyZero(double value, double tolerance = QuantumCalculationTolerance)
    {
        return Math.Abs(value) < tolerance;
    }

    /// <summary>
    /// Checks if a complex number is numerically zero within quantum calculation tolerance.
    /// </summary>
    /// <param name="value">The complex value to check.</param>
    /// <param name="tolerance">Optional custom tolerance.</param>
    /// <returns>True if the complex value is considered numerically zero.</returns>
    public static bool IsNumericallyZero(Complex value, double tolerance = QuantumCalculationTolerance)
    {
        return value.Magnitude < tolerance;
    }

    /// <summary>
    /// Checks if two floating-point numbers are numerically equal within tolerance.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <param name="tolerance">Optional custom tolerance.</param>
    /// <returns>True if the values are numerically equal.</returns>
    public static bool AreNumericallyEqual(double a, double b, double tolerance = QuantumCalculationTolerance)
    {
        return Math.Abs(a - b) < tolerance;
    }

    /// <summary>
    /// Checks if two complex numbers are numerically equal within tolerance.
    /// </summary>
    /// <param name="a">First complex value.</param>
    /// <param name="b">Second complex value.</param>
    /// <param name="tolerance">Optional custom tolerance.</param>
    /// <returns>True if the complex values are numerically equal.</returns>
    public static bool AreNumericallyEqual(Complex a, Complex b, double tolerance = QuantumCalculationTolerance)
    {
        return (a - b).Magnitude < tolerance;
    }

    /// <summary>
    /// Checks if a value is within a reasonable range for quantum calculations.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is finite and within reasonable bounds.</returns>
    public static bool IsValidQuantumValue(double value)
    {
        return double.IsFinite(value) && Math.Abs(value) < 1e100;
    }

    /// <summary>
    /// Checks if a complex value is valid for quantum calculations.
    /// </summary>
    /// <param name="value">The complex value to check.</param>
    /// <returns>True if both real and imaginary parts are finite and reasonable.</returns>
    public static bool IsValidQuantumValue(Complex value)
    {
        return IsValidQuantumValue(value.Real) && IsValidQuantumValue(value.Imaginary);
    }

    /// <summary>
    /// Checks if a probability value is physically valid and numerically stable.
    /// </summary>
    /// <param name="probability">The probability value to check.</param>
    /// <param name="tolerance">Optional custom tolerance.</param>
    /// <returns>True if the probability is valid.</returns>
    public static bool IsValidProbability(double probability, double tolerance = ProbabilityNormalizationTolerance)
    {
        return double.IsFinite(probability) && 
               probability >= -tolerance && 
               probability <= 1.0 + tolerance;
    }

    #endregion

    #region Safe Mathematical Operations

    /// <summary>
    /// Safely computes the square root, returning zero for negative inputs close to zero.
    /// Useful for quantum amplitudes where small negative values may arise from numerical errors.
    /// </summary>
    /// <param name="value">The value to take the square root of.</param>
    /// <param name="tolerance">Tolerance for considering negative values as zero.</param>
    /// <returns>The square root, or zero if input is slightly negative.</returns>
    public static double SafeSqrt(double value, double tolerance = QuantumCalculationTolerance)
    {
        if (value >= 0.0)
            return Math.Sqrt(value);
        
        if (Math.Abs(value) < tolerance)
            return 0.0;
        
        throw new ArgumentException($"Cannot take square root of significantly negative value: {value}");
    }

    /// <summary>
    /// Safely computes the natural logarithm, handling edge cases for quantum calculations.
    /// </summary>
    /// <param name="value">The value to take the logarithm of.</param>
    /// <param name="tolerance">Tolerance for considering zero values.</param>
    /// <returns>The natural logarithm.</returns>
    public static double SafeLog(double value, double tolerance = QuantumCalculationTolerance)
    {
        if (value > tolerance)
            return Math.Log(value);
        
        if (Math.Abs(value) <= tolerance)
            return double.NegativeInfinity;
        
        throw new ArgumentException($"Cannot take logarithm of negative value: {value}");
    }

    /// <summary>
    /// Safely divides two numbers, handling division by near-zero denominators.
    /// </summary>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <param name="tolerance">Tolerance for considering denominator as zero.</param>
    /// <returns>The quotient, or infinity/NaN for division by zero.</returns>
    public static double SafeDivide(double numerator, double denominator, double tolerance = QuantumCalculationTolerance)
    {
        if (Math.Abs(denominator) >= tolerance)
            return numerator / denominator;
        
        if (Math.Abs(numerator) < tolerance)
            return double.NaN; // 0/0 case
        
        return numerator > 0 ? double.PositiveInfinity : double.NegativeInfinity;
    }

    /// <summary>
    /// Safely normalizes a value to ensure it's within [0, 1] for probabilities.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="tolerance">Tolerance for values slightly outside [0, 1].</param>
    /// <returns>The clamped value within [0, 1].</returns>
    public static double ClampProbability(double value, double tolerance = ProbabilityNormalizationTolerance)
    {
        if (value < -tolerance || value > 1.0 + tolerance)
            throw new ArgumentException($"Value {value} is too far outside [0, 1] to be a valid probability.");
        
        return Math.Max(0.0, Math.Min(1.0, value));
    }

    #endregion

    #region High-Precision Calculations

    /// <summary>
    /// Computes the Kahan summation algorithm for improved precision in floating-point addition.
    /// Useful for summing many small quantum amplitudes or probabilities.
    /// </summary>
    /// <param name="values">The values to sum.</param>
    /// <returns>The high-precision sum.</returns>
    public static double KahanSum(IEnumerable<double> values)
    {
        double sum = 0.0;
        double compensation = 0.0; // A running compensation for lost low-order bits

        foreach (double value in values)
        {
            double y = value - compensation; // So far, so good: compensation is zero
            double t = sum + y;              // Alas, sum is big, y small, so low-order digits of y are lost
            compensation = (t - sum) - y;    // (t - sum) cancels the high-order part of y; subtracting y recovers negative (low part of y)
            sum = t;                         // Algebraically, compensation should always be zero. Beware overly-clever compilers!
        }

        return sum;
    }

    /// <summary>
    /// Computes the Kahan summation for complex numbers.
    /// </summary>
    /// <param name="values">The complex values to sum.</param>
    /// <returns>The high-precision complex sum.</returns>
    public static Complex KahanSum(IEnumerable<Complex> values)
    {
        var realParts = values.Select(c => c.Real);
        var imaginaryParts = values.Select(c => c.Imaginary);
        
        return new Complex(KahanSum(realParts), KahanSum(imaginaryParts));
    }

    /// <summary>
    /// Computes the dot product of two arrays using Kahan summation for improved precision.
    /// </summary>
    /// <param name="a">First array.</param>
    /// <param name="b">Second array.</param>
    /// <returns>The high-precision dot product.</returns>
    public static double HighPrecisionDotProduct(IEnumerable<double> a, IEnumerable<double> b)
    {
        var products = a.Zip(b, (x, y) => x * y);
        return KahanSum(products);
    }

    /// <summary>
    /// Computes the complex dot product using Kahan summation.
    /// </summary>
    /// <param name="a">First complex array.</param>
    /// <param name="b">Second complex array.</param>
    /// <returns>The high-precision complex dot product.</returns>
    public static Complex HighPrecisionComplexDotProduct(IEnumerable<Complex> a, IEnumerable<Complex> b)
    {
        var products = a.Zip(b, (x, y) => x.Conjugate * y);
        return KahanSum(products);
    }

    #endregion

    #region Error Estimation

    /// <summary>
    /// Estimates the relative error between two values.
    /// </summary>
    /// <param name="computed">The computed value.</param>
    /// <param name="reference">The reference (exact) value.</param>
    /// <returns>The relative error.</returns>
    public static double RelativeError(double computed, double reference)
    {
        if (Math.Abs(reference) < DoublePrecisionEpsilon)
            return Math.Abs(computed - reference);
        
        return Math.Abs((computed - reference) / reference);
    }

    /// <summary>
    /// Estimates the condition number of a calculation based on input perturbations.
    /// Higher condition numbers indicate more sensitivity to numerical errors.
    /// </summary>
    /// <param name="originalResult">Result from original calculation.</param>
    /// <param name="perturbedResult">Result from slightly perturbed input.</param>
    /// <param name="inputPerturbation">The relative size of input perturbation.</param>
    /// <returns>The estimated condition number.</returns>
    public static double EstimateConditionNumber(double originalResult, double perturbedResult, double inputPerturbation)
    {
        double outputChange = RelativeError(perturbedResult, originalResult);
        return outputChange / inputPerturbation;
    }

    /// <summary>
    /// Checks if a calculation is numerically stable based on condition number.
    /// </summary>
    /// <param name="conditionNumber">The condition number to check.</param>
    /// <returns>True if the calculation is considered stable.</returns>
    public static bool IsNumericallyStable(double conditionNumber)
    {
        return conditionNumber < MaximumConditionNumber;
    }

    #endregion

    #region Quantum-Specific Precision Utilities

    /// <summary>
    /// Ensures a set of quantum state amplitudes maintain proper normalization.
    /// </summary>
    /// <param name="amplitudes">The quantum amplitudes to normalize.</param>
    /// <param name="tolerance">Tolerance for normalization check.</param>
    /// <returns>Normalized amplitudes.</returns>
    public static Complex[] NormalizeQuantumAmplitudes(IEnumerable<Complex> amplitudes, 
                                                      double tolerance = ProbabilityNormalizationTolerance)
    {
        var amplitudeArray = amplitudes.ToArray();
        
        // Calculate norm using high-precision summation
        var magnitudeSquares = amplitudeArray.Select(a => a.Real * a.Real + a.Imaginary * a.Imaginary);
        double normSquared = KahanSum(magnitudeSquares);
        
        if (normSquared < tolerance)
            throw new InvalidOperationException("Cannot normalize zero quantum state.");
        
        double norm = Math.Sqrt(normSquared);
        
        // Normalize each amplitude
        var normalized = new Complex[amplitudeArray.Length];
        for (int i = 0; i < amplitudeArray.Length; i++)
        {
            normalized[i] = new Complex(amplitudeArray[i].Real / norm, amplitudeArray[i].Imaginary / norm);
        }
        
        return normalized;
    }

    /// <summary>
    /// Validates that quantum probabilities sum to unity within tolerance.
    /// </summary>
    /// <param name="probabilities">The probabilities to validate.</param>
    /// <param name="tolerance">Tolerance for unity check.</param>
    /// <returns>True if probabilities are properly normalized.</returns>
    public static bool ValidateQuantumProbabilities(IEnumerable<double> probabilities, 
                                                   double tolerance = ProbabilityNormalizationTolerance)
    {
        var probArray = probabilities.ToArray();
        
        // Check individual probabilities
        foreach (double prob in probArray)
        {
            if (!IsValidProbability(prob, tolerance))
                return false;
        }
        
        // Check normalization using high-precision summation
        double sum = KahanSum(probArray);
        return AreNumericallyEqual(sum, 1.0, tolerance);
    }

    /// <summary>
    /// Ensures matrix elements are within reasonable bounds for quantum calculations.
    /// </summary>
    /// <param name="matrixElements">The matrix elements to validate.</param>
    /// <param name="maxMagnitude">Maximum allowed magnitude.</param>
    /// <returns>True if all elements are within bounds.</returns>
    public static bool ValidateQuantumMatrixElements(IEnumerable<Complex> matrixElements, 
                                                   double maxMagnitude = 1e10)
    {
        return matrixElements.All(element => 
            IsValidQuantumValue(element) && element.Magnitude <= maxMagnitude);
    }

    /// <summary>
    /// Computes the overlap between two quantum states with high precision.
    /// </summary>
    /// <param name="state1">First quantum state.</param>
    /// <param name="state2">Second quantum state.</param>
    /// <returns>The overlap ⟨ψ₁|ψ₂⟩.</returns>
    public static Complex HighPrecisionOverlap(IEnumerable<Complex> state1, IEnumerable<Complex> state2)
    {
        return HighPrecisionComplexDotProduct(state1, state2);
    }

    #endregion

    #region Adaptive Precision

    /// <summary>
    /// Determines the appropriate tolerance for a calculation based on input magnitudes.
    /// </summary>
    /// <param name="inputMagnitudes">Magnitudes of input values.</param>
    /// <param name="baseTolerance">Base tolerance level.</param>
    /// <returns>Adaptive tolerance accounting for input scale.</returns>
    public static double AdaptiveTolerance(IEnumerable<double> inputMagnitudes, 
                                         double baseTolerance = QuantumCalculationTolerance)
    {
        double maxMagnitude = inputMagnitudes.Max();
        
        if (maxMagnitude < 1e-10)
            return baseTolerance * 1e6;  // Relax tolerance for very small values
        
        if (maxMagnitude > 1e10)
            return baseTolerance * 1e-6; // Tighten tolerance for very large values
        
        return baseTolerance;
    }

    /// <summary>
    /// Provides precision recommendations based on calculation type.
    /// </summary>
    public static class PrecisionRecommendations
    {
        /// <summary>
        /// Recommended tolerance for energy eigenvalue calculations.
        /// </summary>
        public const double EnergyEigenvalues = 1e-8;

        /// <summary>
        /// Recommended tolerance for wavefunction normalization.
        /// </summary>
        public const double WavefunctionNormalization = 1e-10;

        /// <summary>
        /// Recommended tolerance for matrix element calculations.
        /// </summary>
        public const double MatrixElements = 1e-12;

        /// <summary>
        /// Recommended tolerance for probability calculations.
        /// </summary>
        public const double Probabilities = 1e-10;

        /// <summary>
        /// Recommended tolerance for phase calculations.
        /// </summary>
        public const double Phases = 1e-8;

        /// <summary>
        /// Recommended tolerance for orthogonality checks.
        /// </summary>
        public const double Orthogonality = 1e-10;
    }

    #endregion

    #region Debugging and Diagnostics

    /// <summary>
    /// Analyzes a set of values for potential numerical issues.
    /// </summary>
    /// <param name="values">Values to analyze.</param>
    /// <returns>A diagnostic report.</returns>
    public static NumericalDiagnostics AnalyzeNumericalStability(IEnumerable<double> values)
    {
        var valueArray = values.ToArray();
        
        return new NumericalDiagnostics
        {
            Count = valueArray.Length,
            MinValue = valueArray.Min(),
            MaxValue = valueArray.Max(),
            Mean = valueArray.Average(),
            StandardDeviation = Math.Sqrt(valueArray.Select(v => Math.Pow(v - valueArray.Average(), 2)).Average()),
            HasInfiniteValues = valueArray.Any(double.IsInfinity),
            HasNaNValues = valueArray.Any(double.IsNaN),
            DynamicRange = Math.Log10(valueArray.Max() / Math.Max(Math.Abs(valueArray.Min()), DoublePrecisionEpsilon)),
            EstimatedPrecisionLoss = EstimatePrecisionLoss(valueArray)
        };
    }

    /// <summary>
    /// Estimates precision loss in a calculation based on value distribution.
    /// </summary>
    /// <param name="values">Values to analyze.</param>
    /// <returns>Estimated number of lost significant digits.</returns>
    private static double EstimatePrecisionLoss(double[] values)
    {
        if (values.Length < 2) return 0;
        
        double maxValue = values.Max();
        double minNonZero = values.Where(v => Math.Abs(v) > DoublePrecisionEpsilon).Min();
        
        return Math.Max(0, Math.Log10(maxValue / minNonZero) - 15); // 15 is approximate precision of double
    }

    /// <summary>
    /// Diagnostic information about numerical stability.
    /// </summary>
    public class NumericalDiagnostics
    {
        public int Count { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public bool HasInfiniteValues { get; set; }
        public bool HasNaNValues { get; set; }
        public double DynamicRange { get; set; }
        public double EstimatedPrecisionLoss { get; set; }

        public bool IsNumericallyHealthy => 
            !HasInfiniteValues && 
            !HasNaNValues && 
            DynamicRange < 14 && 
            EstimatedPrecisionLoss < 5;
    }

    #endregion
}
