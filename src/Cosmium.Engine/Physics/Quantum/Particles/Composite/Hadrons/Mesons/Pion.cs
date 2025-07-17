using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

namespace Cosmium.Engine.Physics.Quantum.Particles.Composite.Hadrons.Mesons;

/// <summary>
/// Implementation of pion particles as composite mesons made of quark-antiquark pairs.
/// Pions are the lightest mesons and play a crucial role in nuclear force as exchange particles.
/// Three types: π⁺ (ud̄), π⁻ (dū), π⁰ (uū-dd̄)/√2
/// </summary>
public class Pion : QuantumParticleBase, ICompositeParticle
{
    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly List<IQuantumParticle> _constituents;
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _bindingEnergy;
    private readonly ScalarMeasurableProperty _excitationEnergy;
    private readonly VectorMeasurableProperty _centerOfMass;
    private readonly ScalarMeasurableProperty _size;
    private readonly object _compositionLock = new();

    #region Pion Types

    /// <summary>
    /// Enumeration of pion types.
    /// </summary>
    public enum PionType
    {
        /// <summary>
        /// Positive pion (π⁺) - ud̄ composition, charge +e
        /// </summary>
        Positive,

        /// <summary>
        /// Negative pion (π⁻) - dū composition, charge -e
        /// </summary>
        Negative,

        /// <summary>
        /// Neutral pion (π⁰) - (uū-dd̄)/√2 composition, charge 0
        /// </summary>
        Neutral
    }

    #endregion

    #region Constants

    /// <summary>
    /// Charged pion rest mass in kg (PDG 2022 value).
    /// </summary>
    public static readonly double ChargedPionMass = 139.57039e6 * 1.602176634e-13 / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // 139.57 MeV/c²

    /// <summary>
    /// Neutral pion rest mass in kg (PDG 2022 value).
    /// </summary>
    public static readonly double NeutralPionMass = 134.9768e6 * 1.602176634e-13 / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // 134.98 MeV/c²

    /// <summary>
    /// Charged pion lifetime in seconds (PDG 2022 value).
    /// </summary>
    public static readonly double ChargedPionLifetime = 2.6033e-8; // seconds

    /// <summary>
    /// Neutral pion lifetime in seconds (PDG 2022 value).
    /// </summary>
    public static readonly double NeutralPionLifetime = 8.52e-17; // seconds

    /// <summary>
    /// Pion charge radius in meters (approximate).
    /// </summary>
    public static readonly double ChargeRadius = 0.67e-15; // meters

    /// <summary>
    /// Binding energy of quarks in pion (approximate value).
    /// </summary>
    public static readonly double QuarkBindingEnergy = 2.235e-11; // ~140 MeV in Joules

    #endregion

    #region Properties

    /// <summary>
    /// Gets the type of this pion.
    /// </summary>
    public PionType Type { get; }

    /// <summary>
    /// Gets the lifetime of this pion type.
    /// </summary>
    public double Lifetime => Type == PionType.Neutral ? NeutralPionLifetime : ChargedPionLifetime;

    /// <summary>
    /// Gets the decay rate of this pion type.
    /// </summary>
    public double DecayRate => 1.0 / Lifetime;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new pion particle of the specified type.
    /// </summary>
    /// <param name="type">The type of pion to create.</param>
    public Pion(PionType type) : base(GetPionName(type), GetPionSymbol(type), isElementary: false, hilbertSpaceDimension: 2)
    {
        Type = type;
        _constituents = new List<IQuantumParticle>();
        
        // Get mass and charge based on pion type
        var mass = type == PionType.Neutral ? NeutralPionMass : ChargedPionMass;
        var charge = GetPionCharge(type);
        
        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", mass);
        _charge = new ScalarMeasurableProperty("Charge", "C", charge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", QuarkBindingEnergy);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", ChargeRadius);

        // Create constituent quarks based on pion type
        InitializeQuarkComposition();

        // Initialize composite quantum state
        InitializeCompositeState();

        Logger.Debug($"Created {type} pion with quark composition", 
            new { ParticleId = Id, Type = type, ConstituentCount = _constituents.Count });
    }

    /// <summary>
    /// Initializes a pion with custom constituent quarks (for advanced usage).
    /// </summary>
    /// <param name="type">The type of pion.</param>
    /// <param name="quark">The quark constituent.</param>
    /// <param name="antiquark">The antiquark constituent.</param>
    public Pion(PionType type, Quark quark, Quark antiquark) 
        : base(GetPionName(type), GetPionSymbol(type), isElementary: false, hilbertSpaceDimension: 2)
    {
        Type = type;
        
        // Validate quark composition
        ValidateQuarkComposition(type, quark, antiquark);

        _constituents = new List<IQuantumParticle> { quark, antiquark };
        
        // Get mass and charge based on pion type
        var mass = type == PionType.Neutral ? NeutralPionMass : ChargedPionMass;
        var charge = GetPionCharge(type);
        
        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", mass);
        _charge = new ScalarMeasurableProperty("Charge", "C", charge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", QuarkBindingEnergy);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", ChargeRadius);

        // Update composite properties
        UpdateCompositeProperties();
        InitializeCompositeState();

        Logger.Debug($"Created {type} pion with custom quark composition", 
            new { ParticleId = Id, Type = type, ConstituentCount = _constituents.Count });
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.0; // Pions are scalar mesons (spin-0)
    public override ParticleStatistics Statistics => ParticleStatistics.BoseEinstein; // Bosons

    #endregion

    #region ICompositeParticle Implementation

    public IReadOnlyList<IQuantumParticle> Constituents
    {
        get
        {
            lock (_compositionLock)
            {
                return _constituents.AsReadOnly();
            }
        }
    }

    public int ConstituentCount => _constituents.Count;
    public IScalarMeasurable BindingEnergy => _bindingEnergy;
    public IScalarMeasurable ExcitationEnergy => _excitationEnergy;
    public IVectorMeasurable CenterOfMass => _centerOfMass;
    public IScalarMeasurable Size => _size;

    public double[,] MomentOfInertiaTensor
    {
        get
        {
            // Simplified moment of inertia for roughly spherical pion
            var I = 0.4 * Mass.Value * ChargeRadius * ChargeRadius; // I = (2/5)MR² for solid sphere
            return new double[,]
            {
                { I, 0, 0 },
                { 0, I, 0 },
                { 0, 0, I }
            };
        }
    }

    public IReadOnlyList<VibrationalMode> VibrationalModes { get; } = new List<VibrationalMode>();
    public IReadOnlyList<RotationalMode> RotationalModes { get; } = new List<RotationalMode>();

    public double TotalAngularMomentumQuantumNumber => 0.0; // J = 0 for ground state pion
    public double GroundStateEnergy => Mass.Value * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
    public IReadOnlyList<ExcitedState> ExcitedStates { get; } = CreateExcitedStates();
    public int ExcitationLevel { get; private set; } = 0; // Ground state

    public double[,] ConstituentInteractionMatrix
    {
        get
        {
            // 2x2 matrix for quark-antiquark interactions
            var strongCoupling = PhysicsConstants.StrongCouplingConstant;
            return new double[,]
            {
                { 0, strongCoupling },      // quark-antiquark interaction
                { strongCoupling, 0 }       // antiquark-quark interaction
            };
        }
    }

    #endregion

    #region Meson-Specific Properties

    /// <summary>
    /// Gets the baryon number (0 for mesons).
    /// </summary>
    public double BaryonNumber => 0.0;

    /// <summary>
    /// Gets the isospin quantum number (I = 1 for pion triplet).
    /// </summary>
    public double Isospin => 1.0;

    /// <summary>
    /// Gets the third component of isospin.
    /// </summary>
    public double IsospinThird => Type switch
    {
        PionType.Positive => 1.0,   // I₃ = +1
        PionType.Neutral => 0.0,    // I₃ = 0
        PionType.Negative => -1.0,  // I₃ = -1
        _ => 0.0
    };

    /// <summary>
    /// Gets the parity (P = -1 for pions - pseudoscalar mesons).
    /// </summary>
    public int Parity => -1;

    /// <summary>
    /// Gets the C-parity (charge conjugation parity).
    /// </summary>
    public int CParity => Type == PionType.Neutral ? 1 : 0; // Only π⁰ has C-parity

    /// <summary>
    /// Gets the G-parity (isospin × C-parity).
    /// </summary>
    public int GParity => -1; // For all pions

    /// <summary>
    /// Gets the quark in this pion.
    /// </summary>
    public IEnumerable<Quark> Quarks => GetConstituentsOfType<Quark>().Where(q => !q.IsAntiquark);

    /// <summary>
    /// Gets the antiquark in this pion.
    /// </summary>
    public IEnumerable<Quark> Antiquarks => GetConstituentsOfType<Quark>().Where(q => q.IsAntiquark);

    #endregion

    #region Composite Operations

    public void AddConstituent(IQuantumParticle particle, double bindingEnergy)
    {
        if (particle == null)
            throw new ArgumentNullException(nameof(particle));

        lock (_compositionLock)
        {
            if (_constituents.Count >= 2)
                throw new InvalidOperationException("Pion already has maximum number of constituents (quark-antiquark pair)");

            _constituents.Add(particle);
            UpdateCompositeProperties();
            OnConstituentAdded(particle, bindingEnergy);
        }
    }

    public bool RemoveConstituent(IQuantumParticle particle)
    {
        if (particle == null)
            throw new ArgumentNullException(nameof(particle));

        lock (_compositionLock)
        {
            var removed = _constituents.Remove(particle);
            if (removed)
            {
                UpdateCompositeProperties();
                OnConstituentRemoved(particle, 0.0);
            }
            return removed;
        }
    }

    public IQuantumParticle RemoveConstituentAt(int index)
    {
        var validation = ParameterValidator.ValidateIntegerRange(index, nameof(index), 0, _constituents.Count - 1);
        validation.ThrowIfInvalid();

        lock (_compositionLock)
        {
            var particle = _constituents[index];
            _constituents.RemoveAt(index);
            UpdateCompositeProperties();
            OnConstituentRemoved(particle, 0.0);
            return particle;
        }
    }

    public IQuantumParticle GetConstituent(int index)
    {
        var validation = ParameterValidator.ValidateIntegerRange(index, nameof(index), 0, _constituents.Count - 1);
        validation.ThrowIfInvalid();

        lock (_compositionLock)
        {
            return _constituents[index];
        }
    }

    public IEnumerable<T> GetConstituentsOfType<T>() where T : class, IQuantumParticle
    {
        lock (_compositionLock)
        {
            return _constituents.OfType<T>();
        }
    }

    public Complex[] CalculateCompositeState()
    {
        lock (_compositionLock)
        {
            // For a pion, we need to construct the quark-antiquark bound state
            // This is a simplified model - full QCD would require much more complex calculations
            
            if (_constituents.Count != 2)
                return StateVector;

            // Create composite state
            var compositeState = new Complex[HilbertSpaceDimension];
            
            // Pions are spin-0 particles (scalar state)
            compositeState[0] = new Complex(1.0, 0.0); // |0⟩ (scalar)
            compositeState[1] = new Complex(0.0, 0.0); // unused for spin-0
            
            return compositeState;
        }
    }

    public void UpdateCompositeProperties()
    {
        lock (_compositionLock)
        {
            if (_constituents.Count == 0) return;

            // Update center of mass (simplified to origin for now)
            _centerOfMass.SetValue(new double[3] { 0.0, 0.0, 0.0 });

            // Update binding energy based on constituent interactions
            var totalBindingEnergy = CalculateInternalInteractionEnergy();
            _bindingEnergy.SetValue(Math.Abs(totalBindingEnergy));

            Logger.Debug("Updated composite properties", 
                new { ParticleId = Id, Type, BindingEnergy = totalBindingEnergy });
        }
    }

    public IEnumerable<IQuantumParticle> Decompose()
    {
        Logger.Debug($"Simulating {Type} pion decay", new { ParticleId = Id, Type, Lifetime });
        
        var products = new List<IQuantumParticle>();
        
        switch (Type)
        {
            case PionType.Positive:
                // π⁺ → μ⁺ + νμ (primary decay mode, 99.99%)
                // In a full implementation, we would create muon and muon neutrino
                Logger.Debug("π⁺ → μ⁺ + νμ decay simulated");
                break;
                
            case PionType.Negative:
                // π⁻ → μ⁻ + ν̄μ (primary decay mode, 99.99%)
                // In a full implementation, we would create antimuon and muon antineutrino
                Logger.Debug("π⁻ → μ⁻ + ν̄μ decay simulated");
                break;
                
            case PionType.Neutral:
                // π⁰ → γ + γ (primary decay mode, 98.8%)
                // In a full implementation, we would create two photons
                Logger.Debug("π⁰ → γ + γ decay simulated");
                break;
        }
        
        var energyRelease = GetDecayEnergyRelease();
        OnParticleDecayed(products, GetDecayMode(), energyRelease);
        
        return products;
    }

    public bool IsStable() => false; // All pions are unstable

    public double CalculateDecayRate() => DecayRate;

    public void EvolveCompositeState(double timeStep)
    {
        var validation = ParameterValidator.ValidatePositive(timeStep, nameof(timeStep));
        validation.ThrowIfInvalid();

        lock (_compositionLock)
        {
            // Update composite state
            StateVector = CalculateCompositeState();
            
            // Check for decay probability
            var decayProbability = 1.0 - Math.Exp(-DecayRate * timeStep);
            if (decayProbability > 0.001) // Only log if significant probability
            {
                Logger.Debug($"{Type} pion decay probability calculated", 
                    new { ParticleId = Id, Type, TimeStep = timeStep, DecayProbability = decayProbability });
            }
        }
    }

    public double CalculateInternalInteractionEnergy()
    {
        lock (_compositionLock)
        {
            if (_constituents.Count < 2) return 0.0;

            // Calculate quark-antiquark interaction (simplified)
            var quarks = GetConstituentsOfType<Quark>().ToArray();
            if (quarks.Length != 2) return 0.0;

            // Simplified QCD potential for quark-antiquark pair
            var distance = ChargeRadius; // Approximate separation
            var coulombTerm = -4.0 * PhysicsConstants.StrongCouplingConstant / (3.0 * distance);
            var confinementTerm = 0.18e9 * distance; // String tension

            return coulombTerm + confinementTerm;
        }
    }

    public void UpdateInternalInteractions()
    {
        // Update the internal QCD interactions between quark and antiquark
        UpdateCompositeProperties();
    }

    #endregion

    #region Decay Methods

    /// <summary>
    /// Calculates the probability of decay occurring within a given time interval.
    /// </summary>
    /// <param name="timeInterval">The time interval in seconds.</param>
    /// <returns>The decay probability.</returns>
    public double CalculateDecayProbability(double timeInterval)
    {
        var validation = ParameterValidator.ValidatePositive(timeInterval, nameof(timeInterval));
        validation.ThrowIfInvalid();

        return 1.0 - Math.Exp(-DecayRate * timeInterval);
    }

    /// <summary>
    /// Gets the main decay mode for this pion type.
    /// </summary>
    /// <returns>String describing the decay mode.</returns>
    public string GetDecayMode()
    {
        return Type switch
        {
            PionType.Positive => "π⁺ → μ⁺ + νμ",
            PionType.Negative => "π⁻ → μ⁻ + ν̄μ",
            PionType.Neutral => "π⁰ → γ + γ",
            _ => "Unknown decay"
        };
    }

    /// <summary>
    /// Gets the energy release for the main decay mode.
    /// </summary>
    /// <returns>Energy release in Joules.</returns>
    public double GetDecayEnergyRelease()
    {
        return Type switch
        {
            PionType.Positive => 4.12e6 * 1.602176634e-13,   // ~4.12 MeV
            PionType.Negative => 4.12e6 * 1.602176634e-13,   // ~4.12 MeV
            PionType.Neutral => 134.98e6 * 1.602176634e-13,  // ~135 MeV (full mass)
            _ => 0.0
        };
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Pions participate in strong interactions with other hadrons
        return other is ICompositeParticle || other.Name == "Gluon";
    }

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Charged pions interact electromagnetically
        return !Charge.Value.Equals(0.0) && (other.Name == "Photon" || !other.Charge.Value.Equals(0.0));
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // All particles participate in weak interactions
        return true;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        var validation = ParameterValidator.ValidatePositive(distance, nameof(distance));
        validation.ThrowIfInvalid();

        if (other is ICompositeParticle)
        {
            // Yukawa potential for pion exchange (nuclear force mediator)
            var range = PhysicsConstants.ReducedPlanckConstant / (Mass.Value * PhysicsConstants.SpeedOfLight);
            return PhysicsConstants.StrongCouplingConstant * Math.Exp(-distance / range) / distance;
        }

        // Electromagnetic interaction for charged pions
        if (!Charge.Value.Equals(0.0) && (other.Name == "Photon" || !other.Charge.Value.Equals(0.0)))
        {
            return base.CalculateInteractionStrength(other, distance);
        }

        return 0.0; // No significant interaction
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Pion(Type);
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        return Type switch
        {
            PionType.Positive => new Pion(PionType.Negative),
            PionType.Negative => new Pion(PionType.Positive),
            PionType.Neutral => this, // π⁰ is its own antiparticle
            _ => null
        };
    }

    #endregion

    #region Events

    public event EventHandler<ConstituentChangedEventArgs>? ConstituentAdded;
    public event EventHandler<ConstituentChangedEventArgs>? ConstituentRemoved;
    public event EventHandler<ParticleDecayEventArgs>? ParticleDecayed;
    public event EventHandler<ExcitationChangedEventArgs>? ExcitationChanged;

    #endregion

    #region Private Methods

    private void InitializeQuarkComposition()
    {
        lock (_compositionLock)
        {
            switch (Type)
            {
                case PionType.Positive:
                    // π⁺: ud̄ (up quark + down antiquark)
                    _constituents.Add(new Quark(QuarkType.Up, QuarkColor.Red));
                    _constituents.Add(new Quark(QuarkType.Down, QuarkColor.Red, isAntiquark: true));
                    break;
                    
                case PionType.Negative:
                    // π⁻: dū (down quark + up antiquark)
                    _constituents.Add(new Quark(QuarkType.Down, QuarkColor.Red));
                    _constituents.Add(new Quark(QuarkType.Up, QuarkColor.Red, isAntiquark: true));
                    break;
                    
                case PionType.Neutral:
                    // π⁰: (uū - dd̄)/√2 - simplified as up-antiup pair
                    _constituents.Add(new Quark(QuarkType.Up, QuarkColor.Red));
                    _constituents.Add(new Quark(QuarkType.Up, QuarkColor.Red, isAntiquark: true));
                    break;
            }

            UpdateCompositeProperties();
        }
    }

    private void ValidateQuarkComposition(PionType type, Quark quark, Quark antiquark)
    {
        if (!antiquark.IsAntiquark)
            throw new ArgumentException("Second constituent must be an antiquark");
        
        if (quark.IsAntiquark)
            throw new ArgumentException("First constituent must be a quark, not an antiquark");

        // Check color neutrality (color-anticolor pair)
        if (quark.Color != antiquark.Color)
            throw new ArgumentException("Quark and antiquark must have the same color for color neutrality");

        // Validate quark types for specific pion types
        switch (type)
        {
            case PionType.Positive:
                if (quark.Type != QuarkType.Up || antiquark.Type != QuarkType.Down)
                    throw new ArgumentException("π⁺ requires up quark and down antiquark");
                break;
                
            case PionType.Negative:
                if (quark.Type != QuarkType.Down || antiquark.Type != QuarkType.Up)
                    throw new ArgumentException("π⁻ requires down quark and up antiquark");
                break;
                
            case PionType.Neutral:
                if ((quark.Type != QuarkType.Up || antiquark.Type != QuarkType.Up) &&
                    (quark.Type != QuarkType.Down || antiquark.Type != QuarkType.Down))
                    throw new ArgumentException("π⁰ requires up-antiup or down-antidown pair");
                break;
        }
    }

    private void InitializeCompositeState()
    {
        // Initialize in ground state (spin-0)
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    private static string GetPionName(PionType type)
    {
        return type switch
        {
            PionType.Positive => "Positive Pion",
            PionType.Negative => "Negative Pion",
            PionType.Neutral => "Neutral Pion",
            _ => "Unknown Pion"
        };
    }

    private static string GetPionSymbol(PionType type)
    {
        return type switch
        {
            PionType.Positive => "π⁺",
            PionType.Negative => "π⁻",
            PionType.Neutral => "π⁰",
            _ => "π"
        };
    }

    private static double GetPionCharge(PionType type)
    {
        return type switch
        {
            PionType.Positive => PhysicsConstants.ElementaryCharge,
            PionType.Negative => -PhysicsConstants.ElementaryCharge,
            PionType.Neutral => 0.0,
            _ => 0.0
        };
    }

    private static List<ExcitedState> CreateExcitedStates()
    {
        // Define known pion excited states
        return new List<ExcitedState>
        {
            new ExcitedState
            {
                Energy = 1300e6 * 1.602176634e-13, // a₁(1260) resonance in Joules
                QuantumNumbers = new Dictionary<string, double> { {"J", 1.0}, {"P", 1}, {"I", 1.0} },
                Lifetime = 4e-24, // ~4×10⁻²⁴ seconds
                DecayChannels = new List<DecayChannel>
                {
                    new DecayChannel { BranchingRatio = 1.0, EnergyRelease = 600e6 * 1.602176634e-13 }
                }
            }
        };
    }

    private void OnConstituentAdded(IQuantumParticle constituent, double bindingEnergy)
    {
        ConstituentAdded?.Invoke(this, new ConstituentChangedEventArgs
        {
            Constituent = constituent,
            BindingEnergy = bindingEnergy,
            Timestamp = DateTime.UtcNow
        });
    }

    private void OnConstituentRemoved(IQuantumParticle constituent, double bindingEnergy)
    {
        ConstituentRemoved?.Invoke(this, new ConstituentChangedEventArgs
        {
            Constituent = constituent,
            BindingEnergy = bindingEnergy,
            Timestamp = DateTime.UtcNow
        });
    }

    private void OnParticleDecayed(IEnumerable<IQuantumParticle> products, string decayMode, double energyRelease)
    {
        ParticleDecayed?.Invoke(this, new ParticleDecayEventArgs
        {
            DecayProducts = products,
            DecayMode = decayMode,
            EnergyRelease = energyRelease,
            Timestamp = DateTime.UtcNow
        });
    }

    #endregion
}
