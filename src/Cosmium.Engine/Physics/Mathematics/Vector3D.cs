using System;
using System.Globalization;

namespace Cosmium.Engine.Physics.Mathematics;

/// <summary>
/// Represents a 3D vector with quantum-specific operations for spatial calculations in quantum mechanics.
/// Provides functionality for position, momentum, angular momentum, and field vectors.
/// </summary>
public readonly struct Vector3D : IEquatable<Vector3D>, IFormattable
{
    #region Properties

    /// <summary>
    /// Gets the X component of the vector.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets the Y component of the vector.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Gets the Z component of the vector.
    /// </summary>
    public double Z { get; }

    /// <summary>
    /// Gets the magnitude (length) of the vector.
    /// </summary>
    public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Gets the squared magnitude of the vector.
    /// Optimized for performance when only relative magnitudes are needed.
    /// </summary>
    public double MagnitudeSquared => X * X + Y * Y + Z * Z;

    /// <summary>
    /// Gets the unit vector in the same direction.
    /// </summary>
    public Vector3D Normalized
    {
        get
        {
            double magnitude = Magnitude;
            if (magnitude < double.Epsilon)
                throw new InvalidOperationException("Cannot normalize zero vector.");
            return this / magnitude;
        }
    }

    /// <summary>
    /// Checks if this vector is a unit vector (magnitude ≈ 1).
    /// </summary>
    public bool IsUnit => Math.Abs(Magnitude - 1.0) < 1e-10;

    /// <summary>
    /// Checks if this vector is approximately zero.
    /// </summary>
    public bool IsZero => MagnitudeSquared < 1e-20;

    #endregion

    #region Constants

    /// <summary>
    /// Represents the zero vector (0, 0, 0).
    /// </summary>
    public static readonly Vector3D Zero = new(0, 0, 0);

    /// <summary>
    /// Represents the unit vector along the X-axis (1, 0, 0).
    /// </summary>
    public static readonly Vector3D UnitX = new(1, 0, 0);

    /// <summary>
    /// Represents the unit vector along the Y-axis (0, 1, 0).
    /// </summary>
    public static readonly Vector3D UnitY = new(0, 1, 0);

    /// <summary>
    /// Represents the unit vector along the Z-axis (0, 0, 1).
    /// </summary>
    public static readonly Vector3D UnitZ = new(0, 0, 1);

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the Vector3D struct.
    /// </summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    /// <param name="z">The Z component.</param>
    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the Vector3D struct with the same value for all components.
    /// </summary>
    /// <param name="value">The value for all components.</param>
    public Vector3D(double value) : this(value, value, value) { }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a vector from spherical coordinates.
    /// </summary>
    /// <param name="radius">The radial distance (r).</param>
    /// <param name="theta">The polar angle in radians (θ, from Z-axis).</param>
    /// <param name="phi">The azimuthal angle in radians (φ, from X-axis).</param>
    /// <returns>A vector in Cartesian coordinates.</returns>
    public static Vector3D FromSpherical(double radius, double theta, double phi)
    {
        double sinTheta = Math.Sin(theta);
        return new Vector3D(
            radius * sinTheta * Math.Cos(phi),
            radius * sinTheta * Math.Sin(phi),
            radius * Math.Cos(theta)
        );
    }

    /// <summary>
    /// Creates a vector from cylindrical coordinates.
    /// </summary>
    /// <param name="rho">The radial distance in the XY plane (ρ).</param>
    /// <param name="phi">The azimuthal angle in radians (φ).</param>
    /// <param name="z">The Z coordinate.</param>
    /// <returns>A vector in Cartesian coordinates.</returns>
    public static Vector3D FromCylindrical(double rho, double phi, double z)
    {
        return new Vector3D(
            rho * Math.Cos(phi),
            rho * Math.Sin(phi),
            z
        );
    }

    /// <summary>
    /// Creates a random unit vector uniformly distributed on the unit sphere.
    /// Useful for random orientations in quantum scattering calculations.
    /// </summary>
    /// <param name="random">The random number generator to use.</param>
    /// <returns>A random unit vector.</returns>
    public static Vector3D RandomUnit(Random random)
    {
        // Use the Marsaglia method for uniform distribution on sphere
        double z = 2.0 * random.NextDouble() - 1.0; // z ∈ [-1, 1]
        double phi = 2.0 * Math.PI * random.NextDouble(); // φ ∈ [0, 2π]
        double rho = Math.Sqrt(1.0 - z * z);

        return new Vector3D(
            rho * Math.Cos(phi),
            rho * Math.Sin(phi),
            z
        );
    }

    #endregion

    #region Arithmetic Operations

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    public static Vector3D operator +(Vector3D left, Vector3D right)
    {
        return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    /// <summary>
    /// Subtracts two vectors.
    /// </summary>
    public static Vector3D operator -(Vector3D left, Vector3D right)
    {
        return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    public static Vector3D operator *(Vector3D vector, double scalar)
    {
        return new Vector3D(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    /// <summary>
    /// Multiplies a scalar by a vector.
    /// </summary>
    public static Vector3D operator *(double scalar, Vector3D vector)
    {
        return vector * scalar;
    }

    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    public static Vector3D operator /(Vector3D vector, double scalar)
    {
        if (Math.Abs(scalar) < double.Epsilon)
            throw new DivideByZeroException("Cannot divide vector by zero scalar.");

        return new Vector3D(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    }

    /// <summary>
    /// Negates a vector.
    /// </summary>
    public static Vector3D operator -(Vector3D vector)
    {
        return new Vector3D(-vector.X, -vector.Y, -vector.Z);
    }

    #endregion

    #region Vector Operations

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The dot product (scalar).</returns>
    public double Dot(Vector3D other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The cross product vector.</returns>
    public Vector3D Cross(Vector3D other)
    {
        return new Vector3D(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
    }

    /// <summary>
    /// Calculates the distance between two points represented as vectors.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The distance between the points.</returns>
    public double DistanceTo(Vector3D other)
    {
        return (this - other).Magnitude;
    }

    /// <summary>
    /// Calculates the squared distance between two points.
    /// Optimized for performance when only relative distances are needed.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The squared distance between the points.</returns>
    public double DistanceSquaredTo(Vector3D other)
    {
        return (this - other).MagnitudeSquared;
    }

    /// <summary>
    /// Calculates the angle between two vectors in radians.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The angle in radians [0, π].</returns>
    public double AngleTo(Vector3D other)
    {
        double cosAngle = Dot(other) / (Magnitude * other.Magnitude);
        // Clamp to handle numerical errors
        cosAngle = Math.Max(-1.0, Math.Min(1.0, cosAngle));
        return Math.Acos(cosAngle);
    }

    /// <summary>
    /// Projects this vector onto another vector.
    /// </summary>
    /// <param name="onto">The vector to project onto.</param>
    /// <returns>The projection of this vector onto the other vector.</returns>
    public Vector3D ProjectOnto(Vector3D onto)
    {
        double ontoMagnitudeSquared = onto.MagnitudeSquared;
        if (ontoMagnitudeSquared < double.Epsilon)
            throw new ArgumentException("Cannot project onto zero vector.");

        return onto * (Dot(onto) / ontoMagnitudeSquared);
    }

    /// <summary>
    /// Reflects this vector about a normal vector.
    /// </summary>
    /// <param name="normal">The normal vector (should be normalized).</param>
    /// <returns>The reflected vector.</returns>
    public Vector3D Reflect(Vector3D normal)
    {
        return this - 2 * Dot(normal) * normal;
    }

    #endregion

    #region Quantum-Specific Operations

    /// <summary>
    /// Converts the position vector to spherical coordinates.
    /// Returns (radius, theta, phi) where theta is polar angle and phi is azimuthal angle.
    /// </summary>
    /// <returns>A tuple containing (radius, theta, phi) in radians.</returns>
    public (double Radius, double Theta, double Phi) ToSpherical()
    {
        double radius = Magnitude;
        if (radius < double.Epsilon)
            return (0, 0, 0);

        double theta = Math.Acos(Z / radius); // Polar angle [0, π]
        double phi = Math.Atan2(Y, X);        // Azimuthal angle [-π, π]

        return (radius, theta, phi);
    }

    /// <summary>
    /// Calculates the component of this vector along a specified direction.
    /// Useful for momentum measurements along specific axes.
    /// </summary>
    /// <param name="direction">The direction vector (will be normalized).</param>
    /// <returns>The component along the direction.</returns>
    public double ComponentAlong(Vector3D direction)
    {
        return Dot(direction.Normalized);
    }

    /// <summary>
    /// Calculates the orbital angular momentum L = r × p.
    /// </summary>
    /// <param name="momentum">The momentum vector.</param>
    /// <returns>The angular momentum vector.</returns>
    public Vector3D OrbitalAngularMomentum(Vector3D momentum)
    {
        return Cross(momentum);
    }

    /// <summary>
    /// Checks if this vector represents a valid physical quantity (finite components).
    /// </summary>
    /// <returns>True if all components are finite.</returns>
    public bool IsValidPhysicalVector()
    {
        return double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
    }

    /// <summary>
    /// Rotates this vector around an arbitrary axis by a specified angle.
    /// Uses Rodrigues' rotation formula.
    /// </summary>
    /// <param name="axis">The rotation axis (should be normalized).</param>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>The rotated vector.</returns>
    public Vector3D RotateAround(Vector3D axis, double angle)
    {
        if (!axis.IsUnit)
            axis = axis.Normalized;

        double cosAngle = Math.Cos(angle);
        double sinAngle = Math.Sin(angle);

        // Rodrigues' formula: v' = v*cos(θ) + (k×v)*sin(θ) + k*(k·v)*(1-cos(θ))
        Vector3D crossProduct = axis.Cross(this);
        double dotProduct = axis.Dot(this);

        return this * cosAngle + crossProduct * sinAngle + axis * dotProduct * (1 - cosAngle);
    }

    #endregion

    #region Component Access

    /// <summary>
    /// Gets the component at the specified index.
    /// </summary>
    /// <param name="index">The index (0=X, 1=Y, 2=Z).</param>
    /// <returns>The component value.</returns>
    public double this[int index] => index switch
    {
        0 => X,
        1 => Y,
        2 => Z,
        _ => throw new IndexOutOfRangeException("Vector3D index must be 0, 1, or 2.")
    };

    /// <summary>
    /// Returns an array containing the vector components.
    /// </summary>
    /// <returns>An array [X, Y, Z].</returns>
    public double[] ToArray() => [X, Y, Z];

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether two vectors are equal.
    /// </summary>
    public static bool operator ==(Vector3D left, Vector3D right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two vectors are not equal.
    /// </summary>
    public static bool operator !=(Vector3D left, Vector3D right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Determines whether this vector is equal to another.
    /// </summary>
    public bool Equals(Vector3D other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    /// <summary>
    /// Determines whether this vector is approximately equal to another within a tolerance.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the vectors are approximately equal.</returns>
    public bool Equals(Vector3D other, double tolerance)
    {
        return Math.Abs(X - other.X) < tolerance &&
               Math.Abs(Y - other.Y) < tolerance &&
               Math.Abs(Z - other.Z) < tolerance;
    }

    /// <summary>
    /// Determines whether this instance is equal to a specified object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Vector3D other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the vector.
    /// </summary>
    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a string representation of the vector with specified formatting.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
            format = "G";

        if (formatProvider == null)
            formatProvider = CultureInfo.CurrentCulture;

        return $"({X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)}, {Z.ToString(format, formatProvider)})";
    }

    #endregion
}
