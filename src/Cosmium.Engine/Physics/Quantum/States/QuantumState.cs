using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;

namespace Cosmium.Engine.Physics.Quantum.States;

/// <summary>
/// Represents a general quantum state with operations for quantum state manipulation.
/// Provides functionality for both pure and mixed quantum states with proper normalization and validation.
/// </summary>
public class QuantumState : IEquatable<QuantumState>
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _stateLock = new();
    private Complex[] _amplitudes;
    private bool _isNormalized;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the dimension of the Hilbert space for this quantum state.
    /// </summary>
    public int Dimension { get; }

    /// <summary>
    /// Gets the state vector amplitudes. Returns a copy to maintain immutability.
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
    /// Gets whether this quantum state is normalized (|ψ|² = 1).
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
    /// Gets the norm of the quantum state (√⟨ψ|ψ⟩).
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
    /// Gets the probability density |ψ|² for each basis state.
    /// </summary>
    public double[] ProbabilityDensity
    {
        get
        {
            lock (_stateLock)
            {
                return _amplitudes.Select(amp => amp.Real * amp.Real + amp.Imaginary * amp.Imaginary).ToArray();
            }
        }
    }

    /// <summary>
    /// Gets whether this represents a pure quantum state (as opposed to a mixed state).
    /// </summary>
    public virtual bool IsPureState => true;

    /// <summary>
    /// Gets the creation time of this quantum state.
    /// </summary>
    public DateTime CreationTime { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new quantum state with the specified amplitudes.
    /// </summary>
    /// <param name="amplitudes">The amplitude vector for the quantum state.</param>
    /// <param name="normalize">Whether to automatically normalize the state.</param>
    public QuantumState(Complex[] amplitudes, bool normalize = true)
    {
        if (amplitudes == null)
            throw new ArgumentNullException(nameof(amplitudes));
        
        // Validate that amplitudes array is not null and has finite values
        if (amplitudes == null)
            throw new ArgumentNullException(nameof(amplitudes));
        
        if (amplitudes.Length < 2)
            throw new ArgumentException("Quantum state must have at least 2 dimensions", nameof(amplitudes));
        
        for (int i = 0; i < amplitudes.Length; i++)
        {
            if (!double.IsFinite(amplitudes[i].Real) || !double.IsFinite(amplitudes[i].Imaginary))
                throw new ArgumentException($"Amplitude at index {i} contains non-finite values", nameof(amplitudes));
        }

        Dimension = amplitudes.Length;
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

        Logger.Debug($"Created quantum state with dimension {Dimension}", 
            new { Dimension, IsNormalized, Norm = CalculateNorm() });
    }

    /// <summary>
    /// Initializes a new quantum state in the ground state (|0⟩).
    /// </summary>
    /// <param name="dimension">The dimension of the Hilbert space.</param>
    public QuantumState(int dimension) : this(CreateGroundState(dimension))
    {
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a quantum state in the computational basis |n⟩.
    /// </summary>
    /// <param name="dimension">The dimension of the Hilbert space.</param>
    /// <param name="basisIndex">The index of the basis state (0 ≤ n < dimension).</param>
    /// <returns>A quantum state representing |n⟩.</returns>
    public static QuantumState CreateBasisState(int dimension, int basisIndex)
    {
        var dimensionValidation = ParameterValidator.ValidateIntegerRange(dimension, nameof(dimension), 2, 1 << 20);
        dimensionValidation.ThrowIfInvalid();

        var indexValidation = ParameterValidator.ValidateIntegerRange(basisIndex, nameof(basisIndex), 0, dimension - 1);
        indexValidation.ThrowIfInvalid();

        var amplitudes = new Complex[dimension];
        amplitudes[basisIndex] = new Complex(1.0, 0.0);

        return new QuantumState(amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a quantum state from a real-valued probability distribution.
    /// </summary>
    /// <param name="probabilities">The probability distribution for each basis state.</param>
    /// <returns>A quantum state with real amplitudes proportional to √probabilities.</returns>
    public static QuantumState FromProbabilities(double[] probabilities)
    {
        if (probabilities == null)
            throw new ArgumentNullException(nameof(probabilities));

        var probabilityValidation = ParameterValidator.ValidateFiniteDoubleArray(probabilities, nameof(probabilities));
        probabilityValidation.ThrowIfInvalid();

        if (probabilities.Any(p => p < 0))
            throw new ArgumentException("Probabilities must be non-negative", nameof(probabilities));

        var amplitudes = probabilities.Select(p => new Complex(Math.Sqrt(p), 0.0)).ToArray();
        return new QuantumState(amplitudes, normalize: true);
    }

    /// <summary>
    /// Creates a uniform superposition state (equal amplitudes for all basis states).
    /// </summary>
    /// <param name="dimension">The dimension of the Hilbert space.</param>
    /// <returns>A quantum state representing (|0⟩ + |1⟩ + ... + |n-1⟩)/√n.</returns>
    public static QuantumState CreateUniformSuperposition(int dimension)
    {
        var dimensionValidation = ParameterValidator.ValidateIntegerRange(dimension, nameof(dimension), 2, 1 << 20);
        dimensionValidation.ThrowIfInvalid();

        var amplitude = new Complex(1.0 / Math.Sqrt(dimension), 0.0);
        var amplitudes = Enumerable.Repeat(amplitude, dimension).ToArray();

        return new QuantumState(amplitudes, normalize: false);
    }

    /// <summary>
    /// Creates a random quantum state with uniformly distributed amplitudes.
    /// </summary>
    /// <param name="dimension">The dimension of the Hilbert space.</param>
    /// <param name="random">The random number generator to use.</param>
    /// <returns>A normalized random quantum state.</returns>
    public static QuantumState CreateRandom(int dimension, Random? random = null)
    {
        var dimensionValidation = ParameterValidator.ValidateIntegerRange(dimension, nameof(dimension), 2, 1 << 20);
        dimensionValidation.ThrowIfInvalid();

        random ??= new Random();

        var amplitudes = new Complex[dimension];
        for (int i = 0; i < dimension; i++)
        {
            // Generate random complex amplitudes with Gaussian distribution
            var real = random.NextGaussian();
            var imaginary = random.NextGaussian();
            amplitudes[i] = new Complex(real, imaginary);
        }

        return new QuantumState(amplitudes, normalize: true);
    }

    #endregion

    #region State Operations

    /// <summary>
    /// Normalizes the quantum state to have unit norm.
    /// </summary>
    /// <returns>This quantum state (for method chaining).</returns>
    public QuantumState Normalize()
    {
        lock (_stateLock)
        {
            var norm = CalculateNorm();
            if (norm < PrecisionHandling.MinimumAmplitudeMagnitude)
            {
                throw new InvalidOperationException("Cannot normalize quantum state with zero or near-zero norm");
            }

            for (int i = 0; i < Dimension; i++)
            {
                _amplitudes[i] /= norm;
            }

            _isNormalized = true;
            Logger.Debug($"Normalized quantum state", new { Dimension, NewNorm = CalculateNorm() });
        }

        return this;
    }

    /// <summary>
    /// Creates a deep copy of this quantum state.
    /// </summary>
    /// <returns>A new quantum state with identical amplitudes.</returns>
    public QuantumState Clone()
    {
        lock (_stateLock)
        {
            return new QuantumState(_amplitudes, normalize: false);
        }
    }

    /// <summary>
    /// Calculates the inner product ⟨this|other⟩ with another quantum state.
    /// </summary>
    /// <param name="other">The other quantum state.</param>
    /// <returns>The complex inner product.</returns>
    public Complex InnerProduct(QuantumState other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));
        
        if (other.Dimension != Dimension)
            throw new ArgumentException($"Dimension mismatch: {Dimension} vs {other.Dimension}", nameof(other));

        lock (_stateLock)
        {
            var otherAmplitudes = other.Amplitudes;
            var innerProduct = Complex.Zero;

            for (int i = 0; i < Dimension; i++)
            {
                innerProduct += _amplitudes[i].Conjugate * otherAmplitudes[i];
            }

            return innerProduct;
        }
    }

    /// <summary>
    /// Calculates the overlap probability |⟨this|other⟩|² between quantum states.
    /// </summary>
    /// <param name="other">The other quantum state.</param>
    /// <returns>The overlap probability [0, 1].</returns>
    public double OverlapProbability(QuantumState other)
    {
        var innerProduct = InnerProduct(other);
        return innerProduct.Real * innerProduct.Real + innerProduct.Imaginary * innerProduct.Imaginary;
    }

    /// <summary>
    /// Calculates the fidelity between this state and another quantum state.
    /// For pure states, fidelity equals the overlap probability.
    /// </summary>
    /// <param name="other">The other quantum state.</param>
    /// <returns>The fidelity [0, 1].</returns>
    public virtual double Fidelity(QuantumState other)
    {
        return OverlapProbability(other);
    }

    /// <summary>
    /// Evolves the quantum state using a unitary operator.
    /// </summary>
    /// <param name="unitaryOperator">The unitary matrix to apply.</param>
    /// <returns>This quantum state after evolution (for method chaining).</returns>
    public QuantumState ApplyUnitary(Complex[,] unitaryOperator)
    {
        // Validate the unitary operator matrix
        if (unitaryOperator == null)
            throw new ArgumentNullException(nameof(unitaryOperator));
        
        int rows = unitaryOperator.GetLength(0);
        int cols = unitaryOperator.GetLength(1);
        
        if (rows != cols)
            throw new ArgumentException("Unitary operator must be a square matrix", nameof(unitaryOperator));
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (!double.IsFinite(unitaryOperator[i, j].Real) || !double.IsFinite(unitaryOperator[i, j].Imaginary))
                    throw new ArgumentException($"Matrix element at [{i},{j}] contains non-finite values", nameof(unitaryOperator));
            }
        }

        if (unitaryOperator.GetLength(0) != Dimension || unitaryOperator.GetLength(1) != Dimension)
        {
            throw new ArgumentException($"Unitary operator dimension mismatch: expected {Dimension}x{Dimension}", 
                nameof(unitaryOperator));
        }

        lock (_stateLock)
        {
            var newAmplitudes = new Complex[Dimension];

            for (int i = 0; i < Dimension; i++)
            {
                newAmplitudes[i] = Complex.Zero;
                for (int j = 0; j < Dimension; j++)
                {
                    newAmplitudes[i] += unitaryOperator[i, j] * _amplitudes[j];
                }
            }

            _amplitudes = newAmplitudes;
            _isNormalized = IsUnitary(unitaryOperator) && _isNormalized;

            Logger.Debug($"Applied unitary operation to quantum state", 
                new { Dimension, NewNorm = CalculateNorm() });
        }

        return this;
    }

    /// <summary>
    /// Calculates the expectation value of an observable for this quantum state.
    /// </summary>
    /// <param name="observable">The Hermitian matrix representing the observable.</param>
    /// <returns>The expectation value ⟨ψ|Ô|ψ⟩.</returns>
    public double ExpectationValue(Complex[,] observable)
    {
        // Validate the observable matrix
        if (observable == null)
            throw new ArgumentNullException(nameof(observable));
        
        int rows = observable.GetLength(0);
        int cols = observable.GetLength(1);
        
        if (rows != cols)
            throw new ArgumentException("Observable must be a square matrix", nameof(observable));
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (!double.IsFinite(observable[i, j].Real) || !double.IsFinite(observable[i, j].Imaginary))
                    throw new ArgumentException($"Matrix element at [{i},{j}] contains non-finite values", nameof(observable));
            }
        }

        if (observable.GetLength(0) != Dimension || observable.GetLength(1) != Dimension)
        {
            throw new ArgumentException($"Observable dimension mismatch: expected {Dimension}x{Dimension}", 
                nameof(observable));
        }

        lock (_stateLock)
        {
            var expectationValue = Complex.Zero;

            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    expectationValue += _amplitudes[i].Conjugate * observable[i, j] * _amplitudes[j];
                }
            }

            // Expectation value should be real for Hermitian operators
            return expectationValue.Real;
        }
    }

    #endregion

    #region Static Operations

    /// <summary>
    /// Adds two quantum states (linear superposition).
    /// </summary>
    /// <param name="state1">The first quantum state.</param>
    /// <param name="state2">The second quantum state.</param>
    /// <returns>A new quantum state representing state1 + state2.</returns>
    public static QuantumState operator +(QuantumState state1, QuantumState state2)
    {
        if (state1 == null || state2 == null)
            throw new ArgumentNullException();

        if (state1.Dimension != state2.Dimension)
            throw new ArgumentException("Cannot add quantum states with different dimensions");

        var amplitudes1 = state1.Amplitudes;
        var amplitudes2 = state2.Amplitudes;
        var resultAmplitudes = new Complex[state1.Dimension];

        for (int i = 0; i < state1.Dimension; i++)
        {
            resultAmplitudes[i] = amplitudes1[i] + amplitudes2[i];
        }

        return new QuantumState(resultAmplitudes, normalize: false);
    }

    /// <summary>
    /// Subtracts two quantum states.
    /// </summary>
    /// <param name="state1">The first quantum state.</param>
    /// <param name="state2">The second quantum state.</param>
    /// <returns>A new quantum state representing state1 - state2.</returns>
    public static QuantumState operator -(QuantumState state1, QuantumState state2)
    {
        if (state1 == null || state2 == null)
            throw new ArgumentNullException();

        if (state1.Dimension != state2.Dimension)
            throw new ArgumentException("Cannot subtract quantum states with different dimensions");

        var amplitudes1 = state1.Amplitudes;
        var amplitudes2 = state2.Amplitudes;
        var resultAmplitudes = new Complex[state1.Dimension];

        for (int i = 0; i < state1.Dimension; i++)
        {
            resultAmplitudes[i] = amplitudes1[i] - amplitudes2[i];
        }

        return new QuantumState(resultAmplitudes, normalize: false);
    }

    /// <summary>
    /// Multiplies a quantum state by a complex scalar.
    /// </summary>
    /// <param name="scalar">The complex scalar.</param>
    /// <param name="state">The quantum state.</param>
    /// <returns>A new quantum state representing scalar * state.</returns>
    public static QuantumState operator *(Complex scalar, QuantumState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        var amplitudes = state.Amplitudes;
        var resultAmplitudes = amplitudes.Select(amp => scalar * amp).ToArray();

        return new QuantumState(resultAmplitudes, normalize: false);
    }

    /// <summary>
    /// Multiplies a quantum state by a real scalar.
    /// </summary>
    /// <param name="scalar">The real scalar.</param>
    /// <param name="state">The quantum state.</param>
    /// <returns>A new quantum state representing scalar * state.</returns>
    public static QuantumState operator *(double scalar, QuantumState state)
    {
        return new Complex(scalar, 0.0) * state;
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether two quantum states are equal within numerical tolerance.
    /// </summary>
    public bool Equals(QuantumState? other)
    {
        if (other == null || other.Dimension != Dimension)
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
    /// Determines whether this quantum state equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is QuantumState other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this quantum state.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Dimension);
        
        var amplitudes = Amplitudes;
        foreach (var amplitude in amplitudes)
        {
            hash.Add(amplitude.GetHashCode());
        }
        
        return hash.ToHashCode();
    }

    /// <summary>
    /// Equality operator for quantum states.
    /// </summary>
    public static bool operator ==(QuantumState? left, QuantumState? right)
    {
        return EqualityComparer<QuantumState>.Default.Equals(left, right);
    }

    /// <summary>
    /// Inequality operator for quantum states.
    /// </summary>
    public static bool operator !=(QuantumState? left, QuantumState? right)
    {
        return !(left == right);
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the quantum state.
    /// </summary>
    public override string ToString()
    {
        var amplitudes = Amplitudes;
        var terms = new List<string>();

        for (int i = 0; i < Math.Min(Dimension, 10); i++) // Limit display for large states
        {
            var amp = amplitudes[i];
            if (amp.Magnitude > PrecisionHandling.QuantumCalculationTolerance)
            {
                var ampStr = amp.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                terms.Add($"({ampStr})|{i}⟩");
            }
        }

        if (Dimension > 10)
        {
            terms.Add("...");
        }

        var stateStr = string.Join(" + ", terms);
        return string.IsNullOrEmpty(stateStr) ? "|0⟩" : stateStr;
    }

    #endregion

    #region Private Methods

    private static Complex[] CreateGroundState(int dimension)
    {
        var dimensionValidation = ParameterValidator.ValidateIntegerRange(dimension, nameof(dimension), 2, 1 << 20);
        dimensionValidation.ThrowIfInvalid();

        var amplitudes = new Complex[dimension];
        amplitudes[0] = new Complex(1.0, 0.0);
        return amplitudes;
    }

    private double CalculateNorm()
    {
        return Math.Sqrt(_amplitudes.Sum(amp => amp.Real * amp.Real + amp.Imaginary * amp.Imaginary));
    }

    private static bool IsUnitary(Complex[,] matrix)
    {
        // Simplified check - in a full implementation, we would verify U†U = I
        // For now, just check if the matrix is square and finite
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        if (rows != cols)
            return false;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (!double.IsFinite(matrix[i, j].Real) || !double.IsFinite(matrix[i, j].Imaginary))
                    return false;
            }
        }

        return true;
    }

    #endregion
}

/// <summary>
/// Extension methods for Random class to support Gaussian distribution.
/// </summary>
internal static class RandomExtensions
{
    private static double? _spare;

    /// <summary>
    /// Generates a random number from a standard normal distribution (μ=0, σ=1).
    /// Uses the Box-Muller transform.
    /// </summary>
    public static double NextGaussian(this Random random)
    {
        if (_spare.HasValue)
        {
            var value = _spare.Value;
            _spare = null;
            return value;
        }

        double u = random.NextDouble();
        double v = random.NextDouble();
        
        double magnitude = Math.Sqrt(-2.0 * Math.Log(u));
        double angle = 2.0 * Math.PI * v;
        
        _spare = magnitude * Math.Sin(angle);
        return magnitude * Math.Cos(angle);
    }
}
