using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Leptons;

/// <summary>
/// Implementation of a muon following the Standard Model of particle physics.
/// The muon is a second-generation lepton, essentially a heavy electron.
/// It has the same charge as an electron but is ~207 times more massive and unstable.
/// </summary>
public class Muon : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _leptonNumber;
    private readonly ScalarMeasurableProperty _meanLifetime;

    #region Constants

    /// <summary>
    /// Muon rest mass in kg (PDG 2022 value).
    /// </summary>
    public const double MuonRestMass = 1.883531627e-28; // ≈ 105.66 MeV/c²

    /// <summary>
    /// Muon mean lifetime in seconds.
    /// </summary>
    public const double MuonMeanLifetime = 2.1969811e-6; // ≈ 2.2 μs

    /// <summary>
    /// Muon anomalous magnetic moment (g-2)/2.
    /// </summary>
    public const double MuonAnomalousMoment = 1.16592089e-3;

    #endregion

    #region Properties

    /// <summary>
    /// Gets whether this is an antimuon (positive muon).
    /// </summary>
    public bool IsAntimuon { get; }

    /// <summary>
    /// Gets the muon lepton number (+1 for muon, -1 for antimuon).
    /// </summary>
    public double MuonLeptonNumber => IsAntimuon ? -1.0 : 1.0;

    /// <summary>
    /// Gets the generation number (2 for muon).
    /// </summary>
    public int Generation => 2;

    /// <summary>
    /// Gets the magnetic moment of the muon in nuclear magnetons.
    /// </summary>
    public double MagneticMoment 
    { 
        get
        {
            var factor = IsAntimuon ? -1.0 : 1.0;
            return factor * (1.0 + MuonAnomalousMoment);
        }
    }

    /// <summary>
    /// Gets whether the muon has decayed.
    /// </summary>
    public bool HasDecayed { get; private set; }

    /// <summary>
    /// Gets the time since creation (for decay calculations).
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - CreationTime;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new muon.
    /// </summary>
    /// <param name="isAntimuon">Whether this is an antimuon (positive muon). Default: false.</param>
    public Muon(bool isAntimuon = false) 
        : base(isAntimuon ? "Antimuon" : "Muon", 
               isAntimuon ? "μ⁺" : "μ⁻", 
               isElementary: true, hilbertSpaceDimension: 2) // Spin-1/2 fermion
    {
        IsAntimuon = isAntimuon;

        // Initialize physical properties
        var mass = MuonRestMass;
        var charge = (isAntimuon ? 1.0 : -1.0) * PhysicsConstants.ElementaryCharge;
        var leptonNumber = isAntimuon ? -1.0 : 1.0;

        _mass = new ScalarMeasurableProperty("Mass", "kg", mass);
        _charge = new ScalarMeasurableProperty("Charge", "C", charge);
        _leptonNumber = new ScalarMeasurableProperty("MuonLeptonNumber", "dimensionless", leptonNumber);
        _meanLifetime = new ScalarMeasurableProperty("MeanLifetime", "s", MuonMeanLifetime);

        // Initialize quantum state
        InitializeMuonState();
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.5; // Spin-1/2 fermion
    public override ParticleStatistics Statistics => ParticleStatistics.FermiDirac; // Fermion

    #endregion

    #region Muon-Specific Properties

    /// <summary>
    /// Gets the muon lepton number as a measurable property.
    /// </summary>
    public IScalarMeasurable MuonLeptonNumberMeasurable => _leptonNumber;

    /// <summary>
    /// Gets the mean lifetime as a measurable property.
    /// </summary>
    public IScalarMeasurable MeanLifetime => _meanLifetime;

    /// <summary>
    /// Gets the classical muon radius (similar to electron radius but for muon mass).
    /// </summary>
    public double ClassicalRadius
    {
        get
        {
            var e2 = PhysicsConstants.ElementaryCharge * PhysicsConstants.ElementaryCharge;
            var denominator = 4.0 * Math.PI * PhysicsConstants.VacuumPermittivity * 
                             MuonRestMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
            return e2 / denominator;
        }
    }

    /// <summary>
    /// Gets the Compton wavelength of the muon.
    /// </summary>
    public double ComptonWavelength
    {
        get
        {
            return PhysicsConstants.PlanckConstant / 
                   (MuonRestMass * PhysicsConstants.SpeedOfLight);
        }
    }

    #endregion

    #region Decay Methods

    /// <summary>
    /// Calculates the decay probability based on exponential decay law.
    /// </summary>
    /// <returns>Probability that the muon has decayed by now.</returns>
    public double CalculateDecayProbability()
    {
        var ageInSeconds = Age.TotalSeconds;
        return 1.0 - Math.Exp(-ageInSeconds / MuonMeanLifetime);
    }

    /// <summary>
    /// Calculates the remaining probability that the muon is still alive.
    /// </summary>
    /// <returns>Probability that the muon has not yet decayed.</returns>
    public double CalculateSurvivalProbability()
    {
        var ageInSeconds = Age.TotalSeconds;
        return Math.Exp(-ageInSeconds / MuonMeanLifetime);
    }

    /// <summary>
    /// Simulates muon decay and returns the decay products.
    /// Muon decay: μ⁻ → e⁻ + ν̄ₑ + νμ (or charge conjugate for μ⁺)
    /// </summary>
    /// <param name="random">Random number generator for stochastic decay.</param>
    /// <returns>Decay products if decay occurs, null otherwise.</returns>
    public MuonDecayProducts? AttemptDecay(Random? random = null)
    {
        if (HasDecayed) return null;

        random ??= new Random();
        var decayProbability = CalculateDecayProbability();
        
        if (random.NextDouble() < decayProbability)
        {
            HasDecayed = true;
            return SimulateDecay(random);
        }
        
        return null;
    }

    /// <summary>
    /// Forces immediate decay and returns the products.
    /// </summary>
    /// <param name="random">Random number generator for decay kinematics.</param>
    /// <returns>The decay products.</returns>
    public MuonDecayProducts ForceDecay(Random? random = null)
    {
        if (!HasDecayed)
        {
            HasDecayed = true;
            return SimulateDecay(random ?? new Random());
        }
        
        throw new InvalidOperationException("Muon has already decayed");
    }

    private MuonDecayProducts SimulateDecay(Random random)
    {
        // Create decay products
        var electron = new Electron(IsAntimuon); // Electron (or positron for antimuon)
        var electronNeutrino = new Neutrino(NeutrinoType.Electron, IsAntimuon); // ν̄ₑ or νₑ
        var muonNeutrino = new Neutrino(NeutrinoType.Muon, !IsAntimuon); // νμ or ν̄μ
        
        // Simulate energy and momentum distribution (simplified)
        var totalEnergy = MuonRestMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
        
        // In reality, this would involve detailed phase space calculations
        // For simplicity, we'll assign random energies that conserve total energy
        var electronEnergy = totalEnergy * (0.1 + 0.4 * random.NextDouble()); // 10-50% of total
        var neutrinoEnergy1 = totalEnergy * (0.2 + 0.3 * random.NextDouble()); // 20-50% of total
        var neutrinoEnergy2 = totalEnergy - electronEnergy - neutrinoEnergy1;
        
        return new MuonDecayProducts(electron, electronNeutrino, muonNeutrino, 
                                   electronEnergy, neutrinoEnergy1, neutrinoEnergy2);
    }

    #endregion

    #region Muonic Atom Methods

    /// <summary>
    /// Calculates the binding energy in a muonic atom (atom with muon instead of electron).
    /// Due to the muon's larger mass, it orbits much closer to the nucleus.
    /// </summary>
    /// <param name="nuclearCharge">Nuclear charge (Z).</param>
    /// <param name="principalQuantumNumber">Principal quantum number (n).</param>
    /// <returns>Binding energy in Joules.</returns>
    public double CalculateMuonicBindingEnergy(int nuclearCharge, int principalQuantumNumber)
    {
        if (principalQuantumNumber <= 0) 
            throw new ArgumentException("Principal quantum number must be positive", nameof(principalQuantumNumber));
        if (nuclearCharge <= 0) 
            throw new ArgumentException("Nuclear charge must be positive", nameof(nuclearCharge));

        // Muonic atoms have much stronger binding due to larger reduced mass
        var massRatio = MuonRestMass / PhysicsConstants.ElectronMass;
        var rydbergEnergy = 13.6 * 1.602176634e-19; // Convert eV to Joules
        
        return -rydbergEnergy * massRatio * nuclearCharge * nuclearCharge / 
               (principalQuantumNumber * principalQuantumNumber);
    }

    /// <summary>
    /// Calculates the orbital radius in a muonic atom.
    /// </summary>
    /// <param name="nuclearCharge">Nuclear charge (Z).</param>
    /// <param name="principalQuantumNumber">Principal quantum number (n).</param>
    /// <returns>Orbital radius in meters.</returns>
    public double CalculateMuonicRadius(int nuclearCharge, int principalQuantumNumber)
    {
        if (principalQuantumNumber <= 0) 
            throw new ArgumentException("Principal quantum number must be positive", nameof(principalQuantumNumber));
        if (nuclearCharge <= 0) 
            throw new ArgumentException("Nuclear charge must be positive", nameof(nuclearCharge));

        // Muonic orbits are much smaller due to larger mass
        var massRatio = PhysicsConstants.ElectronMass / MuonRestMass;
        return PhysicsConstants.BohrRadius * massRatio * principalQuantumNumber * principalQuantumNumber / nuclearCharge;
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Muons interact electromagnetically with all charged particles
        return !other.Charge.Value.Equals(0.0);
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // All leptons participate in weak interactions
        return true;
    }

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Muons do not participate in strong interactions
        return false;
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Muon(IsAntimuon);
        clone.StateVector = StateVector;
        // Note: Clone does not copy decay state - clones start fresh
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        return new Muon(!IsAntimuon);
    }

    #endregion

    #region Private Helper Methods

    private void InitializeMuonState()
    {
        // Initialize in a definite spin-up state
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a standard muon (negative charge).
    /// </summary>
    /// <returns>A new muon instance.</returns>
    public static Muon CreateMuon() => new(false);

    /// <summary>
    /// Creates an antimuon (positive charge).
    /// </summary>
    /// <returns>A new antimuon instance.</returns>
    public static Muon CreateAntimuon() => new(true);

    #endregion
}

/// <summary>
/// Represents the products of muon decay.
/// </summary>
public class MuonDecayProducts
{
    public Electron Electron { get; }
    public Neutrino ElectronNeutrino { get; }
    public Neutrino MuonNeutrino { get; }
    public double ElectronEnergy { get; }
    public double ElectronNeutrinoEnergy { get; }
    public double MuonNeutrinoEnergy { get; }

    public MuonDecayProducts(Electron electron, Neutrino electronNeutrino, Neutrino muonNeutrino,
                           double electronEnergy, double electronNeutrinoEnergy, double muonNeutrinoEnergy)
    {
        Electron = electron;
        ElectronNeutrino = electronNeutrino;
        MuonNeutrino = muonNeutrino;
        ElectronEnergy = electronEnergy;
        ElectronNeutrinoEnergy = electronNeutrinoEnergy;
        MuonNeutrinoEnergy = muonNeutrinoEnergy;
    }

    public double TotalEnergy => ElectronEnergy + ElectronNeutrinoEnergy + MuonNeutrinoEnergy;
}
