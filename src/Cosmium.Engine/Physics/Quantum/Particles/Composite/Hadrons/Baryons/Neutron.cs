using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

namespace Cosmium.Engine.Physics.Quantum.Particles.Composite.Hadrons.Baryons;

/// <summary>
/// Implementation of a neutron particle as a composite baryon made of quarks.
/// The neutron is composed of one up quark and two down quarks (udd) in a color-neutral configuration.
/// Neutrons are stable within atomic nuclei but undergo beta decay when free.
/// </summary>
public class Neutron : QuantumParticleBase, ICompositeParticle
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

    #region Constants

    /// <summary>
    /// Neutron rest mass in kg (PDG 2022 value).
    /// </summary>
    public static readonly double RestMass = PhysicsConstants.NeutronMass;

    /// <summary>
    /// Neutron electric charge (zero - neutrons are electrically neutral).
    /// </summary>
    public static readonly double ElectricCharge = 0.0;

    /// <summary>
    /// Neutron magnetic moment in J/T (PDG 2022 value).
    /// </summary>
    public static readonly double MagneticMoment = -0.96623651e-26; // μₙ (negative due to spin flip)

    /// <summary>
    /// Neutron charge radius in meters (approximately equal to proton).
    /// </summary>
    public static readonly double ChargeRadius = 0.8414e-15; // meters

    /// <summary>
    /// Binding energy of quarks in neutron (approximate value based on QCD calculations).
    /// </summary>
    public static readonly double QuarkBindingEnergy = 1.505e-10; // ~939 MeV in Joules

    /// <summary>
    /// Neutron lifetime in seconds (PDG 2022 value).
    /// </summary>
    public static readonly double Lifetime = 881.5; // seconds

    /// <summary>
    /// Neutron decay rate (1/lifetime) in inverse seconds.
    /// </summary>
    public static readonly double DecayRate = 1.0 / Lifetime;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new neutron particle with the standard quark composition (udd).
    /// </summary>
    public Neutron() : base("Neutron", "n⁰", isElementary: false, hilbertSpaceDimension: 2)
    {
        _constituents = new List<IQuantumParticle>();
        
        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", RestMass);
        _charge = new ScalarMeasurableProperty("Charge", "C", ElectricCharge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", QuarkBindingEnergy);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", ChargeRadius);

        // Create constituent quarks with color-neutral configuration
        InitializeQuarkComposition();

        // Initialize composite quantum state
        InitializeCompositeState();

        Logger.Debug("Created neutron with quark composition", 
            new { ParticleId = Id, ConstituentCount = _constituents.Count });
    }

    /// <summary>
    /// Initializes a neutron with custom constituent quarks (for advanced usage).
    /// </summary>
    /// <param name="upQuark">Up quark.</param>
    /// <param name="downQuark1">First down quark.</param>
    /// <param name="downQuark2">Second down quark.</param>
    public Neutron(Quark upQuark, Quark downQuark1, Quark downQuark2) 
        : base("Neutron", "n⁰", isElementary: false, hilbertSpaceDimension: 2)
    {
        // Validate quark composition
        ValidateQuarkComposition(upQuark, downQuark1, downQuark2);

        _constituents = new List<IQuantumParticle> { upQuark, downQuark1, downQuark2 };
        
        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", RestMass);
        _charge = new ScalarMeasurableProperty("Charge", "C", ElectricCharge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", QuarkBindingEnergy);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", ChargeRadius);

        // Update composite properties
        UpdateCompositeProperties();
        InitializeCompositeState();

        Logger.Debug("Created neutron with custom quark composition", 
            new { ParticleId = Id, ConstituentCount = _constituents.Count });
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.5; // Neutron is a spin-1/2 fermion
    public override ParticleStatistics Statistics => ParticleStatistics.FermiDirac; // Fermion

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
            // Simplified moment of inertia for roughly spherical neutron
            var I = 0.4 * RestMass * ChargeRadius * ChargeRadius; // I = (2/5)MR² for solid sphere
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

    public double TotalAngularMomentumQuantumNumber => 0.5; // J = 1/2 for ground state neutron
    public double GroundStateEnergy => RestMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
    public IReadOnlyList<ExcitedState> ExcitedStates { get; } = CreateExcitedStates();
    public int ExcitationLevel { get; private set; } = 0; // Ground state

    public double[,] ConstituentInteractionMatrix
    {
        get
        {
            // 3x3 matrix for quark-quark interactions (simplified QCD potential)
            var strongCoupling = PhysicsConstants.StrongCouplingConstant;
            return new double[,]
            {
                { 0, strongCoupling, strongCoupling },      // up interactions
                { strongCoupling, 0, strongCoupling },      // down1 interactions  
                { strongCoupling, strongCoupling, 0 }       // down2 interactions
            };
        }
    }

    #endregion

    #region Baryon-Specific Properties

    /// <summary>
    /// Gets the baryon number (+1 for neutron).
    /// </summary>
    public double BaryonNumber => 1.0;

    /// <summary>
    /// Gets the isospin quantum number (I = 1/2 for nucleon).
    /// </summary>
    public double Isospin => 0.5;

    /// <summary>
    /// Gets the third component of isospin (I₃ = -1/2 for neutron).
    /// </summary>
    public double IsospinThird => -0.5;

    /// <summary>
    /// Gets the magnetic moment in units of nuclear magnetons.
    /// </summary>
    public double MagneticMomentInNuclearMagnetons => -1.91304273; // μₙ/μₙ

    /// <summary>
    /// Gets the up quark in this neutron.
    /// </summary>
    public IEnumerable<Quark> UpQuarks => GetConstituentsOfType<Quark>().Where(q => q.Type == QuarkType.Up);

    /// <summary>
    /// Gets the down quarks in this neutron.
    /// </summary>
    public IEnumerable<Quark> DownQuarks => GetConstituentsOfType<Quark>().Where(q => q.Type == QuarkType.Down);

    #endregion

    #region Composite Operations

    public void AddConstituent(IQuantumParticle particle, double bindingEnergy)
    {
        if (particle == null)
            throw new ArgumentNullException(nameof(particle));

        lock (_compositionLock)
        {
            if (_constituents.Count >= 3)
                throw new InvalidOperationException("Neutron already has maximum number of constituents (3 quarks)");

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
            // For a neutron, we need to construct the antisymmetric three-quark state
            // This is a simplified model - full QCD would require much more complex calculations
            
            if (_constituents.Count != 3)
                return StateVector;

            // Create composite state by tensor product of constituent states
            var compositeState = new Complex[HilbertSpaceDimension];
            
            // Simplified: ground state neutron in spin-up configuration
            compositeState[0] = new Complex(1.0, 0.0); // |↑⟩
            compositeState[1] = new Complex(0.0, 0.0); // |↓⟩
            
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
                new { ParticleId = Id, BindingEnergy = totalBindingEnergy });
        }
    }

    public IEnumerable<IQuantumParticle> Decompose()
    {
        // Neutrons undergo beta decay: n → p + e⁻ + ν̄ₑ
        Logger.Debug("Simulating neutron beta decay", new { ParticleId = Id, Lifetime = Lifetime });
        
        // Create decay products
        var products = new List<IQuantumParticle>();
        
        // Create proton from quarks (one down quark converts to up via W⁻ emission)
        var proton = new Proton();
        products.Add(proton);
        
        // In a full implementation, we would also create:
        // - Electron (from W⁻ → e⁻ + ν̄ₑ)
        // - Electron antineutrino
        // For now, we just return the proton
        
        OnParticleDecayed(products, "Beta Decay", 0.782e6 * 1.602176634e-13); // Q-value in Joules
        
        return products;
    }

    public bool IsStable() => false; // Free neutrons are unstable

    public double CalculateDecayRate() => DecayRate;

    public void EvolveCompositeState(double timeStep)
    {
        var validation = ParameterValidator.ValidatePositive(timeStep, nameof(timeStep));
        validation.ThrowIfInvalid();

        // Evolve the composite quantum state
        // This would involve complex QCD calculations in a full implementation
        
        lock (_compositionLock)
        {
            // Simplified evolution - just evolve each constituent
            foreach (var constituent in _constituents)
            {
                // Individual quarks would evolve according to QCD Hamiltonian
                // For now, we keep them in their bound states
            }
            
            // Update composite state
            StateVector = CalculateCompositeState();
            
            // Check for decay probability
            var decayProbability = 1.0 - Math.Exp(-DecayRate * timeStep);
            if (decayProbability > 0.001) // Only log if significant probability
            {
                Logger.Debug("Neutron decay probability calculated", 
                    new { ParticleId = Id, TimeStep = timeStep, DecayProbability = decayProbability });
            }
        }
    }

    public double CalculateInternalInteractionEnergy()
    {
        lock (_compositionLock)
        {
            if (_constituents.Count < 2) return 0.0;

            var totalEnergy = 0.0;
            var quarks = GetConstituentsOfType<Quark>().ToArray();

            // Calculate pairwise quark interactions (simplified QCD potential)
            for (int i = 0; i < quarks.Length; i++)
            {
                for (int j = i + 1; j < quarks.Length; j++)
                {
                    // Simplified QCD potential: V(r) = -4αₛ/(3r) + kr
                    // where k is the string tension and αₛ is strong coupling
                    var distance = ChargeRadius / Math.Sqrt(3); // Approximate inter-quark distance
                    var coulombTerm = -4.0 * PhysicsConstants.StrongCouplingConstant / (3.0 * distance);
                    var confinementTerm = 0.18e9 * distance; // String tension ~0.18 GeV/fm converted to SI
                    
                    totalEnergy += coulombTerm + confinementTerm;
                }
            }

            return totalEnergy;
        }
    }

    public void UpdateInternalInteractions()
    {
        // Update the internal QCD interactions between quarks
        UpdateCompositeProperties();
    }

    #endregion

    #region Beta Decay Methods

    /// <summary>
    /// Simulates the beta decay process: n → p + e⁻ + ν̄ₑ
    /// </summary>
    /// <returns>The decay products.</returns>
    public IEnumerable<IQuantumParticle> SimulateBetaDecay()
    {
        return Decompose();
    }

    /// <summary>
    /// Calculates the probability of beta decay occurring within a given time interval.
    /// </summary>
    /// <param name="timeInterval">The time interval in seconds.</param>
    /// <returns>The decay probability.</returns>
    public double CalculateBetaDecayProbability(double timeInterval)
    {
        var validation = ParameterValidator.ValidatePositive(timeInterval, nameof(timeInterval));
        validation.ThrowIfInvalid();

        return 1.0 - Math.Exp(-DecayRate * timeInterval);
    }

    /// <summary>
    /// Gets the Q-value (energy release) for neutron beta decay in Joules.
    /// </summary>
    public double BetaDecayQValue => 0.782e6 * 1.602176634e-13; // 0.782 MeV

    #endregion

    #region Interaction Overrides

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Neutrons participate in strong interactions with other hadrons
        return other is ICompositeParticle || other.Name == "Gluon";
    }

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Neutrons have no electric charge but have magnetic moment
        // They can interact with magnetic fields and via their magnetic moment
        return other.Name == "Photon" && MagneticMoment != 0.0;
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
            // Nuclear force (residual strong force) - approximate with Yukawa potential
            var yukawaMass = 140e6 * 1.602176634e-13 / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // Pion mass
            var range = PhysicsConstants.ReducedPlanckConstant / (yukawaMass * PhysicsConstants.SpeedOfLight);
            
            return PhysicsConstants.StrongCouplingConstant * Math.Exp(-distance / range) / distance;
        }

        // Neutrons don't have electromagnetic interactions (no charge)
        // But they can interact via their magnetic moment
        if (other.Name == "Photon" && MagneticMoment != 0.0)
        {
            // Magnetic dipole interaction (simplified)
            return Math.Abs(MagneticMoment) / (distance * distance * distance);
        }

        return 0.0; // No significant interaction
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Neutron();
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        // Return an antineutron (would need to be implemented separately)
        // For now, return null as antineutron is not yet implemented
        return null;
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
            // Create standard neutron composition: udd with color neutrality
            var upQuark = new Quark(QuarkType.Up, QuarkColor.Red);
            var downQuark1 = new Quark(QuarkType.Down, QuarkColor.Green);
            var downQuark2 = new Quark(QuarkType.Down, QuarkColor.Blue);

            _constituents.Add(upQuark);
            _constituents.Add(downQuark1);
            _constituents.Add(downQuark2);

            UpdateCompositeProperties();
        }
    }

    private void ValidateQuarkComposition(Quark upQuark, Quark downQuark1, Quark downQuark2)
    {
        if (upQuark.Type != QuarkType.Up)
            throw new ArgumentException("Neutron requires one up quark");
        
        if (downQuark1.Type != QuarkType.Down || downQuark2.Type != QuarkType.Down)
            throw new ArgumentException("Neutron requires two down quarks");

        // Check color neutrality
        if (!QuarkColorExtensions.IsColorNeutral(upQuark.Color, downQuark1.Color, downQuark2.Color))
            throw new ArgumentException("Quark combination must be color-neutral");

        if (upQuark.IsAntiquark || downQuark1.IsAntiquark || downQuark2.IsAntiquark)
            throw new ArgumentException("Neutron constituents must be quarks, not antiquarks");
    }

    private void InitializeCompositeState()
    {
        // Initialize in ground state
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    private static List<ExcitedState> CreateExcitedStates()
    {
        // Define known neutron excited states (resonances)
        return new List<ExcitedState>
        {
            new ExcitedState
            {
                Energy = 1.232e9 * 1.602176634e-13, // Δ(1232) resonance in Joules
                QuantumNumbers = new Dictionary<string, double> { {"J", 1.5}, {"I", 1.5} },
                Lifetime = 6e-24, // ~6×10⁻²⁴ seconds
                DecayChannels = new List<DecayChannel>
                {
                    new DecayChannel { BranchingRatio = 1.0, EnergyRelease = 300e6 * 1.602176634e-13 }
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
