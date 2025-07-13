using System.Numerics;

namespace Cosmium.Engine.Physics.Quantum.Particles.Abstract;

/// <summary>
/// Interface for measurable properties in quantum systems.
/// Defines the contract for properties that can be observed and measured in quantum mechanics.
/// </summary>
/// <typeparam name="T">The type of the measured value.</typeparam>
public interface IMeasurable<T>
{
    /// <summary>
    /// Gets the current value of the observable property.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Gets the uncertainty in the measurement of this property.
    /// Represents the quantum mechanical uncertainty in the observable.
    /// </summary>
    double Uncertainty { get; }

    /// <summary>
    /// Gets whether this property has been measured.
    /// In quantum mechanics, measurement can affect the system state.
    /// </summary>
    bool IsMeasured { get; }

    /// <summary>
    /// Gets the time when this property was last measured.
    /// </summary>
    DateTime? LastMeasurementTime { get; }

    /// <summary>
    /// Performs a measurement of this property.
    /// This may cause wave function collapse for quantum systems.
    /// </summary>
    /// <returns>The measured value.</returns>
    T Measure();

    /// <summary>
    /// Resets the measurement state, allowing the property to return to a superposition.
    /// </summary>
    void ResetMeasurement();
}

/// <summary>
/// Interface for complex-valued measurable properties common in quantum mechanics.
/// </summary>
public interface IComplexMeasurable : IMeasurable<Complex>
{
    /// <summary>
    /// Gets the magnitude of the complex value.
    /// </summary>
    double Magnitude { get; }

    /// <summary>
    /// Gets the phase of the complex value in radians.
    /// </summary>
    double Phase { get; }

    /// <summary>
    /// Gets the real part of the complex value.
    /// </summary>
    double Real { get; }

    /// <summary>
    /// Gets the imaginary part of the complex value.
    /// </summary>
    double Imaginary { get; }
}

/// <summary>
/// Interface for vector-valued measurable properties (e.g., momentum, angular momentum).
/// </summary>
public interface IVectorMeasurable : IMeasurable<double[]>
{
    /// <summary>
    /// Gets the magnitude of the vector.
    /// </summary>
    double Magnitude { get; }

    /// <summary>
    /// Gets the x-component of the vector.
    /// </summary>
    double X { get; }

    /// <summary>
    /// Gets the y-component of the vector.
    /// </summary>
    double Y { get; }

    /// <summary>
    /// Gets the z-component of the vector.
    /// </summary>
    double Z { get; }

    /// <summary>
    /// Gets the direction unit vector.
    /// </summary>
    double[] Direction { get; }
}

/// <summary>
/// Interface for scalar measurable properties (e.g., energy, mass, charge).
/// </summary>
public interface IScalarMeasurable : IMeasurable<double>
{
    /// <summary>
    /// Gets the units of measurement for this property.
    /// </summary>
    string Units { get; }

    /// <summary>
    /// Gets the value in the specified unit system.
    /// </summary>
    /// <param name="unitSystem">The target unit system.</param>
    /// <returns>The value converted to the specified units.</returns>
    double GetValueInUnits(string unitSystem);
}
