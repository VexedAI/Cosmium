using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;

namespace Cosmium.Engine.Physics.Quantum.States;

/// <summary>
/// Represents a quantum spin state with operations for spin measurements and rotations.
/// Supports arbitrary spin values (1/2, 1, 3/2, etc.) and provides common spin operations.
/// </summary>
public class SpinState : IEquatable<SpinState>
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _stateLock = new();
    private Complex[] _amplitudes;
    private bool _isNormalized;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the spin quantum number (j = 1/2, 1, 3/2, 2, ...).
    /// </summary>
    public double SpinQuantumNumber { get; }

    /// <summary>
    /// Gets the dimension of the spin state space (2j + 1).
    /// </summary>
    public int Dimension { get; }

    /// <summary>
    /// Gets the possible magnetic quantum numbers (mj values).
    /// </summary>
    public double[] MagneticQuantumNumbers
    {
        get
        {
            var values = new double[Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                values[i] = SpinQuantumNumber - i;
            }
            return values;
        }
    }

    /// <summary>
    /// Gets the spin state amplitudes. Returns a copy to maintain immutability.
    /// </summary>
    public Complex[] Amplitudes
    {
        get
        {
            lock (_stateLock)
            {
                return (Complex[])_amplitudes.Clone();
            }
        }
    }

    /// <summary>
    /// Gets whether this spin state is normalized.
    /// </summary>
    public bool IsNormalized
    {
        get
        {
            lock (_stateLock)
            {
                return _isNormalized && Math.Abs(CalculateNorm() - 1.0) < PrecisionHandling.ProbabilityNormalizationTolerance;
            }
        }
    }

    /// <summary>
    /// Gets the norm of the spin state.
    /// </summary>
    public double Norm
    {
        get
        {
            lock (_stateLock)
            {
                return CalculateNorm();
            }
        }
    }

    /// <summary>
    /// Gets the probability distribution for each magnetic quantum number.
    /// </summary>
    public double[] ProbabilityDistribution
    {
        get
        {
            lock (_stateLock)
            {
                return _amplitudes.Select(amp => amp.MagnitudeSquared).ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the creation time of this spin state.
    /// </summary>
    public DateTime CreationTime { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new spin state with the specified quantum number and amplitudes.
    /// </summary>
    /// <param name="spinQuantumNumber">The spin quantum number (j).</param>
    /// <param name="amplitudes">The amplitude coefficients for each magnetic quantum number.</param>
    /// <param name="normalize">Whether to automatically normalize the state.</param>
    public SpinState(double spinQuantumNumber, Complex[] amplitudes, bool normalize = true)
    {
        if (amplitudes == null)
            throw new ArgumentNullException(nameof(amplitudes));

        // Validate spin quantum number (must be non-negative half-integer)
        if (spinQuantumNumber < 0 || Math.Abs(spinQuantumNumber * 2 - Math.Round(spinQuantumNumber * 2)) > 1e-10)
            throw new ArgumentException("Spin quantum number must be a non-negative half-integer", nameof(spinQuantumNumber));

        var expectedDimension = (int)(2 * spinQuantumNumber + 1);
        if (amplitudes.Length != expectedDimension)
            throw new ArgumentException($"Amplitude array length ({amplitudes.Length}) must equal 2j+1 ({expectedDimension})", nameof(amplitudes));

        // Validate finite amplitudes
        for (int i = 0; i < amplitudes.Length; i++)
        {
            if (!double.IsFinite(amplitudes[i].Real) || !double.IsFinite(amplitudes[i].Imaginary))
                throw new ArgumentException($"Amplitude at index {i} contains non-finite values", nameof(amplitudes));
        }

        SpinQuantumNumber = spinQuantumNumber;
        Dimension = expectedDimension;
        _amplitudes = (Complex[])amplitudes.Clone();
        CreationTime = DateTime.UtcNow;

        if (normalize)
        {
            Normalize();
        }
        else
        {
            _isNormalized = Math.Abs(CalculateNorm() - 1.0) < PrecisionHandling.ProbabilityNormalizationTolerance;
        }

        Logger.Debug($"Created spin-{spinQuantumNumber} state with dimension {Dimension}",
            new { SpinQuantumNumber = spinQuantumNumber, Dimension, IsNormalized, Norm = CalculateNorm() });
    }

    #endregion

    #region Factory Methods for Common Spin States

    /// <summary>
    /// Creates a spin-1/2 state in the |+⟩ (spin-up) eigenstate.
    /// </summary>
    /// <returns>A normalized spin-up state.</returns>
    public static SpinState CreateSpinHalfUp()
    {
        var amplitudes = new Complex[] { new Complex(1.0, 0.0), new Complex(0.0, 0.0) };
        return new SpinState(0.5, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a spin-1/2 state in the |−⟩ (spin-down) eigenstate.
    /// </summary>
    /// <returns>A normalized spin-down state.</returns>
    public static SpinState CreateSpinHalfDown()
    {
        var amplitudes = new Complex[] { new Complex(0.0, 0.0), new Complex(1.0, 0.0) };
        return new SpinState(0.5, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a spin-1/2 state in the |+⟩_x (right) eigenstate along x-axis.
    /// </summary>
    /// <returns>A normalized spin-right state.</returns>
    public static SpinState CreateSpinHalfRight()
    {
        var inv_sqrt2 = 1.0 / Math.Sqrt(2.0);
        var amplitudes = new Complex[] { new Complex(inv_sqrt2, 0.0), new Complex(inv_sqrt2, 0.0) };
        return new SpinState(0.5, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a spin-1/2 state in the |−⟩_x (left) eigenstate along x-axis.
    /// </summary>
    /// <returns>A normalized spin-left state.</returns>
    public static SpinState CreateSpinHalfLeft()
    {
        var inv_sqrt2 = 1.0 / Math.Sqrt(2.0);
        var amplitudes = new Complex[] { new Complex(inv_sqrt2, 0.0), new Complex(-inv_sqrt2, 0.0) };
        return new SpinState(0.5, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a spin-1/2 state in the |+⟩_y (in) eigenstate along y-axis.
    /// </summary>
    /// <returns>A normalized spin-in state.</returns>
    public static SpinState CreateSpinHalfIn()
    {
        var inv_sqrt2 = 1.0 / Math.Sqrt(2.0);
        var amplitudes = new Complex[] { new Complex(inv_sqrt2, 0.0), new Complex(0.0, inv_sqrt2) };
        return new SpinState(0.5, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a spin-1/2 state in the |−⟩_y (out) eigenstate along y-axis.
    /// </summary>
    /// <returns>A normalized spin-out state.</returns>
    public static SpinState CreateSpinHalfOut()
    {
        var inv_sqrt2 = 1.0 / Math.Sqrt(2.0);
        var amplitudes = new Complex[] { new Complex(inv_sqrt2, 0.0), new Complex(0.0, -inv_sqrt2) };
        return new SpinState(0.5, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a spin-1 state in the specified magnetic quantum number eigenstate.
    /// </summary>
    /// <param name="magneticQuantumNumber">The magnetic quantum number (-1, 0, or +1).</param>
    /// <returns>A normalized spin-1 eigenstate.</returns>
    public static SpinState CreateSpinOneEigenstate(int magneticQuantumNumber)
    {
        if (magneticQuantumNumber < -1 || magneticQuantumNumber > 1)
            throw new ArgumentOutOfRangeException(nameof(magneticQuantumNumber), "Must be -1, 0, or +1 for spin-1");

        var amplitudes = new Complex[3];
        var index = 1 - magneticQuantumNumber; // Convert mj to array index: +1→0, 0→1, -1→2
        amplitudes[index] = new Complex(1.0, 0.0);

        return new SpinState(1.0, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a general spin state in the specified magnetic quantum number eigenstate.
    /// </summary>
    /// <param name="spinQuantumNumber">The spin quantum number.</param>
    /// <param name="magneticQuantumNumber">The magnetic quantum number.</param>
    /// <returns>A normalized spin eigenstate.</returns>
    public static SpinState CreateEigenstate(double spinQuantumNumber, double magneticQuantumNumber)
    {
        // Validate inputs
        if (spinQuantumNumber < 0 || Math.Abs(spinQuantumNumber * 2 - Math.Round(spinQuantumNumber * 2)) > 1e-10)
            throw new ArgumentException("Spin quantum number must be a non-negative half-integer", nameof(spinQuantumNumber));

        if (Math.Abs(magneticQuantumNumber) > spinQuantumNumber + 1e-10)
            throw new ArgumentException("Magnetic quantum number magnitude cannot exceed spin quantum number", nameof(magneticQuantumNumber));

        if (Math.Abs(magneticQuantumNumber * 2 - Math.Round(magneticQuantumNumber * 2)) > 1e-10)
            throw new ArgumentException("Magnetic quantum number must be a half-integer", nameof(magneticQuantumNumber));

        var dimension = (int)(2 * spinQuantumNumber + 1);
        var amplitudes = new Complex[dimension];

        // Convert mj to array index
        var index = (int)(spinQuantumNumber - magneticQuantumNumber);
        amplitudes[index] = new Complex(1.0, 0.0);

        return new SpinState(spinQuantumNumber, amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a coherent spin state pointing in the specified direction.
    /// </summary>
    /// <param name="spinQuantumNumber">The spin quantum number.</param>
    /// <param name="theta">The polar angle (0 to π).</param>
    /// <param name="phi">The azimuthal angle (0 to 2π).</param>
    /// <returns>A normalized coherent spin state.</returns>
    public static SpinState CreateCoherentState(double spinQuantumNumber, double theta, double phi)
    {
        if (spinQuantumNumber < 0 || Math.Abs(spinQuantumNumber * 2 - Math.Round(spinQuantumNumber * 2)) > 1e-10)
            throw new ArgumentException("Spin quantum number must be a non-negative half-integer", nameof(spinQuantumNumber));

        var thetaValidation = ParameterValidator.ValidateRange(theta, nameof(theta), 0.0, Math.PI);
        thetaValidation.ThrowIfInvalid();

        var phiValidation = ParameterValidator.ValidateRange(phi, nameof(phi), 0.0, 2.0 * Math.PI);
        phiValidation.ThrowIfInvalid();

        var dimension = (int)(2 * spinQuantumNumber + 1);
        var amplitudes = new Complex[dimension];

        // Calculate coherent state amplitudes using the formula:
        // |n,θ,φ⟩ = Σ_m √(C(2j,j-m)) * cos^(j-m)(θ/2) * sin^(j+m)(θ/2) * e^(i*m*φ) |j,m⟩
        var halfTheta = theta / 2.0;
        var cosHalfTheta = Math.Cos(halfTheta);
        var sinHalfTheta = Math.Sin(halfTheta);

        for (int i = 0; i < dimension; i++)
        {
            var mj = spinQuantumNumber - i;
            var binomCoeff = BinomialCoefficient((int)(2 * spinQuantumNumber), i);
            var amplitude = Math.Sqrt(binomCoeff) * Math.Pow(cosHalfTheta, spinQuantumNumber - mj) * Math.Pow(sinHalfTheta, spinQuantumNumber + mj);
            var phase = mj * phi;

            amplitudes[i] = new Complex(amplitude * Math.Cos(phase), amplitude * Math.Sin(phase));
        }

        return new SpinState(spinQuantumNumber, amplitudes, normalize: true);
    }

    #endregion

    #region Spin Operations

    /// <summary>
    /// Normalizes the spin state to have unit norm.
    /// </summary>
    /// <returns>This spin state (for method chaining).</returns>
    public SpinState Normalize()
    {
        lock (_stateLock)
        {
            var norm = CalculateNorm();
            if (norm < PrecisionHandling.MinimumAmplitudeMagnitude)
            {
                throw new InvalidOperationException("Cannot normalize spin state with zero or near-zero norm");
            }

            for (int i = 0; i < Dimension; i++)
            {
                _amplitudes[i] /= norm;
            }

            _isNormalized = true;
            Logger.Debug($"Normalized spin state", new { SpinQuantumNumber, Dimension, NewNorm = CalculateNorm() });
        }

        return this;
    }

    /// <summary>
    /// Creates a deep copy of this spin state.
    /// </summary>
    /// <returns>A new spin state with identical amplitudes and properties.</returns>
    public SpinState Clone()
    {
        lock (_stateLock)
        {
            return new SpinState(SpinQuantumNumber, _amplitudes, normalize: false);
        }
    }

    /// <summary>
    /// Calculates the overlap ⟨this|other⟩ with another spin state.
    /// </summary>
    /// <param name="other">The other spin state.</param>
    /// <returns>The complex overlap.</returns>
    public Complex Overlap(SpinState other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (Math.Abs(other.SpinQuantumNumber - SpinQuantumNumber) > 1e-10)
            throw new ArgumentException($"Spin quantum number mismatch: {SpinQuantumNumber} vs {other.SpinQuantumNumber}", nameof(other));

        lock (_stateLock)
        {
            var otherAmplitudes = other.Amplitudes;
            var overlap = Complex.Zero;

            for (int i = 0; i < Dimension; i++)
            {
                overlap += _amplitudes[i].Conjugate * otherAmplitudes[i];
            }

            return overlap;
        }
    }

    /// <summary>
    /// Applies a rotation around the z-axis by the specified angle.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>This spin state after rotation (for method chaining).</returns>
    public SpinState RotateZ(double angle)
    {
        lock (_stateLock)
        {
            var magneticNumbers = MagneticQuantumNumbers;

            for (int i = 0; i < Dimension; i++)
            {
                var phase = -magneticNumbers[i] * angle;
                var rotationFactor = Complex.FromPhase(phase);
                _amplitudes[i] *= rotationFactor;
            }

            Logger.Debug($"Applied z-rotation by {angle} radians", 
                new { SpinQuantumNumber, Angle = angle });
        }

        return this;
    }

    /// <summary>
    /// Applies a rotation around the y-axis by the specified angle.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>This spin state after rotation (for method chaining).</returns>
    public SpinState RotateY(double angle)
    {
        lock (_stateLock)
        {
            var oldAmplitudes = (Complex[])_amplitudes.Clone();
            var rotationMatrix = CalculateYRotationMatrix(angle);

            // Apply rotation matrix
            for (int i = 0; i < Dimension; i++)
            {
                _amplitudes[i] = Complex.Zero;
                for (int j = 0; j < Dimension; j++)
                {
                    _amplitudes[i] += rotationMatrix[i, j] * oldAmplitudes[j];
                }
            }

            Logger.Debug($"Applied y-rotation by {angle} radians", 
                new { SpinQuantumNumber, Angle = angle });
        }

        return this;
    }

    /// <summary>
    /// Applies a rotation around the x-axis by the specified angle.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>This spin state after rotation (for method chaining).</returns>
    public SpinState RotateX(double angle)
    {
        lock (_stateLock)
        {
            var oldAmplitudes = (Complex[])_amplitudes.Clone();
            var rotationMatrix = CalculateXRotationMatrix(angle);

            // Apply rotation matrix
            for (int i = 0; i < Dimension; i++)
            {
                _amplitudes[i] = Complex.Zero;
                for (int j = 0; j < Dimension; j++)
                {
                    _amplitudes[i] += rotationMatrix[i, j] * oldAmplitudes[j];
                }
            }

            Logger.Debug($"Applied x-rotation by {angle} radians", 
                new { SpinQuantumNumber, Angle = angle });
        }

        return this;
    }

    /// <summary>
    /// Calculates the expectation value of the z-component of angular momentum ⟨Jz⟩.
    /// </summary>
    /// <returns>The expectation value in units of ℏ.</returns>
    public double ExpectationValueJz()
    {
        lock (_stateLock)
        {
            var expectationValue = 0.0;
            var magneticNumbers = MagneticQuantumNumbers;

            for (int i = 0; i < Dimension; i++)
            {
                var probability = _amplitudes[i].MagnitudeSquared;
                expectationValue += magneticNumbers[i] * probability;
            }

            return expectationValue;
        }
    }

    /// <summary>
    /// Calculates the expectation value of the square of total angular momentum ⟨J²⟩.
    /// </summary>
    /// <returns>The expectation value in units of ℏ².</returns>
    public double ExpectationValueJSquared()
    {
        // For any spin state, ⟨J²⟩ = j(j+1)ℏ²
        return SpinQuantumNumber * (SpinQuantumNumber + 1);
    }

    /// <summary>
    /// Calculates the probability of measuring the specified magnetic quantum number.
    /// </summary>
    /// <param name="magneticQuantumNumber">The magnetic quantum number to measure.</param>
    /// <returns>The probability [0, 1].</returns>
    public double MeasurementProbability(double magneticQuantumNumber)
    {
        if (Math.Abs(magneticQuantumNumber) > SpinQuantumNumber + 1e-10)
            throw new ArgumentException("Magnetic quantum number magnitude cannot exceed spin quantum number", nameof(magneticQuantumNumber));

        if (Math.Abs(magneticQuantumNumber * 2 - Math.Round(magneticQuantumNumber * 2)) > 1e-10)
            throw new ArgumentException("Magnetic quantum number must be a half-integer", nameof(magneticQuantumNumber));

        lock (_stateLock)
        {
            var index = (int)(SpinQuantumNumber - magneticQuantumNumber);
            if (index < 0 || index >= Dimension)
                return 0.0;

            return _amplitudes[index].MagnitudeSquared;
        }
    }

    #endregion

    #region Static Operations

    /// <summary>
    /// Adds two spin states (only if they have the same spin quantum number).
    /// </summary>
    /// <param name="state1">The first spin state.</param>
    /// <param name="state2">The second spin state.</param>
    /// <returns>A new spin state representing state1 + state2.</returns>
    public static SpinState operator +(SpinState state1, SpinState state2)
    {
        if (state1 == null || state2 == null)
            throw new ArgumentNullException();

        if (Math.Abs(state1.SpinQuantumNumber - state2.SpinQuantumNumber) > 1e-10)
            throw new ArgumentException("Cannot add spin states with different spin quantum numbers");

        var amplitudes1 = state1.Amplitudes;
        var amplitudes2 = state2.Amplitudes;
        var resultAmplitudes = new Complex[state1.Dimension];

        for (int i = 0; i < state1.Dimension; i++)
        {
            resultAmplitudes[i] = amplitudes1[i] + amplitudes2[i];
        }

        return new SpinState(state1.SpinQuantumNumber, resultAmplitudes, normalize: false);
    }

    /// <summary>
    /// Multiplies a spin state by a complex scalar.
    /// </summary>
    /// <param name="scalar">The complex scalar.</param>
    /// <param name="spinState">The spin state.</param>
    /// <returns>A new spin state representing scalar * spinState.</returns>
    public static SpinState operator *(Complex scalar, SpinState spinState)
    {
        if (spinState == null)
            throw new ArgumentNullException(nameof(spinState));

        var amplitudes = spinState.Amplitudes;
        var resultAmplitudes = amplitudes.Select(amp => scalar * amp).ToArray();

        return new SpinState(spinState.SpinQuantumNumber, resultAmplitudes, normalize: false);
    }

    /// <summary>
    /// Multiplies a spin state by a real scalar.
    /// </summary>
    /// <param name="scalar">The real scalar.</param>
    /// <param name="spinState">The spin state.</param>
    /// <returns>A new spin state representing scalar * spinState.</returns>
    public static SpinState operator *(double scalar, SpinState spinState)
    {
        return new Complex(scalar, 0.0) * spinState;
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether two spin states are equal within numerical tolerance.
    /// </summary>
    public bool Equals(SpinState? other)
    {
        if (other == null || Math.Abs(other.SpinQuantumNumber - SpinQuantumNumber) > 1e-10)
            return false;

        var amplitudes1 = Amplitudes;
        var amplitudes2 = other.Amplitudes;

        for (int i = 0; i < Dimension; i++)
        {
            if (!amplitudes1[i].Equals(amplitudes2[i], PrecisionHandling.QuantumCalculationTolerance))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether this spin state equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is SpinState other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this spin state.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(SpinQuantumNumber);

        var amplitudes = Amplitudes;
        foreach (var amplitude in amplitudes)
        {
            hash.Add(amplitude.GetHashCode());
        }

        return hash.ToHashCode();
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the spin state.
    /// </summary>
    public override string ToString()
    {
        var amplitudes = Amplitudes;
        var magneticNumbers = MagneticQuantumNumbers;
        var terms = new List<string>();

        for (int i = 0; i < Dimension; i++)
        {
            var amp = amplitudes[i];
            if (amp.Magnitude > PrecisionHandling.QuantumCalculationTolerance)
            {
                var ampStr = amp.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                var mjStr = magneticNumbers[i] % 1 == 0 ? magneticNumbers[i].ToString("F0") : magneticNumbers[i].ToString("F1");
                terms.Add($"({ampStr})|j={SpinQuantumNumber},mj={mjStr}⟩");
            }
        }

        var stateStr = string.Join(" + ", terms);
        return string.IsNullOrEmpty(stateStr) ? $"|j={SpinQuantumNumber},mj={SpinQuantumNumber}⟩" : stateStr;
    }

    #endregion

    #region Private Methods

    private double CalculateNorm()
    {
        return Math.Sqrt(_amplitudes.Sum(amp => amp.MagnitudeSquared));
    }

    private Complex[,] CalculateYRotationMatrix(double angle)
    {
        // For general spin j, the y-rotation matrix elements are:
        // D^j_{m'm}(0,β,0) = d^j_{m'm}(β)
        // where d^j_{m'm}(β) are the Wigner small d-matrices
        
        var matrix = new Complex[Dimension, Dimension];
        var halfAngle = angle / 2.0;
        var cosHalf = Math.Cos(halfAngle);
        var sinHalf = Math.Sin(halfAngle);

        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                var mp = SpinQuantumNumber - i; // m'
                var m = SpinQuantumNumber - j;   // m
                
                matrix[i, j] = CalculateWignerSmallD(SpinQuantumNumber, mp, m, halfAngle, cosHalf, sinHalf);
            }
        }

        return matrix;
    }

    private Complex[,] CalculateXRotationMatrix(double angle)
    {
        // X-rotation can be expressed in terms of Y-rotation:
        // R_x(θ) = R_z(π/2) R_y(θ) R_z(-π/2)
        
        var matrix = new Complex[Dimension, Dimension];
        var halfAngle = angle / 2.0;

        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                var mp = SpinQuantumNumber - i; // m'
                var m = SpinQuantumNumber - j;   // m
                
                // Apply phase factors for the z-rotations and use y-rotation formula
                var phase1 = Complex.FromPhase(-mp * Math.PI / 2);
                var phase2 = Complex.FromPhase(m * Math.PI / 2);
                var dElement = CalculateWignerSmallD(SpinQuantumNumber, mp, m, halfAngle, Math.Cos(halfAngle), Math.Sin(halfAngle));
                
                matrix[i, j] = phase1 * dElement * phase2;
            }
        }

        return matrix;
    }

    private static Complex CalculateWignerSmallD(double j, double mp, double m, double halfAngle, double cosHalf, double sinHalf)
    {
        // Calculate Wigner small d-matrix elements using the formula:
        // d^j_{m',m}(β) = Σ_k (-1)^k * √[(j+m')!(j-m')!(j+m)!(j-m)!] / [k!(j+m'-k)!(j-m-k)!(k+m-m')!]
        //                 * cos^(2j+m-m'-2k)(β/2) * sin^(2k+m'-m)(β/2)

        if (Math.Abs(mp) > j + 1e-10 || Math.Abs(m) > j + 1e-10)
            return Complex.Zero;

        var result = 0.0;
        var kMin = Math.Max(0, Math.Max(m - mp, 0));
        var kMax = Math.Min(j + mp, Math.Min(j - m, j + j)); // j + j = 2j

        for (var k = kMin; k <= kMax; k++)
        {
            var sign = Math.Pow(-1, k);
            var cosExp = 2 * j + m - mp - 2 * k;
            var sinExp = 2 * k + mp - m;

            if (cosExp < 0 || sinExp < 0) continue;

            var cosTerm = Math.Pow(cosHalf, cosExp);
            var sinTerm = Math.Pow(sinHalf, sinExp);

            var factorial1 = LogFactorial(j + mp) + LogFactorial(j - mp) + LogFactorial(j + m) + LogFactorial(j - m);
            var factorial2 = LogFactorial(k) + LogFactorial(j + mp - k) + LogFactorial(j - m - k) + LogFactorial(k + m - mp);

            var coefficient = Math.Exp(0.5 * factorial1 - factorial2);
            result += sign * coefficient * cosTerm * sinTerm;
        }

        return new Complex(result, 0.0);
    }

    private static double LogFactorial(double n)
    {
        if (n < 0) return double.NegativeInfinity;
        if (n == 0 || n == 1) return 0.0;

        // Use Stirling's approximation for large n
        if (n > 10)
        {
            return n * Math.Log(n) - n + 0.5 * Math.Log(2 * Math.PI * n);
        }

        // Direct calculation for small n
        double result = 0.0;
        for (int i = 2; i <= n; i++)
        {
            result += Math.Log(i);
        }
        return result;
    }

    private static double BinomialCoefficient(int n, int k)
    {
        if (k > n || k < 0) return 0.0;
        if (k == 0 || k == n) return 1.0;

        // Use the multiplicative formula to avoid large factorials
        double result = 1.0;
        for (int i = 0; i < k; i++)
        {
            result = result * (n - i) / (i + 1);
        }
        return result;
    }

    #endregion
}
