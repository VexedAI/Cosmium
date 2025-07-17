using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

namespace Cosmium.Engine.Physics.Quantum.Particles.Composite.Hadrons.Baryons;

/// <summary>
/// Implementation of a proton particle as a composite baryon made of quarks.
/// The proton is composed of two up quarks and one down quark (uud) in a color-neutral configuration.
/// It is the most stable baryon and forms the nucleus of hydrogen atoms.
/// </summary>
public class Proton : QuantumParticleBase, ICompositeParticle
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
    /// Proton rest mass in kg (PDG 2022 value).
    /// </summary>
    public static readonly double RestMass = PhysicsConstants.ProtonMass;

    /// <summary>
    /// Proton electric charge in Coulombs (+e).
    /// </summary>
    public static readonly double ElectricCharge = PhysicsConstants.ElementaryCharge;

    /// <summary>
    /// Proton magnetic moment in J/T (PDG 2022 value).
    /// </summary>
    public static readonly double MagneticMoment = 1.41060679736e-26; // μₚ

    /// <summary>
    /// Proton charge radius in meters (PDG 2022 value).
    /// </summary>
    public static readonly double ChargeRadius = 0.8414e-15; // meters

    /// <summary>
    /// Binding energy of quarks in proton (approximate value based on QCD calculations).
    /// </summary>
    public static readonly double QuarkBindingEnergy = 1.5e-10; // ~938 MeV in Joules

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new proton particle with the standard quark composition (uud).
    /// </summary>
    public Proton() : base("Proton", "p⁺", isElementary: false, hilbertSpaceDimension: 2)
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

        Logger.Debug("Created proton with quark composition", 
            new { ParticleId = Id, ConstituentCount = _constituents.Count });
    }

    /// <summary>
    /// Initializes a proton with custom constituent quarks (for advanced usage).
    /// </summary>
    /// <param name="upQuark1">First up quark.</param>
    /// <param name="upQuark2">Second up quark.</param>
    /// <param name="downQuark">Down quark.</param>
    public Proton(Quark upQuark1, Quark upQuark2, Quark downQuark) 
        : base("Proton", "p⁺", isElementary: false, hilbertSpaceDimension: 2)
    {
        // Validate quark composition
        ValidateQuarkComposition(upQuark1, upQuark2, downQuark);

        _constituents = new List<IQuantumParticle> { upQuark1, upQuark2, downQuark };
        
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

        Logger.Debug("Created proton with custom quark composition", 
            new { ParticleId = Id, ConstituentCount = _constituents.Count });
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.5; // Proton is a spin-1/2 fermion
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
            // Simplified moment of inertia for roughly spherical proton
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

    public double TotalAngularMomentumQuantumNumber => 0.5; // J = 1/2 for ground state proton
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
                { 0, strongCoupling, strongCoupling },      // up1 interactions
                { strongCoupling, 0, strongCoupling },      // up2 interactions  
                { strongCoupling, strongCoupling, 0 }       // down interactions
            };
        }
    }

    #endregion

    #region Baryon-Specific Properties

    /// <summary>
    /// Gets the baryon number (+1 for proton).
    /// </summary>
    public double BaryonNumber => 1.0;

    /// <summary>
    /// Gets the isospin quantum number (I = 1/2 for nucleon).
    /// </summary>
    public double Isospin => 0.5;

    /// <summary>
    /// Gets the third component of isospin (I₃ = +1/2 for proton).
    /// </summary>
    public double IsospinThird => 0.5;

    /// <summary>
    /// Gets the magnetic moment in units of nuclear magnetons.
    /// </summary>
    public double MagneticMomentInNuclearMagnetons => 2.79284734463; // μₚ/μₙ

    /// <summary>
    /// Gets the up quarks in this proton.
    /// </summary>
    public IEnumerable<Quark> UpQuarks => GetConstituentsOfType<Quark>().Where(q => q.Type == QuarkType.Up);

    /// <summary>
    /// Gets the down quarks in this proton.
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
                throw new InvalidOperationException("Proton already has maximum number of constituents (3 quarks)");

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
            // For a proton, we need to construct the antisymmetric three-quark state
            // This is a simplified model - full QCD would require much more complex calculations
            
            if (_constituents.Count != 3)
                return StateVector;

            // Create composite state by tensor product of constituent states
            var compositeState = new Complex[HilbertSpaceDimension];
            
            // Simplified: ground state proton in spin-up configuration
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
        // Protons are stable under normal conditions - they don't spontaneously decay
        Logger.Warning("Attempted to decompose stable proton", new { ParticleId = Id });
        throw new InvalidOperationException("Protons are stable particles and cannot be decomposed under normal conditions");
    }

    public bool IsStable() => true; // Protons are stable (or have extremely long lifetime)

    public double CalculateDecayRate()
    {
        // Current experimental lower bound on proton lifetime is > 10³⁴ years
        // For practical purposes, return effectively zero decay rate
        return 0.0;
    }

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

    #region Interaction Overrides

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Protons participate in strong interactions with other hadrons
        return other is ICompositeParticle || other.Name == "Gluon";
    }

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Protons interact electromagnetically due to their electric charge
        return !other.Charge.Value.Equals(0.0) || other.Name == "Photon";
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
            var nucleonMass = PhysicsConstants.ProtonMass;
            var yukawaMass = 140e6 * 1.602176634e-13 / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight); // Pion mass
            var range = PhysicsConstants.ReducedPlanckConstant / (yukawaMass * PhysicsConstants.SpeedOfLight);
            
            return PhysicsConstants.StrongCouplingConstant * Math.Exp(-distance / range) / distance;
        }

        // Fall back to electromagnetic interaction
        return base.CalculateInteractionStrength(other, distance);
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Proton();
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        // Return an antiproton (would need to be implemented separately)
        // For now, return null as antiproton is not yet implemented
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
            // Create standard proton composition: uud with color neutrality
            var upQuark1 = new Quark(QuarkType.Up, QuarkColor.Red);
            var upQuark2 = new Quark(QuarkType.Up, QuarkColor.Green);
            var downQuark = new Quark(QuarkType.Down, QuarkColor.Blue);

            _constituents.Add(upQuark1);
            _constituents.Add(upQuark2);
            _constituents.Add(downQuark);

            UpdateCompositeProperties();
        }
    }

    private void ValidateQuarkComposition(Quark upQuark1, Quark upQuark2, Quark downQuark)
    {
        if (upQuark1.Type != QuarkType.Up || upQuark2.Type != QuarkType.Up)
            throw new ArgumentException("Proton requires two up quarks");
        
        if (downQuark.Type != QuarkType.Down)
            throw new ArgumentException("Proton requires one down quark");

        // Check color neutrality
        if (!QuarkColorExtensions.IsColorNeutral(upQuark1.Color, upQuark2.Color, downQuark.Color))
            throw new ArgumentException("Quark combination must be color-neutral");

        if (upQuark1.IsAntiquark || upQuark2.IsAntiquark || downQuark.IsAntiquark)
            throw new ArgumentException("Proton constituents must be quarks, not antiquarks");
    }

    private void InitializeCompositeState()
    {
        // Initialize in ground state
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    private static List<ExcitedState> CreateExcitedStates()
    {
        // Define known proton excited states (resonances)
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

    #endregion
}
