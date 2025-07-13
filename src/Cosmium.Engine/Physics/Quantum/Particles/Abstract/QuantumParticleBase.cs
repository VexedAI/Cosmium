using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;

namespace Cosmium.Engine.Physics.Quantum.Particles.Abstract;

/// <summary>
/// Base implementation for quantum particles providing common functionality.
/// This abstract class implements the common behavior and properties shared by all quantum particles.
/// </summary>
public abstract class QuantumParticleBase : IQuantumParticle
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _stateLock = new();
    private Complex[] _stateVector;
    private bool _isMeasured;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the QuantumParticleBase class.
    /// </summary>
    /// <param name="name">The name of the particle.</param>
    /// <param name="symbol">The symbol representing the particle.</param>
    /// <param name="isElementary">Whether this is an elementary particle.</param>
    /// <param name="hilbertSpaceDimension">The dimension of the particle's Hilbert space.</param>
    protected QuantumParticleBase(string name, string symbol, bool isElementary, int hilbertSpaceDimension)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Particle name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Particle symbol cannot be null or empty", nameof(symbol));
        
        var dimensionValidation = ParameterValidator.ValidateIntegerRange(hilbertSpaceDimension, nameof(hilbertSpaceDimension), 2, 1 << 20);
        dimensionValidation.ThrowIfInvalid();

        Id = Guid.NewGuid();
        Name = name;
        Symbol = symbol;
        IsElementary = isElementary;
        CreationTime = DateTime.UtcNow;
        HilbertSpaceDimension = hilbertSpaceDimension;

        // Initialize quantum state to ground state (first basis vector)
        _stateVector = new Complex[hilbertSpaceDimension];
        _stateVector[0] = new Complex(1.0, 0.0); // |0⟩ state
        
        // Initialize measurable properties
        InitializeMeasurableProperties();

        Logger.Debug($"Created {(isElementary ? "elementary" : "composite")} particle: {name} ({symbol})", 
            new { ParticleId = Id, HilbertDimension = hilbertSpaceDimension });
    }

    #endregion

    #region Basic Properties

    public Guid Id { get; }
    public string Name { get; }
    public string Symbol { get; }
    public bool IsElementary { get; }
    public DateTime CreationTime { get; }

    #endregion

    #region Abstract Properties (Must be implemented by derived classes)

    /// <summary>
    /// Gets the rest mass of the particle. Must be implemented by derived classes.
    /// </summary>
    public abstract IScalarMeasurable Mass { get; }

    /// <summary>
    /// Gets the electric charge of the particle. Must be implemented by derived classes.
    /// </summary>
    public abstract IScalarMeasurable Charge { get; }

    /// <summary>
    /// Gets the spin quantum number. Must be implemented by derived classes.
    /// </summary>
    public abstract double SpinQuantumNumber { get; }

    /// <summary>
    /// Gets the particle statistics. Must be implemented by derived classes.
    /// </summary>
    public abstract ParticleStatistics Statistics { get; }

    #endregion

    #region Quantum State Properties

    public Complex[] StateVector
    {
        get
        {
            lock (_stateLock)
            {
                return (Complex[])_stateVector.Clone();
            }
        }
        protected set
        {
            var validation = ParameterValidator.ValidateQuantumState(value, nameof(StateVector));
            validation.ThrowIfInvalid();

            lock (_stateLock)
            {
                var oldState = (Complex[])_stateVector.Clone();
                _stateVector = (Complex[])value.Clone();
                OnStateChanged(oldState, _stateVector, "Direct state assignment");
            }
        }
    }

    public int HilbertSpaceDimension { get; }

    public virtual bool IsPureState => true; // Base implementation assumes pure states

    public virtual double Normalization
    {
        get
        {
            lock (_stateLock)
            {
                return Math.Sqrt(_stateVector.Sum(amplitude => 
                    amplitude.Real * amplitude.Real + amplitude.Imaginary * amplitude.Imaginary));
            }
        }
    }

    #endregion

    #region Computed Properties

    public virtual bool IsFermion => (SpinQuantumNumber % 1.0) != 0.0; // Half-integer spin

    #endregion

    #region Measurable Properties

    public IVectorMeasurable Spin { get; private set; } = null!;
    public IVectorMeasurable Position { get; private set; } = null!;
    public IVectorMeasurable Momentum { get; private set; } = null!;
    public IScalarMeasurable Energy { get; private set; } = null!;
    public IScalarMeasurable KineticEnergy { get; private set; } = null!;
    public IVectorMeasurable Velocity { get; private set; } = null!;

    #endregion

    #region Quantum Mechanics Operations

    public virtual void EvolveState(Complex[,] hamiltonian, double timeStep)
    {
        var hamiltonianValidation = ParameterValidator.ValidateFiniteComplexMatrix(hamiltonian, nameof(hamiltonian));
        hamiltonianValidation.ThrowIfInvalid();
        
        var timeValidation = ParameterValidator.ValidatePositive(timeStep, nameof(timeStep));
        timeValidation.ThrowIfInvalid();

        using var operation = Logger.BeginScope("QuantumStateEvolution", 
            new { ParticleId = Id, TimeStep = timeStep });

        lock (_stateLock)
        {
            var oldState = (Complex[])_stateVector.Clone();
            
            // Time evolution: |ψ(t+Δt)⟩ = exp(-iHΔt/ℏ)|ψ(t)⟩
            // For small time steps, use first-order approximation: |ψ(t+Δt)⟩ ≈ (I - iHΔt/ℏ)|ψ(t)⟩
            var evolutionFactor = -Complex.ImaginaryOne * timeStep / PhysicsConstants.ReducedPlanckConstant;
            
            var newState = new Complex[HilbertSpaceDimension];
            
            // Apply (I - iHΔt/ℏ) to the state vector
            for (int i = 0; i < HilbertSpaceDimension; i++)
            {
                newState[i] = _stateVector[i]; // Identity term
                
                // Hamiltonian term: -iHΔt/ℏ|ψ⟩
                for (int j = 0; j < HilbertSpaceDimension; j++)
                {
                    newState[i] += evolutionFactor * hamiltonian[i, j] * _stateVector[j];
                }
            }
            
            // Normalize the new state
            var norm = Math.Sqrt(newState.Sum(amp => amp.Real * amp.Real + amp.Imaginary * amp.Imaginary));
            for (int i = 0; i < HilbertSpaceDimension; i++)
            {
                newState[i] /= norm;
            }
            
            _stateVector = newState;
            OnStateChanged(oldState, _stateVector, "Time evolution");
        }
    }

    public virtual double MeasureObservable(Complex[,] observable)
    {
        var validation = ParameterValidator.ValidateFiniteComplexMatrix(observable, nameof(observable));
        validation.ThrowIfInvalid();

        using var operation = Logger.BeginScope("ObservableMeasurement", 
            new { ParticleId = Id, ObservableSize = $"{observable.GetLength(0)}x{observable.GetLength(1)}" });

        lock (_stateLock)
        {
            var stateBeforeMeasurement = (Complex[])_stateVector.Clone();
            
            // Calculate expectation value first
            var expectationValue = CalculateExpectationValue(observable);
            
            // For simplicity, assume measurement collapses to an eigenstate
            // In a full implementation, we would:
            // 1. Find eigenvalues and eigenvectors of the observable
            // 2. Calculate probabilities for each eigenvalue
            // 3. Randomly select an eigenvalue based on probabilities
            // 4. Collapse the state to the corresponding eigenstate
            
            // For now, simulate measurement by adding uncertainty
            var random = new Random();
            var uncertainty = CalculateUncertainty(observable);
            var measuredValue = expectationValue + (random.NextDouble() - 0.5) * uncertainty;
            
            _isMeasured = true;
            
            OnMeasurementPerformed("Observable", measuredValue, uncertainty, 
                stateBeforeMeasurement, _stateVector);
            
            return measuredValue;
        }
    }

    public virtual double CalculateExpectationValue(Complex[,] observable)
    {
        var validation = ParameterValidator.ValidateFiniteComplexMatrix(observable, nameof(observable));
        validation.ThrowIfInvalid();

        lock (_stateLock)
        {
            // ⟨ψ|Ô|ψ⟩ = Σᵢⱼ ψᵢ* Ôᵢⱼ ψⱼ
            var expectationValue = Complex.Zero;
            
            for (int i = 0; i < HilbertSpaceDimension; i++)
            {
                for (int j = 0; j < HilbertSpaceDimension; j++)
                {
                    expectationValue += Complex.Conjugate(_stateVector[i]) * observable[i, j] * _stateVector[j];
                }
            }
            
            // Expectation value should be real for Hermitian operators
            return expectationValue.Real;
        }
    }

    public virtual double CalculateMeasurementProbability(Complex[,] observable, double eigenvalue)
    {
        var validation = ParameterValidator.ValidateFiniteComplexMatrix(observable, nameof(observable));
        validation.ThrowIfInvalid();

        // This is a simplified implementation
        // In a complete implementation, we would find the eigenvector corresponding to the eigenvalue
        // and calculate |⟨eigenvector|ψ⟩|²
        
        var expectationValue = CalculateExpectationValue(observable);
        var uncertainty = CalculateUncertainty(observable);
        
        // Use Gaussian approximation for probability
        var gaussian = Math.Exp(-0.5 * Math.Pow((eigenvalue - expectationValue) / uncertainty, 2));
        return gaussian / (uncertainty * Math.Sqrt(2 * Math.PI));
    }

    #endregion

    #region Interaction Methods

    public virtual bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Particles can interact electromagnetically if they have electric charge
        return !Charge.Value.Equals(0.0) && !other.Charge.Value.Equals(0.0);
    }

    public virtual bool CanInteractWeakly(IQuantumParticle other)
    {
        // All particles except photons can interact via weak force
        return Name != "Photon" && other.Name != "Photon";
    }

    public virtual bool CanInteractStrongly(IQuantumParticle other)
    {
        // Only quarks and gluons interact via strong force (simplified)
        return false; // Base implementation - override in derived classes
    }

    public virtual double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        var distanceValidation = ParameterValidator.ValidatePositive(distance, nameof(distance));
        distanceValidation.ThrowIfInvalid();

        // Default to electromagnetic interaction if both particles are charged
        if (CanInteractElectromagnetically(other))
        {
            // Coulomb force: F = k * q₁ * q₂ / r²
            var k = PhysicsConstants.CoulombConstant;
            var q1 = Charge.Value;
            var q2 = other.Charge.Value;
            return k * q1 * q2 / (distance * distance);
        }
        
        return 0.0; // No interaction
    }

    #endregion

    #region Cloning and Antiparticles

    public abstract IQuantumParticle Clone();

    public virtual IQuantumParticle? GetAntiparticle()
    {
        // Default implementation returns null - override in derived classes
        return null;
    }

    #endregion

    #region Events

    public event EventHandler<QuantumStateChangedEventArgs>? StateChanged;
    public event EventHandler<MeasurementPerformedEventArgs>? MeasurementPerformed;
    public event EventHandler<ParticleInteractionEventArgs>? InteractionOccurred;

    #endregion

    #region Protected Methods

    protected virtual void InitializeMeasurableProperties()
    {
        // Initialize with default implementations
        // Derived classes should override to provide specific implementations
        Spin = new VectorMeasurableProperty("Spin", "ℏ", new double[3]);
        Position = new VectorMeasurableProperty("Position", "m", new double[3]);
        Momentum = new VectorMeasurableProperty("Momentum", "kg⋅m/s", new double[3]);
        Energy = new ScalarMeasurableProperty("Energy", "J", 0.0);
        KineticEnergy = new ScalarMeasurableProperty("KineticEnergy", "J", 0.0);
        Velocity = new VectorMeasurableProperty("Velocity", "m/s", new double[3]);
    }

    protected virtual void OnStateChanged(Complex[] oldState, Complex[] newState, string reason)
    {
        StateChanged?.Invoke(this, new QuantumStateChangedEventArgs
        {
            OldState = oldState,
            NewState = newState,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        });
    }

    protected virtual void OnMeasurementPerformed(string observableName, double measuredValue, 
        double uncertainty, Complex[] stateBefore, Complex[] stateAfter)
    {
        MeasurementPerformed?.Invoke(this, new MeasurementPerformedEventArgs
        {
            ObservableName = observableName,
            MeasuredValue = measuredValue,
            Uncertainty = uncertainty,
            StateBeforeMeasurement = stateBefore,
            StateAfterMeasurement = stateAfter,
            Timestamp = DateTime.UtcNow
        });
    }

    protected virtual void OnInteractionOccurred(IQuantumParticle other, string interactionType, 
        double strength, double distance)
    {
        InteractionOccurred?.Invoke(this, new ParticleInteractionEventArgs
        {
            OtherParticle = other,
            InteractionType = interactionType,
            InteractionStrength = strength,
            Distance = distance,
            Timestamp = DateTime.UtcNow
        });
    }

    private double CalculateUncertainty(Complex[,] observable)
    {
        // Calculate uncertainty: Δ = √(⟨Ô²⟩ - ⟨Ô⟩²)
        var expectation = CalculateExpectationValue(observable);
        
        // Calculate Ô²
        var observableSquared = new Complex[HilbertSpaceDimension, HilbertSpaceDimension];
        for (int i = 0; i < HilbertSpaceDimension; i++)
        {
            for (int j = 0; j < HilbertSpaceDimension; j++)
            {
                observableSquared[i, j] = Complex.Zero;
                for (int k = 0; k < HilbertSpaceDimension; k++)
                {
                    observableSquared[i, j] += observable[i, k] * observable[k, j];
                }
            }
        }
        
        var expectationSquared = CalculateExpectationValue(observableSquared);
        return Math.Sqrt(Math.Max(0, expectationSquared - expectation * expectation));
    }

    #endregion
}

/// <summary>
/// Simple implementation of IScalarMeasurable for use in base class.
/// </summary>
internal class ScalarMeasurableProperty : IScalarMeasurable
{
    private double _value;
    private bool _isMeasured;
    private DateTime? _lastMeasurementTime;

    public ScalarMeasurableProperty(string name, string units, double initialValue)
    {
        Name = name;
        Units = units;
        _value = initialValue;
    }

    public string Name { get; }
    public string Units { get; }
    public double Value => _value;
    public double Uncertainty { get; private set; } = 0.0;
    public bool IsMeasured => _isMeasured;
    public DateTime? LastMeasurementTime => _lastMeasurementTime;

    public double Measure()
    {
        _isMeasured = true;
        _lastMeasurementTime = DateTime.UtcNow;
        return _value;
    }

    public void ResetMeasurement()
    {
        _isMeasured = false;
        _lastMeasurementTime = null;
    }

    public double GetValueInUnits(string unitSystem)
    {
        // Simplified - would need full unit conversion in real implementation
        return _value;
    }

    public void SetValue(double value, double uncertainty = 0.0)
    {
        _value = value;
        Uncertainty = uncertainty;
    }
}

/// <summary>
/// Simple implementation of IVectorMeasurable for use in base class.
/// </summary>
internal class VectorMeasurableProperty : IVectorMeasurable
{
    private double[] _value;
    private bool _isMeasured;
    private DateTime? _lastMeasurementTime;

    public VectorMeasurableProperty(string name, string units, double[] initialValue)
    {
        Name = name;
        Units = units;
        _value = (double[])initialValue.Clone();
    }

    public string Name { get; }
    public string Units { get; }
    public double[] Value => (double[])_value.Clone();
    public double Uncertainty { get; private set; } = 0.0;
    public bool IsMeasured => _isMeasured;
    public DateTime? LastMeasurementTime => _lastMeasurementTime;
    public double Magnitude => Math.Sqrt(_value.Sum(x => x * x));
    public double X => _value.Length > 0 ? _value[0] : 0.0;
    public double Y => _value.Length > 1 ? _value[1] : 0.0;
    public double Z => _value.Length > 2 ? _value[2] : 0.0;
    public double[] Direction
    {
        get
        {
            var mag = Magnitude;
            return mag > 0 ? _value.Select(x => x / mag).ToArray() : new double[_value.Length];
        }
    }

    public double[] Measure()
    {
        _isMeasured = true;
        _lastMeasurementTime = DateTime.UtcNow;
        return Value;
    }

    public void ResetMeasurement()
    {
        _isMeasured = false;
        _lastMeasurementTime = null;
    }

    public void SetValue(double[] value, double uncertainty = 0.0)
    {
        _value = (double[])value.Clone();
        Uncertainty = uncertainty;
    }
}
