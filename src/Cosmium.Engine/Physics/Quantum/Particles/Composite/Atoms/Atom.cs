using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Leptons;

namespace Cosmium.Engine.Physics.Quantum.Particles.Composite.Atoms;

/// <summary>
/// Represents a complete atom with nucleus and electron shells.
/// Manages atomic structure, electron configuration, and chemical properties.
/// </summary>
public class Atom : QuantumParticleBase, ICompositeParticle
{
    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly List<ElectronShell> _electronShells;
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _bindingEnergy;
    private readonly ScalarMeasurableProperty _excitationEnergy;
    private readonly VectorMeasurableProperty _centerOfMass;
    private readonly ScalarMeasurableProperty _size;
    private readonly object _atomicLock = new();

    #region Constructors

    /// <summary>
    /// Initializes a new atom with the specified atomic number and mass number.
    /// </summary>
    /// <param name="atomicNumber">Number of protons (Z).</param>
    /// <param name="massNumber">Mass number (A).</param>
    /// <param name="electronCount">Number of electrons (default equals protons for neutral atom).</param>
    public Atom(int atomicNumber, int massNumber, int? electronCount = null)
        : base($"Atom-{GetElementSymbol(atomicNumber)}-{massNumber}", GetElementSymbol(atomicNumber), 
               isElementary: false, hilbertSpaceDimension: 2)
    {
        var validation = ParameterValidator.ValidateIntegerRange(atomicNumber, nameof(atomicNumber), 1, 118);
        validation.ThrowIfInvalid();

        var massValidation = ParameterValidator.ValidateIntegerRange(massNumber, nameof(massNumber), atomicNumber, 300);
        massValidation.ThrowIfInvalid();

        AtomicNumber = atomicNumber;
        MassNumber = massNumber;
        ElectronCount = electronCount ?? atomicNumber; // Neutral atom by default

        if (ElectronCount < 0)
            throw new ArgumentException("Electron count cannot be negative");

        // Initialize nucleus
        Nucleus = new Nucleus(atomicNumber, massNumber);

        // Initialize electron shells
        _electronShells = new List<ElectronShell>();
        InitializeElectronShells();

        // Calculate atomic properties
        var atomicMass = CalculateAtomicMass();
        var netCharge = CalculateNetCharge();
        var atomicRadius = CalculateAtomicRadius();

        _mass = new ScalarMeasurableProperty("Mass", "kg", atomicMass);
        _charge = new ScalarMeasurableProperty("Charge", "C", netCharge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", 0.0);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", atomicRadius);

        // Calculate total binding energy
        var totalBindingEnergy = CalculateTotalBindingEnergy();
        _bindingEnergy.SetValue(totalBindingEnergy);

        // Configure electrons using Aufbau principle
        ConfigureElectrons();

        // Initialize quantum state
        InitializeAtomicState();

        Logger.Debug("Created atom", 
            new { 
                Element = ElementSymbol, 
                AtomicNumber, 
                MassNumber, 
                ElectronCount, 
                Charge = GetIonizationState(),
                Mass = UnitConversions.KilogramsToAtomicMassUnits(atomicMass)
            });
    }

    /// <summary>
    /// Creates an atom with a specific nucleus and electron configuration.
    /// </summary>
    /// <param name="nucleus">The atomic nucleus.</param>
    /// <param name="electronCount">Number of electrons.</param>
    public Atom(Nucleus nucleus, int electronCount = 0)
        : base($"Atom-{GetElementSymbol(nucleus.AtomicNumber)}-{nucleus.MassNumber}", 
               GetElementSymbol(nucleus.AtomicNumber), isElementary: false, hilbertSpaceDimension: 2)
    {
        Nucleus = nucleus ?? throw new ArgumentNullException(nameof(nucleus));
        AtomicNumber = nucleus.AtomicNumber;
        MassNumber = nucleus.MassNumber;
        ElectronCount = Math.Max(0, electronCount);

        _electronShells = new List<ElectronShell>();
        InitializeElectronShells();

        var atomicMass = CalculateAtomicMass();
        var netCharge = CalculateNetCharge();
        var atomicRadius = CalculateAtomicRadius();

        _mass = new ScalarMeasurableProperty("Mass", "kg", atomicMass);
        _charge = new ScalarMeasurableProperty("Charge", "C", netCharge);
        _bindingEnergy = new ScalarMeasurableProperty("BindingEnergy", "J", 0.0);
        _excitationEnergy = new ScalarMeasurableProperty("ExcitationEnergy", "J", 0.0);
        _centerOfMass = new VectorMeasurableProperty("CenterOfMass", "m", new double[3]);
        _size = new ScalarMeasurableProperty("Size", "m", atomicRadius);

        var totalBindingEnergy = CalculateTotalBindingEnergy();
        _bindingEnergy.SetValue(totalBindingEnergy);

        ConfigureElectrons();
        InitializeAtomicState();

        Logger.Debug("Created atom from nucleus", 
            new { Element = ElementSymbol, ElectronCount, Charge = GetIonizationState() });
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the atomic number (number of protons).
    /// </summary>
    public int AtomicNumber { get; }

    /// <summary>
    /// Gets the mass number.
    /// </summary>
    public int MassNumber { get; }

    /// <summary>
    /// Gets the number of electrons.
    /// </summary>
    public int ElectronCount { get; private set; }

    /// <summary>
    /// Gets the number of neutrons.
    /// </summary>
    public int NeutronCount => MassNumber - AtomicNumber;

    /// <summary>
    /// Gets the atomic nucleus.
    /// </summary>
    public Nucleus Nucleus { get; }

    /// <summary>
    /// Gets all electron shells.
    /// </summary>
    public IReadOnlyList<ElectronShell> ElectronShells
    {
        get
        {
            lock (_atomicLock)
            {
                return _electronShells.AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets the element symbol (H, He, Li, etc.).
    /// </summary>
    public string ElementSymbol => GetElementSymbol(AtomicNumber);

    /// <summary>
    /// Gets the element name.
    /// </summary>
    public string ElementName => GetElementName(AtomicNumber);

    /// <summary>
    /// Gets the ionization state (+1, -2, etc., 0 for neutral).
    /// </summary>
    public int IonizationState => AtomicNumber - ElectronCount;

    /// <summary>
    /// Gets whether this atom is neutral (equal protons and electrons).
    /// </summary>
    public bool IsNeutral => ElectronCount == AtomicNumber;

    /// <summary>
    /// Gets whether this atom is an ion.
    /// </summary>
    public bool IsIon => !IsNeutral;

    /// <summary>
    /// Gets whether this atom is a cation (positive ion).
    /// </summary>
    public bool IsCation => ElectronCount < AtomicNumber;

    /// <summary>
    /// Gets whether this atom is an anion (negative ion).
    /// </summary>
    public bool IsAnion => ElectronCount > AtomicNumber;

    /// <summary>
    /// Gets the number of valence electrons.
    /// </summary>
    public int ValenceElectrons
    {
        get
        {
            lock (_atomicLock)
            {
                var outerShell = _electronShells.LastOrDefault(s => s.ElectronCount > 0);
                return outerShell?.ValenceElectrons ?? 0;
            }
        }
    }

    /// <summary>
    /// Gets the atomic radius in meters.
    /// </summary>
    public double AtomicRadius => CalculateAtomicRadius();

    /// <summary>
    /// Gets the ionic radius in meters (if ionized).
    /// </summary>
    public double IonicRadius => IsIon ? CalculateIonicRadius() : AtomicRadius;

    /// <summary>
    /// Gets the electronegativity (Pauling scale).
    /// </summary>
    public double Electronegativity => CalculateElectronegativity();

    /// <summary>
    /// Gets the first ionization energy in Joules.
    /// </summary>
    public double FirstIonizationEnergy => CalculateFirstIonizationEnergy();

    /// <summary>
    /// Gets the electron affinity in Joules.
    /// </summary>
    public double ElectronAffinity => CalculateElectronAffinity();

    /// <summary>
    /// Gets the electron configuration string.
    /// </summary>
    public string ElectronConfiguration => GetElectronConfiguration();

    /// <summary>
    /// Gets the ground state electronic term symbol.
    /// </summary>
    public string GroundStateTermSymbol => CalculateGroundStateTermSymbol();

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => CalculateAtomicSpin();
    public override ParticleStatistics Statistics => 
        (int)(2 * SpinQuantumNumber) % 2 == 0 ? ParticleStatistics.BoseEinstein : ParticleStatistics.FermiDirac;

    #endregion

    #region ICompositeParticle Implementation

    public IReadOnlyList<IQuantumParticle> Constituents
    {
        get
        {
            lock (_atomicLock)
            {
                var constituents = new List<IQuantumParticle> { Nucleus };
                constituents.AddRange(_electronShells.SelectMany(shell => shell.Electrons));
                return constituents.AsReadOnly();
            }
        }
    }

    public int ConstituentCount => 1 + ElectronCount; // Nucleus + electrons
    public IScalarMeasurable BindingEnergy => _bindingEnergy;
    public IScalarMeasurable ExcitationEnergy => _excitationEnergy;
    public IVectorMeasurable CenterOfMass => _centerOfMass;
    public IScalarMeasurable Size => _size;

    public double[,] MomentOfInertiaTensor
    {
        get
        {
            // Simplified moment of inertia for spherical atom
            var atomicRadius = AtomicRadius;
            var totalMass = _mass.Value;
            var I = 0.4 * totalMass * atomicRadius * atomicRadius; // I = (2/5)MR² for uniform sphere
            
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
    public double TotalAngularMomentumQuantumNumber => CalculateAtomicAngularMomentum();
    public double GroundStateEnergy => -_bindingEnergy.Value;
    public IReadOnlyList<ExcitedState> ExcitedStates { get; } = new List<ExcitedState>();
    public int ExcitationLevel { get; private set; } = 0;

    public double[,] ConstituentInteractionMatrix
    {
        get
        {
            var n = ConstituentCount;
            var matrix = new double[n, n];
            
            // Simplified atomic interactions
            // Nuclear-electron and electron-electron interactions
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    matrix[i, j] = matrix[j, i] = CalculateConstituentInteraction(i, j);
                }
            }
            
            return matrix;
        }
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a hydrogen atom.
    /// </summary>
    /// <param name="electronCount">Number of electrons (1 for neutral, 0 for H+).</param>
    public static Atom CreateHydrogen(int electronCount = 1) => new(1, 1, electronCount);

    /// <summary>
    /// Creates a helium atom.
    /// </summary>
    /// <param name="electronCount">Number of electrons (2 for neutral).</param>
    public static Atom CreateHelium(int electronCount = 2) => new(2, 4, electronCount);

    /// <summary>
    /// Creates a lithium atom.
    /// </summary>
    /// <param name="electronCount">Number of electrons (3 for neutral).</param>
    public static Atom CreateLithium(int electronCount = 3) => new(3, 7, electronCount);

    /// <summary>
    /// Creates a carbon atom.
    /// </summary>
    /// <param name="massNumber">Mass number (default 12).</param>
    /// <param name="electronCount">Number of electrons (6 for neutral).</param>
    public static Atom CreateCarbon(int massNumber = 12, int electronCount = 6) => new(6, massNumber, electronCount);

    /// <summary>
    /// Creates an oxygen atom.
    /// </summary>
    /// <param name="massNumber">Mass number (default 16).</param>
    /// <param name="electronCount">Number of electrons (8 for neutral).</param>
    public static Atom CreateOxygen(int massNumber = 16, int electronCount = 8) => new(8, massNumber, electronCount);

    /// <summary>
    /// Creates an atom from element symbol.
    /// </summary>
    /// <param name="elementSymbol">Element symbol (H, He, Li, etc.).</param>
    /// <param name="massNumber">Mass number (0 for most common isotope).</param>
    /// <param name="electronCount">Number of electrons (null for neutral atom).</param>
    public static Atom CreateFromSymbol(string elementSymbol, int massNumber = 0, int? electronCount = null)
    {
        var atomicNumber = GetAtomicNumberFromSymbol(elementSymbol);
        if (massNumber == 0)
            massNumber = GetMostCommonMassNumber(atomicNumber);
        
        return new Atom(atomicNumber, massNumber, electronCount);
    }

    #endregion

    #region Electron Configuration

    /// <summary>
    /// Adds an electron to the atom.
    /// </summary>
    /// <returns>True if electron was added successfully.</returns>
    public bool AddElectron()
    {
        lock (_atomicLock)
        {
            // Find the lowest energy available orbital
            foreach (var shell in _electronShells.OrderBy(s => s.PrincipalQuantumNumber))
            {
                if (shell.AddElectrons(1) > 0)
                {
                    ElectronCount++;
                    UpdateAtomicProperties();
                    return true;
                }
            }

            // Need to add a new shell
            var newShell = new ElectronShell(_electronShells.Count + 1, AtomicNumber);
            _electronShells.Add(newShell);
            
            if (newShell.AddElectrons(1) > 0)
            {
                ElectronCount++;
                UpdateAtomicProperties();
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Removes an electron from the atom.
    /// </summary>
    /// <returns>The removed electron, or null if no electrons to remove.</returns>
    public Electron? RemoveElectron()
    {
        lock (_atomicLock)
        {
            if (ElectronCount == 0) return null;

            // Remove from highest energy shell first
            for (int i = _electronShells.Count - 1; i >= 0; i--)
            {
                var shell = _electronShells[i];
                if (shell.ElectronCount > 0)
                {
                    var removedElectrons = shell.RemoveElectrons(1);
                    if (removedElectrons.Count > 0)
                    {
                        ElectronCount--;
                        UpdateAtomicProperties();
                        return removedElectrons[0];
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Ionizes the atom by removing multiple electrons.
    /// </summary>
    /// <param name="electronCount">Number of electrons to remove.</param>
    /// <returns>Collection of removed electrons.</returns>
    public IList<Electron> Ionize(int electronCount)
    {
        var validation = ParameterValidator.ValidateIntegerRange(electronCount, nameof(electronCount), 0, ElectronCount);
        validation.ThrowIfInvalid();

        var removedElectrons = new List<Electron>();

        for (int i = 0; i < electronCount; i++)
        {
            var electron = RemoveElectron();
            if (electron != null)
                removedElectrons.Add(electron);
            else
                break;
        }

        Logger.Debug("Ionized atom", 
            new { Element = ElementSymbol, RemovedElectrons = removedElectrons.Count, 
                  FinalCharge = GetIonizationState() });

        return removedElectrons;
    }

    /// <summary>
    /// Gets the complete electron configuration string.
    /// </summary>
    public string GetElectronConfiguration()
    {
        lock (_atomicLock)
        {
            var configurations = new List<string>();
            
            foreach (var shell in _electronShells.Where(s => s.ElectronCount > 0))
            {
                var shellConfig = shell.GetElectronConfiguration();
                if (!string.IsNullOrEmpty(shellConfig))
                    configurations.Add(shellConfig);
            }

            return string.Join(" ", configurations);
        }
    }

    /// <summary>
    /// Gets the noble gas configuration (abbreviated form).
    /// </summary>
    public string GetNobleGasConfiguration()
    {
        // Find the nearest lower noble gas
        var nobleGases = new[] { 2, 10, 18, 36, 54, 86, 118 }; // He, Ne, Ar, Kr, Xe, Rn, Og
        var nobleGasSymbols = new[] { "He", "Ne", "Ar", "Kr", "Xe", "Rn", "Og" };

        var nearestNobleGas = 0;
        var nearestSymbol = "";

        for (int i = 0; i < nobleGases.Length; i++)
        {
            if (nobleGases[i] < AtomicNumber)
            {
                nearestNobleGas = nobleGases[i];
                nearestSymbol = nobleGasSymbols[i];
            }
            else
            {
                break;
            }
        }

        if (nearestNobleGas == 0)
            return GetElectronConfiguration(); // No noble gas core

        // Create abbreviated configuration
        var remainingElectrons = ElectronCount - nearestNobleGas;
        if (remainingElectrons <= 0)
            return $"[{nearestSymbol}]";

        // Create temporary atom with remaining electrons to get configuration
        var tempAtom = new Atom(AtomicNumber, MassNumber, remainingElectrons);
        var remainingConfig = tempAtom.GetElectronConfiguration();

        return $"[{nearestSymbol}] {remainingConfig}";
    }

    #endregion

    #region Spectroscopic Properties

    /// <summary>
    /// Calculates the atomic emission spectrum lines.
    /// </summary>
    /// <returns>Dictionary of transition names and wavelengths.</returns>
    public Dictionary<string, double> CalculateEmissionSpectrum()
    {
        var spectrum = new Dictionary<string, double>();

        lock (_atomicLock)
        {
            // Calculate transitions between electron shells
            for (int i = 0; i < _electronShells.Count; i++)
            {
                for (int j = i + 1; j < _electronShells.Count; j++)
                {
                    var shell1 = _electronShells[i];
                    var shell2 = _electronShells[j];

                    if (shell1.ElectronCount > 0 && shell2.IsTransitionAllowed(shell1))
                    {
                        var wavelength = shell2.CalculateTransitionWavelength(shell1);
                        var transitionName = $"{shell2.ShellDesignation}→{shell1.ShellDesignation}";
                        spectrum[transitionName] = wavelength;
                    }
                }
            }
        }

        return spectrum;
    }

    /// <summary>
    /// Calculates the X-ray emission lines for this atom.
    /// </summary>
    public Dictionary<string, double> CalculateXRaySpectrum()
    {
        var xrayLines = new Dictionary<string, double>();

        lock (_atomicLock)
        {
            foreach (var shell in _electronShells.Where(s => s.PrincipalQuantumNumber <= 3))
            {
                var shellLines = shell.CalculateXRayLines();
                foreach (var line in shellLines)
                {
                    xrayLines[line.Key] = line.Value;
                }
            }
        }

        return xrayLines;
    }

    /// <summary>
    /// Determines if this atom can undergo a specific electronic transition.
    /// </summary>
    public bool CanTransition(int fromShell, int toShell)
    {
        if (fromShell < 1 || toShell < 1 || fromShell >= _electronShells.Count || toShell >= _electronShells.Count)
            return false;

        var shell1 = _electronShells[fromShell - 1];
        var shell2 = _electronShells[toShell - 1];

        return shell1.IsTransitionAllowed(shell2);
    }

    #endregion

    #region Chemical Properties

    /// <summary>
    /// Determines the oxidation states this atom can exhibit.
    /// </summary>
    public List<int> GetPossibleOxidationStates()
    {
        var states = new List<int>();

        // Common oxidation states based on electron configuration
        var valenceElectrons = ValenceElectrons;
        
        // Can lose valence electrons (positive oxidation states)
        for (int i = 1; i <= valenceElectrons && i <= 7; i++)
        {
            states.Add(i);
        }

        // Can gain electrons to complete shell (negative oxidation states)
        if (valenceElectrons > 4)
        {
            var shellCapacity = _electronShells.LastOrDefault()?.MaxElectrons ?? 8;
            var electronsToFill = shellCapacity - valenceElectrons;
            if (electronsToFill > 0 && electronsToFill <= 4)
            {
                states.Add(-electronsToFill);
            }
        }

        // Zero oxidation state (elemental form)
        states.Add(0);

        return states.Distinct().OrderBy(x => x).ToList();
    }

    /// <summary>
    /// Calculates the metallic character of this atom.
    /// </summary>
    public double CalculateMetallicCharacter()
    {
        // Based on position in periodic table and properties
        var group = GetPeriodicGroup();
        var period = GetPeriodicPeriod();

        // Metallic character decreases across period, increases down group
        var metallicScore = 1.0 - (group - 1) / 17.0 + (period - 1) / 6.0;
        
        return Math.Max(0.0, Math.Min(1.0, metallicScore));
    }

    /// <summary>
    /// Determines if this atom exhibits metallic behavior.
    /// </summary>
    public bool IsMetallic() => CalculateMetallicCharacter() > 0.5;

    /// <summary>
    /// Determines if this atom is a nonmetal.
    /// </summary>
    public bool IsNonmetal() => CalculateMetallicCharacter() < 0.3;

    /// <summary>
    /// Determines if this atom is a metalloid.
    /// </summary>
    public bool IsMetalloid() => !IsMetallic() && !IsNonmetal();

    #endregion

    #region Composite Operations

    public void AddConstituent(IQuantumParticle particle, double bindingEnergy)
    {
        if (particle is Electron electron)
        {
            AddElectron();
        }
        else
        {
            throw new ArgumentException("Can only add electrons to atoms");
        }
    }

    public bool RemoveConstituent(IQuantumParticle particle)
    {
        if (particle is Electron)
        {
            return RemoveElectron() != null;
        }
        return false;
    }

    public IQuantumParticle RemoveConstituentAt(int index)
    {
        if (index == 0) return Nucleus; // Cannot remove nucleus
        
        var electron = RemoveElectron();
        return electron ?? throw new ArgumentOutOfRangeException(nameof(index));
    }

    public IQuantumParticle GetConstituent(int index)
    {
        if (index == 0) return Nucleus;
        
        var electronIndex = index - 1;
        var allElectrons = _electronShells.SelectMany(s => s.Electrons).ToList();
        
        if (electronIndex < allElectrons.Count)
            return allElectrons[electronIndex];
            
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public IEnumerable<T> GetConstituentsOfType<T>() where T : class, IQuantumParticle
    {
        var constituents = new List<IQuantumParticle> { Nucleus };
        constituents.AddRange(_electronShells.SelectMany(s => s.Electrons));
        return constituents.OfType<T>();
    }

    public Complex[] CalculateCompositeState()
    {
        // Simplified atomic quantum state
        var compositeState = new Complex[HilbertSpaceDimension];
        compositeState[0] = new Complex(1.0, 0.0); // Ground state
        compositeState[1] = new Complex(0.0, 0.0); // Excited state
        return compositeState;
    }

    public void UpdateCompositeProperties()
    {
        UpdateAtomicProperties();
    }

    public IEnumerable<IQuantumParticle> Decompose()
    {
        var constituents = new List<IQuantumParticle> { Nucleus };
        constituents.AddRange(_electronShells.SelectMany(s => s.Electrons));
        return constituents;
    }

    public double CalculateDecayRate()
    {
        return Nucleus.CalculateDecayRate(); // Atomic decay is primarily nuclear
    }

    public void EvolveCompositeState(double timeStep)
    {
        // Evolve electronic states and nuclear decay
        Nucleus.EvolveCompositeState(timeStep);
        
        // Electronic state evolution would be implemented here
        // For now, we keep electrons in ground state
    }

    public double CalculateInternalInteractionEnergy()
    {
        // Nuclear binding energy + electron binding energies + electron-electron repulsion
        var nuclearEnergy = Nucleus.CalculateInternalInteractionEnergy();
        var electronicEnergy = CalculateElectronicBindingEnergy();
        return nuclearEnergy + electronicEnergy;
    }

    public void UpdateInternalInteractions()
    {
        UpdateCompositeProperties();
    }

    public bool IsStable()
    {
        return Nucleus.IsStable && ElectronCount <= AtomicNumber + 2; // Reasonable electron limit
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Atom(AtomicNumber, MassNumber, ElectronCount);
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        // Anti-atom would have anti-nucleus and positrons
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

    private void InitializeElectronShells()
    {
        lock (_atomicLock)
        {
            // Create shells up to what's needed for this atom
            var maxShell = CalculateRequiredShells();
            
            for (int n = 1; n <= maxShell; n++)
            {
                _electronShells.Add(new ElectronShell(n, AtomicNumber));
            }
        }
    }

    private int CalculateRequiredShells()
    {
        // Determine how many shells we need based on electron count
        if (ElectronCount <= 2) return 1;      // K shell
        if (ElectronCount <= 10) return 2;     // K, L shells
        if (ElectronCount <= 28) return 3;     // K, L, M shells
        if (ElectronCount <= 60) return 4;     // K, L, M, N shells
        if (ElectronCount <= 102) return 5;    // K, L, M, N, O shells
        if (ElectronCount <= 152) return 6;    // K, L, M, N, O, P shells
        return 7;                              // All shells
    }

    private void ConfigureElectrons()
    {
        lock (_atomicLock)
        {
            var remainingElectrons = ElectronCount;

            // Fill shells using Aufbau principle
            foreach (var shell in _electronShells.OrderBy(s => s.PrincipalQuantumNumber))
            {
                if (remainingElectrons <= 0) break;

                var electronsToAdd = Math.Min(remainingElectrons, shell.MaxElectrons);
                var added = shell.AddElectrons(electronsToAdd);
                remainingElectrons -= added;
            }
        }
    }

    private void InitializeAtomicState()
    {
        // Initialize in ground electronic state
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    private double CalculateAtomicMass()
    {
        var nuclearMass = Nucleus.Mass.Value;
        var electronMass = ElectronCount * PhysicsConstants.ElectronMass;
        var bindingEnergyMass = CalculateElectronicBindingEnergy() / (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);
        
        return nuclearMass + electronMass - bindingEnergyMass;
    }

    private double CalculateNetCharge()
    {
        return (AtomicNumber - ElectronCount) * PhysicsConstants.ElementaryCharge;
    }

    private double CalculateAtomicRadius()
    {
        if (ElectronCount == 0)
            return Nucleus.NuclearRadius; // Bare nucleus

        lock (_atomicLock)
        {
            var outerShell = _electronShells.LastOrDefault(s => s.ElectronCount > 0);
            return outerShell?.CalculateShellRadius() ?? PhysicsConstants.BohrRadius;
        }
    }

    private double CalculateIonicRadius()
    {
        // Simplified ionic radius calculation
        var neutralRadius = CalculateAtomicRadius();
        var chargeEffect = IonizationState * 0.1e-10; // ~0.1 Å per charge
        
        return Math.Max(Nucleus.NuclearRadius, neutralRadius - chargeEffect);
    }

    private double CalculateTotalBindingEnergy()
    {
        return Nucleus.BindingEnergy.Value + CalculateElectronicBindingEnergy();
    }

    private double CalculateElectronicBindingEnergy()
    {
        lock (_atomicLock)
        {
            var totalBindingEnergy = 0.0;

            foreach (var shell in _electronShells)
            {
                totalBindingEnergy += shell.ElectronCount * Math.Abs(shell.AverageElectronEnergy);
            }

            return totalBindingEnergy;
        }
    }

    private void UpdateAtomicProperties()
    {
        var newMass = CalculateAtomicMass();
        var newCharge = CalculateNetCharge();
        var newRadius = CalculateAtomicRadius();
        var newBindingEnergy = CalculateTotalBindingEnergy();

        _mass.SetValue(newMass);
        _charge.SetValue(newCharge);
        _size.SetValue(newRadius);
        _bindingEnergy.SetValue(newBindingEnergy);
    }

    private double CalculateAtomicSpin()
    {
        // Simplified atomic spin calculation
        lock (_atomicLock)
        {
            var totalSpin = 0.0;
            foreach (var shell in _electronShells)
            {
                totalSpin += shell.UnpairedElectrons * 0.5;
            }
            return totalSpin;
        }
    }

    private double CalculateAtomicAngularMomentum()
    {
        lock (_atomicLock)
        {
            var totalJ = 0.0;
            foreach (var shell in _electronShells)
            {
                totalJ += shell.CalculateTotalAngularMomentum();
            }
            return totalJ;
        }
    }

    private double CalculateElectronegativity()
    {
        // Simplified electronegativity using Mulliken scale
        var ionizationEnergy = FirstIonizationEnergy;
        var electronAffinity = ElectronAffinity;
        
        // Convert to eV and calculate Mulliken electronegativity
        var ieEv = UnitConversions.JoulesToElectronVolts(ionizationEnergy);
        var eaEv = UnitConversions.JoulesToElectronVolts(electronAffinity);
        
        return (ieEv + eaEv) / 2.0; // In eV, can be converted to Pauling scale
    }

    private double CalculateFirstIonizationEnergy()
    {
        lock (_atomicLock)
        {
            var outerShell = _electronShells.LastOrDefault(s => s.ElectronCount > 0);
            return outerShell?.IonizationEnergy ?? 0.0;
        }
    }

    private double CalculateElectronAffinity()
    {
        // Simplified electron affinity calculation
        // Based on effective nuclear charge and atomic radius
        var effectiveCharge = _electronShells.LastOrDefault()?.EffectiveNuclearCharge ?? 1.0;
        var atomicRadius = AtomicRadius;
        
        // Rough approximation in Joules
        return effectiveCharge * PhysicsConstants.ElementaryCharge * PhysicsConstants.ElementaryCharge / 
               (4.0 * Math.PI * PhysicsConstants.VacuumPermittivity * atomicRadius);
    }

    private string CalculateGroundStateTermSymbol()
    {
        // Simplified term symbol calculation
        // Would require detailed implementation of Hund's rules and term analysis
        return IsNeutral ? "¹S₀" : "²S₁/₂"; // Placeholder
    }

    private double CalculateConstituentInteraction(int i, int j)
    {
        // Simplified interaction energy between atomic constituents
        if (i == 0 || j == 0) // Interaction with nucleus
        {
            // Nuclear-electron Coulomb attraction
            return -PhysicsConstants.ElementaryCharge * PhysicsConstants.ElementaryCharge * AtomicNumber / 
                   (4.0 * Math.PI * PhysicsConstants.VacuumPermittivity * AtomicRadius);
        }
        else
        {
            // Electron-electron Coulomb repulsion
            return PhysicsConstants.ElementaryCharge * PhysicsConstants.ElementaryCharge / 
                   (4.0 * Math.PI * PhysicsConstants.VacuumPermittivity * AtomicRadius);
        }
    }

    private int GetPeriodicGroup()
    {
        // Simplified group determination based on valence electrons
        if (AtomicNumber <= 2) return AtomicNumber;
        if (AtomicNumber <= 10) return (AtomicNumber - 2) % 8 + 1;
        // More complex logic for transition metals would go here
        return 1; // Placeholder
    }

    private int GetPeriodicPeriod()
    {
        if (AtomicNumber <= 2) return 1;
        if (AtomicNumber <= 10) return 2;
        if (AtomicNumber <= 18) return 3;
        if (AtomicNumber <= 36) return 4;
        if (AtomicNumber <= 54) return 5;
        if (AtomicNumber <= 86) return 6;
        return 7;
    }

    private string GetIonizationState()
    {
        var charge = IonizationState;
        if (charge == 0) return "neutral";
        if (charge > 0) return $"{charge}+";
        return $"{Math.Abs(charge)}-";
    }

    private static string GetElementSymbol(int atomicNumber)
    {
        var symbols = new[]
        {
            "", "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne",
            "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K", "Ca",
            "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn",
            "Ga", "Ge", "As", "Se", "Br", "Kr", "Rb", "Sr", "Y", "Zr",
            "Nb", "Mo", "Tc", "Ru", "Rh", "Pd", "Ag", "Cd", "In", "Sn",
            "Sb", "Te", "I", "Xe", "Cs", "Ba", "La", "Ce", "Pr", "Nd",
            "Pm", "Sm", "Eu", "Gd", "Tb", "Dy", "Ho", "Er", "Tm", "Yb",
            "Lu", "Hf", "Ta", "W", "Re", "Os", "Ir", "Pt", "Au", "Hg",
            "Tl", "Pb", "Bi", "Po", "At", "Rn", "Fr", "Ra", "Ac", "Th",
            "Pa", "U", "Np", "Pu", "Am", "Cm", "Bk", "Cf", "Es", "Fm",
            "Md", "No", "Lr", "Rf", "Db", "Sg", "Bh", "Hs", "Mt", "Ds",
            "Rg", "Cn", "Nh", "Fl", "Mc", "Lv", "Ts", "Og"
        };

        return atomicNumber < symbols.Length ? symbols[atomicNumber] : $"E{atomicNumber}";
    }

    private static string GetElementName(int atomicNumber)
    {
        var names = new[]
        {
            "", "Hydrogen", "Helium", "Lithium", "Beryllium", "Boron", "Carbon", "Nitrogen", "Oxygen", "Fluorine", "Neon",
            "Sodium", "Magnesium", "Aluminum", "Silicon", "Phosphorus", "Sulfur", "Chlorine", "Argon", "Potassium", "Calcium"
            // Add more as needed
        };

        return atomicNumber < names.Length ? names[atomicNumber] : $"Element {atomicNumber}";
    }

    private static int GetAtomicNumberFromSymbol(string symbol)
    {
        return symbol.ToUpper() switch
        {
            "H" => 1, "HE" => 2, "LI" => 3, "BE" => 4, "B" => 5, "C" => 6, "N" => 7, "O" => 8, "F" => 9, "NE" => 10,
            "NA" => 11, "MG" => 12, "AL" => 13, "SI" => 14, "P" => 15, "S" => 16, "CL" => 17, "AR" => 18, "K" => 19, "CA" => 20,
            _ => throw new ArgumentException($"Unknown element symbol: {symbol}")
        };
    }

    private static int GetMostCommonMassNumber(int atomicNumber)
    {
        // Most common isotopes for first 20 elements
        return atomicNumber switch
        {
            1 => 1,   // H-1
            2 => 4,   // He-4
            3 => 7,   // Li-7
            4 => 9,   // Be-9
            5 => 11,  // B-11
            6 => 12,  // C-12
            7 => 14,  // N-14
            8 => 16,  // O-16
            9 => 19,  // F-19
            10 => 20, // Ne-20
            11 => 23, // Na-23
            12 => 24, // Mg-24
            13 => 27, // Al-27
            14 => 28, // Si-28
            15 => 31, // P-31
            16 => 32, // S-32
            17 => 35, // Cl-35
            18 => 40, // Ar-40
            19 => 39, // K-39
            20 => 40, // Ca-40
            _ => atomicNumber * 2 // Rough approximation for heavier elements
        };
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        var chargeStr = IonizationState == 0 ? "" : GetIonizationState();
        return $"{ElementSymbol}-{MassNumber}{chargeStr}";
    }

    public string ToDetailedString()
    {
        var details = new List<string>
        {
            $"Element: {ElementName} ({ElementSymbol})",
            $"Atomic Number: {AtomicNumber}",
            $"Mass Number: {MassNumber}",
            $"Electrons: {ElectronCount}",
            $"Charge: {GetIonizationState()}",
            $"Mass: {UnitConversions.KilogramsToAtomicMassUnits(_mass.Value):F6} u",
            $"Atomic Radius: {AtomicRadius * 1e12:F1} pm",
            $"Electron Configuration: {ElectronConfiguration}",
            $"Valence Electrons: {ValenceElectrons}",
            $"First Ionization Energy: {UnitConversions.JoulesToElectronVolts(FirstIonizationEnergy):F2} eV",
            $"Electronegativity: {Electronegativity:F2}",
            $"Ground State Term: {GroundStateTermSymbol}"
        };

        return string.Join("\n", details);
    }

    #endregion
}
