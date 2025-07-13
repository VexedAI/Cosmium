using Cosmium.Engine.Infrastructure.Configuration;

namespace Cosmium.Engine.Infrastructure.Configuration;

/// <summary>
/// Computational settings for the Cosmium Engine.
/// Controls numerical methods, precision, and algorithmic parameters.
/// </summary>
public class ComputationSettings : ConfigurationSection
{
    /// <summary>
    /// Default numerical tolerance for quantum calculations.
    /// </summary>
    public double DefaultTolerance { get; set; } = 1e-12;

    /// <summary>
    /// Maximum number of iterations for iterative algorithms.
    /// </summary>
    public int MaxIterations { get; set; } = 10000;

    /// <summary>
    /// Number of threads to use for parallel computations (0 = auto-detect).
    /// </summary>
    public int ThreadCount { get; set; } = 0;

    /// <summary>
    /// Whether to use GPU acceleration when available.
    /// </summary>
    public bool UseGpuAcceleration { get; set; } = false;

    /// <summary>
    /// Memory settings for computational algorithms.
    /// </summary>
    public MemorySettings Memory { get; set; } = new();

    /// <summary>
    /// Numerical precision settings.
    /// </summary>
    public PrecisionSettings Precision { get; set; } = new();

    /// <summary>
    /// Parallel processing settings.
    /// </summary>
    public ParallelSettings Parallel { get; set; } = new();

    /// <summary>
    /// Algorithm-specific settings.
    /// </summary>
    public AlgorithmSettings Algorithms { get; set; } = new();

    public override bool Validate()
    {
        return DefaultTolerance > 0 &&
               DefaultTolerance < 1.0 &&
               MaxIterations > 0 &&
               ThreadCount >= 0 &&
               Memory.Validate() &&
               Precision.Validate() &&
               Parallel.Validate() &&
               Algorithms.Validate();
    }

    /// <summary>
    /// Gets the actual thread count to use (resolves auto-detection).
    /// </summary>
    /// <returns>Number of threads to use.</returns>
    public int GetEffectiveThreadCount()
    {
        return ThreadCount <= 0 ? Environment.ProcessorCount : ThreadCount;
    }
}

/// <summary>
/// Memory management settings for computations.
/// </summary>
public class MemorySettings : ConfigurationSection
{
    /// <summary>
    /// Maximum memory usage in MB for matrix operations.
    /// </summary>
    public long MaxMatrixMemoryMB { get; set; } = 1024;

    /// <summary>
    /// Whether to use memory-mapped files for large datasets.
    /// </summary>
    public bool UseMemoryMapping { get; set; } = false;

    /// <summary>
    /// Cache size for frequently accessed data in MB.
    /// </summary>
    public int CacheSizeMB { get; set; } = 256;

    /// <summary>
    /// Whether to enable garbage collection optimization for large objects.
    /// </summary>
    public bool OptimizeGarbageCollection { get; set; } = true;

    /// <summary>
    /// Memory cleanup threshold as percentage of available memory.
    /// </summary>
    public double CleanupThresholdPercent { get; set; } = 80.0;

    public override bool Validate()
    {
        return MaxMatrixMemoryMB > 0 &&
               CacheSizeMB > 0 &&
               CleanupThresholdPercent > 0 &&
               CleanupThresholdPercent < 100;
    }
}

/// <summary>
/// Numerical precision settings for quantum calculations.
/// </summary>
public class PrecisionSettings : ConfigurationSection
{
    /// <summary>
    /// Tolerance for considering values as numerically zero.
    /// </summary>
    public double ZeroTolerance { get; set; } = 1e-15;

    /// <summary>
    /// Tolerance for quantum amplitude normalization.
    /// </summary>
    public double NormalizationTolerance { get; set; } = 1e-10;

    /// <summary>
    /// Tolerance for eigenvalue calculations.
    /// </summary>
    public double EigenvalueTolerance { get; set; } = 1e-12;

    /// <summary>
    /// Whether to use extended precision arithmetic when needed.
    /// </summary>
    public bool UseExtendedPrecision { get; set; } = false;

    /// <summary>
    /// Whether to enable Kahan summation for improved numerical stability.
    /// </summary>
    public bool UseKahanSummation { get; set; } = true;

    /// <summary>
    /// Precision mode for quantum calculations.
    /// </summary>
    public PrecisionMode Mode { get; set; } = PrecisionMode.Balanced;

    public override bool Validate()
    {
        return ZeroTolerance > 0 &&
               NormalizationTolerance > 0 &&
               EigenvalueTolerance > 0 &&
               ZeroTolerance <= NormalizationTolerance &&
               Enum.IsDefined(typeof(PrecisionMode), Mode);
    }
}

/// <summary>
/// Parallel processing configuration.
/// </summary>
public class ParallelSettings : ConfigurationSection
{
    /// <summary>
    /// Minimum problem size to trigger parallel processing.
    /// </summary>
    public int MinParallelSize { get; set; } = 1000;

    /// <summary>
    /// Maximum degree of parallelism for mathematical operations.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = -1; // -1 = unlimited

    /// <summary>
    /// Whether to use parallel matrix multiplication.
    /// </summary>
    public bool EnableParallelMatrixOps { get; set; } = true;

    /// <summary>
    /// Whether to use parallel FFT computations.
    /// </summary>
    public bool EnableParallelFFT { get; set; } = true;

    /// <summary>
    /// Task scheduler type for parallel operations.
    /// </summary>
    public TaskSchedulerType SchedulerType { get; set; } = TaskSchedulerType.Default;

    public override bool Validate()
    {
        return MinParallelSize > 0 &&
               Enum.IsDefined(typeof(TaskSchedulerType), SchedulerType);
    }
}

/// <summary>
/// Algorithm-specific settings.
/// </summary>
public class AlgorithmSettings : ConfigurationSection
{
    /// <summary>
    /// Preferred method for matrix diagonalization.
    /// </summary>
    public DiagonalizationMethod DiagonalizationMethod { get; set; } = DiagonalizationMethod.Jacobi;

    /// <summary>
    /// Preferred method for solving linear systems.
    /// </summary>
    public LinearSolverMethod LinearSolver { get; set; } = LinearSolverMethod.LU;

    /// <summary>
    /// Method for computing matrix exponentials.
    /// </summary>
    public MatrixExponentialMethod MatrixExponential { get; set; } = MatrixExponentialMethod.PadeApproximation;

    /// <summary>
    /// Random number generator algorithm.
    /// </summary>
    public RandomGeneratorType RandomGenerator { get; set; } = RandomGeneratorType.MersenneTwister;

    /// <summary>
    /// Whether to use adaptive algorithms that adjust based on problem characteristics.
    /// </summary>
    public bool UseAdaptiveAlgorithms { get; set; } = true;

    public override bool Validate()
    {
        return Enum.IsDefined(typeof(DiagonalizationMethod), DiagonalizationMethod) &&
               Enum.IsDefined(typeof(LinearSolverMethod), LinearSolver) &&
               Enum.IsDefined(typeof(MatrixExponentialMethod), MatrixExponential) &&
               Enum.IsDefined(typeof(RandomGeneratorType), RandomGenerator);
    }
}

/// <summary>
/// Precision modes for quantum calculations.
/// </summary>
public enum PrecisionMode
{
    /// <summary>
    /// Fast calculations with standard double precision.
    /// </summary>
    Fast,

    /// <summary>
    /// Balanced approach between speed and precision.
    /// </summary>
    Balanced,

    /// <summary>
    /// High precision for critical calculations.
    /// </summary>
    HighPrecision,

    /// <summary>
    /// Maximum precision using extended arithmetic.
    /// </summary>
    Maximum
}

/// <summary>
/// Task scheduler types for parallel operations.
/// </summary>
public enum TaskSchedulerType
{
    Default,
    ThreadPool,
    LimitedConcurrency,
    Custom
}

/// <summary>
/// Matrix diagonalization methods.
/// </summary>
public enum DiagonalizationMethod
{
    Jacobi,
    QR,
    DivideAndConquer,
    Bisection
}

/// <summary>
/// Linear system solver methods.
/// </summary>
public enum LinearSolverMethod
{
    LU,
    QR,
    Cholesky,
    SVD,
    Iterative
}

/// <summary>
/// Matrix exponential computation methods.
/// </summary>
public enum MatrixExponentialMethod
{
    PadeApproximation,
    ScalingAndSquaring,
    EigenDecomposition,
    TaylorSeries
}

/// <summary>
/// Random number generator types.
/// </summary>
public enum RandomGeneratorType
{
    System,
    MersenneTwister,
    XorShift,
    CryptographicSecure
}
