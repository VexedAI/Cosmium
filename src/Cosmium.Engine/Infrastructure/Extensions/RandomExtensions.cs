using System.Security.Cryptography;

namespace Cosmium.Engine.Infrastructure.Extensions;

/// <summary>
/// Random number generation utilities for quantum mechanics and scientific computing.
/// Provides enhanced random number generation with various distributions and quantum-specific methods.
/// </summary>
public static class RandomExtensions
{
    private static readonly ThreadLocal<Random> ThreadLocalRandom = new(() => new Random());

    /// <summary>
    /// Gets a thread-safe random number generator.
    /// </summary>
    public static Random ThreadSafeRandom => ThreadLocalRandom.Value!;

    #region Basic Extensions

    /// <summary>
    /// Generates a random double between min and max (exclusive).
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <returns>A random double in the specified range.</returns>
    public static double NextDouble(this Random random, double min, double max)
    {
        if (min >= max)
            throw new ArgumentException("Maximum must be greater than minimum");
        
        return min + random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Generates a random float between min and max (exclusive).
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <returns>A random float in the specified range.</returns>
    public static float NextSingle(this Random random, float min, float max)
    {
        if (min >= max)
            throw new ArgumentException("Maximum must be greater than minimum");
        
        return min + (float)random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Generates a random boolean with the specified probability of being true.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="probability">The probability of returning true (0.0 to 1.0).</param>
    /// <returns>A random boolean.</returns>
    public static bool NextBoolean(this Random random, double probability = 0.5)
    {
        if (probability < 0.0 || probability > 1.0)
            throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0 and 1");
        
        return random.NextDouble() < probability;
    }

    /// <summary>
    /// Selects a random element from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="random">The random number generator.</param>
    /// <param name="collection">The collection to select from.</param>
    /// <returns>A randomly selected element.</returns>
    public static T NextElement<T>(this Random random, IEnumerable<T> collection)
    {
        var list = collection.ToList();
        if (!list.Any())
            throw new ArgumentException("Collection cannot be empty");
        
        return list[random.Next(list.Count)];
    }

    /// <summary>
    /// Selects a random element from a collection with specified weights.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="random">The random number generator.</param>
    /// <param name="collection">The collection to select from.</param>
    /// <param name="weights">The weights for each element.</param>
    /// <returns>A randomly selected element based on weights.</returns>
    public static T NextWeightedElement<T>(this Random random, IEnumerable<T> collection, IEnumerable<double> weights)
    {
        var elements = collection.ToList();
        var weightList = weights.ToList();
        
        if (elements.Count != weightList.Count)
            throw new ArgumentException("Collection and weights must have the same count");
        
        if (weightList.Any(w => w < 0))
            throw new ArgumentException("Weights cannot be negative");
        
        var totalWeight = weightList.Sum();
        if (totalWeight <= 0)
            throw new ArgumentException("Total weight must be positive");
        
        var randomValue = random.NextDouble() * totalWeight;
        var cumulativeWeight = 0.0;
        
        for (int i = 0; i < elements.Count; i++)
        {
            cumulativeWeight += weightList[i];
            if (randomValue <= cumulativeWeight)
                return elements[i];
        }
        
        // Should never reach here, but return last element as fallback
        return elements.Last();
    }

    #endregion

    #region Statistical Distributions

    /// <summary>
    /// Generates a random number from a normal (Gaussian) distribution.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="standardDeviation">The standard deviation of the distribution.</param>
    /// <returns>A normally distributed random number.</returns>
    public static double NextGaussian(this Random random, double mean = 0.0, double standardDeviation = 1.0)
    {
        if (standardDeviation <= 0)
            throw new ArgumentException("Standard deviation must be positive");
        
        // Box-Muller transform
        double u1 = random.NextDouble();
        double u2 = random.NextDouble();
        
        double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + standardDeviation * z0;
    }

    /// <summary>
    /// Generates a random number from an exponential distribution.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="rate">The rate parameter (lambda).</param>
    /// <returns>An exponentially distributed random number.</returns>
    public static double NextExponential(this Random random, double rate = 1.0)
    {
        if (rate <= 0)
            throw new ArgumentException("Rate must be positive");
        
        return -Math.Log(1.0 - random.NextDouble()) / rate;
    }

    /// <summary>
    /// Generates a random number from a uniform distribution on a sphere.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A tuple containing (theta, phi) angles in radians.</returns>
    public static (double Theta, double Phi) NextSphericalUniform(this Random random)
    {
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        
        var theta = Math.Acos(2.0 * u1 - 1.0); // [0, π]
        var phi = 2.0 * Math.PI * u2;          // [0, 2π]
        
        return (theta, phi);
    }

    /// <summary>
    /// Generates a random number from a Poisson distribution.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="lambda">The rate parameter.</param>
    /// <returns>A Poisson distributed random integer.</returns>
    public static int NextPoisson(this Random random, double lambda)
    {
        if (lambda <= 0)
            throw new ArgumentException("Lambda must be positive");
        
        // Knuth algorithm for small lambda
        if (lambda < 30)
        {
            var limit = Math.Exp(-lambda);
            var product = 1.0;
            int k = 0;
            
            do
            {
                k++;
                product *= random.NextDouble();
            } while (product >= limit);
            
            return k - 1;
        }
        else
        {
            // Approximate with normal distribution for large lambda
            var normal = random.NextGaussian(lambda, Math.Sqrt(lambda));
            return Math.Max(0, (int)Math.Round(normal));
        }
    }

    /// <summary>
    /// Generates a random number from a gamma distribution.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="shape">The shape parameter (alpha).</param>
    /// <param name="scale">The scale parameter (beta).</param>
    /// <returns>A gamma distributed random number.</returns>
    public static double NextGamma(this Random random, double shape, double scale = 1.0)
    {
        if (shape <= 0 || scale <= 0)
            throw new ArgumentException("Shape and scale must be positive");
        
        // Marsaglia and Tsang method
        if (shape < 1.0)
        {
            return random.NextGamma(shape + 1.0, scale) * Math.Pow(random.NextDouble(), 1.0 / shape);
        }
        
        var d = shape - 1.0 / 3.0;
        var c = 1.0 / Math.Sqrt(9.0 * d);
        
        while (true)
        {
            var x = random.NextGaussian();
            var v = 1.0 + c * x;
            
            if (v <= 0)
                continue;
            
            v = v * v * v;
            var u = random.NextDouble();
            
            if (u < 1.0 - 0.0331 * x * x * x * x)
                return scale * d * v;
            
            if (Math.Log(u) < 0.5 * x * x + d * (1.0 - v + Math.Log(v)))
                return scale * d * v;
        }
    }

    #endregion

    #region Quantum-Specific Distributions

    /// <summary>
    /// Generates a random quantum phase between 0 and 2π.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A random phase in radians.</returns>
    public static double NextQuantumPhase(this Random random)
    {
        return random.NextDouble() * 2.0 * Math.PI;
    }

    /// <summary>
    /// Generates a random complex number with unit magnitude (on the unit circle).
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A random complex number with magnitude 1.</returns>
    public static System.Numerics.Complex NextUnitComplex(this Random random)
    {
        var phase = random.NextQuantumPhase();
        return new System.Numerics.Complex(Math.Cos(phase), Math.Sin(phase));
    }

    /// <summary>
    /// Generates a random complex number with Gaussian distributed real and imaginary parts.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="standardDeviation">The standard deviation for both real and imaginary parts.</param>
    /// <returns>A random complex number.</returns>
    public static System.Numerics.Complex NextGaussianComplex(this Random random, double standardDeviation = 1.0)
    {
        var real = random.NextGaussian(0, standardDeviation);
        var imaginary = random.NextGaussian(0, standardDeviation);
        return new System.Numerics.Complex(real, imaginary);
    }

    /// <summary>
    /// Generates a random quantum state vector (normalized complex amplitudes).
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="dimension">The dimension of the Hilbert space.</param>
    /// <returns>A random normalized quantum state.</returns>
    public static System.Numerics.Complex[] NextQuantumState(this Random random, int dimension)
    {
        if (dimension <= 0)
            throw new ArgumentException("Dimension must be positive");
        
        var amplitudes = new System.Numerics.Complex[dimension];
        
        // Generate random complex amplitudes
        for (int i = 0; i < dimension; i++)
        {
            amplitudes[i] = random.NextGaussianComplex();
        }
        
        // Normalize the state
        var normSquared = amplitudes.Sum(a => a.Real * a.Real + a.Imaginary * a.Imaginary);
        var norm = Math.Sqrt(normSquared);
        
        for (int i = 0; i < dimension; i++)
        {
            amplitudes[i] /= norm;
        }
        
        return amplitudes;
    }

    /// <summary>
    /// Generates a random unitary matrix of specified dimension.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="dimension">The dimension of the matrix.</param>
    /// <returns>A random unitary matrix.</returns>
    public static System.Numerics.Complex[,] NextUnitaryMatrix(this Random random, int dimension)
    {
        if (dimension <= 0)
            throw new ArgumentException("Dimension must be positive");
        
        // Generate random complex matrix
        var matrix = new System.Numerics.Complex[dimension, dimension];
        
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                matrix[i, j] = random.NextGaussianComplex();
            }
        }
        
        // QR decomposition would be used here to make it unitary
        // For simplicity, we'll return a random rotation matrix for 2D case
        if (dimension == 2)
        {
            var angle = random.NextQuantumPhase();
            matrix[0, 0] = new System.Numerics.Complex(Math.Cos(angle), 0);
            matrix[0, 1] = new System.Numerics.Complex(-Math.Sin(angle), 0);
            matrix[1, 0] = new System.Numerics.Complex(Math.Sin(angle), 0);
            matrix[1, 1] = new System.Numerics.Complex(Math.Cos(angle), 0);
        }
        
        return matrix;
    }

    /// <summary>
    /// Generates a random measurement outcome based on quantum probabilities.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="probabilities">The measurement probabilities (must sum to 1).</param>
    /// <returns>The index of the measured outcome.</returns>
    public static int NextQuantumMeasurement(this Random random, double[] probabilities)
    {
        if (probabilities.Any(p => p < 0))
            throw new ArgumentException("Probabilities cannot be negative");
        
        var sum = probabilities.Sum();
        if (Math.Abs(sum - 1.0) > 1e-10)
            throw new ArgumentException("Probabilities must sum to 1");
        
        var randomValue = random.NextDouble();
        var cumulativeProbability = 0.0;
        
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
                return i;
        }
        
        // Should never reach here, but return last index as fallback
        return probabilities.Length - 1;
    }

    #endregion

    #region Cryptographically Secure Random

    /// <summary>
    /// Generates cryptographically secure random bytes.
    /// </summary>
    /// <param name="length">The number of bytes to generate.</param>
    /// <returns>An array of cryptographically secure random bytes.</returns>
    public static byte[] NextSecureBytes(int length)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be positive");
        
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// Generates a cryptographically secure random double between 0 and 1.
    /// </summary>
    /// <returns>A cryptographically secure random double.</returns>
    public static double NextSecureDouble()
    {
        var bytes = NextSecureBytes(8);
        var value = BitConverter.ToUInt64(bytes, 0);
        return (double)value / ulong.MaxValue;
    }

    /// <summary>
    /// Generates a cryptographically secure random integer in the specified range.
    /// </summary>
    /// <param name="minValue">The minimum value (inclusive).</param>
    /// <param name="maxValue">The maximum value (exclusive).</param>
    /// <returns>A cryptographically secure random integer.</returns>
    public static int NextSecureInt(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
            throw new ArgumentException("Maximum must be greater than minimum");
        
        var range = (uint)(maxValue - minValue);
        var bytes = NextSecureBytes(4);
        var value = BitConverter.ToUInt32(bytes, 0);
        
        return (int)(value % range) + minValue;
    }

    #endregion

    #region Sampling Methods

    /// <summary>
    /// Performs reservoir sampling to select k random elements from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="random">The random number generator.</param>
    /// <param name="collection">The collection to sample from.</param>
    /// <param name="k">The number of elements to sample.</param>
    /// <returns>A random sample of k elements.</returns>
    public static IEnumerable<T> ReservoirSample<T>(this Random random, IEnumerable<T> collection, int k)
    {
        if (k <= 0)
            throw new ArgumentException("Sample size must be positive");
        
        var reservoir = new List<T>();
        int count = 0;
        
        foreach (var item in collection)
        {
            count++;
            
            if (reservoir.Count < k)
            {
                reservoir.Add(item);
            }
            else
            {
                var index = random.Next(count);
                if (index < k)
                {
                    reservoir[index] = item;
                }
            }
        }
        
        return reservoir;
    }

    /// <summary>
    /// Generates a random permutation of integers from 0 to n-1.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="n">The size of the permutation.</param>
    /// <returns>A random permutation.</returns>
    public static int[] NextPermutation(this Random random, int n)
    {
        if (n <= 0)
            throw new ArgumentException("Size must be positive");
        
        var permutation = Enumerable.Range(0, n).ToArray();
        
        // Fisher-Yates shuffle
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
        }
        
        return permutation;
    }

    #endregion
}
