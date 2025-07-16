using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Bosons;

/// <summary>
/// Implementation of the Higgs boson following the Standard Model of particle physics.
/// The Higgs boson is a massive spin-0 scalar boson that gives mass to other particles
/// through the Higgs mechanism and spontaneous symmetry breaking.
/// </summary>
public class HiggsBoson : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _fieldValue;
    private readonly ScalarMeasurableProperty _meanLifetime;
    private readonly VectorMeasurableProperty _momentum;

    #region Constants

    /// <summary>
    /// Higgs boson rest mass in kg (≈ 125.1 GeV/c²).
    /// Value from ATLAS and CMS experiments at LHC.
    /// </summary>
    public const double HiggsMass = 2.231e-25; // ≈ 125.1 GeV/c²

    /// <summary>
    /// Higgs boson mean lifetime in seconds.
    /// Extremely short due to rapid decay through multiple channels.
    /// </summary>
    public const double HiggsMeanLifetime = 1.56e-22; // ≈ 1.56 × 10⁻²² s

    /// <summary>
    /// Vacuum expectation value of the Higgs field in GeV.
    /// This is the value that breaks electroweak symmetry.
    /// </summary>
    public const double VacuumExpectationValue = 246.22; // GeV

    /// <summary>
    /// Higgs field vacuum expectation value in SI units (J/C).
    /// </summary>
    public static readonly double VacuumExpectationValueSI = VacuumExpectationValue * 1e9 * 1.602176634e-19;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current value of the Higgs field at this location.
    /// </summary>
    public double FieldValue => _fieldValue.Value;

    /// <summary>
    /// Gets whether this Higgs boson has decayed.
    /// </summary>
    public bool HasDecayed { get; private set; }

    /// <summary>
    /// Gets the time since creation (for decay calculations).
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - CreationTime;

    /// <summary>
    /// Gets the energy of the Higgs boson.
    /// </summary>
    public new double Energy
    {
        get
        {
            var p = _momentum.Magnitude;
            var m = _mass.Value;
            var c = PhysicsConstants.SpeedOfLight;
            return Math.Sqrt(p * p * c * c + m * m * c * c * c * c);
        }
    }

    /// <summary>
    /// Gets whether this Higgs boson is at rest (zero momentum).
    /// </summary>
    public bool IsAtRest => _momentum.Magnitude < 1e-30;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new Higgs boson.
    /// </summary>
    /// <param name="momentum">Initial momentum in kg⋅m/s. Default: at rest.</param>
    /// <param name="fieldValue">Local Higgs field value. Default: vacuum expectation value.</param>
    public HiggsBoson(double momentum = 0.0, double? fieldValue = null) 
        : base("Higgs Boson", "H⁰", isElementary: true, hilbertSpaceDimension: 2) // Spin-0 scalar (includes field fluctuations)
    {
        fieldValue ??= VacuumExpectationValueSI;

        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", HiggsMass);
        _charge = new ScalarMeasurableProperty("Charge", "C", 0.0); // Electrically neutral
        _fieldValue = new ScalarMeasurableProperty("FieldValue", "J/C", fieldValue.Value);
        _meanLifetime = new ScalarMeasurableProperty("MeanLifetime", "s", HiggsMeanLifetime);
        
        // Momentum (can be at rest or moving)
        var momentumDirection = new[] { 0.0, 0.0, 1.0 }; // Default direction
        var momentumVector = momentumDirection.Select(x => x * momentum).ToArray();
        _momentum = new VectorMeasurableProperty("Momentum", "kg⋅m/s", momentumVector);

        // Initialize quantum state
        InitializeHiggsState();
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.0; // Spin-0 scalar boson
    public override ParticleStatistics Statistics => ParticleStatistics.BoseEinstein; // Boson

    #endregion

    #region Higgs-Specific Properties

    /// <summary>
    /// Gets the Higgs field value as a measurable property.
    /// </summary>
    public IScalarMeasurable FieldValueMeasurable => _fieldValue;

    /// <summary>
    /// Gets the mean lifetime as a measurable property.
    /// </summary>
    public IScalarMeasurable MeanLifetime => _meanLifetime;

    /// <summary>
    /// Gets the momentum as a vector measurable property.
    /// </summary>
    public new IVectorMeasurable Momentum => _momentum;

    #endregion

    #region Higgs Mechanism Methods

    /// <summary>
    /// Calculates the mass contribution to a fermion through Yukawa coupling.
    /// </summary>
    /// <param name="yukawaCoupling">Yukawa coupling constant for the fermion.</param>
    /// <returns>Generated mass in kg.</returns>
    public double CalculateFermionMass(double yukawaCoupling)
    {
        // m_fermion = (g_Yukawa * v) / √2
        // where v is the vacuum expectation value
        var vev = VacuumExpectationValueSI;
        return (yukawaCoupling * vev) / Math.Sqrt(2.0);
    }

    /// <summary>
    /// Calculates the mass of gauge bosons through the Higgs mechanism.
    /// </summary>
    /// <param name="gaugeCoupling">Gauge coupling constant.</param>
    /// <returns>Generated gauge boson mass in kg.</returns>
    public double CalculateGaugeBosonMass(double gaugeCoupling)
    {
        // m_gauge = (g * v) / 2
        var vev = VacuumExpectationValueSI;
        return (gaugeCoupling * vev) / 2.0;
    }

    /// <summary>
    /// Calculates the Higgs potential at a given field value.
    /// V(φ) = -μ²φ² + λφ⁴ (simplified Mexican hat potential)
    /// </summary>
    /// <param name="fieldValue">Higgs field value.</param>
    /// <returns>Potential energy.</returns>
    public double CalculateHiggsPotential(double fieldValue)
    {
        // Parameters for the Higgs potential
        var mu2 = Math.Pow(HiggsMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight / Math.Sqrt(2.0), 2);
        var lambda = mu2 / (2.0 * VacuumExpectationValueSI * VacuumExpectationValueSI);
        
        return -mu2 * fieldValue * fieldValue + lambda * Math.Pow(fieldValue, 4);
    }

    /// <summary>
    /// Determines if the current field value represents a stable vacuum state.
    /// </summary>
    /// <returns>True if in a stable vacuum (minimum of potential).</returns>
    public bool IsInStableVacuum()
    {
        var currentPotential = CalculateHiggsPotential(FieldValue);
        var vacuumPotential = CalculateHiggsPotential(VacuumExpectationValueSI);
        
        // Check if we're at or near the vacuum expectation value
        return Math.Abs(FieldValue - VacuumExpectationValueSI) < 0.1 * VacuumExpectationValueSI;
    }

    #endregion

    #region Decay Methods

    /// <summary>
    /// Calculates the decay probability based on exponential decay law.
    /// </summary>
    /// <returns>Probability that the Higgs has decayed by now.</returns>
    public double CalculateDecayProbability()
    {
        var ageInSeconds = Age.TotalSeconds;
        return 1.0 - Math.Exp(-ageInSeconds / HiggsMeanLifetime);
    }

    /// <summary>
    /// Calculates the remaining probability that the Higgs is still alive.
    /// </summary>
    /// <returns>Probability that the Higgs has not yet decayed.</returns>
    public double CalculateSurvivalProbability()
    {
        var ageInSeconds = Age.TotalSeconds;
        return Math.Exp(-ageInSeconds / HiggsMeanLifetime);
    }

    /// <summary>
    /// Gets the branching ratios for different decay channels.
    /// </summary>
    /// <returns>Dictionary of decay channels and their probabilities.</returns>
    public Dictionary<string, double> GetDecayBranchingRatios()
    {
        // Standard Model Higgs decay channels at 125 GeV
        return new Dictionary<string, double>
        {
            { "bb̄", 0.577 },      // Bottom quark pair
            { "WW*", 0.215 },     // W boson pair (one off-shell)
            { "gg", 0.086 },      // Gluon pair (via loop)
            { "ττ", 0.063 },      // Tau lepton pair
            { "cc̄", 0.029 },      // Charm quark pair
            { "ZZ*", 0.026 },     // Z boson pair (one off-shell)
            { "γγ", 0.002 },      // Photon pair (via loop)
            { "Zγ", 0.0015 },     // Z boson and photon
            { "μμ", 0.0002 }      // Muon pair
        };
    }

    /// <summary>
    /// Simulates Higgs decay and returns the decay products.
    /// </summary>
    /// <param name="random">Random number generator for stochastic decay.</param>
    /// <returns>Decay products if decay occurs, null otherwise.</returns>
    public HiggsDecayProducts? AttemptDecay(Random? random = null)
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
    public HiggsDecayProducts ForceDecay(Random? random = null)
    {
        if (!HasDecayed)
        {
            HasDecayed = true;
            return SimulateDecay(random ?? new Random());
        }
        
        throw new InvalidOperationException("Higgs boson has already decayed");
    }

    private HiggsDecayProducts SimulateDecay(Random random)
    {
        var branchingRatios = GetDecayBranchingRatios();
        
        // Select decay channel based on branching ratios
        var rand = random.NextDouble();
        var cumulative = 0.0;
        string selectedChannel = "bb̄"; // Default
        
        foreach (var kvp in branchingRatios)
        {
            cumulative += kvp.Value;
            if (rand <= cumulative)
            {
                selectedChannel = kvp.Key;
                break;
            }
        }
        
        // Simulate the selected decay channel
        return selectedChannel switch
        {
            "bb̄" => SimulateBottomQuarkDecay(random),
            "WW*" => SimulateWBosonDecay(random),
            "gg" => SimulateGluonDecay(random),
            "ττ" => SimulateTauDecay(random),
            "γγ" => SimulatePhotonDecay(random),
            _ => SimulateBottomQuarkDecay(random) // Default fallback
        };
    }

    private HiggsDecayProducts SimulateBottomQuarkDecay(Random random)
    {
        // H → bb̄ decay
        var totalEnergy = Energy;
        
        // Simple two-body decay kinematics
        var particle1Energy = totalEnergy * (0.4 + 0.2 * random.NextDouble());
        var particle2Energy = totalEnergy - particle1Energy;
        
        return new HiggsDecayProducts("bb̄", new[] { particle1Energy, particle2Energy });
    }

    private HiggsDecayProducts SimulateWBosonDecay(Random random)
    {
        // H → WW* decay
        var totalEnergy = Energy;
        
        var particle1Energy = totalEnergy * (0.3 + 0.4 * random.NextDouble());
        var particle2Energy = totalEnergy - particle1Energy;
        
        return new HiggsDecayProducts("WW*", new[] { particle1Energy, particle2Energy });
    }

    private HiggsDecayProducts SimulateGluonDecay(Random random)
    {
        // H → gg decay
        var totalEnergy = Energy;
        
        var particle1Energy = totalEnergy * (0.4 + 0.2 * random.NextDouble());
        var particle2Energy = totalEnergy - particle1Energy;
        
        return new HiggsDecayProducts("gg", new[] { particle1Energy, particle2Energy });
    }

    private HiggsDecayProducts SimulateTauDecay(Random random)
    {
        // H → ττ decay
        var totalEnergy = Energy;
        
        var particle1Energy = totalEnergy * (0.4 + 0.2 * random.NextDouble());
        var particle2Energy = totalEnergy - particle1Energy;
        
        return new HiggsDecayProducts("ττ", new[] { particle1Energy, particle2Energy });
    }

    private HiggsDecayProducts SimulatePhotonDecay(Random random)
    {
        // H → γγ decay
        var totalEnergy = Energy;
        
        // Two-photon decay: equal energy photons in CM frame
        var photonEnergy = totalEnergy / 2.0;
        
        return new HiggsDecayProducts("γγ", new[] { photonEnergy, photonEnergy });
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Higgs is electrically neutral but can interact via virtual photons
        return false; // Direct EM interaction is negligible
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // Higgs participates in weak interactions
        return true;
    }

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Higgs doesn't directly participate in strong interactions
        // but can couple to quarks and gluons
        return false;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        // Higgs interactions are typically very weak and short-range
        if (CanInteractWeakly(other))
        {
            // Weak interaction scale
            var weakScale = 1e-18; // ~10⁻¹⁸ m
            return Math.Exp(-distance / weakScale) / (distance * distance);
        }
        
        return 0.0;
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new HiggsBoson(_momentum.Magnitude, _fieldValue.Value);
        clone.StateVector = StateVector;
        // Note: Clone does not copy decay state - clones start fresh
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        // Higgs is its own antiparticle (real scalar field)
        return Clone();
    }

    #endregion

    #region Private Helper Methods

    private void InitializeHiggsState()
    {
        // Spin-0 particle has only one quantum state
        StateVector = new Complex[] { new(1.0, 0.0) };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a Higgs boson at rest in the vacuum state.
    /// </summary>
    /// <returns>A new Higgs boson at rest.</returns>
    public static HiggsBoson CreateAtRest() => new(0.0);

    /// <summary>
    /// Creates a Higgs boson with specified energy.
    /// </summary>
    /// <param name="energy">Total energy in Joules.</param>
    /// <returns>A new Higgs boson with the specified energy.</returns>
    public static HiggsBoson CreateWithEnergy(double energy)
    {
        // Calculate momentum from energy: E² = p²c² + m²c⁴
        var m = HiggsMass;
        var c = PhysicsConstants.SpeedOfLight;
        var restEnergy = m * c * c;
        
        if (energy < restEnergy)
            throw new ArgumentException("Energy must be at least the rest mass energy", nameof(energy));
        
        var momentum = Math.Sqrt(energy * energy - restEnergy * restEnergy) / c;
        return new HiggsBoson(momentum);
    }

    /// <summary>
    /// Creates a Higgs boson in a false vacuum state.
    /// </summary>
    /// <param name="fieldValue">Non-vacuum field value.</param>
    /// <returns>A new Higgs boson in false vacuum.</returns>
    public static HiggsBoson CreateInFalseVacuum(double fieldValue) => new(0.0, fieldValue);

    #endregion
}

/// <summary>
/// Represents the products of Higgs boson decay.
/// </summary>
public class HiggsDecayProducts
{
    public string DecayChannel { get; }
    public double[] ParticleEnergies { get; }
    public double TotalEnergy => ParticleEnergies.Sum();

    public HiggsDecayProducts(string decayChannel, double[] particleEnergies)
    {
        DecayChannel = decayChannel;
        ParticleEnergies = particleEnergies;
    }
}
