using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;

namespace Cosmium.Engine.Physics.Quantum.Orbitals;

/// <summary>
/// Abstract base class representing a quantum mechanical orbital.
/// Provides the foundation for atomic and molecular orbital calculations with common quantum mechanical properties.
/// </summary>
public abstract class Orbital : IEquatable<Orbital>
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _calculationLock = new();
    private double? _cachedNormalization;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the principal quantum number (n).
    /// Determines the energy level and size of the orbital.
    /// </summary>
    public int PrincipalQuantumNumber { get; }

    /// <summary>
    /// Gets the orbital angular momentum quantum number (l).
    /// Determines the shape of the orbital (s=0, p=1, d=2, f=3, etc.).
    /// </summary>
    public int OrbitalAngularMomentumQuantumNumber { get; }

    /// <summary>
    /// Gets the magnetic quantum number (ml).
    /// Determines the orientation of the orbital in space.
    /// </summary>
    public int MagneticQuantumNumber { get; }

    /// <summary>
    /// Gets the effective nuclear charge experienced by the electron.
    /// For hydrogen-like atoms, this equals the atomic number.
    /// </summary>
    public double EffectiveNuclearCharge { get; }

    /// <summary>
    /// Gets the orbital energy in Joules.
    /// </summary>
    public abstract double Energy { get; }

    /// <summary>
    /// Gets the orbital designation (e.g., "1s", "2p", "3d").
    /// </summary>
    public string OrbitalDesignation
    {
        get
        {
            string shell = OrbitalAngularMomentumQuantumNumber switch
            {
                0 => "s",
                1 => "p",
                2 => "d",
                3 => "f",
                4 => "g",
                5 => "h",
                _ => $"l={OrbitalAngularMomentumQuantumNumber}"
            };
            return $"{PrincipalQuantumNumber}{shell}";
        }
    }

    /// <summary>
    /// Gets whether this orbital is normalized.
    /// </summary>
    public abstract bool IsNormalized { get; }

    /// <summary>
    /// Gets the maximum number of electrons that can occupy this orbital.
    /// Always 2 due to Pauli exclusion principle (spin up and spin down).
    /// </summary>
    public int MaxElectrons => 2;

    /// <summary>
    /// Gets the creation time of this orbital.
    /// </summary>
    public DateTime CreationTime { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new orbital with the specified quantum numbers.
    /// </summary>
    /// <param name="n">Principal quantum number (n ≥ 1).</param>
    /// <param name="l">Orbital angular momentum quantum number (0 ≤ l ≤ n-1).</param>
    /// <param name="ml">Magnetic quantum number (-l ≤ ml ≤ l).</param>
    /// <param name="effectiveNuclearCharge">Effective nuclear charge (Z_eff > 0).</param>
    protected Orbital(int n, int l, int ml, double effectiveNuclearCharge)
    {
        // Validate quantum numbers
        var nValidation = ParameterValidator.ValidateIntegerRange(n, nameof(n), 1, 100);
        nValidation.ThrowIfInvalid();

        var lValidation = ParameterValidator.ValidateIntegerRange(l, nameof(l), 0, n - 1);
        lValidation.ThrowIfInvalid();

        var mlValidation = ParameterValidator.ValidateIntegerRange(ml, nameof(ml), -l, l);
        mlValidation.ThrowIfInvalid();

        var zEffValidation = ParameterValidator.ValidatePositive(effectiveNuclearCharge, nameof(effectiveNuclearCharge));
        zEffValidation.ThrowIfInvalid();

        PrincipalQuantumNumber = n;
        OrbitalAngularMomentumQuantumNumber = l;
        MagneticQuantumNumber = ml;
        EffectiveNuclearCharge = effectiveNuclearCharge;
        CreationTime = DateTime.UtcNow;

        Logger.Debug($"Created orbital {OrbitalDesignation}",
            new { n, l, ml, Z_eff = effectiveNuclearCharge });
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Calculates the radial wave function R(r) at the specified distance from the nucleus.
    /// </summary>
    /// <param name="r">The radial distance from the nucleus in meters.</param>
    /// <returns>The value of the radial wave function.</returns>
    public abstract double RadialWaveFunction(double r);

    /// <summary>
    /// Calculates the angular wave function Y(θ, φ) at the specified spherical coordinates.
    /// </summary>
    /// <param name="theta">The polar angle in radians (0 ≤ θ ≤ π).</param>
    /// <param name="phi">The azimuthal angle in radians (0 ≤ φ ≤ 2π).</param>
    /// <returns>The complex value of the spherical harmonic.</returns>
    public abstract Complex AngularWaveFunction(double theta, double phi);

    /// <summary>
    /// Calculates the complete wave function ψ(r, θ, φ) = R(r) * Y(θ, φ).
    /// </summary>
    /// <param name="position">The position vector in Cartesian coordinates.</param>
    /// <returns>The complex value of the complete wave function.</returns>
    public virtual Complex WaveFunction(Vector3D position)
    {
        var (r, theta, phi) = position.ToSpherical();
        return RadialWaveFunction(r) * AngularWaveFunction(theta, phi);
    }

    /// <summary>
    /// Calculates the electron probability density |ψ|² at the specified position.
    /// </summary>
    /// <param name="position">The position vector in Cartesian coordinates.</param>
    /// <returns>The probability density in m⁻³.</returns>
    public virtual double ProbabilityDensity(Vector3D position)
    {
        var waveFunction = WaveFunction(position);
        return waveFunction.MagnitudeSquared;
    }

    #endregion

    #region Quantum Mechanical Calculations

    /// <summary>
    /// Calculates the expectation value of the radial position ⟨r⟩.
    /// </summary>
    /// <returns>The expectation value of r in meters.</returns>
    public abstract double ExpectationValueRadius();

    /// <summary>
    /// Calculates the expectation value of r² for this orbital.
    /// </summary>
    /// <returns>The expectation value of r² in m².</returns>
    public abstract double ExpectationValueRadiusSquared();

    /// <summary>
    /// Calculates the most probable radius (where radial probability density is maximum).
    /// </summary>
    /// <returns>The most probable radius in meters.</returns>
    public abstract double MostProbableRadius();

    /// <summary>
    /// Calculates the overlap integral with another orbital.
    /// </summary>
    /// <param name="other">The other orbital.</param>
    /// <returns>The overlap integral ⟨ψ₁|ψ₂⟩.</returns>
    public virtual Complex OverlapIntegral(Orbital other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        // For different quantum numbers, orthogonality gives zero overlap
        if (PrincipalQuantumNumber != other.PrincipalQuantumNumber ||
            OrbitalAngularMomentumQuantumNumber != other.OrbitalAngularMomentumQuantumNumber ||
            MagneticQuantumNumber != other.MagneticQuantumNumber)
        {
            return Complex.Zero;
        }

        // For identical orbitals, overlap is 1 if normalized
        if (Equals(other) && IsNormalized && other.IsNormalized)
        {
            return Complex.One;
        }

        // For general case, would need numerical integration
        throw new NotImplementedException("General overlap integral requires numerical integration.");
    }

    /// <summary>
    /// Calculates the kinetic energy expectation value for this orbital.
    /// </summary>
    /// <returns>The kinetic energy expectation value in Joules.</returns>
    public virtual double KineticEnergyExpectation()
    {
        // For hydrogen-like atoms: T = -E - V = -E - (-2E) = E
        // This is a consequence of the virial theorem for Coulomb potential
        return -Energy;
    }

    /// <summary>
    /// Calculates the potential energy expectation value for this orbital.
    /// </summary>
    /// <returns>The potential energy expectation value in Joules.</returns>
    public virtual double PotentialEnergyExpectation()
    {
        // For hydrogen-like atoms: V = 2E (from virial theorem)
        return 2.0 * Energy;
    }

    /// <summary>
    /// Calculates the orbital angular momentum magnitude expectation value.
    /// </summary>
    /// <returns>The orbital angular momentum magnitude ⟨|L|⟩ in J·s.</returns>
    public double OrbitalAngularMomentumMagnitude()
    {
        return PhysicsConstants.ReducedPlanckConstant * Math.Sqrt(OrbitalAngularMomentumQuantumNumber * (OrbitalAngularMomentumQuantumNumber + 1));
    }

    /// <summary>
    /// Calculates the z-component of orbital angular momentum.
    /// </summary>
    /// <returns>The z-component ⟨Lz⟩ in J·s.</returns>
    public double OrbitalAngularMomentumZ()
    {
        return PhysicsConstants.ReducedPlanckConstant * MagneticQuantumNumber;
    }

    #endregion

    #region Validation and Normalization

    /// <summary>
    /// Validates that the quantum numbers follow the proper quantum mechanical rules.
    /// </summary>
    /// <returns>True if quantum numbers are valid.</returns>
    public bool ValidateQuantumNumbers()
    {
        return PrincipalQuantumNumber >= 1 &&
               OrbitalAngularMomentumQuantumNumber >= 0 &&
               OrbitalAngularMomentumQuantumNumber < PrincipalQuantumNumber &&
               Math.Abs(MagneticQuantumNumber) <= OrbitalAngularMomentumQuantumNumber &&
               EffectiveNuclearCharge > 0;
    }

    /// <summary>
    /// Calculates the normalization constant for this orbital.
    /// </summary>
    /// <returns>The normalization constant.</returns>
    protected virtual double CalculateNormalization()
    {
        lock (_calculationLock)
        {
            if (_cachedNormalization.HasValue)
                return _cachedNormalization.Value;

            // This would typically require numerical integration
            // Derived classes should override with analytical expressions where possible
            _cachedNormalization = 1.0;
            return _cachedNormalization.Value;
        }
    }

    /// <summary>
    /// Checks if the orbital satisfies physical constraints and quantum mechanical rules.
    /// </summary>
    /// <returns>True if the orbital is physically valid.</returns>
    public virtual bool IsPhysicallyValid()
    {
        return ValidateQuantumNumbers() &&
               double.IsFinite(EffectiveNuclearCharge) &&
               EffectiveNuclearCharge > 0 &&
               double.IsFinite(Energy);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Creates a formatted string describing the orbital's quantum state.
    /// </summary>
    /// <returns>A quantum state description.</returns>
    public virtual string GetQuantumStateDescription()
    {
        return $"|n={PrincipalQuantumNumber}, l={OrbitalAngularMomentumQuantumNumber}, ml={MagneticQuantumNumber}⟩";
    }

    /// <summary>
    /// Gets the number of radial nodes for this orbital.
    /// </summary>
    /// <returns>The number of radial nodes (n - l - 1).</returns>
    public int RadialNodes => PrincipalQuantumNumber - OrbitalAngularMomentumQuantumNumber - 1;

    /// <summary>
    /// Gets the number of angular nodes for this orbital.
    /// </summary>
    /// <returns>The number of angular nodes (l).</returns>
    public int AngularNodes => OrbitalAngularMomentumQuantumNumber;

    /// <summary>
    /// Gets the total number of nodes for this orbital.
    /// </summary>
    /// <returns>The total number of nodes (n - 1).</returns>
    public int TotalNodes => PrincipalQuantumNumber - 1;

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether two orbitals are equal based on their quantum numbers and nuclear charge.
    /// </summary>
    public virtual bool Equals(Orbital? other)
    {
        if (other == null) return false;

        return PrincipalQuantumNumber == other.PrincipalQuantumNumber &&
               OrbitalAngularMomentumQuantumNumber == other.OrbitalAngularMomentumQuantumNumber &&
               MagneticQuantumNumber == other.MagneticQuantumNumber &&
               Math.Abs(EffectiveNuclearCharge - other.EffectiveNuclearCharge) < PrecisionHandling.QuantumCalculationTolerance;
    }

    /// <summary>
    /// Determines whether this orbital equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Orbital other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this orbital.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(PrincipalQuantumNumber, OrbitalAngularMomentumQuantumNumber, 
                               MagneticQuantumNumber, EffectiveNuclearCharge);
    }

    /// <summary>
    /// Equality operator for orbitals.
    /// </summary>
    public static bool operator ==(Orbital? left, Orbital? right)
    {
        return EqualityComparer<Orbital>.Default.Equals(left, right);
    }

    /// <summary>
    /// Inequality operator for orbitals.
    /// </summary>
    public static bool operator !=(Orbital? left, Orbital? right)
    {
        return !(left == right);
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the orbital.
    /// </summary>
    public override string ToString()
    {
        return $"{OrbitalDesignation} orbital (Z_eff = {EffectiveNuclearCharge:F2}, E = {QuantumMath.JoulesToElectronVolts(Energy):F3} eV)";
    }

    /// <summary>
    /// Returns a detailed string representation including all quantum numbers.
    /// </summary>
    public virtual string ToDetailedString()
    {
        return $"Orbital: {OrbitalDesignation}\n" +
               $"Quantum Numbers: n={PrincipalQuantumNumber}, l={OrbitalAngularMomentumQuantumNumber}, ml={MagneticQuantumNumber}\n" +
               $"Effective Nuclear Charge: {EffectiveNuclearCharge:F3}\n" +
               $"Energy: {QuantumMath.JoulesToElectronVolts(Energy):F6} eV\n" +
               $"Radial Nodes: {RadialNodes}, Angular Nodes: {AngularNodes}\n" +
               $"Created: {CreationTime:yyyy-MM-dd HH:mm:ss} UTC";
    }

    #endregion
}
