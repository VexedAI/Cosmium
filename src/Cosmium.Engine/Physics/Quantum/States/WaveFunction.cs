using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;

namespace Cosmium.Engine.Physics.Quantum.States;

/// <summary>
/// Represents a quantum mechanical wave function with operations for position and momentum space calculations.
/// Provides functionality for wave function evolution, normalization, and probability distributions.
/// </summary>
public class WaveFunction : IEquatable<WaveFunction>
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _stateLock = new();
    private Complex[] _positionAmplitudes;
    private Complex[]? _momentumAmplitudes;
    private bool _isNormalized;
    private bool _momentumCacheValid;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of spatial grid points for the wave function.
    /// </summary>
    public int GridSize { get; }

    /// <summary>
    /// Gets the spatial extent of the wave function grid.
    /// </summary>
    public double SpatialExtent { get; }

    /// <summary>
    /// Gets the grid spacing in position.
    /// </summary>
    public double DeltaX => SpatialExtent / GridSize;

    /// <summary>
    /// Gets the grid spacing in momentum (from Fourier transform).
    /// </summary>
    public double DeltaP => 2.0 * Math.PI * PhysicsConstants.ReducedPlanckConstant / SpatialExtent;

    /// <summary>
    /// Gets the position grid points.
    /// </summary>
    public double[] PositionGrid
    {
        get
        {
            var positions = new double[GridSize];
            var startX = -SpatialExtent / 2.0;
            for (int i = 0; i < GridSize; i++)
            {
                positions[i] = startX + i * DeltaX;
            }
            return positions;
        }
    }

    /// <summary>
    /// Gets the momentum grid points.
    /// </summary>
    public double[] MomentumGrid
    {
        get
        {
            var momenta = new double[GridSize];
            var startP = -Math.PI * PhysicsConstants.ReducedPlanckConstant / DeltaX;
            for (int i = 0; i < GridSize; i++)
            {
                momenta[i] = startP + i * DeltaP;
            }
            return momenta;
        }
    }

    /// <summary>
    /// Gets the wave function amplitudes in position space. Returns a copy to maintain immutability.
    /// </summary>
    public Complex[] PositionAmplitudes
    {
        get
        {
            lock (_stateLock)
            {
                return (Complex[])_positionAmplitudes.Clone();
            }
        }
    }

    /// <summary>
    /// Gets the wave function amplitudes in momentum space (calculated via Fourier transform).
    /// </summary>
    public Complex[] MomentumAmplitudes
    {
        get
        {
            lock (_stateLock)
            {
                if (!_momentumCacheValid || _momentumAmplitudes == null)
                {
                    _momentumAmplitudes = CalculateFourierTransform(_positionAmplitudes);
                    _momentumCacheValid = true;
                }
                return (Complex[])_momentumAmplitudes.Clone();
            }
        }
    }

    /// <summary>
    /// Gets whether the wave function is normalized.
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
    /// Gets the norm of the wave function.
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
    /// Gets the probability density in position space |ψ(x)|².
    /// </summary>
    public double[] PositionProbabilityDensity
    {
        get
        {
            lock (_stateLock)
            {
                return _positionAmplitudes.Select(amp => amp.MagnitudeSquared).ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the probability density in momentum space |ψ̃(p)|².
    /// </summary>
    public double[] MomentumProbabilityDensity
    {
        get
        {
            var momentumAmps = MomentumAmplitudes;
            return momentumAmps.Select(amp => amp.MagnitudeSquared).ToArray();
        }
    }

    /// <summary>
    /// Gets the creation time of this wave function.
    /// </summary>
    public DateTime CreationTime { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new wave function with the specified position amplitudes.
    /// </summary>
    /// <param name="positionAmplitudes">The amplitude values in position space.</param>
    /// <param name="spatialExtent">The total spatial extent of the grid.</param>
    /// <param name="normalize">Whether to automatically normalize the wave function.</param>
    public WaveFunction(Complex[] positionAmplitudes, double spatialExtent, bool normalize = true)
    {
        if (positionAmplitudes == null)
            throw new ArgumentNullException(nameof(positionAmplitudes));

        if (positionAmplitudes.Length < 2)
            throw new ArgumentException("Wave function must have at least 2 grid points", nameof(positionAmplitudes));

        var extentValidation = ParameterValidator.ValidatePositive(spatialExtent, nameof(spatialExtent));
        extentValidation.ThrowIfInvalid();

        // Validate finite amplitudes
        for (int i = 0; i < positionAmplitudes.Length; i++)
        {
            if (!double.IsFinite(positionAmplitudes[i].Real) || !double.IsFinite(positionAmplitudes[i].Imaginary))
                throw new ArgumentException($"Amplitude at index {i} contains non-finite values", nameof(positionAmplitudes));
        }

        GridSize = positionAmplitudes.Length;
        SpatialExtent = spatialExtent;
        _positionAmplitudes = (Complex[])positionAmplitudes.Clone();
        CreationTime = DateTime.UtcNow;
        _momentumCacheValid = false;

        if (normalize)
        {
            Normalize();
        }
        else
        {
            _isNormalized = Math.Abs(CalculateNorm() - 1.0) < PrecisionHandling.ProbabilityNormalizationTolerance;
        }

        Logger.Debug($"Created wave function with {GridSize} grid points and spatial extent {spatialExtent}",
            new { GridSize, SpatialExtent = spatialExtent, IsNormalized, Norm = CalculateNorm() });
    }

    /// <summary>
    /// Initializes a new wave function from real-valued position data.
    /// </summary>
    /// <param name="positionValues">The real-valued amplitudes in position space.</param>
    /// <param name="spatialExtent">The total spatial extent of the grid.</param>
    /// <param name="normalize">Whether to automatically normalize the wave function.</param>
    public WaveFunction(double[] positionValues, double spatialExtent, bool normalize = true)
        : this(positionValues.Select(v => new Complex(v, 0.0)).ToArray(), spatialExtent, normalize)
    {
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a Gaussian wave packet.
    /// </summary>
    /// <param name="gridSize">The number of grid points.</param>
    /// <param name="spatialExtent">The total spatial extent.</param>
    /// <param name="centerPosition">The center position of the wave packet.</param>
    /// <param name="width">The width (standard deviation) of the Gaussian.</param>
    /// <param name="momentum">The initial momentum of the wave packet.</param>
    /// <returns>A normalized Gaussian wave packet.</returns>
    public static WaveFunction CreateGaussianWavePacket(int gridSize, double spatialExtent, 
        double centerPosition = 0.0, double width = 1.0, double momentum = 0.0)
    {
        var gridValidation = ParameterValidator.ValidateIntegerRange(gridSize, nameof(gridSize), 4, 1 << 20);
        gridValidation.ThrowIfInvalid();

        var extentValidation = ParameterValidator.ValidatePositive(spatialExtent, nameof(spatialExtent));
        extentValidation.ThrowIfInvalid();

        var widthValidation = ParameterValidator.ValidatePositive(width, nameof(width));
        widthValidation.ThrowIfInvalid();

        var amplitudes = new Complex[gridSize];
        var positions = CreatePositionGrid(gridSize, spatialExtent);

        for (int i = 0; i < gridSize; i++)
        {
            var x = positions[i];
            var gaussian = Math.Exp(-0.5 * Math.Pow((x - centerPosition) / width, 2));
            var phase = momentum * x / PhysicsConstants.ReducedPlanckConstant;
            amplitudes[i] = new Complex(gaussian * Math.Cos(phase), gaussian * Math.Sin(phase));
        }

        return new WaveFunction(amplitudes, spatialExtent, normalize: true);
    }

    /// <summary>
    /// Creates a plane wave with specified momentum.
    /// </summary>
    /// <param name="gridSize">The number of grid points.</param>
    /// <param name="spatialExtent">The total spatial extent.</param>
    /// <param name="momentum">The momentum of the plane wave.</param>
    /// <returns>A normalized plane wave.</returns>
    public static WaveFunction CreatePlaneWave(int gridSize, double spatialExtent, double momentum)
    {
        var gridValidation = ParameterValidator.ValidateIntegerRange(gridSize, nameof(gridSize), 4, 1 << 20);
        gridValidation.ThrowIfInvalid();

        var extentValidation = ParameterValidator.ValidatePositive(spatialExtent, nameof(spatialExtent));
        extentValidation.ThrowIfInvalid();

        var amplitudes = new Complex[gridSize];
        var positions = CreatePositionGrid(gridSize, spatialExtent);

        for (int i = 0; i < gridSize; i++)
        {
            var phase = momentum * positions[i] / PhysicsConstants.ReducedPlanckConstant;
            amplitudes[i] = new Complex(Math.Cos(phase), Math.Sin(phase));
        }

        return new WaveFunction(amplitudes, spatialExtent, normalize: true);
    }

    /// <summary>
    /// Creates a harmonic oscillator eigenstate.
    /// </summary>
    /// <param name="gridSize">The number of grid points.</param>
    /// <param name="spatialExtent">The total spatial extent.</param>
    /// <param name="quantumNumber">The quantum number (n = 0, 1, 2, ...).</param>
    /// <param name="frequency">The angular frequency of the oscillator.</param>
    /// <param name="mass">The mass of the particle.</param>
    /// <returns>A normalized harmonic oscillator eigenstate.</returns>
    public static WaveFunction CreateHarmonicOscillatorEigenstate(int gridSize, double spatialExtent,
        int quantumNumber, double frequency, double mass)
    {
        var gridValidation = ParameterValidator.ValidateIntegerRange(gridSize, nameof(gridSize), 4, 1 << 20);
        gridValidation.ThrowIfInvalid();

        var nValidation = ParameterValidator.ValidateIntegerRange(quantumNumber, nameof(quantumNumber), 0, 100);
        nValidation.ThrowIfInvalid();

        var freqValidation = ParameterValidator.ValidatePositive(frequency, nameof(frequency));
        freqValidation.ThrowIfInvalid();

        var massValidation = ParameterValidator.ValidatePositive(mass, nameof(mass));
        massValidation.ThrowIfInvalid();

        var amplitudes = new Complex[gridSize];
        var positions = CreatePositionGrid(gridSize, spatialExtent);

        // Characteristic length scale
        var x0 = Math.Sqrt(PhysicsConstants.ReducedPlanckConstant / (mass * frequency));

        for (int i = 0; i < gridSize; i++)
        {
            var xi = positions[i] / x0;
            var hermite = CalculateHermitePolynomial(quantumNumber, xi);
            var gaussian = Math.Exp(-0.5 * xi * xi);
            var normalization = 1.0 / Math.Sqrt(Math.Pow(2, quantumNumber) * Factorial(quantumNumber) * Math.Sqrt(Math.PI));

            amplitudes[i] = new Complex(normalization * hermite * gaussian, 0.0);
        }

        return new WaveFunction(amplitudes, spatialExtent, normalize: true);
    }

    #endregion

    #region Wave Function Operations

    /// <summary>
    /// Normalizes the wave function to have unit norm.
    /// </summary>
    /// <returns>This wave function (for method chaining).</returns>
    public WaveFunction Normalize()
    {
        lock (_stateLock)
        {
            var norm = CalculateNorm();
            if (norm < PrecisionHandling.MinimumAmplitudeMagnitude)
            {
                throw new InvalidOperationException("Cannot normalize wave function with zero or near-zero norm");
            }

            for (int i = 0; i < GridSize; i++)
            {
                _positionAmplitudes[i] /= norm;
            }

            _isNormalized = true;
            _momentumCacheValid = false; // Invalidate momentum cache

            Logger.Debug($"Normalized wave function", new { GridSize, NewNorm = CalculateNorm() });
        }

        return this;
    }

    /// <summary>
    /// Creates a deep copy of this wave function.
    /// </summary>
    /// <returns>A new wave function with identical amplitudes and properties.</returns>
    public WaveFunction Clone()
    {
        lock (_stateLock)
        {
            return new WaveFunction(_positionAmplitudes, SpatialExtent, normalize: false);
        }
    }

    /// <summary>
    /// Calculates the overlap integral ⟨this|other⟩ with another wave function.
    /// </summary>
    /// <param name="other">The other wave function.</param>
    /// <returns>The complex overlap integral.</returns>
    public Complex Overlap(WaveFunction other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (other.GridSize != GridSize)
            throw new ArgumentException($"Grid size mismatch: {GridSize} vs {other.GridSize}", nameof(other));

        if (Math.Abs(other.SpatialExtent - SpatialExtent) > PrecisionHandling.QuantumCalculationTolerance)
            throw new ArgumentException($"Spatial extent mismatch: {SpatialExtent} vs {other.SpatialExtent}", nameof(other));

        lock (_stateLock)
        {
            var otherAmplitudes = other.PositionAmplitudes;
            var overlap = Complex.Zero;

            for (int i = 0; i < GridSize; i++)
            {
                overlap += _positionAmplitudes[i].Conjugate * otherAmplitudes[i];
            }

            return overlap * DeltaX; // Include grid spacing for proper integration
        }
    }

    /// <summary>
    /// Evolves the wave function in time using the free particle Hamiltonian.
    /// </summary>
    /// <param name="timeStep">The time step for evolution.</param>
    /// <param name="mass">The mass of the particle.</param>
    /// <returns>This wave function after time evolution (for method chaining).</returns>
    public WaveFunction EvolveFreeparticle(double timeStep, double mass)
    {
        var timeValidation = ParameterValidator.ValidatePositive(timeStep, nameof(timeStep));
        timeValidation.ThrowIfInvalid();

        var massValidation = ParameterValidator.ValidatePositive(mass, nameof(mass));
        massValidation.ThrowIfInvalid();

        lock (_stateLock)
        {
            // Transform to momentum space
            var momentumAmps = CalculateFourierTransform(_positionAmplitudes);
            var momenta = MomentumGrid;

            // Apply time evolution operator in momentum space: exp(-i * p² * t / (2 * m * ħ))
            for (int i = 0; i < GridSize; i++)
            {
                var p = momenta[i];
                var phase = -p * p * timeStep / (2.0 * mass * PhysicsConstants.ReducedPlanckConstant);
                var evolutionFactor = Complex.FromPhase(phase);
                momentumAmps[i] *= evolutionFactor;
            }

            // Transform back to position space
            _positionAmplitudes = CalculateInverseFourierTransform(momentumAmps);
            _momentumCacheValid = false;

            Logger.Debug($"Evolved wave function for time step {timeStep}",
                new { TimeStep = timeStep, Mass = mass, NewNorm = CalculateNorm() });
        }

        return this;
    }

    /// <summary>
    /// Calculates the expectation value of position ⟨x⟩.
    /// </summary>
    /// <returns>The expectation value of position.</returns>
    public double ExpectationValuePosition()
    {
        lock (_stateLock)
        {
            var positions = PositionGrid;
            var expectationValue = 0.0;

            for (int i = 0; i < GridSize; i++)
            {
                var probability = _positionAmplitudes[i].MagnitudeSquared;
                expectationValue += positions[i] * probability;
            }

            return expectationValue * DeltaX;
        }
    }

    /// <summary>
    /// Calculates the expectation value of momentum ⟨p⟩.
    /// </summary>
    /// <returns>The expectation value of momentum.</returns>
    public double ExpectationValueMomentum()
    {
        var momentumAmps = MomentumAmplitudes;
        var momenta = MomentumGrid;
        var expectationValue = 0.0;

        for (int i = 0; i < GridSize; i++)
        {
            var probability = momentumAmps[i].MagnitudeSquared;
            expectationValue += momenta[i] * probability;
        }

        return expectationValue * DeltaP;
    }

    /// <summary>
    /// Calculates the uncertainty in position (standard deviation).
    /// </summary>
    /// <returns>The uncertainty in position Δx.</returns>
    public double UncertaintyPosition()
    {
        var meanX = ExpectationValuePosition();
        var positions = PositionGrid;
        var meanXSquared = 0.0;

        lock (_stateLock)
        {
            for (int i = 0; i < GridSize; i++)
            {
                var probability = _positionAmplitudes[i].MagnitudeSquared;
                meanXSquared += positions[i] * positions[i] * probability;
            }
        }

        meanXSquared *= DeltaX;
        return Math.Sqrt(Math.Max(0, meanXSquared - meanX * meanX));
    }

    /// <summary>
    /// Calculates the uncertainty in momentum (standard deviation).
    /// </summary>
    /// <returns>The uncertainty in momentum Δp.</returns>
    public double UncertaintyMomentum()
    {
        var meanP = ExpectationValueMomentum();
        var momentumAmps = MomentumAmplitudes;
        var momenta = MomentumGrid;
        var meanPSquared = 0.0;

        for (int i = 0; i < GridSize; i++)
        {
            var probability = momentumAmps[i].MagnitudeSquared;
            meanPSquared += momenta[i] * momenta[i] * probability;
        }

        meanPSquared *= DeltaP;
        return Math.Sqrt(Math.Max(0, meanPSquared - meanP * meanP));
    }

    /// <summary>
    /// Calculates the uncertainty product Δx·Δp.
    /// </summary>
    /// <returns>The uncertainty product.</returns>
    public double UncertaintyProduct()
    {
        return UncertaintyPosition() * UncertaintyMomentum();
    }

    #endregion

    #region Static Operations

    /// <summary>
    /// Adds two wave functions.
    /// </summary>
    /// <param name="wf1">The first wave function.</param>
    /// <param name="wf2">The second wave function.</param>
    /// <returns>A new wave function representing wf1 + wf2.</returns>
    public static WaveFunction operator +(WaveFunction wf1, WaveFunction wf2)
    {
        if (wf1 == null || wf2 == null)
            throw new ArgumentNullException();

        if (wf1.GridSize != wf2.GridSize)
            throw new ArgumentException("Cannot add wave functions with different grid sizes");

        if (Math.Abs(wf1.SpatialExtent - wf2.SpatialExtent) > PrecisionHandling.QuantumCalculationTolerance)
            throw new ArgumentException("Cannot add wave functions with different spatial extents");

        var amplitudes1 = wf1.PositionAmplitudes;
        var amplitudes2 = wf2.PositionAmplitudes;
        var resultAmplitudes = new Complex[wf1.GridSize];

        for (int i = 0; i < wf1.GridSize; i++)
        {
            resultAmplitudes[i] = amplitudes1[i] + amplitudes2[i];
        }

        return new WaveFunction(resultAmplitudes, wf1.SpatialExtent, normalize: false);
    }

    /// <summary>
    /// Multiplies a wave function by a complex scalar.
    /// </summary>
    /// <param name="scalar">The complex scalar.</param>
    /// <param name="waveFunction">The wave function.</param>
    /// <returns>A new wave function representing scalar * waveFunction.</returns>
    public static WaveFunction operator *(Complex scalar, WaveFunction waveFunction)
    {
        if (waveFunction == null)
            throw new ArgumentNullException(nameof(waveFunction));

        var amplitudes = waveFunction.PositionAmplitudes;
        var resultAmplitudes = amplitudes.Select(amp => scalar * amp).ToArray();

        return new WaveFunction(resultAmplitudes, waveFunction.SpatialExtent, normalize: false);
    }

    /// <summary>
    /// Multiplies a wave function by a real scalar.
    /// </summary>
    /// <param name="scalar">The real scalar.</param>
    /// <param name="waveFunction">The wave function.</param>
    /// <returns>A new wave function representing scalar * waveFunction.</returns>
    public static WaveFunction operator *(double scalar, WaveFunction waveFunction)
    {
        return new Complex(scalar, 0.0) * waveFunction;
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether two wave functions are equal within numerical tolerance.
    /// </summary>
    public bool Equals(WaveFunction? other)
    {
        if (other == null || other.GridSize != GridSize)
            return false;

        if (Math.Abs(other.SpatialExtent - SpatialExtent) > PrecisionHandling.QuantumCalculationTolerance)
            return false;

        var amplitudes1 = PositionAmplitudes;
        var amplitudes2 = other.PositionAmplitudes;

        for (int i = 0; i < GridSize; i++)
        {
            if (!amplitudes1[i].Equals(amplitudes2[i], PrecisionHandling.QuantumCalculationTolerance))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether this wave function equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is WaveFunction other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this wave function.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GridSize);
        hash.Add(SpatialExtent);

        var amplitudes = PositionAmplitudes;
        foreach (var amplitude in amplitudes.Take(10)) // Limit for performance
        {
            hash.Add(amplitude.GetHashCode());
        }

        return hash.ToHashCode();
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the wave function.
    /// </summary>
    public override string ToString()
    {
        return $"WaveFunction[GridSize={GridSize}, SpatialExtent={SpatialExtent:F2}, Norm={Norm:F6}]";
    }

    #endregion

    #region Private Methods

    private static double[] CreatePositionGrid(int gridSize, double spatialExtent)
    {
        var positions = new double[gridSize];
        var deltaX = spatialExtent / gridSize;
        var startX = -spatialExtent / 2.0;

        for (int i = 0; i < gridSize; i++)
        {
            positions[i] = startX + i * deltaX;
        }

        return positions;
    }

    private double CalculateNorm()
    {
        var norm = 0.0;
        for (int i = 0; i < GridSize; i++)
        {
            norm += _positionAmplitudes[i].MagnitudeSquared;
        }
        return Math.Sqrt(norm * DeltaX);
    }

    private Complex[] CalculateFourierTransform(Complex[] input)
    {
        // Simplified DFT implementation - in a full implementation, use FFT
        var output = new Complex[input.Length];
        var n = input.Length;

        for (int k = 0; k < n; k++)
        {
            output[k] = Complex.Zero;
            for (int j = 0; j < n; j++)
            {
                var phase = -2.0 * Math.PI * k * j / n;
                var factor = Complex.FromPhase(phase);
                output[k] += input[j] * factor;
            }
            output[k] /= Math.Sqrt(n); // Normalization
        }

        return output;
    }

    private Complex[] CalculateInverseFourierTransform(Complex[] input)
    {
        // Simplified inverse DFT - in a full implementation, use IFFT
        var output = new Complex[input.Length];
        var n = input.Length;

        for (int j = 0; j < n; j++)
        {
            output[j] = Complex.Zero;
            for (int k = 0; k < n; k++)
            {
                var phase = 2.0 * Math.PI * k * j / n;
                var factor = Complex.FromPhase(phase);
                output[j] += input[k] * factor;
            }
            output[j] /= Math.Sqrt(n); // Normalization
        }

        return output;
    }

    private static double CalculateHermitePolynomial(int n, double x)
    {
        // Calculate Hermite polynomial H_n(x) using recurrence relation
        if (n == 0) return 1.0;
        if (n == 1) return 2.0 * x;

        double h0 = 1.0;
        double h1 = 2.0 * x;

        for (int i = 2; i <= n; i++)
        {
            double h2 = 2.0 * x * h1 - 2.0 * (i - 1) * h0;
            h0 = h1;
            h1 = h2;
        }

        return h1;
    }

    private static double Factorial(int n)
    {
        if (n <= 1) return 1.0;
        double result = 1.0;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    #endregion
}
