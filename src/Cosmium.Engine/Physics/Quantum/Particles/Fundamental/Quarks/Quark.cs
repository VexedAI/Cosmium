using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

/// <summary>
/// Implementation of a quark particle following the Standard Model of particle physics.
/// Quarks are fermions with fractional electric charge and color charge that participate in strong interactions.
/// They are confined within hadrons and cannot exist as free particles under normal conditions.
/// </summary>
public class Quark : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _colorCharge;
    private readonly ScalarMeasurableProperty _weakIsospin;

    #region Properties

    /// <summary>
    /// Gets the type of this quark (up, down, charm, strange, top, bottom).
    /// </summary>
    public QuarkType Type { get; }

    /// <summary>
    /// Gets the color charge of this quark (red, green, blue).
    /// </summary>
    public QuarkColor Color { get; }

    /// <summary>
    /// Gets whether this is an antiquark.
    /// </summary>
    public bool IsAntiquark { get; }

    /// <summary>
    /// Gets the generation (family) number of this quark (1, 2, or 3).
    /// </summary>
    public int Generation => Type.GetGeneration();

    /// <summary>
    /// Gets whether this is an up-type quark (charge +2/3).
    /// </summary>
    public bool IsUpType => Type.IsUpType();

    /// <summary>
    /// Gets whether this is a down-type quark (charge -1/3).
    /// </summary>
    public bool IsDownType => Type.IsDownType();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new quark with the specified type and color.
    /// </summary>
    /// <param name="type">The type of quark.</param>
    /// <param name="color">The color charge.</param>
    /// <param name="isAntiquark">Whether this is an antiquark (default: false).</param>
    public Quark(QuarkType type, QuarkColor color, bool isAntiquark = false) 
        : base(GetQuarkName(type, isAntiquark), GetQuarkSymbol(type, isAntiquark), 
               isElementary: true, hilbertSpaceDimension: 2) // Spin-1/2 fermion
    {
        Type = type;
        Color = color;
        IsAntiquark = isAntiquark;

        // Initialize physical properties based on quark type
        var mass = type.GetRestMass();
        var charge = type.GetElectricCharge() * (isAntiquark ? -1.0 : 1.0) * PhysicsConstants.ElementaryCharge;
        var weakIsospin = type.GetWeakIsospin() * (isAntiquark ? -1.0 : 1.0);

        _mass = new ScalarMeasurableProperty("Mass", "kg", mass);
        _charge = new ScalarMeasurableProperty("Charge", "C", charge);
        _colorCharge = new ScalarMeasurableProperty("ColorCharge", "dimensionless", GetColorChargeValue());
        _weakIsospin = new ScalarMeasurableProperty("WeakIsospin", "dimensionless", weakIsospin);

        // Initialize quantum state in a definite color/spin state
        InitializeQuarkState();
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.5; // All quarks are spin-1/2 fermions
    public override ParticleStatistics Statistics => ParticleStatistics.FermiDirac; // Fermions

    #endregion

    #region Quark-Specific Properties

    /// <summary>
    /// Gets the color charge as a measurable property.
    /// </summary>
    public IScalarMeasurable ColorCharge => _colorCharge;

    /// <summary>
    /// Gets the weak isospin as a measurable property.
    /// </summary>
    public IScalarMeasurable WeakIsospin => _weakIsospin;

    /// <summary>
    /// Gets the baryon number of this quark (+1/3 for quarks, -1/3 for antiquarks).
    /// </summary>
    public double BaryonNumber => IsAntiquark ? -1.0/3.0 : 1.0/3.0;

    /// <summary>
    /// Gets the third component of weak isospin (T₃).
    /// </summary>
    public double WeakIsospinThird => _weakIsospin.Value;

    /// <summary>
    /// Gets the hypercharge (Y = 2(Q - T₃)) where Q is electric charge.
    /// </summary>
    public double Hypercharge
    {
        get
        {
            var electricChargeInUnits = _charge.Value / PhysicsConstants.ElementaryCharge;
            return 2.0 * (electricChargeInUnits - WeakIsospinThird);
        }
    }

    #endregion

    #region Strong Interaction Methods

    /// <summary>
    /// Determines if this quark can form a color-neutral combination with other quarks.
    /// </summary>
    /// <param name="otherQuarks">The other quarks to check combination with.</param>
    /// <returns>True if the combination would be color-neutral.</returns>
    public bool CanFormColorNeutralState(params Quark[] otherQuarks)
    {
        if (otherQuarks.Length == 2)
        {
            // Check for baryon formation (three quarks of different colors)
            return QuarkColorExtensions.IsColorNeutral(Color, otherQuarks[0].Color, otherQuarks[1].Color);
        }
        else if (otherQuarks.Length == 1)
        {
            // Check for meson formation (quark-antiquark pair)
            var other = otherQuarks[0];
            return (IsAntiquark != other.IsAntiquark) && (Color == other.Color);
        }
        
        return false;
    }

    /// <summary>
    /// Calculates the strong coupling constant at the energy scale of this quark.
    /// This is a simplified model - real QCD running coupling is much more complex.
    /// </summary>
    /// <returns>Approximate strong coupling constant.</returns>
    public double GetStrongCouplingConstant()
    {
        // Simplified running coupling - real implementation would use QCD β-function
        var energyScale = Mass.Value * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
        var logTerm = Math.Log(energyScale / (200e6 * 1.602176634e-13)); // Relative to 200 MeV scale
        
        // Very simplified model: αₛ decreases with energy (asymptotic freedom)
        return Math.Max(0.1, PhysicsConstants.StrongCouplingConstant / (1.0 + 0.1 * logTerm));
    }

    #endregion

    #region Weak Interaction Methods

    /// <summary>
    /// Determines if this quark can undergo weak decay to another quark type.
    /// </summary>
    /// <param name="targetType">The target quark type after decay.</param>
    /// <returns>True if weak decay is possible.</returns>
    public bool CanWeakDecayTo(QuarkType targetType)
    {
        // Weak interactions can change quark flavor but preserve certain quantum numbers
        // Allow transitions within generations and between generations (with CKM matrix elements)
        
        // Up-type quarks can decay to down-type quarks
        if (IsUpType && targetType.IsDownType()) return true;
        
        // Down-type quarks can decay to up-type quarks (with W⁺ emission)
        if (IsDownType && targetType.IsUpType()) return true;
        
        return false;
    }

    /// <summary>
    /// Gets the CKM matrix element for weak decay to another quark type.
    /// This is a simplified model using approximate values.
    /// </summary>
    /// <param name="targetType">The target quark type.</param>
    /// <returns>The magnitude of the CKM matrix element.</returns>
    public double GetCkmMatrixElement(QuarkType targetType)
    {
        // Simplified CKM matrix elements (approximate values)
        return (Type, targetType) switch
        {
            (QuarkType.Up, QuarkType.Down) => 0.974,      // |Vud|
            (QuarkType.Up, QuarkType.Strange) => 0.225,   // |Vus|
            (QuarkType.Up, QuarkType.Bottom) => 0.004,    // |Vub|
            (QuarkType.Charm, QuarkType.Down) => 0.225,   // |Vcd|
            (QuarkType.Charm, QuarkType.Strange) => 0.973, // |Vcs|
            (QuarkType.Charm, QuarkType.Bottom) => 0.041, // |Vcb|
            (QuarkType.Top, QuarkType.Down) => 0.009,     // |Vtd|
            (QuarkType.Top, QuarkType.Strange) => 0.040,  // |Vts|
            (QuarkType.Top, QuarkType.Bottom) => 0.999,   // |Vtb|
            _ => 0.0 // No direct weak coupling
        };
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Quarks participate in strong interactions with other quarks and gluons
        // TODO: Add Gluon check when Gluon class is implemented
        return other is Quark || (other.Name == "Gluon");
    }

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Quarks interact electromagnetically with photons and other charged particles
        return !Charge.Value.Equals(0.0) && (other.Name == "Photon" || !other.Charge.Value.Equals(0.0));
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // All quarks participate in weak interactions
        return true;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        if (other is Quark otherQuark)
        {
            // Strong interaction dominates at short distances
            var strongCoupling = GetStrongCouplingConstant();
            return strongCoupling / (distance * distance); // Simplified Coulomb-like potential
        }
        
        // Fall back to electromagnetic interaction for other particles
        return base.CalculateInteractionStrength(other, distance);
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Quark(Type, Color, IsAntiquark);
        clone.StateVector = StateVector; // This will trigger validation and events
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        return new Quark(Type, Color.GetAnticolor(), !IsAntiquark);
    }

    #endregion

    #region Private Helper Methods

    private static string GetQuarkName(QuarkType type, bool isAntiquark)
    {
        var baseName = type.ToString().ToLower();
        return isAntiquark ? $"anti{baseName}" : baseName;
    }

    private static string GetQuarkSymbol(QuarkType type, bool isAntiquark)
    {
        var symbol = type.GetSymbol();
        return isAntiquark ? $"{symbol}̄" : symbol; // Adding combining overline for antiquark
    }

    private double GetColorChargeValue()
    {
        // Assign numerical values for color charges (for calculations)
        return Color switch
        {
            QuarkColor.Red => 1.0,
            QuarkColor.Green => 2.0,
            QuarkColor.Blue => 3.0,
            _ => 0.0
        };
    }

    private void InitializeQuarkState()
    {
        // Initialize in a definite spin-up state
        // In a full implementation, this would include color and flavor quantum numbers
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    #endregion
}
