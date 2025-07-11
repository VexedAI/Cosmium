using System;
using System.Globalization;

namespace Cosmium.Engine.Physics.Mathematics;

/// <summary>
/// Represents a complex number with real and imaginary parts, optimized for quantum mechanics calculations.
/// Provides extended functionality beyond System.Numerics.Complex for quantum-specific operations.
/// </summary>
public readonly struct Complex : IEquatable<Complex>, IFormattable
{
    #region Properties

    /// <summary>
    /// Gets the real component of the complex number.
    /// </summary>
    public double Real { get; }

    /// <summary>
    /// Gets the imaginary component of the complex number.
    /// </summary>
    public double Imaginary { get; }

    /// <summary>
    /// Gets the magnitude (absolute value) of the complex number.
    /// </summary>
    public double Magnitude => Math.Sqrt(Real * Real + Imaginary * Imaginary);

    /// <summary>
    /// Gets the phase (argument) of the complex number in radians.
    /// </summary>
    public double Phase => Math.Atan2(Imaginary, Real);

    /// <summary>
    /// Gets the squared magnitude of the complex number (|z|²).
    /// Optimized for quantum probability calculations.
    /// </summary>
    public double MagnitudeSquared => Real * Real + Imaginary * Imaginary;

    /// <summary>
    /// Gets the complex conjugate of this number.
    /// </summary>
    public Complex Conjugate => new(Real, -Imaginary);

    #endregion

    #region Constants

    /// <summary>
    /// Represents the complex number zero (0 + 0i).
    /// </summary>
    public static readonly Complex Zero = new(0, 0);

    /// <summary>
    /// Represents the complex number one (1 + 0i).
    /// </summary>
    public static readonly Complex One = new(1, 0);

    /// <summary>
    /// Represents the imaginary unit (0 + 1i).
    /// </summary>
    public static readonly Complex ImaginaryUnit = new(0, 1);

    /// <summary>
    /// Represents negative imaginary unit (0 - 1i).
    /// </summary>
    public static readonly Complex NegativeImaginaryUnit = new(0, -1);

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the Complex struct.
    /// </summary>
    /// <param name="real">The real component.</param>
    /// <param name="imaginary">The imaginary component.</param>
    public Complex(double real, double imaginary)
    {
        Real = real;
        Imaginary = imaginary;
    }

    /// <summary>
    /// Initializes a new instance of the Complex struct with only a real component.
    /// </summary>
    /// <param name="real">The real component.</param>
    public Complex(double real) : this(real, 0) { }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a complex number from polar coordinates.
    /// </summary>
    /// <param name="magnitude">The magnitude (r).</param>
    /// <param name="phase">The phase in radians (θ).</param>
    /// <returns>A complex number representing r * e^(iθ).</returns>
    public static Complex FromPolar(double magnitude, double phase)
    {
        return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
    }

    /// <summary>
    /// Creates a complex number representing e^(iθ).
    /// Commonly used in quantum mechanics for phase factors.
    /// </summary>
    /// <param name="phase">The phase in radians.</param>
    /// <returns>A unit complex number with the specified phase.</returns>
    public static Complex FromPhase(double phase)
    {
        return new Complex(Math.Cos(phase), Math.Sin(phase));
    }

    #endregion

    #region Arithmetic Operations

    /// <summary>
    /// Adds two complex numbers.
    /// </summary>
    public static Complex operator +(Complex left, Complex right)
    {
        return new Complex(left.Real + right.Real, left.Imaginary + right.Imaginary);
    }

    /// <summary>
    /// Subtracts two complex numbers.
    /// </summary>
    public static Complex operator -(Complex left, Complex right)
    {
        return new Complex(left.Real - right.Real, left.Imaginary - right.Imaginary);
    }

    /// <summary>
    /// Multiplies two complex numbers.
    /// </summary>
    public static Complex operator *(Complex left, Complex right)
    {
        return new Complex(
            left.Real * right.Real - left.Imaginary * right.Imaginary,
            left.Real * right.Imaginary + left.Imaginary * right.Real
        );
    }

    /// <summary>
    /// Divides two complex numbers.
    /// </summary>
    public static Complex operator /(Complex left, Complex right)
    {
        double denominator = right.MagnitudeSquared;
        if (Math.Abs(denominator) < double.Epsilon)
            throw new DivideByZeroException("Cannot divide by zero complex number.");

        return new Complex(
            (left.Real * right.Real + left.Imaginary * right.Imaginary) / denominator,
            (left.Imaginary * right.Real - left.Real * right.Imaginary) / denominator
        );
    }

    /// <summary>
    /// Negates a complex number.
    /// </summary>
    public static Complex operator -(Complex value)
    {
        return new Complex(-value.Real, -value.Imaginary);
    }

    /// <summary>
    /// Multiplies a complex number by a real scalar.
    /// </summary>
    public static Complex operator *(Complex complex, double scalar)
    {
        return new Complex(complex.Real * scalar, complex.Imaginary * scalar);
    }

    /// <summary>
    /// Multiplies a real scalar by a complex number.
    /// </summary>
    public static Complex operator *(double scalar, Complex complex)
    {
        return complex * scalar;
    }

    /// <summary>
    /// Divides a complex number by a real scalar.
    /// </summary>
    public static Complex operator /(Complex complex, double scalar)
    {
        if (Math.Abs(scalar) < double.Epsilon)
            throw new DivideByZeroException("Cannot divide by zero scalar.");

        return new Complex(complex.Real / scalar, complex.Imaginary / scalar);
    }

    #endregion

    #region Quantum-Specific Operations

    /// <summary>
    /// Calculates the inner product of two complex numbers.
    /// In quantum mechanics, this represents ⟨ψ|φ⟩ for single complex amplitudes.
    /// </summary>
    /// <param name="other">The other complex number.</param>
    /// <returns>The inner product (this* × other).</returns>
    public Complex InnerProduct(Complex other)
    {
        return Conjugate * other;
    }

    /// <summary>
    /// Normalizes the complex number to unit magnitude.
    /// Essential for quantum state normalization.
    /// </summary>
    /// <returns>A normalized complex number with magnitude 1.</returns>
    public Complex Normalize()
    {
        double magnitude = Magnitude;
        if (magnitude < double.Epsilon)
            throw new InvalidOperationException("Cannot normalize zero complex number.");

        return this / magnitude;
    }

    /// <summary>
    /// Checks if this complex number represents a valid quantum amplitude.
    /// </summary>
    /// <returns>True if the complex number is finite and well-defined.</returns>
    public bool IsValidQuantumAmplitude()
    {
        return double.IsFinite(Real) && double.IsFinite(Imaginary);
    }

    #endregion

    #region Mathematical Functions

    /// <summary>
    /// Calculates e^z for complex z.
    /// </summary>
    public static Complex Exp(Complex value)
    {
        double expReal = Math.Exp(value.Real);
        return new Complex(expReal * Math.Cos(value.Imaginary), expReal * Math.Sin(value.Imaginary));
    }

    /// <summary>
    /// Calculates the natural logarithm of a complex number.
    /// </summary>
    public static Complex Log(Complex value)
    {
        return new Complex(Math.Log(value.Magnitude), value.Phase);
    }

    /// <summary>
    /// Raises a complex number to a real power.
    /// </summary>
    public static Complex Pow(Complex value, double power)
    {
        if (value == Zero)
        {
            if (power > 0) return Zero;
            if (power == 0) return One;
            throw new ArgumentException("0^(negative power) is undefined.");
        }

        double magnitude = Math.Pow(value.Magnitude, power);
        double phase = value.Phase * power;
        return FromPolar(magnitude, phase);
    }

    /// <summary>
    /// Calculates the square root of a complex number.
    /// </summary>
    public static Complex Sqrt(Complex value)
    {
        return Pow(value, 0.5);
    }

    #endregion

    #region Conversion Operators

    /// <summary>
    /// Implicitly converts a real number to a complex number.
    /// </summary>
    public static implicit operator Complex(double value)
    {
        return new Complex(value, 0);
    }

    /// <summary>
    /// Explicitly converts a complex number to its real part.
    /// </summary>
    public static explicit operator double(Complex value)
    {
        return value.Real;
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether two complex numbers are equal.
    /// </summary>
    public static bool operator ==(Complex left, Complex right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two complex numbers are not equal.
    /// </summary>
    public static bool operator !=(Complex left, Complex right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Determines whether this complex number is equal to another.
    /// </summary>
    public bool Equals(Complex other)
    {
        return Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary);
    }

    /// <summary>
    /// Determines whether this complex number is approximately equal to another within a tolerance.
    /// </summary>
    /// <param name="other">The other complex number.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the numbers are approximately equal.</returns>
    public bool Equals(Complex other, double tolerance)
    {
        return Math.Abs(Real - other.Real) < tolerance && Math.Abs(Imaginary - other.Imaginary) < tolerance;
    }

    /// <summary>
    /// Determines whether this instance is equal to a specified object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Complex other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Real, Imaginary);
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the complex number.
    /// </summary>
    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a string representation of the complex number with specified formatting.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
            format = "G";

        if (formatProvider == null)
            formatProvider = CultureInfo.CurrentCulture;

        if (Imaginary == 0)
            return Real.ToString(format, formatProvider);

        if (Real == 0)
        {
            if (Imaginary == 1)
                return "i";
            if (Imaginary == -1)
                return "-i";
            return $"{Imaginary.ToString(format, formatProvider)}i";
        }

        string imaginaryPart = Imaginary > 0 ? "+" : "";
        if (Math.Abs(Imaginary - 1) < double.Epsilon)
            imaginaryPart += "i";
        else if (Math.Abs(Imaginary + 1) < double.Epsilon)
            imaginaryPart = "-i";
        else
            imaginaryPart += $"{Imaginary.ToString(format, formatProvider)}i";

        return $"{Real.ToString(format, formatProvider)}{imaginaryPart}";
    }

    #endregion
}
