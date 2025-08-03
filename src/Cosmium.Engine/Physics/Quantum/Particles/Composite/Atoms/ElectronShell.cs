using System.Numerics;
using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Leptons;
using Cosmium.Engine.Physics.Quantum.Orbitals;

namespace Cosmium.Engine.Physics.Quantum.Particles.Composite.Atoms;

/// <summary>
/// Represents an electron shell in an atom with quantum mechanical orbital structure.
/// Manages electron configuration, occupancy, and energy levels according to quantum mechanics principles.
/// </summary>
public class ElectronShell
{
    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly List<OrbitalElectronPair> _orbitals;
    private readonly object _configurationLock = new();

    #region Shell Constants

    /// <summary>
    /// Maximum number of electrons per orbital (Pauli exclusion principle).
    /// </summary>
    public const int MaxElectronsPerOrbital = 2;

    /// <summary>
    /// Shell designations (K, L, M, N, O, P, Q).
    /// </summary>
    public static readonly string[] ShellNames = { "K", "L", "M", "N", "O", "P", "Q" };

    /// <summary>
    /// Subshell designations (s, p, d, f, g, h, i).
    /// </summary>
    public static readonly string[] SubshellNames = { "s", "p", "d", "f", "g", "h", "i" };

    /// <summary>
    /// Maximum number of electrons per subshell type.
    /// </summary>
    public static readonly Dictionary<int, int> MaxElectronsPerSubshell = new()
    {
        { 0, 2 },  // s subshell: 2 electrons
        { 1, 6 },  // p subshell: 6 electrons
        { 2, 10 }, // d subshell: 10 electrons
        { 3, 14 }, // f subshell: 14 electrons
        { 4, 18 }, // g subshell: 18 electrons
        { 5, 22 }, // h subshell: 22 electrons
        { 6, 26 }  // i subshell: 26 electrons
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new electron shell with the specified configuration.
    /// </summary>
    /// <param name="principalQuantumNumber">Principal quantum number (n).</param>
    /// <param name="atomicNumber">Atomic number for calculating effective nuclear charge.</param>
    public ElectronShell(int principalQuantumNumber, int atomicNumber)
    {
        var validation = ParameterValidator.ValidateIntegerRange(principalQuantumNumber, nameof(principalQuantumNumber), 1, 7);
        validation.ThrowIfInvalid();

        var atomicValidation = ParameterValidator.ValidateIntegerRange(atomicNumber, nameof(atomicNumber), 1, 118);
        atomicValidation.ThrowIfInvalid();

        PrincipalQuantumNumber = principalQuantumNumber;
        AtomicNumber = atomicNumber;
        _orbitals = new List<OrbitalElectronPair>();

        // Initialize orbitals for this shell
        InitializeOrbitals();

        Logger.Debug("Created electron shell", 
            new { Shell = ShellDesignation, PrincipalN = principalQuantumNumber, AtomicZ = atomicNumber });
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the principal quantum number (n) for this shell.
    /// </summary>
    public int PrincipalQuantumNumber { get; }

    /// <summary>
    /// Gets the atomic number used for effective nuclear charge calculations.
    /// </summary>
    public int AtomicNumber { get; }

    /// <summary>
    /// Gets the shell designation (K, L, M, etc.).
    /// </summary>
    public string ShellDesignation => 
        PrincipalQuantumNumber <= ShellNames.Length ? ShellNames[PrincipalQuantumNumber - 1] : $"n={PrincipalQuantumNumber}";

    /// <summary>
    /// Gets the maximum number of electrons this shell can hold.
    /// </summary>
    public int MaxElectrons => 2 * PrincipalQuantumNumber * PrincipalQuantumNumber;

    /// <summary>
    /// Gets the current number of electrons in this shell.
    /// </summary>
    public int ElectronCount
    {
        get
        {
            lock (_configurationLock)
            {
                return _orbitals.Sum(orbital => orbital.ElectronCount);
            }
        }
    }

    /// <summary>
    /// Gets whether this shell is completely filled.
    /// </summary>
    public bool IsFilled => ElectronCount == MaxElectrons;

    /// <summary>
    /// Gets whether this shell is empty.
    /// </summary>
    public bool IsEmpty => ElectronCount == 0;

    /// <summary>
    /// Gets the number of unpaired electrons in this shell.
    /// </summary>
    public int UnpairedElectrons
    {
        get
        {
            lock (_configurationLock)
            {
                return _orbitals.Sum(orbital => orbital.UnpairedElectrons);
            }
        }
    }

    /// <summary>
    /// Gets the effective nuclear charge experienced by electrons in this shell.
    /// </summary>
    public double EffectiveNuclearCharge => CalculateEffectiveNuclearCharge();

    /// <summary>
    /// Gets the average energy of electrons in this shell.
    /// </summary>
    public double AverageElectronEnergy => CalculateAverageElectronEnergy();

    /// <summary>
    /// Gets the ionization energy required to remove an electron from this shell.
    /// </summary>
    public double IonizationEnergy => CalculateIonizationEnergy();

    /// <summary>
    /// Gets all orbitals in this shell.
    /// </summary>
    public IReadOnlyList<OrbitalElectronPair> Orbitals
    {
        get
        {
            lock (_configurationLock)
            {
                return _orbitals.AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets all electrons in this shell.
    /// </summary>
    public IEnumerable<Electron> Electrons
    {
        get
        {
            lock (_configurationLock)
            {
                return _orbitals.SelectMany(orbital => orbital.Electrons);
            }
        }
    }

    /// <summary>
    /// Gets the valence electron count (for outermost shell).
    /// </summary>
    public int ValenceElectrons => ElectronCount;

    #endregion

    #region Electron Configuration Methods

    /// <summary>
    /// Adds electrons to the shell using the Aufbau principle.
    /// </summary>
    /// <param name="electronCount">Number of electrons to add.</param>
    /// <returns>Number of electrons actually added.</returns>
    public int AddElectrons(int electronCount)
    {
        var validation = ParameterValidator.ValidateIntegerRange(electronCount, nameof(electronCount), 0, int.MaxValue);
        validation.ThrowIfInvalid();

        lock (_configurationLock)
        {
            var electronsToAdd = Math.Min(electronCount, MaxElectrons - ElectronCount);
            var electronsAdded = 0;

            // Sort orbitals by energy (Aufbau principle)
            var sortedOrbitals = _orbitals.OrderBy(o => o.Orbital.Energy).ToList();

            foreach (var orbital in sortedOrbitals)
            {
                if (electronsAdded >= electronsToAdd) break;

                var canAdd = Math.Min(electronsToAdd - electronsAdded, MaxElectronsPerOrbital - orbital.ElectronCount);
                if (canAdd > 0)
                {
                    orbital.AddElectrons(canAdd);
                    electronsAdded += canAdd;
                }
            }

            Logger.Debug("Added electrons to shell", 
                new { Shell = ShellDesignation, Added = electronsAdded, Total = ElectronCount });

            return electronsAdded;
        }
    }

    /// <summary>
    /// Removes electrons from the shell (highest energy first).
    /// </summary>
    /// <param name="electronCount">Number of electrons to remove.</param>
    /// <returns>Collection of removed electrons.</returns>
    public IList<Electron> RemoveElectrons(int electronCount)
    {
        var validation = ParameterValidator.ValidateIntegerRange(electronCount, nameof(electronCount), 0, int.MaxValue);
        validation.ThrowIfInvalid();

        lock (_configurationLock)
        {
            var electronsToRemove = Math.Min(electronCount, ElectronCount);
            var removedElectrons = new List<Electron>();

            // Remove from highest energy orbitals first
            var sortedOrbitals = _orbitals.OrderByDescending(o => o.Orbital.Energy).ToList();

            foreach (var orbital in sortedOrbitals)
            {
                if (removedElectrons.Count >= electronsToRemove) break;

                var canRemove = Math.Min(electronsToRemove - removedElectrons.Count, orbital.ElectronCount);
                if (canRemove > 0)
                {
                    var removed = orbital.RemoveElectrons(canRemove);
                    removedElectrons.AddRange(removed);
                }
            }

            Logger.Debug("Removed electrons from shell", 
                new { Shell = ShellDesignation, Removed = removedElectrons.Count, Total = ElectronCount });

            return removedElectrons;
        }
    }

    /// <summary>
    /// Gets the electron configuration string for this shell.
    /// </summary>
    /// <returns>Configuration string (e.g., "1s² 2s² 2p⁶").</returns>
    public string GetElectronConfiguration()
    {
        lock (_configurationLock)
        {
            var configurations = new List<string>();

            foreach (var orbital in _orbitals.Where(o => o.ElectronCount > 0).OrderBy(o => o.Orbital.Energy))
            {
                var config = $"{orbital.Orbital.PrincipalQuantumNumber}{SubshellNames[orbital.Orbital.OrbitalAngularMomentumQuantumNumber]}";
                var electronCount = orbital.ElectronCount;
                
                // Add superscript numbers
                config += electronCount switch
                {
                    1 => "¹",
                    2 => "²",
                    3 => "³",
                    4 => "⁴",
                    5 => "⁵",
                    6 => "⁶",
                    7 => "⁷",
                    8 => "⁸",
                    9 => "⁹",
                    10 => "¹⁰",
                    _ => $"^{electronCount}"
                };
                
                configurations.Add(config);
            }

            return string.Join(" ", configurations);
        }
    }

    /// <summary>
    /// Calculates the total angular momentum quantum number for this shell.
    /// </summary>
    /// <returns>Total angular momentum quantum number J.</returns>
    public double CalculateTotalAngularMomentum()
    {
        lock (_configurationLock)
        {
            // Simplified calculation using Hund's rules
            var totalL = 0.0;
            var totalS = 0.0;

            foreach (var orbital in _orbitals.Where(o => o.ElectronCount > 0))
            {
                totalL += orbital.Orbital.OrbitalAngularMomentumQuantumNumber * orbital.ElectronCount;
                totalS += orbital.UnpairedElectrons * 0.5;
            }

            // J = |L ± S| depending on shell filling
            return IsFilled ? Math.Abs(totalL - totalS) : totalL + totalS;
        }
    }

    /// <summary>
    /// Calculates the magnetic moment of this shell.
    /// </summary>
    /// <returns>Magnetic moment in Bohr magnetons.</returns>
    public double CalculateMagneticMoment()
    {
        var totalJ = CalculateTotalAngularMomentum();
        if (totalJ == 0) return 0.0;

        var totalL = 0.0;
        var totalS = 0.0;

        lock (_configurationLock)
        {
            foreach (var orbital in _orbitals.Where(o => o.ElectronCount > 0))
            {
                totalL += orbital.Orbital.OrbitalAngularMomentumQuantumNumber * orbital.ElectronCount;
                totalS += orbital.UnpairedElectrons * 0.5;
            }
        }

        // Landé g-factor
        var gJ = 1.0 + (totalJ * (totalJ + 1) + totalS * (totalS + 1) - totalL * (totalL + 1)) / 
                      (2.0 * totalJ * (totalJ + 1));

        return gJ * Math.Sqrt(totalJ * (totalJ + 1));
    }

    #endregion

    #region Spectroscopic Methods

    /// <summary>
    /// Calculates the energy required for an electronic transition from this shell.
    /// </summary>
    /// <param name="targetShell">Target shell for the transition.</param>
    /// <returns>Transition energy in Joules.</returns>
    public double CalculateTransitionEnergy(ElectronShell targetShell)
    {
        if (targetShell == null)
            throw new ArgumentNullException(nameof(targetShell));

        return Math.Abs(AverageElectronEnergy - targetShell.AverageElectronEnergy);
    }

    /// <summary>
    /// Calculates the wavelength of photon emission for a transition.
    /// </summary>
    /// <param name="targetShell">Target shell for the transition.</param>
    /// <returns>Photon wavelength in meters.</returns>
    public double CalculateTransitionWavelength(ElectronShell targetShell)
    {
        var transitionEnergy = CalculateTransitionEnergy(targetShell);
        return PhysicsConstants.PlanckConstant * PhysicsConstants.SpeedOfLight / transitionEnergy;
    }

    /// <summary>
    /// Determines if a transition to the target shell is allowed by selection rules.
    /// </summary>
    /// <param name="targetShell">Target shell for the transition.</param>
    /// <returns>True if transition is allowed.</returns>
    public bool IsTransitionAllowed(ElectronShell targetShell)
    {
        if (targetShell == null) return false;

        var deltaL = Math.Abs(CalculateTotalAngularMomentum() - targetShell.CalculateTotalAngularMomentum());
        
        // Electric dipole selection rules: ΔL = ±1, ΔJ = 0, ±1 (but J=0 ↔ J=0 forbidden)
        return deltaL == 1.0;
    }

    /// <summary>
    /// Calculates the X-ray emission lines from inner shell transitions.
    /// </summary>
    /// <returns>Dictionary of X-ray line names and energies.</returns>
    public Dictionary<string, double> CalculateXRayLines()
    {
        var lines = new Dictionary<string, double>();

        if (PrincipalQuantumNumber == 1) // K shell
        {
            // K-alpha and K-beta lines would be calculated here
            // This is a simplified version
            var kAlpha = IonizationEnergy * 0.85; // Approximate
            var kBeta = IonizationEnergy * 0.95;
            
            lines["Kα"] = kAlpha;
            lines["Kβ"] = kBeta;
        }
        else if (PrincipalQuantumNumber == 2) // L shell
        {
            var lAlpha = IonizationEnergy * 0.75;
            lines["Lα"] = lAlpha;
        }

        return lines;
    }

    #endregion

    #region Chemical Properties

    /// <summary>
    /// Determines the chemical bonding capability of this shell.
    /// </summary>
    /// <returns>Bonding capability descriptor.</returns>
    public BondingCapability GetBondingCapability()
    {
        var valenceElectrons = ValenceElectrons;
        var maxElectrons = MaxElectrons;

        if (valenceElectrons == 0)
            return new BondingCapability { Type = "Inert", Capacity = 0, Description = "No electrons for bonding" };

        if (valenceElectrons == maxElectrons)
            return new BondingCapability { Type = "Inert", Capacity = 0, Description = "Closed shell configuration" };

        if (valenceElectrons <= maxElectrons / 2)
            return new BondingCapability 
            { 
                Type = "Metallic", 
                Capacity = valenceElectrons, 
                Description = "Tends to lose electrons" 
            };

        return new BondingCapability 
        { 
            Type = "Covalent", 
            Capacity = maxElectrons - valenceElectrons, 
            Description = "Tends to gain electrons" 
        };
    }

    /// <summary>
    /// Calculates the electronegativity contribution from this shell.
    /// </summary>
    /// <returns>Electronegativity value (Pauling scale).</returns>
    public double CalculateElectronegativity()
    {
        var effectiveCharge = EffectiveNuclearCharge;
        var shellRadius = CalculateShellRadius();
        
        // Simplified electronegativity calculation
        return effectiveCharge / (shellRadius * 1e10); // Convert to Angstroms
    }

    /// <summary>
    /// Determines if this shell contributes to metallic behavior.
    /// </summary>
    /// <returns>True if shell exhibits metallic character.</returns>
    public bool IsMetallic()
    {
        return ValenceElectrons <= 3 && !IsFilled;
    }

    #endregion

    #region Quantum Mechanical Properties

    /// <summary>
    /// Calculates the radial probability density at a given distance.
    /// </summary>
    /// <param name="radius">Distance from nucleus in meters.</param>
    /// <returns>Probability density.</returns>
    public double CalculateRadialProbabilityDensity(double radius)
    {
        var validation = ParameterValidator.ValidatePositive(radius, nameof(radius));
        validation.ThrowIfInvalid();

        lock (_configurationLock)
        {
            var totalDensity = 0.0;

            foreach (var orbital in _orbitals.Where(o => o.ElectronCount > 0))
            {
                var radialWaveFunction = orbital.Orbital.RadialWaveFunction(radius);
                totalDensity += orbital.ElectronCount * radialWaveFunction * radialWaveFunction * radius * radius;
            }

            return totalDensity;
        }
    }

    /// <summary>
    /// Calculates the most probable radius for electrons in this shell.
    /// </summary>
    /// <returns>Most probable radius in meters.</returns>
    public double CalculateMostProbableRadius()
    {
        lock (_configurationLock)
        {
            if (!_orbitals.Any(o => o.ElectronCount > 0))
                return 0.0;

            var weightedSum = 0.0;
            var totalElectrons = 0;

            foreach (var orbital in _orbitals.Where(o => o.ElectronCount > 0))
            {
                var radius = orbital.Orbital.MostProbableRadius();
                weightedSum += orbital.ElectronCount * radius;
                totalElectrons += orbital.ElectronCount;
            }

            return weightedSum / totalElectrons;
        }
    }

    /// <summary>
    /// Calculates the shell radius based on orbital sizes.
    /// </summary>
    /// <returns>Shell radius in meters.</returns>
    public double CalculateShellRadius()
    {
        return PrincipalQuantumNumber * PrincipalQuantumNumber * PhysicsConstants.BohrRadius / EffectiveNuclearCharge;
    }

    #endregion

    #region Private Methods

    private void InitializeOrbitals()
    {
        lock (_configurationLock)
        {
            var n = PrincipalQuantumNumber;

            // Create orbitals for all valid l values in this shell
            for (int l = 0; l < n; l++)
            {
                // Create orbitals for all valid ml values
                for (int ml = -l; ml <= l; ml++)
                {
                    var orbital = new HydrogenicOrbital(n, l, ml, EffectiveNuclearCharge);
                    _orbitals.Add(new OrbitalElectronPair(orbital));
                }
            }
        }
    }

    private double CalculateEffectiveNuclearCharge()
    {
        // Slater's rules for screening
        var Z = AtomicNumber;
        var n = PrincipalQuantumNumber;
        
        // Simplified Slater screening constants
        var screening = 0.0;
        
        if (n == 1) // 1s
        {
            screening = 0.30 * (ElectronCount - 1);
        }
        else if (n == 2) // 2s, 2p
        {
            screening = 0.85 * GetElectronsInShell(1) + 0.35 * (ElectronCount - 1);
        }
        else if (n >= 3) // 3s, 3p, 3d and higher
        {
            for (int shell = 1; shell < n; shell++)
            {
                if (shell < n - 1)
                    screening += GetElectronsInShell(shell);
                else
                    screening += 0.85 * GetElectronsInShell(shell);
            }
            screening += 0.35 * (ElectronCount - 1);
        }

        return Math.Max(1.0, Z - screening);
    }

    private int GetElectronsInShell(int shellNumber)
    {
        // This would need access to other shells in a complete atom
        // For now, return a simplified estimate
        if (shellNumber < PrincipalQuantumNumber)
            return 2 * shellNumber * shellNumber; // Assume lower shells are filled
        if (shellNumber == PrincipalQuantumNumber)
            return ElectronCount;
        return 0;
    }

    private double CalculateAverageElectronEnergy()
    {
        lock (_configurationLock)
        {
            if (ElectronCount == 0) return 0.0;

            var weightedSum = 0.0;
            var totalElectrons = 0;

            foreach (var orbital in _orbitals.Where(o => o.ElectronCount > 0))
            {
                weightedSum += orbital.ElectronCount * orbital.Orbital.Energy;
                totalElectrons += orbital.ElectronCount;
            }

            return weightedSum / totalElectrons;
        }
    }

    private double CalculateIonizationEnergy()
    {
        // Simplified ionization energy calculation
        var effectiveCharge = EffectiveNuclearCharge;
        var n = PrincipalQuantumNumber;
        
        // Hydrogen-like ionization energy: -13.6 eV * Z_eff² / n²
        var ionizationEnergyEV = 13.6 * effectiveCharge * effectiveCharge / (n * n);
        
        // Convert to Joules
        return ionizationEnergyEV * 1.602176634e-19;
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Creates an electron shell with a specific electron configuration.
    /// </summary>
    /// <param name="principalQuantumNumber">Principal quantum number.</param>
    /// <param name="atomicNumber">Atomic number.</param>
    /// <param name="electronCount">Number of electrons to add.</param>
    /// <returns>Configured electron shell.</returns>
    public static ElectronShell CreateWithElectrons(int principalQuantumNumber, int atomicNumber, int electronCount)
    {
        var shell = new ElectronShell(principalQuantumNumber, atomicNumber);
        shell.AddElectrons(electronCount);
        return shell;
    }

    /// <summary>
    /// Creates a completely filled electron shell.
    /// </summary>
    /// <param name="principalQuantumNumber">Principal quantum number.</param>
    /// <param name="atomicNumber">Atomic number.</param>
    /// <returns>Filled electron shell.</returns>
    public static ElectronShell CreateFilled(int principalQuantumNumber, int atomicNumber)
    {
        var shell = new ElectronShell(principalQuantumNumber, atomicNumber);
        shell.AddElectrons(shell.MaxElectrons);
        return shell;
    }

    #endregion

    #region ToString

    public override string ToString()
    {
        return $"Shell {ShellDesignation} (n={PrincipalQuantumNumber}): {ElectronCount}/{MaxElectrons} electrons";
    }

    public string ToDetailedString()
    {
        var details = new List<string>
        {
            $"Shell: {ShellDesignation} (n={PrincipalQuantumNumber})",
            $"Electrons: {ElectronCount}/{MaxElectrons}",
            $"Configuration: {GetElectronConfiguration()}",
            $"Effective Z: {EffectiveNuclearCharge:F2}",
            $"Average Energy: {UnitConversions.JoulesToElectronVolts(AverageElectronEnergy):F2} eV",
            $"Ionization Energy: {UnitConversions.JoulesToElectronVolts(IonizationEnergy):F2} eV",
            $"Shell Radius: {CalculateShellRadius() * 1e12:F1} pm",
            $"Unpaired Electrons: {UnpairedElectrons}",
            $"Total Angular Momentum: {CalculateTotalAngularMomentum():F1}"
        };

        return string.Join("\n", details);
    }

    #endregion
}

/// <summary>
/// Represents an orbital with its associated electrons.
/// </summary>
public class OrbitalElectronPair
{
    private readonly List<Electron> _electrons;
    private readonly object _electronLock = new();

    public OrbitalElectronPair(Orbital orbital)
    {
        Orbital = orbital ?? throw new ArgumentNullException(nameof(orbital));
        _electrons = new List<Electron>();
    }

    /// <summary>
    /// Gets the orbital associated with this pair.
    /// </summary>
    public Orbital Orbital { get; }

    /// <summary>
    /// Gets the current number of electrons in this orbital.
    /// </summary>
    public int ElectronCount
    {
        get
        {
            lock (_electronLock)
            {
                return _electrons.Count;
            }
        }
    }

    /// <summary>
    /// Gets the number of unpaired electrons in this orbital.
    /// </summary>
    public int UnpairedElectrons => ElectronCount % 2;

    /// <summary>
    /// Gets whether this orbital is completely filled.
    /// </summary>
    public bool IsFilled => ElectronCount >= ElectronShell.MaxElectronsPerOrbital;

    /// <summary>
    /// Gets all electrons in this orbital.
    /// </summary>
    public IReadOnlyList<Electron> Electrons
    {
        get
        {
            lock (_electronLock)
            {
                return _electrons.AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Adds electrons to this orbital.
    /// </summary>
    /// <param name="count">Number of electrons to add.</param>
    /// <returns>Number of electrons actually added.</returns>
    public int AddElectrons(int count)
    {
        lock (_electronLock)
        {
            var canAdd = Math.Min(count, ElectronShell.MaxElectronsPerOrbital - ElectronCount);
            
            for (int i = 0; i < canAdd; i++)
            {
                var electron = new Electron(false); // Create electron
                _electrons.Add(electron);
            }

            return canAdd;
        }
    }

    /// <summary>
    /// Removes electrons from this orbital.
    /// </summary>
    /// <param name="count">Number of electrons to remove.</param>
    /// <returns>Collection of removed electrons.</returns>
    public IList<Electron> RemoveElectrons(int count)
    {
        lock (_electronLock)
        {
            var canRemove = Math.Min(count, ElectronCount);
            var removed = new List<Electron>();

            for (int i = 0; i < canRemove; i++)
            {
                var electron = _electrons[_electrons.Count - 1];
                _electrons.RemoveAt(_electrons.Count - 1);
                removed.Add(electron);
            }

            return removed;
        }
    }

    public override string ToString()
    {
        var orbitalDesignation = Orbital.OrbitalDesignation;
        return $"{orbitalDesignation}: {ElectronCount}/{ElectronShell.MaxElectronsPerOrbital} electrons";
    }
}

/// <summary>
/// Describes the bonding capability of an electron shell.
/// </summary>
public class BondingCapability
{
    /// <summary>
    /// Type of bonding (Metallic, Covalent, Ionic, Inert).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of bonds this shell can form.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Description of the bonding behavior.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Type} bonding (capacity: {Capacity}) - {Description}";
    }
}
