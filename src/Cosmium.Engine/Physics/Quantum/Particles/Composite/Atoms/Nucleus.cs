using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Composite.Hadrons.Baryons;

namespace Cosmium.Engine.Physics.Quantum.Particles.Composite.Atoms;

/// <summary>
/// Implementation of an atomic nucleus as a composite particle made of protons and neutrons.
/// Represents the dense core of an atom containing most of its mass and positive charge.
/// Includes nuclear physics calculations such as binding energy, stability, and decay processes.
/// </summary>
public class Nucleus : QuantumParticleBase, ICompositeParticle
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

    #region Nuclear Data Constants

    /// <summary>
    /// Nuclear radius constant r₀ in meters (r = r₀A^(1/3)).
    /// </summary>
    public static readonly double NuclearRadiusConstant = 1.2e-15; // 1.2 fm

    /// <summary>
    /// Semi-empirical mass formula coefficients (Weizsäcker formula).
    /// </summary>
    public static class SemfCoefficients
    {
        /// <summary>Volume term coefficient (aᵥ) in MeV.</summary>
        public static readonly double Volume = 15.75;
        /// <summary>Surface term coefficient (aₛ) in MeV.</summary>
        public static readonly double Surface = 17.8;
        /// <summary>Coulomb term coefficient (aᶜ) in MeV.</summary>
        public static readonly double Coulomb = 0.711;
        /// <summary>Asymmetry term coefficient (aₐ) in MeV.</summary>
        public static readonly double Asymmetry = 23.7;
        /// <summary>Pairing term coefficient (δ) in MeV.</summary>
        public static readonly double Pairing = 11.18;
    }

    /// <summary>
    /// Magic numbers for nuclear shells (protons and neutrons).
    /// </summary>
    public static readonly int[] MagicNumbers = { 2, 8, 20, 28, 50, 82, 126 };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new nucleus with the specified atomic number and mass number.
    /// </summary>
    /// <param name="atomicNumber">Number of protons (Z).</param>
    /// <param name="massNumber">Total number of nucleons (A = Z + N).</param>
    public Nucleus(int atomicNumber, int massNumber) 
        : base($"Nucleus-{atomicNumber}-{massNumber}", GetNuclearSymbol(atomicNumber, massNumber), 
               isElementary: false, hilbertSpaceDimension: 2)
    {
        // Validate nuclear composition
        ValidateNuclearParameters(atomicNumber, massNumber);

        AtomicNumber = atomicNumber;
        MassNumber = massNumber;
        NeutronNumber = massNumber - atomicNumber;

        _constituents = new List<IQuantumParticle>();
        
        // Initialize physical properties
        var computedMass = CalculateNuclearMass();
        var nuclearCharge = atomicNumber * PhysicsConstants.ElementaryCharge;
        var nuclearRadius = CalculateNuclearRadius();
        
        _mass = new ScalarMeasurableProperty("Mass", "kg", computedMass);
        _charge = new ScalarMeasurableProperty("Charge", "C", nuclearCharge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", 0.0);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", nuclearRadius);

        // Create constituent nucleons
        InitializeNucleonComposition();

        // Calculate binding energy using SEMF
        var bindingEnergyValue = CalculateBindingEnergy();
        _bindingEnergy.SetValue(bindingEnergyValue);

        // Initialize quantum state
        InitializeNuclearState();

        Logger.Debug("Created nucleus", 
            new { ParticleId = Id, Z = AtomicNumber, A = MassNumber, N = NeutronNumber });
    }

    /// <summary>
    /// Creates a nucleus with custom nucleon composition (for advanced usage).
    /// </summary>
    /// <param name="protons">Collection of proton particles.</param>
    /// <param name="neutrons">Collection of neutron particles.</param>
    public Nucleus(IEnumerable<Proton> protons, IEnumerable<Neutron> neutrons)
        : base("Custom-Nucleus", "N", isElementary: false, hilbertSpaceDimension: 2)
    {
        var protonList = protons.ToList();
        var neutronList = neutrons.ToList();
        
        AtomicNumber = protonList.Count;
        NeutronNumber = neutronList.Count;
        MassNumber = AtomicNumber + NeutronNumber;

        ValidateNuclearParameters(AtomicNumber, MassNumber);

        _constituents = new List<IQuantumParticle>();
        _constituents.AddRange(protonList);
        _constituents.AddRange(neutronList);

        // Update name and symbol
        var newName = $"Nucleus-{AtomicNumber}-{MassNumber}";
        var newSymbol = GetNuclearSymbol(AtomicNumber, MassNumber);
        
        // Initialize physical properties
        var computedMass = CalculateActualMass();
        var nuclearCharge = AtomicNumber * PhysicsConstants.ElementaryCharge;
        var nuclearRadius = CalculateNuclearRadius();
        
        _mass = new ScalarMeasurableProperty("Mass", "kg", computedMass);
        _charge = new ScalarMeasurableProperty("Charge", "C", nuclearCharge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", 0.0);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", nuclearRadius);

        UpdateCompositeProperties();
        InitializeNuclearState();

        Logger.Debug("Created custom nucleus", 
            new { ParticleId = Id, Z = AtomicNumber, A = MassNumber, N = NeutronNumber });
    }

    #endregion

    #region Nuclear Properties

    /// <summary>
    /// Gets the atomic number (number of protons, Z).
    /// </summary>
    public int AtomicNumber { get; }

    /// <summary>
    /// Gets the mass number (total number of nucleons, A).
    /// </summary>
    public int MassNumber { get; }

    /// <summary>
    /// Gets the neutron number (N = A - Z).
    /// </summary>
    public int NeutronNumber { get; }

    /// <summary>
    /// Gets the nuclear charge in elementary charge units.
    /// </summary>
    public double NuclearChargeUnits => AtomicNumber;

    /// <summary>
    /// Gets the nuclear radius in meters.
    /// </summary>
    public double NuclearRadius => CalculateNuclearRadius();

    /// <summary>
    /// Gets whether this nucleus is stable against all forms of decay.
    /// </summary>
    public bool IsStable => DetermineStability();

    /// <summary>
    /// Gets whether this nucleus has magic numbers (particularly stable).
    /// </summary>
    public bool HasMagicNumbers => IsMagicNumber(AtomicNumber) || IsMagicNumber(NeutronNumber);

    /// <summary>
    /// Gets whether this nucleus is doubly magic (both Z and N are magic).
    /// </summary>
    public bool IsDoublyMagic => IsMagicNumber(AtomicNumber) && IsMagicNumber(NeutronNumber);

    /// <summary>
    /// Gets the neutron-to-proton ratio.
    /// </summary>
    public double NeutronToProtonRatio => (double)NeutronNumber / AtomicNumber;

    /// <summary>
    /// Gets the nuclear spin quantum number.
    /// </summary>
    public double NuclearSpin => CalculateNuclearSpin();

    /// <summary>
    /// Gets the nuclear magnetic moment in nuclear magnetons.
    /// </summary>
    public double NuclearMagneticMoment => CalculateNuclearMagneticMoment();

    /// <summary>
    /// Gets the electric quadrupole moment in barn (10⁻²⁴ cm²).
    /// </summary>
    public double ElectricQuadrupoleMoment => CalculateQuadrupoleMoment();

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => NuclearSpin;
    public override ParticleStatistics Statistics => 
        (int)(2 * NuclearSpin) % 2 == 0 ? ParticleStatistics.BoseEinstein : ParticleStatistics.FermiDirac;

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
            // Simplified moment of inertia for spherical nucleus
            var nuclearRadius = CalculateNuclearRadius();
            var nuclearMass = _mass.Value;
            var I = 0.4 * nuclearMass * nuclearRadius * nuclearRadius; // I = (2/5)MR² for uniform sphere
            
            return new double[,]
            {
                { I, 0, 0 },
                { 0, I, 0 },
                { 0, 0, I }
            };
        }
    }

    public IReadOnlyList<VibrationalMode> VibrationalModes { get; } = new List<VibrationalMode>();
    
    public IReadOnlyList<RotationalMode> RotationalModes => CreateRotationalModes().AsReadOnly();

    public double TotalAngularMomentumQuantumNumber => NuclearSpin;
    public double GroundStateEnergy => -_bindingEnergy.Value; // Bound state has negative energy
    public IReadOnlyList<ExcitedState> ExcitedStates { get; } = new List<ExcitedState>();
    public int ExcitationLevel { get; private set; } = 0;

    public double[,] ConstituentInteractionMatrix
    {
        get
        {
            var n = _constituents.Count;
            var matrix = new double[n, n];
            
            // Nuclear force matrix (simplified)
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    // Strong nuclear force between nucleons
                    matrix[i, j] = matrix[j, i] = CalculateNucleonInteraction(i, j);
                }
            }
            
            return matrix;
        }
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a hydrogen nucleus (single proton).
    /// </summary>
    public static Nucleus CreateHydrogen() => new(1, 1);

    /// <summary>
    /// Creates a deuterium nucleus (proton + neutron).
    /// </summary>
    public static Nucleus CreateDeuterium() => new(1, 2);

    /// <summary>
    /// Creates a tritium nucleus (proton + 2 neutrons).
    /// </summary>
    public static Nucleus CreateTritium() => new(1, 3);

    /// <summary>
    /// Creates a helium-4 nucleus (alpha particle).
    /// </summary>
    public static Nucleus CreateHelium4() => new(2, 4);

    /// <summary>
    /// Creates a carbon-12 nucleus.
    /// </summary>
    public static Nucleus CreateCarbon12() => new(6, 12);

    /// <summary>
    /// Creates a nucleus from element symbol and mass number.
    /// </summary>
    /// <param name="elementSymbol">Chemical element symbol (e.g., "H", "He", "C").</param>
    /// <param name="massNumber">Mass number.</param>
    public static Nucleus CreateFromSymbol(string elementSymbol, int massNumber)
    {
        var atomicNumber = GetAtomicNumberFromSymbol(elementSymbol);
        return new Nucleus(atomicNumber, massNumber);
    }

    /// <summary>
    /// Creates a nucleus from atomic and mass numbers.
    /// </summary>
    /// <param name="atomicNumber">Atomic number (Z).</param>
    /// <param name="massNumber">Mass number (A).</param>
    public static Nucleus CreateFromNumbers(int atomicNumber, int massNumber) => new(atomicNumber, massNumber);

    #endregion

    #region Nuclear Physics Calculations

    /// <summary>
    /// Calculates the nuclear binding energy using the semi-empirical mass formula.
    /// </summary>
    /// <returns>Binding energy in Joules.</returns>
    public double CalculateBindingEnergy()
    {
        var A = MassNumber;
        var Z = AtomicNumber;
        var N = NeutronNumber;

        // Semi-empirical mass formula (Weizsäcker formula) in MeV
        var volumeTerm = SemfCoefficients.Volume * A;
        var surfaceTerm = -SemfCoefficients.Surface * Math.Pow(A, 2.0/3.0);
        var coulombTerm = -SemfCoefficients.Coulomb * Z * Z / Math.Pow(A, 1.0/3.0);
        var asymmetryTerm = -SemfCoefficients.Asymmetry * (N - Z) * (N - Z) / A;
        
        // Pairing term
        var pairingTerm = 0.0;
        if (Z % 2 == 0 && N % 2 == 0) // Even-even
            pairingTerm = SemfCoefficients.Pairing / Math.Sqrt(A);
        else if (Z % 2 == 1 && N % 2 == 1) // Odd-odd
            pairingTerm = -SemfCoefficients.Pairing / Math.Sqrt(A);
        // Even-odd and odd-even have zero pairing term

        var bindingEnergyMeV = volumeTerm + surfaceTerm + coulombTerm + asymmetryTerm + pairingTerm;
        
        // Convert MeV to Joules
        return bindingEnergyMeV * 1e6 * 1.602176634e-19; // MeV to J
    }

    /// <summary>
    /// Calculates the Q-value for a nuclear reaction or decay.
    /// </summary>
    /// <param name="products">The products of the reaction.</param>
    /// <returns>Q-value in Joules (positive for exothermic).</returns>
    public double CalculateQValue(params Nucleus[] products)
    {
        var initialMass = _mass.Value;
        var finalMass = products.Sum(p => p.Mass.Value);
        
        // Q = (initial mass - final mass) × c²
        return (initialMass - finalMass) * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
    }

    /// <summary>
    /// Estimates the half-life for the most probable decay mode.
    /// </summary>
    /// <returns>Half-life in seconds, or double.PositiveInfinity for stable nuclei.</returns>
    public double EstimateHalfLife()
    {
        if (IsStable) return double.PositiveInfinity;

        var decayMode = DeterminePrimaryDecayMode();
        
        return decayMode switch
        {
            DecayMode.AlphaDecay => EstimateAlphaDecayHalfLife(),
            DecayMode.BetaMinusDecay => EstimateBetaDecayHalfLife(),
            DecayMode.BetaPlusDecay => EstimateBetaDecayHalfLife(),
            DecayMode.ElectronCapture => EstimateBetaDecayHalfLife(),
            DecayMode.SpontaneousFission => EstimateFissionHalfLife(),
            _ => 1e10 * 365.25 * 24 * 3600 // 10 billion years default
        };
    }

    /// <summary>
    /// Determines the primary decay mode for this nucleus.
    /// </summary>
    public DecayMode DeterminePrimaryDecayMode()
    {
        if (IsStable) return DecayMode.Stable;

        var Z = AtomicNumber;
        var N = NeutronNumber;
        var A = MassNumber;

        // Very heavy nuclei tend to undergo spontaneous fission
        if (A > 240) return DecayMode.SpontaneousFission;

        // Heavy nuclei favor alpha decay
        if (A > 200 && Z > 82) return DecayMode.AlphaDecay;

        // Neutron-rich nuclei undergo beta-minus decay
        if (N > Z + 1.5 + 0.015 * A) return DecayMode.BetaMinusDecay;

        // Proton-rich nuclei undergo beta-plus decay or electron capture
        if (N < Z - 1) 
        {
            return Z < 20 ? DecayMode.BetaPlusDecay : DecayMode.ElectronCapture;
        }

        // Near the valley of stability
        return DecayMode.Stable;
    }

    /// <summary>
    /// Calculates the separation energy for removing one nucleon.
    /// </summary>
    /// <param name="nucleonType">Type of nucleon to remove.</param>
    /// <returns>Separation energy in Joules.</returns>
    public double CalculateSeparationEnergy(NucleonType nucleonType)
    {
        if (nucleonType == NucleonType.Proton && AtomicNumber == 1)
            return double.PositiveInfinity; // Cannot remove last proton

        if (nucleonType == NucleonType.Neutron && NeutronNumber == 0)
            return double.PositiveInfinity; // No neutrons to remove

        var parentBindingEnergy = CalculateBindingEnergy();
        
        Nucleus daughter;
        double nucleonMass;

        if (nucleonType == NucleonType.Proton)
        {
            daughter = new Nucleus(AtomicNumber - 1, MassNumber - 1);
            nucleonMass = PhysicsConstants.ProtonMass;
        }
        else
        {
            daughter = new Nucleus(AtomicNumber, MassNumber - 1);
            nucleonMass = PhysicsConstants.NeutronMass;
        }

        var daughterBindingEnergy = daughter.CalculateBindingEnergy();
        
        // Separation energy = daughter BE + nucleon mass - parent mass
        var parentMass = _mass.Value;
        var daughterMass = daughter.Mass.Value;
        
        return daughterBindingEnergy + nucleonMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight - 
               parentMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
    }

    #endregion

    #region Composite Operations

    public void AddConstituent(IQuantumParticle particle, double bindingEnergy)
    {
        if (particle == null)
            throw new ArgumentNullException(nameof(particle));

        lock (_compositionLock)
        {
            if (!(particle is Proton) && !(particle is Neutron))
                throw new ArgumentException("Nucleus can only contain protons and neutrons");

            _constituents.Add(particle);
            UpdateCompositeProperties();
            OnConstituentAdded(particle, bindingEnergy);
        }
    }

    public bool RemoveConstituent(IQuantumParticle particle)
    {
        if (particle == null) return false;

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
            // Nuclear quantum state (simplified)
            var compositeState = new Complex[HilbertSpaceDimension];
            
            // Ground state nuclear configuration
            compositeState[0] = new Complex(1.0, 0.0); // |ground⟩
            compositeState[1] = new Complex(0.0, 0.0); // |excited⟩
            
            return compositeState;
        }
    }

    public void UpdateCompositeProperties()
    {
        lock (_compositionLock)
        {
            if (_constituents.Count == 0) return;

            // Update center of mass (simplified to origin)
            _centerOfMass.SetValue(new double[3] { 0.0, 0.0, 0.0 });

            // Update mass and binding energy
            var actualMass = CalculateActualMass();
            _mass.SetValue(actualMass);

            var bindingEnergyValue = CalculateBindingEnergy();
            _bindingEnergy.SetValue(bindingEnergyValue);

            Logger.Debug("Updated nuclear composite properties", 
                new { ParticleId = Id, Mass = actualMass, BindingEnergy = bindingEnergyValue });
        }
    }

    public IEnumerable<IQuantumParticle> Decompose()
    {
        // Nuclear decay simulation
        var decayMode = DeterminePrimaryDecayMode();
        
        return decayMode switch
        {
            DecayMode.AlphaDecay => SimulateAlphaDecay(),
            DecayMode.BetaMinusDecay => SimulateBetaMinusDecay(),
            DecayMode.BetaPlusDecay => SimulateBetaPlusDecay(),
            DecayMode.SpontaneousFission => SimulateSpontaneousFission(),
            _ => new[] { this } // Stable nucleus
        };
    }


    public double CalculateDecayRate()
    {
        var halfLife = EstimateHalfLife();
        return double.IsInfinity(halfLife) ? 0.0 : Math.Log(2) / halfLife;
    }

    public void EvolveCompositeState(double timeStep)
    {
        var validation = ParameterValidator.ValidatePositive(timeStep, nameof(timeStep));
        validation.ThrowIfInvalid();

        lock (_compositionLock)
        {
            // Nuclear time evolution (simplified)
            // In reality, this would involve complex nuclear shell model calculations
            
            var decayRate = CalculateDecayRate();
            var decayProbability = 1.0 - Math.Exp(-decayRate * timeStep);
            
            // Random decay simulation
            var random = new Random();
            if (random.NextDouble() < decayProbability)
            {
                // Trigger decay event
                var products = Decompose();
                OnParticleDecayed(products, DeterminePrimaryDecayMode().ToString(), 0.0);
            }
        }
    }

    public double CalculateInternalInteractionEnergy()
    {
        lock (_compositionLock)
        {
            if (_constituents.Count < 2) return 0.0;

            var totalEnergy = 0.0;
            var nucleons = _constituents.ToArray();

            // Nuclear force energy between nucleons
            for (int i = 0; i < nucleons.Length; i++)
            {
                for (int j = i + 1; j < nucleons.Length; j++)
                {
                    totalEnergy += CalculateNucleonInteraction(i, j);
                }
            }

            return totalEnergy;
        }
    }

    public void UpdateInternalInteractions()
    {
        UpdateCompositeProperties();
    }

    bool ICompositeParticle.IsStable()
    {
        return IsStable;
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Nucleus(AtomicNumber, MassNumber);
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        // Anti-nucleus would have antiprotons and antineutrons
        // Not implemented in this version
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

    private void ValidateNuclearParameters(int atomicNumber, int massNumber)
    {
        if (atomicNumber < 1 || atomicNumber > 118)
            throw new ArgumentOutOfRangeException(nameof(atomicNumber), "Atomic number must be between 1 and 118");
        
        if (massNumber < atomicNumber)
            throw new ArgumentException("Mass number cannot be less than atomic number");
        
        if (massNumber > 300)
            throw new ArgumentOutOfRangeException(nameof(massNumber), "Mass number too large for stable calculation");
        
        var neutronNumber = massNumber - atomicNumber;
        if (neutronNumber > 200)
            throw new ArgumentOutOfRangeException("Too many neutrons for realistic nucleus");
    }

    private void InitializeNucleonComposition()
    {
        lock (_compositionLock)
        {
            // Create protons
            for (int i = 0; i < AtomicNumber; i++)
            {
                _constituents.Add(new Proton());
            }

            // Create neutrons
            for (int i = 0; i < NeutronNumber; i++)
            {
                _constituents.Add(new Neutron());
            }
        }
    }

    private void InitializeNuclearState()
    {
        // Initialize in ground state
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    private double CalculateNuclearMass()
    {
        // Use SEMF to calculate nuclear mass
        var bindingEnergyJ = CalculateBindingEnergy();
        var separateMass = AtomicNumber * PhysicsConstants.ProtonMass + NeutronNumber * PhysicsConstants.NeutronMass;
        
        // Nuclear mass = separate masses - binding energy/c²
        return separateMass - bindingEnergyJ / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);
    }

    private double CalculateActualMass()
    {
        lock (_compositionLock)
        {
            return _constituents.Sum(nucleon => nucleon.Mass.Value);
        }
    }

    private double CalculateNuclearRadius()
    {
        return NuclearRadiusConstant * Math.Pow(MassNumber, 1.0/3.0);
    }

    private double CalculateNuclearSpin()
    {
        // Simplified nuclear spin calculation
        // In reality, this requires detailed nuclear shell model
        
        if (AtomicNumber % 2 == 0 && NeutronNumber % 2 == 0)
            return 0.0; // Even-even nuclei have spin 0

        if (AtomicNumber % 2 == 1 && NeutronNumber % 2 == 0)
            return 0.5; // Odd proton number typically gives half-integer spin

        if (AtomicNumber % 2 == 0 && NeutronNumber % 2 == 1)
            return 0.5; // Odd neutron number typically gives half-integer spin

        // Odd-odd nuclei (rare and typically unstable)
        return 1.0; // Simplified assumption
    }

    private double CalculateNuclearMagneticMoment()
    {
        // Simplified magnetic moment calculation
        var spin = NuclearSpin;
        if (spin == 0) return 0.0;

        // Use nuclear magneton as unit
        var nuclearMagneton = PhysicsConstants.ElementaryCharge * PhysicsConstants.ReducedPlanckConstant / (2.0 * PhysicsConstants.ProtonMass);
        
        // Simplified model
        return spin * nuclearMagneton; // This is very approximate
    }

    private double CalculateQuadrupoleMoment()
    {
        var spin = NuclearSpin;
        if (spin < 1.0) return 0.0; // No quadrupole moment for spin < 1

        // Very simplified estimate in barn (10⁻²⁴ cm²)
        var radius = CalculateNuclearRadius();
        return 0.1 * radius * radius * 1e24; // Convert m² to barn
    }

    private bool DetermineStability()
    {
        // Check against known stable isotopes (simplified)
        if (AtomicNumber == 1 && MassNumber <= 2) return true; // H, D
        if (AtomicNumber == 2 && MassNumber <= 4) return true; // He isotopes
        if (AtomicNumber <= 20 && Math.Abs(NeutronNumber - AtomicNumber) <= 2) return true; // Light stable nuclei
        if (HasMagicNumbers) return true; // Magic number nuclei tend to be stable
        
        // More complex stability criteria would go here
        return AtomicNumber <= 82 && NeutronNumber <= 126 && Math.Abs(NeutronToProtonRatio - 1.2) < 0.3;
    }

    private static bool IsMagicNumber(int number)
    {
        return MagicNumbers.Contains(number);
    }

    private double EstimateAlphaDecayHalfLife()
    {
        // Geiger-Nuttal law for alpha decay
        var qAlpha = CalculateQValue(new Nucleus(AtomicNumber - 2, MassNumber - 4), CreateHelium4());
        if (qAlpha <= 0) return double.PositiveInfinity;

        var qAlphaMeV = qAlpha / (1e6 * 1.602176634e-19);
        var logHalfLife = 50.0 - 1.5 * qAlphaMeV; // Empirical relation
        
        return Math.Pow(10, logHalfLife) * 365.25 * 24 * 3600; // Convert years to seconds
    }

    private double EstimateBetaDecayHalfLife()
    {
        // Simplified beta decay estimate
        return 1e6 * 365.25 * 24 * 3600; // ~1 million years (very approximate)
    }

    private double EstimateFissionHalfLife()
    {
        // Very simplified fission half-life
        return 1e9 * 365.25 * 24 * 3600; // ~1 billion years
    }

    private double CalculateNucleonInteraction(int i, int j)
    {
        // Simplified nucleon-nucleon interaction energy
        var strongForceRange = 1.5e-15; // ~1.5 fm
        var nuclearRadius = CalculateNuclearRadius();
        var distance = nuclearRadius / Math.Sqrt(3); // Approximate separation
        
        // Yukawa potential for nuclear force
        var strength = -50e6 * 1.602176634e-19; // ~50 MeV in Joules
        return strength * Math.Exp(-distance / strongForceRange) / distance * strongForceRange;
    }

    private IList<RotationalMode> CreateRotationalModes()
    {
        var modes = new List<RotationalMode>();
        
        if (NuclearSpin > 0)
        {
            // Nuclear rotational states
            for (int j = 0; j <= (int)(2 * NuclearSpin); j++)
            {
                modes.Add(new RotationalMode
                {
                    QuantumNumber = j,
                    Energy = j * (j + 1) * PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.ReducedPlanckConstant / (2.0 * MomentOfInertiaTensor[0, 0]),
                    RotationalConstant = PhysicsConstants.ReducedPlanckConstant * PhysicsConstants.ReducedPlanckConstant / (2.0 * MomentOfInertiaTensor[0, 0])
                });
            }
        }

        return modes;
    }

    private IEnumerable<IQuantumParticle> SimulateAlphaDecay()
    {
        var daughter = new Nucleus(AtomicNumber - 2, MassNumber - 4);
        var alpha = CreateHelium4();
        return new IQuantumParticle[] { daughter, alpha };
    }

    private IEnumerable<IQuantumParticle> SimulateBetaMinusDecay()
    {
        var daughter = new Nucleus(AtomicNumber + 1, MassNumber);
        // In reality, this would also produce an electron and antineutrino
        return new IQuantumParticle[] { daughter };
    }

    private IEnumerable<IQuantumParticle> SimulateBetaPlusDecay()
    {
        var daughter = new Nucleus(AtomicNumber - 1, MassNumber);
        // In reality, this would also produce a positron and neutrino
        return new IQuantumParticle[] { daughter };
    }

    private IEnumerable<IQuantumParticle> SimulateSpontaneousFission()
    {
        // Simplified symmetric fission
        var fragment1 = new Nucleus(AtomicNumber / 2, MassNumber / 2);
        var fragment2 = new Nucleus(AtomicNumber / 2, MassNumber / 2);
        return new IQuantumParticle[] { fragment1, fragment2 };
    }

    private static string GetNuclearSymbol(int atomicNumber, int massNumber)
    {
        var elementSymbol = GetElementSymbol(atomicNumber);
        return $"{massNumber}{elementSymbol}";
    }

    private static string GetElementSymbol(int atomicNumber)
    {
        var symbols = new[]
        {
            "", "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne",
            "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K", "Ca",
            // Add more as needed
        };

        return atomicNumber < symbols.Length ? symbols[atomicNumber] : $"Z{atomicNumber}";
    }

    private static int GetAtomicNumberFromSymbol(string symbol)
    {
        return symbol.ToUpper() switch
        {
            "H" => 1, "HE" => 2, "LI" => 3, "BE" => 4, "B" => 5, "C" => 6,
            "N" => 7, "O" => 8, "F" => 9, "NE" => 10, "NA" => 11, "MG" => 12,
            "AL" => 13, "SI" => 14, "P" => 15, "S" => 16, "CL" => 17, "AR" => 18,
            "K" => 19, "CA" => 20,
            _ => throw new ArgumentException($"Unknown element symbol: {symbol}")
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

/// <summary>
/// Enumeration of nuclear decay modes.
/// </summary>
public enum DecayMode
{
    Stable,
    AlphaDecay,
    BetaMinusDecay,
    BetaPlusDecay,
    ElectronCapture,
    SpontaneousFission,
    NeutronEmission,
    ProtonEmission
}

/// <summary>
/// Enumeration of nucleon types.
/// </summary>
public enum NucleonType
{
    Proton,
    Neutron
}
