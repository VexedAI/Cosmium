using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;

namespace Cosmium.Engine.Physics.Quantum.States;

/// <summary>
/// Represents a quantum superposition state with operations for coherent combinations of basis states.
/// Provides functionality for creating and manipulating superposition states with phase relationships.
/// </summary>
public class Superposition : IEquatable<Superposition>
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _stateLock = new();
    private readonly Dictionary<string, Complex> _components;
    private bool _isNormalized;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of components in the superposition.
    /// </summary>
    public int ComponentCount => _components.Count;

    /// <summary>
    /// Gets the labels of all basis states in the superposition.
    /// </summary>
    public IReadOnlyList<string> BasisStateLabels
    {
        get
        {
            lock (_stateLock)
            {
                return _components.Keys.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets the coefficients for all basis states. Returns a copy to maintain immutability.
    /// </summary>
    public IReadOnlyDictionary<string, Complex> Coefficients
    {
        get
        {
            lock (_stateLock)
            {
                return new Dictionary<string, Complex>(_components);
            }
        }
    }

    /// <summary>
    /// Gets whether this superposition state is normalized.
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
    /// Gets the norm of the superposition state.
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
    /// Gets the probability distribution for each basis state.
    /// </summary>
    public IReadOnlyDictionary<string, double> ProbabilityDistribution
    {
        get
        {
            lock (_stateLock)
            {
                return _components.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.MagnitudeSquared
                );
            }
        }
    }

    /// <summary>
    /// Gets the creation time of this superposition state.
    /// </summary>
    public DateTime CreationTime { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new superposition state with the specified components.
    /// </summary>
    /// <param name="components">The dictionary of basis state labels and their complex coefficients.</param>
    /// <param name="normalize">Whether to automatically normalize the state.</param>
    public Superposition(Dictionary<string, Complex> components, bool normalize = true)
    {
        if (components == null)
            throw new ArgumentNullException(nameof(components));

        if (components.Count == 0)
            throw new ArgumentException("Superposition must have at least one component", nameof(components));

        // Validate that all coefficients are finite
        foreach (var kvp in components)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
                throw new ArgumentException("Basis state labels cannot be null or whitespace", nameof(components));

            if (!double.IsFinite(kvp.Value.Real) || !double.IsFinite(kvp.Value.Imaginary))
                throw new ArgumentException($"Coefficient for state '{kvp.Key}' contains non-finite values", nameof(components));
        }

        _components = new Dictionary<string, Complex>(components);
        CreationTime = DateTime.UtcNow;

        if (normalize)
        {
            Normalize();
        }
        else
        {
            _isNormalized = Math.Abs(CalculateNorm() - 1.0) < PrecisionHandling.ProbabilityNormalizationTolerance;
        }

        Logger.Debug($"Created superposition state with {ComponentCount} components",
            new { ComponentCount, IsNormalized, Norm = CalculateNorm() });
    }

    /// <summary>
    /// Initializes a new superposition state with equal coefficients for all basis states.
    /// </summary>
    /// <param name="basisStateLabels">The labels of the basis states.</param>
    /// <param name="normalize">Whether to automatically normalize the state.</param>
    public Superposition(IEnumerable<string> basisStateLabels, bool normalize = true)
    {
        if (basisStateLabels == null)
            throw new ArgumentNullException(nameof(basisStateLabels));

        var labels = basisStateLabels.ToList();
        if (labels.Count == 0)
            throw new ArgumentException("Must provide at least one basis state label", nameof(basisStateLabels));

        if (labels.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Basis state labels cannot be null or whitespace", nameof(basisStateLabels));

        var equalCoefficient = new Complex(1.0, 0.0);
        _components = labels.ToDictionary(label => label, _ => equalCoefficient);
        CreationTime = DateTime.UtcNow;

        if (normalize)
        {
            Normalize();
        }
        else
        {
            _isNormalized = Math.Abs(CalculateNorm() - 1.0) < PrecisionHandling.ProbabilityNormalizationTolerance;
        }

        Logger.Debug($"Created equal superposition state with {ComponentCount} components",
            new { ComponentCount, IsNormalized, Norm = CalculateNorm() });
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a superposition state from two basis states with specified coefficients.
    /// </summary>
    /// <param name="state1Label">The label of the first basis state.</param>
    /// <param name="coefficient1">The coefficient for the first state.</param>
    /// <param name="state2Label">The label of the second basis state.</param>
    /// <param name="coefficient2">The coefficient for the second state.</param>
    /// <returns>A normalized superposition state.</returns>
    public static Superposition CreateTwoState(string state1Label, Complex coefficient1, 
        string state2Label, Complex coefficient2)
    {
        if (string.IsNullOrWhiteSpace(state1Label))
            throw new ArgumentException("State label cannot be null or whitespace", nameof(state1Label));
        
        if (string.IsNullOrWhiteSpace(state2Label))
            throw new ArgumentException("State label cannot be null or whitespace", nameof(state2Label));

        if (state1Label == state2Label)
            throw new ArgumentException("State labels must be different", nameof(state2Label));

        var components = new Dictionary<string, Complex>
        {
            { state1Label, coefficient1 },
            { state2Label, coefficient2 }
        };

        return new Superposition(components, normalize: true);
    }

    /// <summary>
    /// Creates a balanced (equal amplitude) superposition of two basis states.
    /// </summary>
    /// <param name="state1Label">The label of the first basis state.</param>
    /// <param name="state2Label">The label of the second basis state.</param>
    /// <param name="relativePhase">The relative phase between the states (in radians).</param>
    /// <returns>A normalized balanced superposition state.</returns>
    public static Superposition CreateBalanced(string state1Label, string state2Label, double relativePhase = 0.0)
    {
        var coefficient1 = new Complex(1.0, 0.0);
        var coefficient2 = Complex.FromPhase(relativePhase);

        return CreateTwoState(state1Label, coefficient1, state2Label, coefficient2);
    }

    /// <summary>
    /// Creates a superposition state with weighted coefficients.
    /// </summary>
    /// <param name="weightedStates">Dictionary of basis state labels and their real-valued weights.</param>
    /// <param name="globalPhase">Global phase to apply to all coefficients.</param>
    /// <returns>A normalized weighted superposition state.</returns>
    public static Superposition CreateWeighted(Dictionary<string, double> weightedStates, double globalPhase = 0.0)
    {
        if (weightedStates == null)
            throw new ArgumentNullException(nameof(weightedStates));

        if (weightedStates.Count == 0)
            throw new ArgumentException("Must provide at least one weighted state", nameof(weightedStates));

        if (weightedStates.Values.Any(w => w < 0))
            throw new ArgumentException("Weights must be non-negative", nameof(weightedStates));

        var globalPhaseFactor = Complex.FromPhase(globalPhase);
        var components = weightedStates.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Sqrt(kvp.Value) * globalPhaseFactor
        );

        return new Superposition(components, normalize: true);
    }

    /// <summary>
    /// Creates a coherent superposition with specified phases.
    /// </summary>
    /// <param name="statePhases">Dictionary of basis state labels and their phases (in radians).</param>
    /// <returns>A normalized coherent superposition state.</returns>
    public static Superposition CreateCoherent(Dictionary<string, double> statePhases)
    {
        if (statePhases == null)
            throw new ArgumentNullException(nameof(statePhases));

        if (statePhases.Count == 0)
            throw new ArgumentException("Must provide at least one state with phase", nameof(statePhases));

        var components = statePhases.ToDictionary(
            kvp => kvp.Key,
            kvp => Complex.FromPhase(kvp.Value)
        );

        return new Superposition(components, normalize: true);
    }

    /// <summary>
    /// Creates a superposition state representing a quantum Fourier transform basis.
    /// </summary>
    /// <param name="dimension">The dimension of the Hilbert space.</param>
    /// <param name="fourierIndex">The index k in the Fourier basis (0 ≤ k < dimension).</param>
    /// <returns>A normalized Fourier basis state.</returns>
    public static Superposition CreateFourierBasis(int dimension, int fourierIndex)
    {
        var dimensionValidation = ParameterValidator.ValidateIntegerRange(dimension, nameof(dimension), 2, 1000);
        dimensionValidation.ThrowIfInvalid();

        var indexValidation = ParameterValidator.ValidateIntegerRange(fourierIndex, nameof(fourierIndex), 0, dimension - 1);
        indexValidation.ThrowIfInvalid();

        var components = new Dictionary<string, Complex>();

        for (int j = 0; j < dimension; j++)
        {
            var phase = 2.0 * Math.PI * fourierIndex * j / dimension;
            var coefficient = Complex.FromPhase(phase);
            components[$"|{j}⟩"] = coefficient;
        }

        return new Superposition(components, normalize: true);
    }

    #endregion

    #region Superposition Operations

    /// <summary>
    /// Normalizes the superposition state to have unit norm.
    /// </summary>
    /// <returns>This superposition state (for method chaining).</returns>
    public Superposition Normalize()
    {
        lock (_stateLock)
        {
            var norm = CalculateNorm();
            if (norm < PrecisionHandling.MinimumAmplitudeMagnitude)
            {
                throw new InvalidOperationException("Cannot normalize superposition state with zero or near-zero norm");
            }

            var normalizedComponents = _components.Keys.ToList();
            foreach (var key in normalizedComponents)
            {
                _components[key] /= norm;
            }

            _isNormalized = true;
            Logger.Debug($"Normalized superposition state", new { ComponentCount, NewNorm = CalculateNorm() });
        }

        return this;
    }

    /// <summary>
    /// Creates a deep copy of this superposition state.
    /// </summary>
    /// <returns>A new superposition state with identical components and properties.</returns>
    public Superposition Clone()
    {
        lock (_stateLock)
        {
            return new Superposition(_components, normalize: false);
        }
    }

    /// <summary>
    /// Adds a new basis state component to the superposition or updates an existing one.
    /// </summary>
    /// <param name="stateLabel">The label of the basis state.</param>
    /// <param name="coefficient">The complex coefficient for the state.</param>
    /// <returns>This superposition state (for method chaining).</returns>
    public Superposition AddComponent(string stateLabel, Complex coefficient)
    {
        if (string.IsNullOrWhiteSpace(stateLabel))
            throw new ArgumentException("State label cannot be null or whitespace", nameof(stateLabel));

        if (!double.IsFinite(coefficient.Real) || !double.IsFinite(coefficient.Imaginary))
            throw new ArgumentException("Coefficient contains non-finite values", nameof(coefficient));

        lock (_stateLock)
        {
            _components[stateLabel] = coefficient;
            _isNormalized = false; // Invalidate normalization

            Logger.Debug($"Added component '{stateLabel}' to superposition", 
                new { StateLabel = stateLabel, Coefficient = coefficient, ComponentCount });
        }

        return this;
    }

    /// <summary>
    /// Removes a basis state component from the superposition.
    /// </summary>
    /// <param name="stateLabel">The label of the basis state to remove.</param>
    /// <returns>This superposition state (for method chaining).</returns>
    public Superposition RemoveComponent(string stateLabel)
    {
        if (string.IsNullOrWhiteSpace(stateLabel))
            throw new ArgumentException("State label cannot be null or whitespace", nameof(stateLabel));

        lock (_stateLock)
        {
            if (_components.Count <= 1)
                throw new InvalidOperationException("Cannot remove component from superposition with only one component");

            var removed = _components.Remove(stateLabel);
            if (removed)
            {
                _isNormalized = false; // Invalidate normalization
                Logger.Debug($"Removed component '{stateLabel}' from superposition", 
                    new { StateLabel = stateLabel, ComponentCount });
            }
        }

        return this;
    }

    /// <summary>
    /// Gets the coefficient for a specific basis state.
    /// </summary>
    /// <param name="stateLabel">The label of the basis state.</param>
    /// <returns>The complex coefficient, or zero if the state is not present.</returns>
    public Complex GetCoefficient(string stateLabel)
    {
        if (string.IsNullOrWhiteSpace(stateLabel))
            return Complex.Zero;

        lock (_stateLock)
        {
            return _components.TryGetValue(stateLabel, out var coefficient) ? coefficient : Complex.Zero;
        }
    }

    /// <summary>
    /// Checks if the superposition contains a specific basis state.
    /// </summary>
    /// <param name="stateLabel">The label of the basis state.</param>
    /// <returns>True if the state is present in the superposition.</returns>
    public bool ContainsState(string stateLabel)
    {
        if (string.IsNullOrWhiteSpace(stateLabel))
            return false;

        lock (_stateLock)
        {
            return _components.ContainsKey(stateLabel);
        }
    }

    /// <summary>
    /// Calculates the overlap ⟨this|other⟩ with another superposition state.
    /// </summary>
    /// <param name="other">The other superposition state.</param>
    /// <returns>The complex overlap.</returns>
    public Complex Overlap(Superposition other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        lock (_stateLock)
        {
            var overlap = Complex.Zero;
            var otherCoefficients = other.Coefficients;

            foreach (var kvp in _components)
            {
                var stateLabel = kvp.Key;
                var thisCoeff = kvp.Value;

                if (otherCoefficients.TryGetValue(stateLabel, out var otherCoeff))
                {
                    overlap += thisCoeff.Conjugate * otherCoeff;
                }
            }

            return overlap;
        }
    }

    /// <summary>
    /// Applies a global phase to all components of the superposition.
    /// </summary>
    /// <param name="phase">The phase to apply (in radians).</param>
    /// <returns>This superposition state (for method chaining).</returns>
    public Superposition ApplyGlobalPhase(double phase)
    {
        lock (_stateLock)
        {
            var phaseFactor = Complex.FromPhase(phase);
            var keys = _components.Keys.ToList();

            foreach (var key in keys)
            {
                _components[key] *= phaseFactor;
            }

            Logger.Debug($"Applied global phase {phase} radians to superposition", 
                new { Phase = phase, ComponentCount });
        }

        return this;
    }

    /// <summary>
    /// Applies individual phases to specific basis states.
    /// </summary>
    /// <param name="statePhases">Dictionary of state labels and their phases (in radians).</param>
    /// <returns>This superposition state (for method chaining).</returns>
    public Superposition ApplyPhases(Dictionary<string, double> statePhases)
    {
        if (statePhases == null)
            throw new ArgumentNullException(nameof(statePhases));

        lock (_stateLock)
        {
            foreach (var kvp in statePhases)
            {
                var stateLabel = kvp.Key;
                var phase = kvp.Value;

                if (_components.ContainsKey(stateLabel))
                {
                    var phaseFactor = Complex.FromPhase(phase);
                    _components[stateLabel] *= phaseFactor;
                }
            }

            Logger.Debug($"Applied individual phases to {statePhases.Count} states", 
                new { PhaseCount = statePhases.Count, ComponentCount });
        }

        return this;
    }

    /// <summary>
    /// Calculates the probability of measuring a specific basis state.
    /// </summary>
    /// <param name="stateLabel">The label of the basis state.</param>
    /// <returns>The probability [0, 1].</returns>
    public double MeasurementProbability(string stateLabel)
    {
        if (string.IsNullOrWhiteSpace(stateLabel))
            return 0.0;

        lock (_stateLock)
        {
            return _components.TryGetValue(stateLabel, out var coefficient) 
                ? coefficient.MagnitudeSquared 
                : 0.0;
        }
    }

    /// <summary>
    /// Performs a measurement and collapses the superposition to a specific basis state.
    /// </summary>
    /// <param name="measuredState">The state that was measured.</param>
    /// <returns>A new superposition containing only the measured state.</returns>
    public Superposition CollapseToState(string measuredState)
    {
        if (string.IsNullOrWhiteSpace(measuredState))
            throw new ArgumentException("Measured state label cannot be null or whitespace", nameof(measuredState));

        lock (_stateLock)
        {
            if (!_components.ContainsKey(measuredState))
                throw new ArgumentException($"State '{measuredState}' is not present in the superposition", nameof(measuredState));

            var collapsedComponents = new Dictionary<string, Complex>
            {
                { measuredState, new Complex(1.0, 0.0) }
            };

            return new Superposition(collapsedComponents, normalize: false);
        }
    }

    #endregion

    #region Static Operations

    /// <summary>
    /// Adds two superposition states.
    /// </summary>
    /// <param name="superposition1">The first superposition state.</param>
    /// <param name="superposition2">The second superposition state.</param>
    /// <returns>A new superposition state representing superposition1 + superposition2.</returns>
    public static Superposition operator +(Superposition superposition1, Superposition superposition2)
    {
        if (superposition1 == null || superposition2 == null)
            throw new ArgumentNullException();

        var resultComponents = new Dictionary<string, Complex>(superposition1._components);

        foreach (var kvp in superposition2._components)
        {
            var stateLabel = kvp.Key;
            var coefficient = kvp.Value;

            if (resultComponents.ContainsKey(stateLabel))
            {
                resultComponents[stateLabel] += coefficient;
            }
            else
            {
                resultComponents[stateLabel] = coefficient;
            }
        }

        // Remove components that became zero
        var zeroStates = resultComponents.Where(kvp => kvp.Value.Magnitude < PrecisionHandling.MinimumAmplitudeMagnitude)
                                       .Select(kvp => kvp.Key)
                                       .ToList();

        foreach (var state in zeroStates)
        {
            resultComponents.Remove(state);
        }

        if (resultComponents.Count == 0)
        {
            throw new InvalidOperationException("Addition resulted in zero superposition state");
        }

        return new Superposition(resultComponents, normalize: false);
    }

    /// <summary>
    /// Multiplies a superposition state by a complex scalar.
    /// </summary>
    /// <param name="scalar">The complex scalar.</param>
    /// <param name="superposition">The superposition state.</param>
    /// <returns>A new superposition state representing scalar * superposition.</returns>
    public static Superposition operator *(Complex scalar, Superposition superposition)
    {
        if (superposition == null)
            throw new ArgumentNullException(nameof(superposition));

        if (scalar.Magnitude < PrecisionHandling.MinimumAmplitudeMagnitude)
            throw new ArgumentException("Cannot multiply by zero or near-zero scalar", nameof(scalar));

        var resultComponents = superposition._components.ToDictionary(
            kvp => kvp.Key,
            kvp => scalar * kvp.Value
        );

        return new Superposition(resultComponents, normalize: false);
    }

    /// <summary>
    /// Multiplies a superposition state by a real scalar.
    /// </summary>
    /// <param name="scalar">The real scalar.</param>
    /// <param name="superposition">The superposition state.</param>
    /// <returns>A new superposition state representing scalar * superposition.</returns>
    public static Superposition operator *(double scalar, Superposition superposition)
    {
        return new Complex(scalar, 0.0) * superposition;
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether two superposition states are equal within numerical tolerance.
    /// </summary>
    public bool Equals(Superposition? other)
    {
        if (other == null || other.ComponentCount != ComponentCount)
            return false;

        var thisCoefficients = Coefficients;
        var otherCoefficients = other.Coefficients;

        foreach (var kvp in thisCoefficients)
        {
            var stateLabel = kvp.Key;
            var thisCoeff = kvp.Value;

            if (!otherCoefficients.TryGetValue(stateLabel, out var otherCoeff))
                return false;

            if (!thisCoeff.Equals(otherCoeff, PrecisionHandling.QuantumCalculationTolerance))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether this superposition state equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Superposition other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this superposition state.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ComponentCount);

        var sortedComponents = Coefficients.OrderBy(kvp => kvp.Key);
        foreach (var kvp in sortedComponents)
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value.GetHashCode());
        }

        return hash.ToHashCode();
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the superposition state.
    /// </summary>
    public override string ToString()
    {
        var coefficients = Coefficients;
        var terms = new List<string>();

        foreach (var kvp in coefficients.OrderBy(x => x.Key))
        {
            var stateLabel = kvp.Key;
            var coefficient = kvp.Value;

            if (coefficient.Magnitude > PrecisionHandling.QuantumCalculationTolerance)
            {
                var coeffStr = coefficient.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
                terms.Add($"({coeffStr}){stateLabel}");
            }
        }

        if (terms.Count == 0)
            return "|0⟩";

        var result = string.Join(" + ", terms);
        return result.Replace(" + (-", " - ("); // Clean up negative signs
    }

    #endregion

    #region Private Methods

    private double CalculateNorm()
    {
        return Math.Sqrt(_components.Values.Sum(coeff => coeff.MagnitudeSquared));
    }

    #endregion
}
