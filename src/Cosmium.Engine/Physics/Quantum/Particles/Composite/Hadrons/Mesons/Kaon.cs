using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

namespace Cosmium.Engine.Physics.Quantum.Particles.Composite.Hadrons.Mesons;

/// <summary>
/// Implementation of kaon particles as composite mesons containing strange quarks.
/// Kaons are pseudoscalar mesons that exhibit interesting CP violation phenomena.
/// Four types: K⁺ (us̄), K⁻ (sū), K⁰ (ds̄), K̄⁰ (sd̄)
/// </summary>
public class Kaon : QuantumParticleBase, ICompositeParticle
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

    #region Kaon Types

    /// <summary>
    /// Enumeration of kaon types.
    /// </summary>
    public enum KaonType
    {
        /// <summary>
        /// Positive kaon (K⁺) - us̄ composition, charge +e
        /// </summary>
        Positive,

        /// <summary>
        /// Negative kaon (K⁻) - sū composition, charge -e
        /// </summary>
        Negative,

        /// <summary>
        /// Neutral kaon (K⁰) - ds̄ composition, charge 0
        /// </summary>
        Neutral,

        /// <summary>
        /// Anti-neutral kaon (K̄⁰) - sd̄ composition, charge 0
        /// </summary>
        AntiNeutral
    }

    #endregion

    #region Constants

    /// <summary>
    /// Charged kaon rest mass in kg (PDG 2022 value).
    /// </summary>
    public static readonly double ChargedKaonMass = 493.677e6 * 1.602176634e-13 / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // 493.677 MeV/c²

    /// <summary>
    /// Neutral kaon rest mass in kg (PDG 2022 value).
    /// </summary>
    public static readonly double NeutralKaonMass = 497.611e6 * 1.602176634e-13 / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // 497.611 MeV/c²

    /// <summary>
    /// Charged kaon lifetime in seconds (PDG 2022 value).
    /// </summary>
    public static readonly double ChargedKaonLifetime = 1.2380e-8; // seconds

    /// <summary>
    /// Short-lived neutral kaon (K_S) lifetime in seconds (PDG 2022 value).
    /// </summary>
    public static readonly double KShortLifetime = 8.954e-11; // seconds

    /// <summary>
    /// Long-lived neutral kaon (K_L) lifetime in seconds (PDG 2022 value).
    /// </summary>
    public static readonly double KLongLifetime = 5.116e-8; // seconds

    /// <summary>
    /// Kaon charge radius in meters (approximate).
    /// </summary>
    public static readonly double ChargeRadius = 0.56e-15; // meters

    /// <summary>
    /// Binding energy of quarks in kaon (approximate value).
    /// </summary>
    public static readonly double QuarkBindingEnergy = 7.9e-11; // ~494 MeV in Joules

    #endregion

    #region Properties

    /// <summary>
    /// Gets the type of this kaon.
    /// </summary>
    public KaonType Type { get; }

    /// <summary>
    /// Gets the strangeness quantum number.
    /// </summary>
    public int Strangeness => Type switch
    {
        KaonType.Positive => 1,    // K⁺ has strangeness +1
        KaonType.Negative => -1,   // K⁻ has strangeness -1
        KaonType.Neutral => 1,     // K⁰ has strangeness +1
        KaonType.AntiNeutral => -1, // K̄⁰ has strangeness -1
        _ => 0
    };

    /// <summary>
    /// Gets the lifetime of this kaon type (simplified - neutral kaons have complex mixing).
    /// </summary>
    public double Lifetime => Type == KaonType.Positive || Type == KaonType.Negative 
        ? ChargedKaonLifetime 
        : KShortLifetime; // Simplified - real K⁰ system is more complex

    /// <summary>
    /// Gets the decay rate of this kaon type.
    /// </summary>
    public double DecayRate => 1.0 / Lifetime;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new kaon particle of the specified type.
    /// </summary>
    /// <param name="type">The type of kaon to create.</param>
    public Kaon(KaonType type) : base(GetKaonName(type), GetKaonSymbol(type), isElementary: false, hilbertSpaceDimension: 2)
    {
        Type = type;
        _constituents = new List<IQuantumParticle>();
        
        // Get mass and charge based on kaon type
        var mass = (type == KaonType.Neutral || type == KaonType.AntiNeutral) ? NeutralKaonMass : ChargedKaonMass;
        var charge = GetKaonCharge(type);
        
        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", mass);
        _charge = new ScalarMeasurableProperty("Charge", "C", charge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", QuarkBindingEnergy);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", ChargeRadius);

        // Create constituent quarks based on kaon type
        InitializeQuarkComposition();

        // Initialize composite quantum state
        InitializeCompositeState();

        Logger.Debug($"Created {type} kaon with quark composition", 
            new { ParticleId = Id, Type = type, ConstituentCount = _constituents.Count, Strangeness });
    }

    /// <summary>
    /// Initializes a kaon with custom constituent quarks (for advanced usage).
    /// </summary>
    /// <param name="type">The type of kaon.</param>
    /// <param name="quark">The quark constituent.</param>
    /// <param name="antiquark">The antiquark constituent.</param>
    public Kaon(KaonType type, Quark quark, Quark antiquark) 
        : base(GetKaonName(type), GetKaonSymbol(type), isElementary: false, hilbertSpaceDimension: 2)
    {
        Type = type;
        
        // Validate quark composition
        ValidateQuarkComposition(type, quark, antiquark);

        _constituents = new List<IQuantumParticle> { quark, antiquark };
        
        // Get mass and charge based on kaon type
        var mass = (type == KaonType.Neutral || type == KaonType.AntiNeutral) ? NeutralKaonMass : ChargedKaonMass;
        var charge = GetKaonCharge(type);
        
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

        Logger.Debug($"Created {type} kaon with custom quark composition", 
            new { ParticleId = Id, Type = type, ConstituentCount = _constituents.Count, Strangeness });
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.0; // Kaons are pseudoscalar mesons (spin-0)
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
            // Simplified moment of inertia for roughly spherical kaon
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

    public double TotalAngularMomentumQuantumNumber => 0.0; // J = 0 for ground state kaon
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
    /// Gets the isospin quantum number (I = 1/2 for kaons).
    /// </summary>
    public double Isospin => 0.5;

    /// <summary>
    /// Gets the third component of isospin.
    /// </summary>
    public double IsospinThird => Type switch
    {
        KaonType.Positive => 0.5,    // I₃ = +1/2
        KaonType.Negative => -0.5,   // I₃ = -1/2
        KaonType.Neutral => 0.5,     // I₃ = +1/2
        KaonType.AntiNeutral => -0.5, // I₃ = -1/2
        _ => 0.0
    };

    /// <summary>
    /// Gets the parity (P = -1 for kaons - pseudoscalar mesons).
    /// </summary>
    public int Parity => -1;

    /// <summary>
    /// Gets the C-parity (charge conjugation parity) - not well-defined for kaons due to strangeness.
    /// </summary>
    public int? CParity => null; // Kaons don't have well-defined C-parity

    /// <summary>
    /// Gets the G-parity - not well-defined for strange particles.
    /// </summary>
    public int? GParity => null; // Not defined for strange particles

    /// <summary>
    /// Gets the hypercharge (Y = B + S).
    /// </summary>
    public double Hypercharge => BaryonNumber + Strangeness;

    /// <summary>
    /// Gets the quark in this kaon.
    /// </summary>
    public IEnumerable<Quark> Quarks => GetConstituentsOfType<Quark>().Where(q => !q.IsAntiquark);

    /// <summary>
    /// Gets the antiquark in this kaon.
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
                throw new InvalidOperationException("Kaon already has maximum number of constituents (quark-antiquark pair)");

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
            // For a kaon, we need to construct the quark-antiquark bound state
            // Neutral kaons exhibit interesting quantum mixing phenomena
            
            if (_constituents.Count != 2)
                return StateVector;

            // Create composite state
            var compositeState = new Complex[HilbertSpaceDimension];
            
            // Kaons are spin-0 particles (scalar state)
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
                new { ParticleId = Id, Type, BindingEnergy = totalBindingEnergy, Strangeness });
        }
    }

    public IEnumerable<IQuantumParticle> Decompose()
    {
        Logger.Debug($"Simulating {Type} kaon decay", new { ParticleId = Id, Type, Lifetime, Strangeness });
        
        var products = new List<IQuantumParticle>();
        
        switch (Type)
        {
            case KaonType.Positive:
                // K⁺ → μ⁺ + νμ (63.6%) or K⁺ → π⁺ + π⁰ (20.7%) or other modes
                Logger.Debug("K⁺ decay simulated (multiple possible modes)");
                break;
                
            case KaonType.Negative:
                // K⁻ → μ⁻ + ν̄μ (63.6%) or K⁻ → π⁻ + π⁰ (20.7%) or other modes
                Logger.Debug("K⁻ decay simulated (multiple possible modes)");
                break;
                
            case KaonType.Neutral:
            case KaonType.AntiNeutral:
                // Complex K⁰-K̄⁰ mixing leads to K_S and K_L eigenstates
                // K_S → π⁺ + π⁻ or π⁰ + π⁰
                // K_L → π⁺ + π⁻ + π⁰ or μ⁺ + μ⁻ + π⁰ or other modes
                Logger.Debug("Neutral kaon decay simulated (CP violation possible)");
                break;
        }
        
        var energyRelease = GetDecayEnergyRelease();
        OnParticleDecayed(products, GetDecayMode(), energyRelease);
        
        return products;
    }

    public bool IsStable() => false; // All kaons are unstable

    public double CalculateDecayRate() => DecayRate;

    public void EvolveCompositeState(double timeStep)
    {
        var validation = ParameterValidator.ValidatePositive(timeStep, nameof(timeStep));
        validation.ThrowIfInvalid();

        lock (_compositionLock)
        {
            // Update composite state
            StateVector = CalculateCompositeState();
            
            // For neutral kaons, this would involve K⁰-K̄⁰ mixing calculations
            // Simplified here for the basic implementation
            
            // Check for decay probability
            var decayProbability = 1.0 - Math.Exp(-DecayRate * timeStep);
            if (decayProbability > 0.001) // Only log if significant probability
            {
                Logger.Debug($"{Type} kaon decay probability calculated", 
                    new { ParticleId = Id, Type, TimeStep = timeStep, DecayProbability = decayProbability });
            }
        }
    }

    public double CalculateInternalInteractionEnergy()
    {
        lock (_compositionLock)
        {
            if (_constituents.Count < 2) return 0.0;

            // Calculate quark-antiquark interaction including strange quark mass difference
            var quarks = GetConstituentsOfType<Quark>().ToArray();
            if (quarks.Length != 2) return 0.0;

            // Simplified QCD potential for quark-antiquark pair with strange quark corrections
            var distance = ChargeRadius; // Approximate separation
            var coulombTerm = -4.0 * PhysicsConstants.StrongCouplingConstant / (3.0 * distance);
            var confinementTerm = 0.18e9 * distance; // String tension
            
            // Additional binding due to strange quark mass (simplified)
            var strangeMassCorrection = Strangeness != 0 ? 0.1 * coulombTerm : 0.0;

            return coulombTerm + confinementTerm + strangeMassCorrection;
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
    /// Gets the main decay mode for this kaon type.
    /// </summary>
    /// <returns>String describing the decay mode.</returns>
    public string GetDecayMode()
    {
        return Type switch
        {
            KaonType.Positive => "K⁺ → μ⁺ + νμ (primary)",
            KaonType.Negative => "K⁻ → μ⁻ + ν̄μ (primary)",
            KaonType.Neutral => "K⁰ → K_S/K_L → various modes",
            KaonType.AntiNeutral => "K̄⁰ → K_S/K_L → various modes",
            _ => "Unknown decay"
        };
    }

    /// <summary>
    /// Gets the energy release for typical decay modes.
    /// </summary>
    /// <returns>Energy release in Joules.</returns>
    public double GetDecayEnergyRelease()
    {
        return Type switch
        {
            KaonType.Positive => 387e6 * 1.602176634e-13,   // ~387 MeV
            KaonType.Negative => 387e6 * 1.602176634e-13,   // ~387 MeV
            KaonType.Neutral => 498e6 * 1.602176634e-13,    // ~498 MeV (variable)
            KaonType.AntiNeutral => 498e6 * 1.602176634e-13, // ~498 MeV (variable)
            _ => 0.0
        };
    }

    /// <summary>
    /// Simulates K⁰-K̄⁰ mixing for neutral kaons (simplified).
    /// In reality, this involves complex CP violation phenomena.
    /// </summary>
    /// <returns>True if the kaon oscillated to its antiparticle state.</returns>
    public bool SimulateKaonOscillation()
    {
        if (Type != KaonType.Neutral && Type != KaonType.AntiNeutral)
            return false;

        // Simplified oscillation probability - real calculation involves mass difference
        var oscillationProbability = 0.5 * (1.0 - Math.Cos(2.0 * 3.484e10 * Lifetime)); // Δm ≈ 3.484×10¹⁰ s⁻¹
        var random = new Random();
        
        if (random.NextDouble() < oscillationProbability)
        {
            Logger.Debug($"Kaon oscillation occurred: {Type} ↔ {(Type == KaonType.Neutral ? KaonType.AntiNeutral : KaonType.Neutral)}");
            return true;
        }
        
        return false;
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Kaons participate in strong interactions with other hadrons
        return other is ICompositeParticle || other.Name == "Gluon";
    }

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Charged kaons interact electromagnetically
        return !Charge.Value.Equals(0.0) && (other.Name == "Photon" || !other.Charge.Value.Equals(0.0));
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // All particles participate in weak interactions
        // Kaons are particularly important for weak interactions due to strangeness changing
        return true;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        var validation = ParameterValidator.ValidatePositive(distance, nameof(distance));
        validation.ThrowIfInvalid();

        if (other is ICompositeParticle)
        {
            // Kaon-nucleon interaction (less strong than pion due to heavier mass)
            var range = PhysicsConstants.ReducedPlanckConstant / (Mass.Value * PhysicsConstants.SpeedOfLight);
            return 0.8 * PhysicsConstants.StrongCouplingConstant * Math.Exp(-distance / range) / distance;
        }

        // Electromagnetic interaction for charged kaons
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
        var clone = new Kaon(Type);
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        return Type switch
        {
            KaonType.Positive => new Kaon(KaonType.Negative),
            KaonType.Negative => new Kaon(KaonType.Positive),
            KaonType.Neutral => new Kaon(KaonType.AntiNeutral),
            KaonType.AntiNeutral => new Kaon(KaonType.Neutral),
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
                case KaonType.Positive:
                    // K⁺: us̄ (up quark + strange antiquark)
                    _constituents.Add(new Quark(QuarkType.Up, QuarkColor.Red));
                    _constituents.Add(new Quark(QuarkType.Strange, QuarkColor.Red, isAntiquark: true));
                    break;
                    
                case KaonType.Negative:
                    // K⁻: sū (strange quark + up antiquark)
                    _constituents.Add(new Quark(QuarkType.Strange, QuarkColor.Red));
                    _constituents.Add(new Quark(QuarkType.Up, QuarkColor.Red, isAntiquark: true));
                    break;
                    
                case KaonType.Neutral:
                    // K⁰: ds̄ (down quark + strange antiquark)
                    _constituents.Add(new Quark(QuarkType.Down, QuarkColor.Red));
                    _constituents.Add(new Quark(QuarkType.Strange, QuarkColor.Red, isAntiquark: true));
                    break;
                    
                case KaonType.AntiNeutral:
                    // K̄⁰: sd̄ (strange quark + down antiquark)
                    _constituents.Add(new Quark(QuarkType.Strange, QuarkColor.Red));
                    _constituents.Add(new Quark(QuarkType.Down, QuarkColor.Red, isAntiquark: true));
                    break;
            }

            UpdateCompositeProperties();
        }
    }

    private void ValidateQuarkComposition(KaonType type, Quark quark, Quark antiquark)
    {
        if (!antiquark.IsAntiquark)
            throw new ArgumentException("Second constituent must be an antiquark");
        
        if (quark.IsAntiquark)
            throw new ArgumentException("First constituent must be a quark, not an antiquark");

        // Check color neutrality (color-anticolor pair)
        if (quark.Color != antiquark.Color)
            throw new ArgumentException("Quark and antiquark must have the same color for color neutrality");

        // Validate quark types for specific kaon types
        switch (type)
        {
            case KaonType.Positive:
                if (quark.Type != QuarkType.Up || antiquark.Type != QuarkType.Strange)
                    throw new ArgumentException("K⁺ requires up quark and strange antiquark");
                break;
                
            case KaonType.Negative:
                if (quark.Type != QuarkType.Strange || antiquark.Type != QuarkType.Up)
                    throw new ArgumentException("K⁻ requires strange quark and up antiquark");
                break;
                
            case KaonType.Neutral:
                if (quark.Type != QuarkType.Down || antiquark.Type != QuarkType.Strange)
                    throw new ArgumentException("K⁰ requires down quark and strange antiquark");
                break;
                
            case KaonType.AntiNeutral:
                if (quark.Type != QuarkType.Strange || antiquark.Type != QuarkType.Down)
                    throw new ArgumentException("K̄⁰ requires strange quark and down antiquark");
                break;
        }
    }

    private void InitializeCompositeState()
    {
        // Initialize in ground state (spin-0)
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    private static string GetKaonName(KaonType type)
    {
        return type switch
        {
            KaonType.Positive => "Positive Kaon",
            KaonType.Negative => "Negative Kaon",
            KaonType.Neutral => "Neutral Kaon",
            KaonType.AntiNeutral => "Anti-Neutral Kaon",
            _ => "Unknown Kaon"
        };
    }

    private static string GetKaonSymbol(KaonType type)
    {
        return type switch
        {
            KaonType.Positive => "K⁺",
            KaonType.Negative => "K⁻",
            KaonType.Neutral => "K⁰",
            KaonType.AntiNeutral => "K̄⁰",
            _ => "K"
        };
    }

    private static double GetKaonCharge(KaonType type)
    {
        return type switch
        {
            KaonType.Positive => PhysicsConstants.ElementaryCharge,
            KaonType.Negative => -PhysicsConstants.ElementaryCharge,
            KaonType.Neutral => 0.0,
            KaonType.AntiNeutral => 0.0,
            _ => 0.0
        };
    }

    private static List<ExcitedState> CreateExcitedStates()
    {
        // Define known kaon excited states (K*)
        return new List<ExcitedState>
        {
            new ExcitedState
            {
                Energy = 892e6 * 1.602176634e-13, // K*(892) resonance in Joules
                QuantumNumbers = new Dictionary<string, double> { {"J", 1.0}, {"P", -1}, {"I", 0.5} },
                Lifetime = 1.3e-23, // ~1.3×10⁻²³ seconds
                DecayChannels = new List<DecayChannel>
                {
                    new DecayChannel { BranchingRatio = 1.0, EnergyRelease = 400e6 * 1.602176634e-13 }
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
