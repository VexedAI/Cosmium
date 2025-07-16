using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Leptons;

/// <summary>
/// Enumeration of neutrino types (flavors) in the Standard Model.
/// </summary>
public enum NeutrinoType
{
    /// <summary>
    /// Electron neutrino (νₑ) - First generation.
    /// </summary>
    Electron,

    /// <summary>
    /// Muon neutrino (νμ) - Second generation.
    /// </summary>
    Muon,

    /// <summary>
    /// Tau neutrino (ντ) - Third generation.
    /// </summary>
    Tau
}

/// <summary>
/// Extension methods for NeutrinoType enum.
/// </summary>
public static class NeutrinoTypeExtensions
{
    /// <summary>
    /// Gets the symbol for the neutrino type.
    /// </summary>
    /// <param name="type">The neutrino type.</param>
    /// <param name="isAntineutrino">Whether this is an antineutrino.</param>
    /// <returns>The symbol string.</returns>
    public static string GetSymbol(this NeutrinoType type, bool isAntineutrino = false)
    {
        var baseSymbol = type switch
        {
            NeutrinoType.Electron => "νₑ",
            NeutrinoType.Muon => "νμ",
            NeutrinoType.Tau => "ντ",
            _ => "ν"
        };
        
        return isAntineutrino ? $"{baseSymbol}̄" : baseSymbol;
    }

    /// <summary>
    /// Gets the generation number for the neutrino type.
    /// </summary>
    /// <param name="type">The neutrino type.</param>
    /// <returns>The generation number (1, 2, or 3).</returns>
    public static int GetGeneration(this NeutrinoType type)
    {
        return type switch
        {
            NeutrinoType.Electron => 1,
            NeutrinoType.Muon => 2,
            NeutrinoType.Tau => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown neutrino type")
        };
    }
}

/// <summary>
/// Implementation of a neutrino following the Standard Model of particle physics.
/// Neutrinos are nearly massless, electrically neutral leptons that interact only via weak force.
/// They undergo flavor oscillations as they propagate through space.
/// </summary>
public class Neutrino : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _leptonNumber;
    private readonly VectorMeasurableProperty _momentum;

    #region Constants

    /// <summary>
    /// Upper bound on neutrino mass from cosmological observations (eV/c²).
    /// Individual neutrino masses are much smaller but not precisely known.
    /// </summary>
    public const double NeutrinoMassUpperBound = 0.12; // eV/c²

    /// <summary>
    /// Neutrino mass in kg (using upper bound estimate).
    /// </summary>
    public static readonly double NeutrinoMassKg = NeutrinoMassUpperBound * 1.602176634e-19 / 
                                                   (PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the type (flavor) of this neutrino.
    /// </summary>
    public NeutrinoType Type { get; }

    /// <summary>
    /// Gets whether this is an antineutrino.
    /// </summary>
    public bool IsAntineutrino { get; }

    /// <summary>
    /// Gets the lepton number for this neutrino (+1 for neutrino, -1 for antineutrino).
    /// </summary>
    public double LeptonNumber => IsAntineutrino ? -1.0 : 1.0;

    /// <summary>
    /// Gets the generation number (1, 2, or 3).
    /// </summary>
    public int Generation => Type.GetGeneration();

    /// <summary>
    /// Gets the helicity of the neutrino (handedness).
    /// Standard Model neutrinos are left-handed, antineutrinos are right-handed.
    /// </summary>
    public double Helicity => IsAntineutrino ? 1.0 : -1.0; // Simplified model

    /// <summary>
    /// Gets whether this neutrino is ultra-relativistic (v ≈ c).
    /// </summary>
    public bool IsUltraRelativistic { get; private set; } = true;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new neutrino.
    /// </summary>
    /// <param name="type">The type (flavor) of neutrino.</param>
    /// <param name="isAntineutrino">Whether this is an antineutrino. Default: false.</param>
    /// <param name="momentum">Initial momentum magnitude in kg⋅m/s. Default: high energy.</param>
    public Neutrino(NeutrinoType type, bool isAntineutrino = false, double momentum = 1e-21) 
        : base(GetNeutrinoName(type, isAntineutrino), 
               type.GetSymbol(isAntineutrino), 
               isElementary: true, hilbertSpaceDimension: 2) // Spin-1/2 fermion
    {
        Type = type;
        IsAntineutrino = isAntineutrino;

        // Initialize physical properties
        var mass = NeutrinoMassKg; // Very small but non-zero
        var charge = 0.0; // Neutrinos are electrically neutral
        var leptonNumber = isAntineutrino ? -1.0 : 1.0;

        _mass = new ScalarMeasurableProperty("Mass", "kg", mass);
        _charge = new ScalarMeasurableProperty("Charge", "C", charge);
        _leptonNumber = new ScalarMeasurableProperty("LeptonNumber", "dimensionless", leptonNumber);
        _momentum = new VectorMeasurableProperty("Momentum", "kg⋅m/s", new[] { momentum, 0.0, 0.0 });

        // Update relativistic status based on momentum
        UpdateRelativisticStatus(momentum);

        // Initialize quantum state (flavor eigenstate)
        InitializeNeutrinoState();
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.5; // Spin-1/2 fermion
    public override ParticleStatistics Statistics => ParticleStatistics.FermiDirac; // Fermion

    #endregion

    #region Neutrino-Specific Properties

    /// <summary>
    /// Gets the lepton number as a measurable property.
    /// </summary>
    public IScalarMeasurable LeptonNumberMeasurable => _leptonNumber;

    /// <summary>
    /// Gets the momentum as a vector measurable property.
    /// </summary>
    public new IVectorMeasurable Momentum => _momentum;

    /// <summary>
    /// Gets the neutrino's velocity (approximately c for ultra-relativistic neutrinos).
    /// </summary>
    public new double Velocity
    {
        get
        {
            if (IsUltraRelativistic)
                return PhysicsConstants.SpeedOfLight * 0.999999999; // Nearly c

            var p = _momentum.Magnitude;
            var m = _mass.Value;
            var E = Math.Sqrt(p * p * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight + 
                             m * m * Math.Pow(PhysicsConstants.SpeedOfLight, 4));
            return p * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight / E;
        }
    }

    /// <summary>
    /// Gets the total energy of the neutrino.
    /// </summary>
    public double TotalEnergy
    {
        get
        {
            var p = _momentum.Magnitude;
            var m = _mass.Value;
            return Math.Sqrt(p * p * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight + 
                            m * m * Math.Pow(PhysicsConstants.SpeedOfLight, 4));
        }
    }

    #endregion

    #region Neutrino Oscillation Methods

    /// <summary>
    /// Calculates the oscillation probability to another neutrino flavor.
    /// This is a simplified two-flavor oscillation model.
    /// </summary>
    /// <param name="targetType">Target neutrino flavor.</param>
    /// <param name="distance">Distance traveled in meters.</param>
    /// <param name="mixingAngle">Mixing angle in radians (default: θ₁₂).</param>
    /// <param name="massDifferenceSquared">Mass difference squared in eV² (default: Δm²₁₂).</param>
    /// <returns>Probability of oscillation to target flavor.</returns>
    public double CalculateOscillationProbability(NeutrinoType targetType, double distance, 
                                                 double mixingAngle = 0.584, // θ₁₂ ≈ 33.5°
                                                 double massDifferenceSquared = 7.5e-5) // Δm²₁₂
    {
        if (targetType == Type) return 1.0; // Same flavor

        var energy = TotalEnergy;
        var L = distance;
        var deltaM2 = massDifferenceSquared * 1.602176634e-19; // Convert eV² to J²⋅c⁻⁴
        
        // Oscillation formula: P(νₐ → νᵦ) = sin²(2θ) sin²(1.27 Δm²L/E)
        var argument = 1.27 * deltaM2 * L / energy;
        var probability = Math.Pow(Math.Sin(2.0 * mixingAngle), 2) * Math.Pow(Math.Sin(argument), 2);
        
        return Math.Max(0.0, Math.Min(1.0, probability));
    }

    /// <summary>
    /// Simulates flavor oscillation over a given distance.
    /// </summary>
    /// <param name="distance">Distance to propagate in meters.</param>
    /// <param name="random">Random number generator for stochastic oscillation.</param>
    /// <returns>The neutrino flavor after oscillation (may be the same).</returns>
    public NeutrinoType PropagateAndOscillate(double distance, Random? random = null)
    {
        random ??= new Random();
        
        // Calculate probabilities for all flavors
        var probabilities = new Dictionary<NeutrinoType, double>();
        var totalProb = 0.0;
        
        foreach (NeutrinoType flavor in Enum.GetValues<NeutrinoType>())
        {
            var prob = flavor == Type ? 
                1.0 - CalculateOscillationProbability(NeutrinoType.Electron, distance) - 
                      CalculateOscillationProbability(NeutrinoType.Muon, distance) - 
                      CalculateOscillationProbability(NeutrinoType.Tau, distance) :
                CalculateOscillationProbability(flavor, distance);
            
            probabilities[flavor] = Math.Max(0.0, prob);
            totalProb += probabilities[flavor];
        }
        
        // Normalize probabilities
        foreach (var key in probabilities.Keys.ToList())
        {
            probabilities[key] /= totalProb;
        }
        
        // Sample based on probabilities
        var rand = random.NextDouble();
        var cumulative = 0.0;
        
        foreach (var kvp in probabilities)
        {
            cumulative += kvp.Value;
            if (rand <= cumulative)
                return kvp.Key;
        }
        
        return Type; // Fallback
    }

    #endregion

    #region Detector Interaction Methods

    /// <summary>
    /// Calculates the cross-section for neutrino-nucleon interaction.
    /// This is extremely small, reflecting neutrinos' weak interaction.
    /// </summary>
    /// <param name="energy">Neutrino energy in Joules.</param>
    /// <returns>Cross-section in square meters.</returns>
    public double CalculateNucleonCrossSection(double energy)
    {
        // Simplified model: σ ∝ E for charged current interactions
        var energyInGeV = energy / (1e9 * 1.602176634e-19); // Convert to GeV
        var baselineCS = 1e-43; // Baseline cross-section in m² at 1 GeV
        
        return baselineCS * energyInGeV;
    }

    /// <summary>
    /// Calculates the mean free path in a medium.
    /// </summary>
    /// <param name="nucleonDensity">Number density of nucleons in the medium (m⁻³).</param>
    /// <returns>Mean free path in meters.</returns>
    public double CalculateMeanFreePath(double nucleonDensity)
    {
        var crossSection = CalculateNucleonCrossSection(TotalEnergy);
        return 1.0 / (nucleonDensity * crossSection);
    }

    /// <summary>
    /// Estimates the probability of interaction over a given path length.
    /// </summary>
    /// <param name="pathLength">Path length in meters.</param>
    /// <param name="nucleonDensity">Nucleon density in m⁻³.</param>
    /// <returns>Interaction probability.</returns>
    public double CalculateInteractionProbability(double pathLength, double nucleonDensity)
    {
        var meanFreePath = CalculateMeanFreePath(nucleonDensity);
        return 1.0 - Math.Exp(-pathLength / meanFreePath);
    }

    #endregion

    #region Momentum and Energy Methods

    /// <summary>
    /// Sets the momentum of the neutrino and updates relativistic properties.
    /// </summary>
    /// <param name="momentum">Momentum vector in kg⋅m/s.</param>
    public void SetMomentum(double[] momentum)
    {
        if (momentum.Length != 3)
            throw new ArgumentException("Momentum vector must have 3 components", nameof(momentum));

        _momentum.SetValue(momentum);
        UpdateRelativisticStatus(_momentum.Magnitude);
    }

    /// <summary>
    /// Sets the energy of the neutrino (adjusts momentum accordingly).
    /// </summary>
    /// <param name="energy">Total energy in Joules.</param>
    public void SetEnergy(double energy)
    {
        if (energy <= 0)
            throw new ArgumentException("Energy must be positive", nameof(energy));

        // For ultra-relativistic neutrinos: E ≈ pc
        var momentum = energy / PhysicsConstants.SpeedOfLight;
        var direction = _momentum.Direction;
        
        SetMomentum(new[] { momentum * direction[0], momentum * direction[1], momentum * direction[2] });
    }

    private void UpdateRelativisticStatus(double momentum)
    {
        var m = _mass.Value;
        var relativisticThreshold = 100.0; // p >> mc for ultra-relativistic
        IsUltraRelativistic = momentum > (relativisticThreshold * m * PhysicsConstants.SpeedOfLight);
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Neutrinos are electrically neutral and don't interact electromagnetically
        return false;
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // All neutrinos participate in weak interactions
        return true;
    }

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Neutrinos do not participate in strong interactions
        return false;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        // Neutrinos only interact weakly, and very weakly at that
        if (CanInteractWeakly(other))
        {
            // Extremely weak interaction
            return 1e-10 / (distance * distance); // Much weaker than electromagnetic
        }
        
        return 0.0;
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Neutrino(Type, IsAntineutrino, _momentum.Magnitude);
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        return new Neutrino(Type, !IsAntineutrino, _momentum.Magnitude);
    }

    #endregion

    #region Private Helper Methods

    private static string GetNeutrinoName(NeutrinoType type, bool isAntineutrino)
    {
        var baseName = $"{type} Neutrino";
        return isAntineutrino ? $"Anti{baseName}" : baseName;
    }

    private void InitializeNeutrinoState()
    {
        // Initialize in definite helicity state
        var helicityState = Helicity > 0 ? 
            new Complex[] { new(0.0, 0.0), new(1.0, 0.0) } : // Right-handed
            new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };  // Left-handed
        
        StateVector = helicityState;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates an electron neutrino.
    /// </summary>
    /// <param name="energy">Energy in Joules.</param>
    /// <returns>A new electron neutrino.</returns>
    public static Neutrino CreateElectronNeutrino(double energy = 1e-12)
    {
        var momentum = energy / PhysicsConstants.SpeedOfLight;
        return new Neutrino(NeutrinoType.Electron, false, momentum);
    }

    /// <summary>
    /// Creates a muon neutrino.
    /// </summary>
    /// <param name="energy">Energy in Joules.</param>
    /// <returns>A new muon neutrino.</returns>
    public static Neutrino CreateMuonNeutrino(double energy = 1e-12)
    {
        var momentum = energy / PhysicsConstants.SpeedOfLight;
        return new Neutrino(NeutrinoType.Muon, false, momentum);
    }

    /// <summary>
    /// Creates a tau neutrino.
    /// </summary>
    /// <param name="energy">Energy in Joules.</param>
    /// <returns>A new tau neutrino.</returns>
    public static Neutrino CreateTauNeutrino(double energy = 1e-12)
    {
        var momentum = energy / PhysicsConstants.SpeedOfLight;
        return new Neutrino(NeutrinoType.Tau, false, momentum);
    }

    #endregion
}
