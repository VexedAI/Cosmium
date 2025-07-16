using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Bosons;

/// <summary>
/// Implementation of a gluon following Quantum Chromodynamics (QCD).
/// Gluons are massless spin-1 bosons that mediate the strong nuclear force.
/// Unlike photons, gluons carry color charge and can interact with themselves.
/// </summary>
public class Gluon : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _colorCharge1;
    private readonly ScalarMeasurableProperty _colorCharge2;
    private readonly VectorMeasurableProperty _momentum;

    #region Properties

    /// <summary>
    /// Gets the first color charge of the gluon.
    /// Gluons carry a color-anticolor combination.
    /// </summary>
    public QuarkColor Color1 { get; }

    /// <summary>
    /// Gets the second color charge (anticolor) of the gluon.
    /// </summary>
    public QuarkColor Color2 { get; }

    /// <summary>
    /// Gets whether this gluon is color-neutral (colorless).
    /// There are specific combinations that are color-neutral.
    /// </summary>
    public bool IsColorNeutral => IsColorNeutralCombination(Color1, Color2);

    /// <summary>
    /// Gets the gluon's energy scale, affecting the running coupling constant.
    /// </summary>
    public double EnergyScale { get; private set; }

    /// <summary>
    /// Gets whether this gluon is virtual (off-shell) or real (on-shell).
    /// </summary>
    public bool IsVirtual { get; private set; }

    /// <summary>
    /// Gets the strong coupling constant at this gluon's energy scale.
    /// </summary>
    public double StrongCouplingConstant => CalculateRunningCoupling(EnergyScale);

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new gluon with specified color charges and momentum.
    /// </summary>
    /// <param name="color1">First color charge.</param>
    /// <param name="color2">Second color charge (anticolor).</param>
    /// <param name="momentum">Momentum magnitude in kg⋅m/s.</param>
    /// <param name="isVirtual">Whether this is a virtual gluon.</param>
    public Gluon(QuarkColor color1, QuarkColor color2, double momentum = 0.0, bool isVirtual = false) 
        : base("Gluon", GetGluonSymbol(color1, color2), 
               isElementary: true, hilbertSpaceDimension: 2) // Spin-1 with 2 transverse polarizations
    {
        Color1 = color1;
        Color2 = color2;
        IsVirtual = isVirtual;

        // Calculate energy scale from momentum
        EnergyScale = momentum * PhysicsConstants.SpeedOfLight;
        if (EnergyScale == 0.0) EnergyScale = 200e6 * 1.602176634e-19; // Default to ~200 MeV

        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", 0.0); // Massless (when real)
        _charge = new ScalarMeasurableProperty("Charge", "C", 0.0); // Electrically neutral
        _colorCharge1 = new ScalarMeasurableProperty("ColorCharge1", "dimensionless", GetColorValue(color1));
        _colorCharge2 = new ScalarMeasurableProperty("ColorCharge2", "dimensionless", GetColorValue(color2));
        
        // Momentum (for massless particles: E = pc)
        var momentumDirection = new[] { 0.0, 0.0, 1.0 }; // Default direction
        var momentumVector = momentumDirection.Select(x => x * momentum).ToArray();
        _momentum = new VectorMeasurableProperty("Momentum", "kg⋅m/s", momentumVector);

        // Initialize quantum state
        InitializeGluonState();
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 1.0; // Spin-1 boson
    public override ParticleStatistics Statistics => ParticleStatistics.BoseEinstein; // Boson

    #endregion

    #region Gluon-Specific Properties

    /// <summary>
    /// Gets the first color charge as a measurable property.
    /// </summary>
    public IScalarMeasurable ColorCharge1 => _colorCharge1;

    /// <summary>
    /// Gets the second color charge as a measurable property.
    /// </summary>
    public IScalarMeasurable ColorCharge2 => _colorCharge2;

    /// <summary>
    /// Gets the momentum as a vector measurable property.
    /// </summary>
    public new IVectorMeasurable Momentum => _momentum;

    /// <summary>
    /// Gets the energy of the gluon (E = pc for massless particles).
    /// </summary>
    public new double Energy => _momentum.Magnitude * PhysicsConstants.SpeedOfLight;

    #endregion

    #region QCD Methods

    /// <summary>
    /// Calculates the running strong coupling constant at a given energy scale.
    /// This implements a simplified version of the QCD β-function.
    /// </summary>
    /// <param name="energyScale">Energy scale in Joules.</param>
    /// <returns>Strong coupling constant at this scale.</returns>
    public static double CalculateRunningCoupling(double energyScale)
    {
        // Convert energy to GeV for convenience
        var energyGeV = energyScale / (1e9 * 1.602176634e-19);
        
        // Reference scale and coupling
        const double referenceScale = 91.2; // Z boson mass in GeV
        const double referenceCoupling = 0.118; // αₛ(MZ)
        
        // One-loop running (simplified)
        var beta0 = (33.0 - 2.0 * 6.0) / (12.0 * Math.PI); // 6 quark flavors
        var logRatio = Math.Log(energyGeV / referenceScale);
        
        var runningCoupling = referenceCoupling / (1.0 + referenceCoupling * beta0 * logRatio);
        
        // Ensure physical range
        return Math.Max(0.01, Math.Min(1.0, runningCoupling));
    }

    /// <summary>
    /// Determines if this gluon can interact with a quark of given color.
    /// </summary>
    /// <param name="quarkColor">The color of the quark.</param>
    /// <returns>True if interaction is possible.</returns>
    public bool CanInteractWithQuark(QuarkColor quarkColor)
    {
        // Gluon can interact if it can change the quark's color
        return Color1 == quarkColor || Color2 == quarkColor;
    }

    /// <summary>
    /// Calculates the color factor for gluon-quark interaction.
    /// </summary>
    /// <param name="quarkColor">The quark's color.</param>
    /// <returns>Color factor (simplified model).</returns>
    public double CalculateColorFactor(QuarkColor quarkColor)
    {
        if (!CanInteractWithQuark(quarkColor)) return 0.0;
        
        // Simplified color factor calculation
        // In real QCD, this involves SU(3) group theory
        return Color1 == quarkColor ? 4.0/3.0 : 1.0/6.0;
    }

    /// <summary>
    /// Simulates gluon splitting into a quark-antiquark pair.
    /// </summary>
    /// <param name="quarkType">Type of quark to create.</param>
    /// <param name="random">Random number generator for kinematics.</param>
    /// <returns>The created quark-antiquark pair.</returns>
    public (Quark quark, Quark antiquark)? SplitToQuarkPair(QuarkType quarkType, Random? random = null)
    {
        random ??= new Random();
        
        // Check if we have enough energy (simplified threshold)
        var quarkMass = quarkType.GetRestMass();
        var threshold = 2.0 * quarkMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
        
        if (Energy < threshold) return null;
        
        // Create quark and antiquark with appropriate colors
        var quark = new Quark(quarkType, Color1, false);
        var antiquark = new Quark(quarkType, Color2, true);
        
        return (quark, antiquark);
    }

    /// <summary>
    /// Calculates the probability of gluon self-interaction (three-gluon vertex).
    /// </summary>
    /// <param name="otherGluon1">First interacting gluon.</param>
    /// <param name="otherGluon2">Second interacting gluon.</param>
    /// <returns>Interaction strength for three-gluon vertex.</returns>
    public double CalculateThreeGluonVertex(Gluon otherGluon1, Gluon otherGluon2)
    {
        // Check color conservation
        if (!IsColorConserved(this, otherGluon1, otherGluon2)) return 0.0;
        
        // Simplified three-gluon coupling
        var coupling = StrongCouplingConstant;
        return coupling; // In real QCD, this involves more complex color algebra
    }

    #endregion

    #region Confinement and Hadronization

    /// <summary>
    /// Estimates the confinement scale for this gluon.
    /// At low energies, gluons cannot exist as free particles.
    /// </summary>
    /// <returns>Confinement scale in Joules.</returns>
    public double GetConfinementScale()
    {
        // QCD confinement scale ~1 GeV
        return 1e9 * 1.602176634e-19; // 1 GeV in Joules
    }

    /// <summary>
    /// Determines if this gluon is below the confinement scale.
    /// </summary>
    /// <returns>True if the gluon should be confined.</returns>
    public bool IsConfinedRegime()
    {
        return EnergyScale < GetConfinementScale();
    }

    /// <summary>
    /// Simulates hadronization - the process by which gluons form bound states.
    /// </summary>
    /// <param name="availableQuarks">Available quarks for hadron formation.</param>
    /// <returns>True if hadronization is likely to occur.</returns>
    public bool ShouldHadronize(IEnumerable<Quark> availableQuarks)
    {
        if (!IsConfinedRegime()) return false;
        
        // Check if color-neutral combinations are possible
        return availableQuarks.Any(q => CanFormColorNeutralWith(q));
    }

    private bool CanFormColorNeutralWith(Quark quark)
    {
        // Simplified check for color neutrality in hadron formation
        return Color1 == quark.Color || Color2 == quark.Color;
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Gluons are electrically neutral and don't interact electromagnetically
        return false;
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // Gluons don't participate in weak interactions directly
        return false;
    }

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Gluons interact strongly with quarks and other gluons
        return other is Quark || other is Gluon;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        if (!CanInteractStrongly(other)) return 0.0;
        
        var coupling = StrongCouplingConstant;
        
        if (other is Quark quark)
        {
            var colorFactor = CalculateColorFactor(quark.Color);
            return coupling * colorFactor / (distance * distance);
        }
        else if (other is Gluon gluon)
        {
            // Gluon-gluon interaction
            return coupling / (distance * distance);
        }
        
        return 0.0;
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Gluon(Color1, Color2, _momentum.Magnitude, IsVirtual);
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        // Gluons are their own antiparticles in a sense, but with swapped color charges
        return new Gluon(Color2, Color1, _momentum.Magnitude, IsVirtual);
    }

    #endregion

    #region Private Helper Methods

    private static string GetGluonSymbol(QuarkColor color1, QuarkColor color2)
    {
        var symbol1 = color1.GetSymbol();
        var symbol2 = color2.GetSymbol();
        return $"g({symbol1}{symbol2}̄)";
    }

    private static double GetColorValue(QuarkColor color)
    {
        return color switch
        {
            QuarkColor.Red => 1.0,
            QuarkColor.Green => 2.0,
            QuarkColor.Blue => 3.0,
            _ => 0.0
        };
    }

    private static bool IsColorNeutralCombination(QuarkColor color1, QuarkColor color2)
    {
        // In real QCD, there are specific linear combinations that are color-neutral
        // This is a simplified check
        return color1 == color2; // Simplified: same color-anticolor
    }

    private static bool IsColorConserved(Gluon g1, Gluon g2, Gluon g3)
    {
        // Simplified color conservation check for three-gluon vertex
        // In real QCD, this involves SU(3) group theory
        return true; // Placeholder - actual implementation would be more complex
    }

    private void InitializeGluonState()
    {
        // Initialize in a definite polarization state
        // For massless spin-1 particles, there are 2 transverse polarizations
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a red-antigreen gluon.
    /// </summary>
    /// <param name="momentum">Momentum in kg⋅m/s.</param>
    public static Gluon CreateRedAntiGreen(double momentum = 0.0) => 
        new(QuarkColor.Red, QuarkColor.Green, momentum);

    /// <summary>
    /// Creates a green-antiblue gluon.
    /// </summary>
    /// <param name="momentum">Momentum in kg⋅m/s.</param>
    public static Gluon CreateGreenAntiBlue(double momentum = 0.0) => 
        new(QuarkColor.Green, QuarkColor.Blue, momentum);

    /// <summary>
    /// Creates a blue-antired gluon.
    /// </summary>
    /// <param name="momentum">Momentum in kg⋅m/s.</param>
    public static Gluon CreateBlueAntiRed(double momentum = 0.0) => 
        new(QuarkColor.Blue, QuarkColor.Red, momentum);

    /// <summary>
    /// Creates a virtual gluon for intermediate calculations.
    /// </summary>
    /// <param name="color1">First color.</param>
    /// <param name="color2">Second color.</param>
    /// <param name="virtualMass">Effective mass for virtual gluon.</param>
    public static Gluon CreateVirtual(QuarkColor color1, QuarkColor color2, double virtualMass = 0.0) => 
        new(color1, color2, virtualMass, true);

    #endregion
}
