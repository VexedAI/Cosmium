using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmium.Engine.Physics.Mathematics;

/// <summary>
/// Represents probability distributions and provides quantum probability calculations.
/// Essential for quantum measurements, state collapse, and statistical analysis.
/// </summary>
public static class Probability
{
    #region Constants

    /// <summary>
    /// Machine epsilon for probability comparisons.
    /// </summary>
    private const double Epsilon = 1e-15;

    #endregion

    #region Probability Validation

    /// <summary>
    /// Validates that a value is a valid probability (0 ≤ p ≤ 1).
    /// </summary>
    /// <param name="probability">The probability value to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when probability is not in [0,1].</exception>
    public static void ValidateProbability(double probability, string paramName = "probability")
    {
        if (probability < 0.0 || probability > 1.0 || !double.IsFinite(probability))
        {
            throw new ArgumentOutOfRangeException(paramName, 
                $"Probability must be finite and in range [0, 1], got {probability}");
        }
    }

    /// <summary>
    /// Validates that a collection of probabilities sum to 1 (within tolerance).
    /// </summary>
    /// <param name="probabilities">The probability values to validate.</param>
    /// <param name="tolerance">The tolerance for the sum check.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentException">Thrown when probabilities don't sum to 1.</exception>
    public static void ValidateProbabilityDistribution(IEnumerable<double> probabilities, 
        double tolerance = 1e-10, string paramName = "probabilities")
    {
        if (probabilities == null)
            throw new ArgumentNullException(paramName);

        var probArray = probabilities.ToArray();
        
        // Validate individual probabilities
        for (int i = 0; i < probArray.Length; i++)
        {
            ValidateProbability(probArray[i], $"{paramName}[{i}]");
        }

        // Check that they sum to 1
        double sum = probArray.Sum();
        if (Math.Abs(sum - 1.0) > tolerance)
        {
            throw new ArgumentException(
                $"Probabilities must sum to 1.0 (±{tolerance}), got sum = {sum}", paramName);
        }
    }

    #endregion

    #region Quantum Probability Calculations

    /// <summary>
    /// Calculates the probability from a quantum amplitude.
    /// P = |ψ|² for complex amplitude ψ.
    /// </summary>
    /// <param name="amplitude">The complex amplitude.</param>
    /// <returns>The probability (real, non-negative).</returns>
    public static double FromAmplitude(Complex amplitude)
    {
        return amplitude.MagnitudeSquared;
    }

    /// <summary>
    /// Calculates probabilities from a collection of quantum amplitudes.
    /// Each probability is |ψᵢ|² and the result is normalized if requested.
    /// </summary>
    /// <param name="amplitudes">The complex amplitudes.</param>
    /// <param name="normalize">Whether to normalize the probabilities to sum to 1.</param>
    /// <returns>An array of probabilities.</returns>
    public static double[] FromAmplitudes(IEnumerable<Complex> amplitudes, bool normalize = true)
    {
        if (amplitudes == null)
            throw new ArgumentNullException(nameof(amplitudes));

        var amplitudeArray = amplitudes.ToArray();
        if (amplitudeArray.Length == 0)
            throw new ArgumentException("Amplitudes collection cannot be empty.");

        // Calculate |ψᵢ|²
        var probabilities = amplitudeArray.Select(a => a.MagnitudeSquared).ToArray();

        if (normalize)
        {
            double sum = probabilities.Sum();
            if (sum < Epsilon)
                throw new InvalidOperationException("Cannot normalize zero probability distribution.");

            for (int i = 0; i < probabilities.Length; i++)
            {
                probabilities[i] /= sum;
            }
        }

        return probabilities;
    }

    /// <summary>
    /// Calculates the amplitude from a probability and phase.
    /// ψ = √P × e^(iφ)
    /// </summary>
    /// <param name="probability">The probability (must be in [0,1]).</param>
    /// <param name="phase">The phase in radians.</param>
    /// <returns>The complex amplitude.</returns>
    public static Complex ToAmplitude(double probability, double phase = 0.0)
    {
        ValidateProbability(probability);
        
        if (!double.IsFinite(phase))
            throw new ArgumentException("Phase must be finite.", nameof(phase));

        double magnitude = Math.Sqrt(probability);
        return Complex.FromPolar(magnitude, phase);
    }

    /// <summary>
    /// Calculates the probability of a quantum state measurement outcome.
    /// P(outcome) = |⟨outcome|state⟩|²
    /// </summary>
    /// <param name="state">The quantum state vector.</param>
    /// <param name="outcome">The measurement outcome basis state.</param>
    /// <returns>The measurement probability.</returns>
    public static double MeasurementProbability(Matrix state, Matrix outcome)
    {
        if (state == null || outcome == null)
            throw new ArgumentNullException("State and outcome vectors cannot be null.");

        if (!state.IsVector || !outcome.IsVector)
            throw new ArgumentException("Both arguments must be vectors.");

        if (state.Rows != outcome.Rows)
            throw new ArgumentException("State and outcome vectors must have the same dimension.");

        Complex amplitude = outcome.InnerProduct(state);
        return FromAmplitude(amplitude);
    }

    /// <summary>
    /// Calculates all measurement probabilities for a quantum state in a given basis.
    /// </summary>
    /// <param name="state">The quantum state vector.</param>
    /// <param name="basis">The measurement basis vectors.</param>
    /// <returns>An array of measurement probabilities.</returns>
    public static double[] MeasurementProbabilities(Matrix state, IEnumerable<Matrix> basis)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        
        if (basis == null)
            throw new ArgumentNullException(nameof(basis));

        var basisArray = basis.ToArray();
        if (basisArray.Length == 0)
            throw new ArgumentException("Basis cannot be empty.");

        var probabilities = new double[basisArray.Length];
        for (int i = 0; i < basisArray.Length; i++)
        {
            probabilities[i] = MeasurementProbability(state, basisArray[i]);
        }

        return probabilities;
    }

    #endregion

    #region Statistical Functions

    /// <summary>
    /// Calculates the expected value of a discrete random variable.
    /// E[X] = Σ xᵢ × P(xᵢ)
    /// </summary>
    /// <param name="values">The possible values.</param>
    /// <param name="probabilities">The corresponding probabilities.</param>
    /// <returns>The expected value.</returns>
    public static double ExpectedValue(IEnumerable<double> values, IEnumerable<double> probabilities)
    {
        if (values == null || probabilities == null)
            throw new ArgumentNullException("Values and probabilities cannot be null.");

        var valueArray = values.ToArray();
        var probArray = probabilities.ToArray();

        if (valueArray.Length != probArray.Length)
            throw new ArgumentException("Values and probabilities must have the same length.");

        if (valueArray.Length == 0)
            throw new ArgumentException("Arrays cannot be empty.");

        ValidateProbabilityDistribution(probArray);

        double expectedValue = 0.0;
        for (int i = 0; i < valueArray.Length; i++)
        {
            expectedValue += valueArray[i] * probArray[i];
        }

        return expectedValue;
    }

    /// <summary>
    /// Calculates the variance of a discrete random variable.
    /// Var[X] = E[X²] - (E[X])²
    /// </summary>
    /// <param name="values">The possible values.</param>
    /// <param name="probabilities">The corresponding probabilities.</param>
    /// <returns>The variance.</returns>
    public static double Variance(IEnumerable<double> values, IEnumerable<double> probabilities)
    {
        if (values == null || probabilities == null)
            throw new ArgumentNullException("Values and probabilities cannot be null.");

        var valueArray = values.ToArray();
        var probArray = probabilities.ToArray();

        double mean = ExpectedValue(valueArray, probArray);
        double expectedSquare = ExpectedValue(valueArray.Select(x => x * x), probArray);

        return expectedSquare - mean * mean;
    }

    /// <summary>
    /// Calculates the standard deviation of a discrete random variable.
    /// σ = √Var[X]
    /// </summary>
    /// <param name="values">The possible values.</param>
    /// <param name="probabilities">The corresponding probabilities.</param>
    /// <returns>The standard deviation.</returns>
    public static double StandardDeviation(IEnumerable<double> values, IEnumerable<double> probabilities)
    {
        return Math.Sqrt(Variance(values, probabilities));
    }

    /// <summary>
    /// Calculates the entropy of a probability distribution.
    /// H(X) = -Σ P(xᵢ) × log₂(P(xᵢ))
    /// </summary>
    /// <param name="probabilities">The probability distribution.</param>
    /// <param name="baseLog">The logarithm base (default: 2 for bits).</param>
    /// <returns>The entropy.</returns>
    public static double Entropy(IEnumerable<double> probabilities, double baseLog = 2.0)
    {
        if (probabilities == null)
            throw new ArgumentNullException(nameof(probabilities));

        var probArray = probabilities.ToArray();
        ValidateProbabilityDistribution(probArray);

        double entropy = 0.0;
        double logBase = Math.Log(baseLog);

        foreach (double p in probArray)
        {
            if (p > Epsilon) // Skip zero probabilities (0 × log(0) = 0)
            {
                entropy -= p * Math.Log(p) / logBase;
            }
        }

        return entropy;
    }

    /// <summary>
    /// Calculates the von Neumann entropy of a quantum state (density matrix).
    /// S(ρ) = -Tr(ρ log ρ)
    /// This is a simplified version that works with diagonal density matrices.
    /// </summary>
    /// <param name="eigenvalues">The eigenvalues of the density matrix.</param>
    /// <param name="baseLog">The logarithm base (default: 2 for qubits).</param>
    /// <returns>The von Neumann entropy.</returns>
    public static double VonNeumannEntropy(IEnumerable<double> eigenvalues, double baseLog = 2.0)
    {
        if (eigenvalues == null)
            throw new ArgumentNullException(nameof(eigenvalues));

        var eigenArray = eigenvalues.ToArray();
        
        // Validate that eigenvalues form a valid probability distribution
        ValidateProbabilityDistribution(eigenArray);

        return Entropy(eigenArray, baseLog);
    }

    #endregion

    #region Sampling and Random Generation

    /// <summary>
    /// Samples from a discrete probability distribution using the inverse transform method.
    /// </summary>
    /// <param name="probabilities">The probability distribution.</param>
    /// <param name="random">The random number generator.</param>
    /// <returns>The sampled index.</returns>
    public static int SampleDiscrete(IEnumerable<double> probabilities, Random random)
    {
        if (probabilities == null)
            throw new ArgumentNullException(nameof(probabilities));
        
        if (random == null)
            throw new ArgumentNullException(nameof(random));

        var probArray = probabilities.ToArray();
        ValidateProbabilityDistribution(probArray);

        double randomValue = random.NextDouble();
        double cumulativeProb = 0.0;

        for (int i = 0; i < probArray.Length; i++)
        {
            cumulativeProb += probArray[i];
            if (randomValue <= cumulativeProb)
            {
                return i;
            }
        }

        // Should never reach here with valid probabilities, but return last index as fallback
        return probArray.Length - 1;
    }

    /// <summary>
    /// Samples multiple values from a discrete probability distribution.
    /// </summary>
    /// <param name="probabilities">The probability distribution.</param>
    /// <param name="count">The number of samples to generate.</param>
    /// <param name="random">The random number generator.</param>
    /// <returns>An array of sampled indices.</returns>
    public static int[] SampleDiscrete(IEnumerable<double> probabilities, int count, Random random)
    {
        if (count < 0)
            throw new ArgumentException("Count must be non-negative.", nameof(count));

        var samples = new int[count];
        for (int i = 0; i < count; i++)
        {
            samples[i] = SampleDiscrete(probabilities, random);
        }

        return samples;
    }

    /// <summary>
    /// Performs a quantum measurement simulation, collapsing the state to one outcome.
    /// </summary>
    /// <param name="state">The quantum state vector.</param>
    /// <param name="basis">The measurement basis vectors.</param>
    /// <param name="random">The random number generator.</param>
    /// <returns>A tuple containing (outcome_index, collapsed_state, probability).</returns>
    public static (int OutcomeIndex, Matrix CollapsedState, double Probability) 
        QuantumMeasurement(Matrix state, IEnumerable<Matrix> basis, Random random)
    {
        if (state == null || basis == null || random == null)
            throw new ArgumentNullException("Arguments cannot be null.");

        var basisArray = basis.ToArray();
        var probabilities = MeasurementProbabilities(state, basisArray);
        
        int outcomeIndex = SampleDiscrete(probabilities, random);
        var collapsedState = basisArray[outcomeIndex];
        double probability = probabilities[outcomeIndex];

        return (outcomeIndex, collapsedState, probability);
    }

    #endregion

    #region Probability Distributions

    /// <summary>
    /// Generates a uniform probability distribution.
    /// </summary>
    /// <param name="count">The number of outcomes.</param>
    /// <returns>A uniform probability distribution.</returns>
    public static double[] Uniform(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive.", nameof(count));

        double probability = 1.0 / count;
        return Enumerable.Repeat(probability, count).ToArray();
    }

    /// <summary>
    /// Normalizes a collection of weights to form a probability distribution.
    /// </summary>
    /// <param name="weights">The non-negative weights.</param>
    /// <returns>A normalized probability distribution.</returns>
    public static double[] Normalize(IEnumerable<double> weights)
    {
        if (weights == null)
            throw new ArgumentNullException(nameof(weights));

        var weightArray = weights.ToArray();
        if (weightArray.Length == 0)
            throw new ArgumentException("Weights cannot be empty.");

        // Validate that all weights are non-negative
        for (int i = 0; i < weightArray.Length; i++)
        {
            if (weightArray[i] < 0.0 || !double.IsFinite(weightArray[i]))
            {
                throw new ArgumentException($"Weight at index {i} must be finite and non-negative, got {weightArray[i]}");
            }
        }

        double sum = weightArray.Sum();
        if (sum < Epsilon)
            throw new ArgumentException("Sum of weights must be positive.");

        var probabilities = new double[weightArray.Length];
        for (int i = 0; i < weightArray.Length; i++)
        {
            probabilities[i] = weightArray[i] / sum;
        }

        return probabilities;
    }

    /// <summary>
    /// Creates a probability distribution with one outcome having probability 1.
    /// </summary>
    /// <param name="count">The total number of outcomes.</param>
    /// <param name="certainIndex">The index of the certain outcome.</param>
    /// <returns>A deterministic probability distribution.</returns>
    public static double[] Deterministic(int count, int certainIndex)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive.", nameof(count));
        
        if (certainIndex < 0 || certainIndex >= count)
            throw new ArgumentOutOfRangeException(nameof(certainIndex), 
                $"Index must be in range [0, {count - 1}], got {certainIndex}");

        var probabilities = new double[count];
        probabilities[certainIndex] = 1.0;
        return probabilities;
    }

    #endregion

    #region Comparison and Similarity

    /// <summary>
    /// Calculates the Kullback-Leibler divergence between two probability distributions.
    /// KL(P||Q) = Σ P(i) × log(P(i)/Q(i))
    /// </summary>
    /// <param name="p">The first probability distribution.</param>
    /// <param name="q">The second probability distribution.</param>
    /// <param name="baseLog">The logarithm base.</param>
    /// <returns>The KL divergence (non-negative, infinite if Q has zeros where P doesn't).</returns>
    public static double KullbackLeiblerDivergence(IEnumerable<double> p, IEnumerable<double> q, double baseLog = Math.E)
    {
        if (p == null || q == null)
            throw new ArgumentNullException("Probability distributions cannot be null.");

        var pArray = p.ToArray();
        var qArray = q.ToArray();

        if (pArray.Length != qArray.Length)
            throw new ArgumentException("Probability distributions must have the same length.");

        ValidateProbabilityDistribution(pArray);
        ValidateProbabilityDistribution(qArray);

        double divergence = 0.0;
        double logBase = Math.Log(baseLog);

        for (int i = 0; i < pArray.Length; i++)
        {
            if (pArray[i] > Epsilon)
            {
                if (qArray[i] < Epsilon)
                    return double.PositiveInfinity; // Undefined when Q(i) = 0 but P(i) > 0

                divergence += pArray[i] * Math.Log(pArray[i] / qArray[i]) / logBase;
            }
        }

        return divergence;
    }

    /// <summary>
    /// Calculates the total variation distance between two probability distributions.
    /// TV(P, Q) = (1/2) × Σ |P(i) - Q(i)|
    /// </summary>
    /// <param name="p">The first probability distribution.</param>
    /// <param name="q">The second probability distribution.</param>
    /// <returns>The total variation distance [0, 1].</returns>
    public static double TotalVariationDistance(IEnumerable<double> p, IEnumerable<double> q)
    {
        if (p == null || q == null)
            throw new ArgumentNullException("Probability distributions cannot be null.");

        var pArray = p.ToArray();
        var qArray = q.ToArray();

        if (pArray.Length != qArray.Length)
            throw new ArgumentException("Probability distributions must have the same length.");

        ValidateProbabilityDistribution(pArray);
        ValidateProbabilityDistribution(qArray);

        double sum = 0.0;
        for (int i = 0; i < pArray.Length; i++)
        {
            sum += Math.Abs(pArray[i] - qArray[i]);
        }

        return 0.5 * sum;
    }

    #endregion
}
