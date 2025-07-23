using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;

namespace Cosmium.Engine.Physics.Quantum.Orbitals;

/// <summary>
/// Represents a hydrogen-like atomic orbital with exact analytical solutions.
/// Implements the complete wave functions for hydrogen-like atoms (H, He+, Li2+, etc.)
/// using spherical harmonics and associated Laguerre polynomials.
/// </summary>
public class HydrogenicOrbital : Orbital
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _calculationLock = new();
    private double? _cachedNormalizationConstant;
    private readonly double _bohrRadiusScaled;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the orbital energy in Joules.
    /// For hydrogen-like atoms: E = -Z²Ry/n² where Ry is the Rydberg energy.
    /// </summary>
    public override double Energy
    {
        get
        {
            var rydbergEnergy = PhysicsConstants.RydbergConstant * PhysicsConstants.PlanckConstant * PhysicsConstants.SpeedOfLight;
            return -EffectiveNuclearCharge * EffectiveNuclearCharge * rydbergEnergy / (PrincipalQuantumNumber * PrincipalQuantumNumber);
        }
    }

    /// <summary>
    /// Gets whether this orbital is normalized. Analytical solutions are always normalized.
    /// </summary>
    public override bool IsNormalized => true;

    /// <summary>
    /// Gets the Bohr radius scaled by effective nuclear charge.
    /// </summary>
    public double BohrRadiusScaled => _bohrRadiusScaled;

    /// <summary>
    /// Gets the classical turning point radius for this orbital.
    /// </summary>
    public double ClassicalTurningPoint => PrincipalQuantumNumber * PrincipalQuantumNumber * PhysicsConstants.BohrRadius / EffectiveNuclearCharge;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new hydrogen-like orbital with the specified quantum numbers.
    /// </summary>
    /// <param name="n">Principal quantum number (n ≥ 1).</param>
    /// <param name="l">Orbital angular momentum quantum number (0 ≤ l ≤ n-1).</param>
    /// <param name="ml">Magnetic quantum number (-l ≤ ml ≤ l).</param>
    /// <param name="nuclearCharge">Nuclear charge Z (default 1 for hydrogen).</param>
    public HydrogenicOrbital(int n, int l, int ml, double nuclearCharge = 1.0)
        : base(n, l, ml, nuclearCharge)
    {
        _bohrRadiusScaled = PhysicsConstants.BohrRadius / EffectiveNuclearCharge;

        Logger.Debug($"Created hydrogen-like orbital {OrbitalDesignation} for Z={nuclearCharge}",
            new { n, l, ml, Z = nuclearCharge, Energy_eV = QuantumMath.JoulesToElectronVolts(Energy) });
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a hydrogen 1s orbital.
    /// </summary>
    /// <param name="nuclearCharge">Nuclear charge (default 1 for hydrogen).</param>
    /// <returns>A 1s orbital.</returns>
    public static HydrogenicOrbital Create1s(double nuclearCharge = 1.0)
    {
        return new HydrogenicOrbital(1, 0, 0, nuclearCharge);
    }

    /// <summary>
    /// Creates a hydrogen 2s orbital.
    /// </summary>
    /// <param name="nuclearCharge">Nuclear charge (default 1 for hydrogen).</param>
    /// <returns>A 2s orbital.</returns>
    public static HydrogenicOrbital Create2s(double nuclearCharge = 1.0)
    {
        return new HydrogenicOrbital(2, 0, 0, nuclearCharge);
    }

    /// <summary>
    /// Creates a hydrogen 2p orbital.
    /// </summary>
    /// <param name="ml">Magnetic quantum number (-1, 0, 1).</param>
    /// <param name="nuclearCharge">Nuclear charge (default 1 for hydrogen).</param>
    /// <returns>A 2p orbital.</returns>
    public static HydrogenicOrbital Create2p(int ml, double nuclearCharge = 1.0)
    {
        return new HydrogenicOrbital(2, 1, ml, nuclearCharge);
    }

    /// <summary>
    /// Creates a hydrogen 3s orbital.
    /// </summary>
    /// <param name="nuclearCharge">Nuclear charge (default 1 for hydrogen).</param>
    /// <returns>A 3s orbital.</returns>
    public static HydrogenicOrbital Create3s(double nuclearCharge = 1.0)
    {
        return new HydrogenicOrbital(3, 0, 0, nuclearCharge);
    }

    /// <summary>
    /// Creates a hydrogen 3p orbital.
    /// </summary>
    /// <param name="ml">Magnetic quantum number (-1, 0, 1).</param>
    /// <param name="nuclearCharge">Nuclear charge (default 1 for hydrogen).</param>
    /// <returns>A 3p orbital.</returns>
    public static HydrogenicOrbital Create3p(int ml, double nuclearCharge = 1.0)
    {
        return new HydrogenicOrbital(3, 1, ml, nuclearCharge);
    }

    /// <summary>
    /// Creates a hydrogen 3d orbital.
    /// </summary>
    /// <param name="ml">Magnetic quantum number (-2, -1, 0, 1, 2).</param>
    /// <param name="nuclearCharge">Nuclear charge (default 1 for hydrogen).</param>
    /// <returns>A 3d orbital.</returns>
    public static HydrogenicOrbital Create3d(int ml, double nuclearCharge = 1.0)
    {
        return new HydrogenicOrbital(3, 2, ml, nuclearCharge);
    }

    #endregion

    #region Wave Function Calculations

    /// <summary>
    /// Calculates the radial wave function R(r) using the analytical hydrogen-like solution.
    /// </summary>
    /// <param name="r">The radial distance from the nucleus in meters.</param>
    /// <returns>The value of the radial wave function.</returns>
    public override double RadialWaveFunction(double r)
    {
        var rValidation = ParameterValidator.ValidatePositive(r, nameof(r), allowZero: true);
        rValidation.ThrowIfInvalid();

        if (r < PrecisionHandling.MinimumAmplitudeMagnitude)
        {
            // Handle r=0 case
            return OrbitalAngularMomentumQuantumNumber == 0 ? CalculateNormalizationConstant() : 0.0;
        }

        // Dimensionless radial coordinate ρ = 2Zr/(na₀)
        double rho = 2.0 * EffectiveNuclearCharge * r / (PrincipalQuantumNumber * PhysicsConstants.BohrRadius);

        // Calculate the associated Laguerre polynomial L_{n-l-1}^{2l+1}(ρ)
        double laguerre = CalculateAssociatedLaguerrePolynomial(PrincipalQuantumNumber - OrbitalAngularMomentumQuantumNumber - 1, 
                                                               2 * OrbitalAngularMomentumQuantumNumber + 1, rho);

        // Radial wave function: R(r) = N * ρ^l * exp(-ρ/2) * L_{n-l-1}^{2l+1}(ρ)
        double exponential = Math.Exp(-rho / 2.0);
        double powerTerm = Math.Pow(rho, OrbitalAngularMomentumQuantumNumber);
        double normalization = CalculateNormalizationConstant();

        return normalization * powerTerm * exponential * laguerre;
    }

    /// <summary>
    /// Calculates the angular wave function Y_l^ml(θ, φ) using spherical harmonics.
    /// </summary>
    /// <param name="theta">The polar angle in radians (0 ≤ θ ≤ π).</param>
    /// <param name="phi">The azimuthal angle in radians (0 ≤ φ ≤ 2π).</param>
    /// <returns>The complex value of the spherical harmonic.</returns>
    public override Complex AngularWaveFunction(double theta, double phi)
    {
        var thetaValidation = ParameterValidator.ValidateRange(theta, nameof(theta), 0.0, Math.PI);
        thetaValidation.ThrowIfInvalid();

        var phiValidation = ParameterValidator.ValidateRange(phi, nameof(phi), 0.0, 2.0 * Math.PI);
        phiValidation.ThrowIfInvalid();

        return CalculateSphericalHarmonic(OrbitalAngularMomentumQuantumNumber, MagneticQuantumNumber, theta, phi);
    }

    #endregion

    #region Quantum Mechanical Properties

    /// <summary>
    /// Calculates the expectation value of the radial position ⟨r⟩.
    /// For hydrogen-like orbitals: ⟨r⟩ = (a₀/Z) * [3n² - l(l+1)] / 2
    /// </summary>
    /// <returns>The expectation value of r in meters.</returns>
    public override double ExpectationValueRadius()
    {
        double term = 3.0 * PrincipalQuantumNumber * PrincipalQuantumNumber - 
                     OrbitalAngularMomentumQuantumNumber * (OrbitalAngularMomentumQuantumNumber + 1);
        return (PhysicsConstants.BohrRadius / EffectiveNuclearCharge) * term / 2.0;
    }

    /// <summary>
    /// Calculates the expectation value of r² for this orbital.
    /// For hydrogen-like orbitals: ⟨r²⟩ = (a₀²/Z²) * n² * [5n² + 1 - 3l(l+1)] / 2
    /// </summary>
    /// <returns>The expectation value of r² in m².</returns>
    public override double ExpectationValueRadiusSquared()
    {
        double n2 = PrincipalQuantumNumber * PrincipalQuantumNumber;
        double ll1 = OrbitalAngularMomentumQuantumNumber * (OrbitalAngularMomentumQuantumNumber + 1);
        double term = 5.0 * n2 + 1.0 - 3.0 * ll1;
        double bohrSquared = PhysicsConstants.BohrRadius * PhysicsConstants.BohrRadius;
        return (bohrSquared / (EffectiveNuclearCharge * EffectiveNuclearCharge)) * n2 * term / 2.0;
    }

    /// <summary>
    /// Calculates the most probable radius (where radial probability density is maximum).
    /// For s orbitals (l=0): r_max = n²a₀/Z
    /// For l>0: approximate formula based on quantum number relationships
    /// </summary>
    /// <returns>The most probable radius in meters.</returns>
    public override double MostProbableRadius()
    {
        if (OrbitalAngularMomentumQuantumNumber == 0)
        {
            // For s orbitals, the most probable radius is n²a₀/Z
            return PrincipalQuantumNumber * PrincipalQuantumNumber * PhysicsConstants.BohrRadius / EffectiveNuclearCharge;
        }
        else
        {
            // For l>0, approximate most probable radius
            // This occurs roughly where the radial probability density P(r) = r²R²(r) is maximum
            // Approximate formula: r_max ≈ a₀(n + l)/Z
            return PhysicsConstants.BohrRadius * (PrincipalQuantumNumber + OrbitalAngularMomentumQuantumNumber) / EffectiveNuclearCharge;
        }
    }

    /// <summary>
    /// Calculates the radial probability density P(r) = r²|R(r)|².
    /// </summary>
    /// <param name="r">The radial distance from the nucleus in meters.</param>
    /// <returns>The radial probability density.</returns>
    public double RadialProbabilityDensity(double r)
    {
        var rValidation = ParameterValidator.ValidatePositive(r, nameof(r), allowZero: true);
        rValidation.ThrowIfInvalid();

        if (r < PrecisionHandling.MinimumAmplitudeMagnitude)
            return 0.0;

        double radialWf = RadialWaveFunction(r);
        return r * r * radialWf * radialWf;
    }

    /// <summary>
    /// Calculates the effective charge experienced by the electron.
    /// For hydrogen-like atoms, this equals the nuclear charge.
    /// </summary>
    /// <returns>The effective nuclear charge.</returns>
    public double CalculateEffectiveCharge()
    {
        return EffectiveNuclearCharge;
    }

    #endregion

    #region Mathematical Helper Methods

    /// <summary>
    /// Calculates the normalization constant for the radial wave function.
    /// </summary>
    /// <returns>The normalization constant.</returns>
    private double CalculateNormalizationConstant()
    {
        lock (_calculationLock)
        {
            if (_cachedNormalizationConstant.HasValue)
                return _cachedNormalizationConstant.Value;

            // Normalization constant for hydrogen-like radial wave functions
            // N = (2Z/na₀)^(3/2) * √[(n-l-1)! / (n+l)!]
            double zOverNa0 = EffectiveNuclearCharge / (PrincipalQuantumNumber * PhysicsConstants.BohrRadius);
            double factor1 = Math.Pow(2.0 * zOverNa0, 1.5);

            // Calculate factorial ratio
            double factorialRatio = CalculateFactorialRatio(PrincipalQuantumNumber - OrbitalAngularMomentumQuantumNumber - 1,
                                                          PrincipalQuantumNumber + OrbitalAngularMomentumQuantumNumber);

            _cachedNormalizationConstant = factor1 * Math.Sqrt(factorialRatio);
            return _cachedNormalizationConstant.Value;
        }
    }

    /// <summary>
    /// Calculates the associated Laguerre polynomial L_n^α(x).
    /// </summary>
    /// <param name="n">The principal index.</param>
    /// <param name="alpha">The associated index.</param>
    /// <param name="x">The argument.</param>
    /// <returns>The value of the associated Laguerre polynomial.</returns>
    private static double CalculateAssociatedLaguerrePolynomial(int n, int alpha, double x)
    {
        if (n == 0) return 1.0;
        if (n == 1) return alpha + 1.0 - x;

        // Use recurrence relation: (n+1)L_{n+1}^α = (2n+1+α-x)L_n^α - (n+α)L_{n-1}^α
        double L0 = 1.0;
        double L1 = alpha + 1.0 - x;

        for (int k = 2; k <= n; k++)
        {
            double L2 = ((2 * (k - 1) + 1 + alpha - x) * L1 - (k - 1 + alpha) * L0) / k;
            L0 = L1;
            L1 = L2;
        }

        return L1;
    }

    /// <summary>
    /// Calculates the spherical harmonic Y_l^m(θ, φ).
    /// </summary>
    /// <param name="l">The orbital angular momentum quantum number.</param>
    /// <param name="m">The magnetic quantum number.</param>
    /// <param name="theta">The polar angle in radians.</param>
    /// <param name="phi">The azimuthal angle in radians.</param>
    /// <returns>The complex value of the spherical harmonic.</returns>
    private static Complex CalculateSphericalHarmonic(int l, int m, double theta, double phi)
    {
        // Calculate the normalization constant
        double normalization = Math.Sqrt((2.0 * l + 1.0) / (4.0 * Math.PI) * 
                                       CalculateFactorialRatio(l - Math.Abs(m), l + Math.Abs(m)));

        // Calculate the associated Legendre polynomial P_l^|m|(cos θ)
        double cosTheta = Math.Cos(theta);
        double legendreValue = CalculateAssociatedLegendrePolynomial(l, Math.Abs(m), cosTheta);

        // Apply Condon-Shortley phase convention for negative m
        if (m < 0)
        {
            normalization *= Math.Pow(-1, Math.Abs(m));
        }

        // Calculate the exponential phase factor
        Complex phaseactor = Complex.FromPhase(m * phi);

        return normalization * legendreValue * phaseactor;
    }

    /// <summary>
    /// Calculates the associated Legendre polynomial P_l^m(x).
    /// </summary>
    /// <param name="l">The degree.</param>
    /// <param name="m">The order.</param>
    /// <param name="x">The argument.</param>
    /// <returns>The value of the associated Legendre polynomial.</returns>
    private static double CalculateAssociatedLegendrePolynomial(int l, int m, double x)
    {
        if (m > l) return 0.0;
        if (m == 0) return CalculateLegendrePolynomial(l, x);

        // Calculate P_l^m using the formula involving derivatives
        // For computational stability, use the recurrence relations
        double factor = Math.Pow(1.0 - x * x, m / 2.0);
        
        // Start with P_m^m
        double pmm = Math.Pow(-1, m) * DoubleFactorial(2 * m - 1) * factor;
        
        if (l == m) return pmm;

        // Calculate P_{m+1}^m
        double pmm1 = x * (2 * m + 1) * pmm;
        
        if (l == m + 1) return pmm1;

        // Use recurrence for higher l
        double plm = 0.0;
        for (int ll = m + 2; ll <= l; ll++)
        {
            plm = (x * (2 * ll - 1) * pmm1 - (ll + m - 1) * pmm) / (ll - m);
            pmm = pmm1;
            pmm1 = plm;
        }

        return plm;
    }

    /// <summary>
    /// Calculates the Legendre polynomial P_n(x).
    /// </summary>
    /// <param name="n">The degree.</param>
    /// <param name="x">The argument.</param>
    /// <returns>The value of the Legendre polynomial.</returns>
    private static double CalculateLegendrePolynomial(int n, double x)
    {
        if (n == 0) return 1.0;
        if (n == 1) return x;

        // Use recurrence relation: (n+1)P_{n+1} = (2n+1)xP_n - nP_{n-1}
        double P0 = 1.0;
        double P1 = x;

        for (int k = 2; k <= n; k++)
        {
            double P2 = ((2 * k - 1) * x * P1 - (k - 1) * P0) / k;
            P0 = P1;
            P1 = P2;
        }

        return P1;
    }

    /// <summary>
    /// Calculates the factorial ratio n1! / n2!.
    /// </summary>
    /// <param name="n1">The numerator factorial.</param>
    /// <param name="n2">The denominator factorial.</param>
    /// <returns>The factorial ratio.</returns>
    private static double CalculateFactorialRatio(int n1, int n2)
    {
        if (n1 == n2) return 1.0;
        if (n1 < 0 || n2 < 0) return 0.0;

        if (n1 > n2)
        {
            double result = 1.0;
            for (int i = n2 + 1; i <= n1; i++)
            {
                result *= i;
            }
            return result;
        }
        else
        {
            double result = 1.0;
            for (int i = n1 + 1; i <= n2; i++)
            {
                result /= i;
            }
            return result;
        }
    }

    /// <summary>
    /// Calculates the double factorial n!! = n × (n-2) × (n-4) × ...
    /// </summary>
    /// <param name="n">The input value.</param>
    /// <returns>The double factorial.</returns>
    private static double DoubleFactorial(int n)
    {
        if (n <= 0) return 1.0;
        
        double result = 1.0;
        for (int i = n; i > 0; i -= 2)
        {
            result *= i;
        }
        return result;
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a detailed string representation of the hydrogen-like orbital.
    /// </summary>
    public override string ToDetailedString()
    {
        return base.ToDetailedString() + "\n" +
               $"Orbital Type: Hydrogen-like\n" +
               $"Bohr Radius (scaled): {BohrRadiusScaled * 1e12:F2} pm\n" +
               $"Most Probable Radius: {MostProbableRadius() * 1e12:F2} pm\n" +
               $"⟨r⟩: {ExpectationValueRadius() * 1e12:F2} pm\n" +
               $"Classical Turning Point: {ClassicalTurningPoint * 1e12:F2} pm";
    }

    #endregion
}
