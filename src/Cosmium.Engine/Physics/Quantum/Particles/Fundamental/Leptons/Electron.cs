using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Leptons;

/// <summary>
/// Implementation of an electron following the Standard Model of particle physics.
/// The electron is a fundamental lepton with electric charge -1 and spin 1/2.
/// It is the lightest charged lepton and stable under normal conditions.
/// </summary>
public class Electron : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _leptonNumber;
    private readonly ScalarMeasurableProperty _weakIsospin;

    #region Properties

    /// <summary>
    /// Gets whether this is an antielectron (positron).
    /// </summary>
    public bool IsPositron { get; }

    /// <summary>
    /// Gets the lepton number for this electron (+1 for electron, -1 for positron).
    /// </summary>
    public double LeptonNumber => IsPositron ? -1.0 : 1.0;

    /// <summary>
    /// Gets the generation number (1 for electron).
    /// </summary>
    public int Generation => 1;

    /// <summary>
    /// Gets the magnetic moment of the electron in Bohr magnetons.
    /// </summary>
    public double MagneticMoment => IsPositron ? -1.001159652181643 : 1.001159652181643; // Anomalous magnetic moment

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new electron.
    /// </summary>
    /// <param name="isPositron">Whether this is a positron (antielectron). Default: false.</param>
    public Electron(bool isPositron = false) 
        : base(isPositron ? "Positron" : "Electron", 
               isPositron ? "e⁺" : "e⁻", 
               isElementary: true, hilbertSpaceDimension: 2) // Spin-1/2 fermion
    {
        IsPositron = isPositron;

        // Initialize physical properties
        var mass = PhysicsConstants.ElectronMass;
        var charge = (isPositron ? 1.0 : -1.0) * PhysicsConstants.ElementaryCharge;
        var leptonNumber = isPositron ? -1.0 : 1.0;
        var weakIsospin = isPositron ? 0.5 : -0.5; // Left-handed electrons have T₃ = -1/2

        _mass = new ScalarMeasurableProperty("Mass", "kg", mass);
        _charge = new ScalarMeasurableProperty("Charge", "C", charge);
        _leptonNumber = new ScalarMeasurableProperty("LeptonNumber", "dimensionless", leptonNumber);
        _weakIsospin = new ScalarMeasurableProperty("WeakIsospin", "dimensionless", weakIsospin);

        // Initialize quantum state in definite spin state
        InitializeElectronState();
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.5; // Spin-1/2 fermion
    public override ParticleStatistics Statistics => ParticleStatistics.FermiDirac; // Fermion

    #endregion

    #region Electron-Specific Properties

    /// <summary>
    /// Gets the lepton number as a measurable property.
    /// </summary>
    public IScalarMeasurable LeptonNumberMeasurable => _leptonNumber;

    /// <summary>
    /// Gets the weak isospin as a measurable property.
    /// </summary>
    public IScalarMeasurable WeakIsospin => _weakIsospin;

    /// <summary>
    /// Gets the classical electron radius (re = e²/(4πε₀mec²)).
    /// </summary>
    public double ClassicalRadius
    {
        get
        {
            var e2 = PhysicsConstants.ElementaryCharge * PhysicsConstants.ElementaryCharge;
            var denominator = 4.0 * Math.PI * PhysicsConstants.VacuumPermittivity * 
                             PhysicsConstants.ElectronMass * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
            return e2 / denominator;
        }
    }

    /// <summary>
    /// Gets the Compton wavelength of the electron (λc = h/(mec)).
    /// </summary>
    public double ComptonWavelength
    {
        get
        {
            return PhysicsConstants.PlanckConstant / 
                   (PhysicsConstants.ElectronMass * PhysicsConstants.SpeedOfLight);
        }
    }

    /// <summary>
    /// Gets the cyclotron frequency in a magnetic field.
    /// </summary>
    /// <param name="magneticFieldStrength">Magnetic field strength in Tesla.</param>
    /// <returns>Cyclotron frequency in Hz.</returns>
    public double GetCyclotronFrequency(double magneticFieldStrength)
    {
        return Math.Abs(Charge.Value) * magneticFieldStrength / (2.0 * Math.PI * Mass.Value);
    }

    #endregion

    #region Atomic Physics Methods

    /// <summary>
    /// Calculates the binding energy in a hydrogen-like atom with nuclear charge Z.
    /// </summary>
    /// <param name="nuclearCharge">Nuclear charge (Z).</param>
    /// <param name="principalQuantumNumber">Principal quantum number (n).</param>
    /// <returns>Binding energy in Joules.</returns>
    public double CalculateHydrogenicBindingEnergy(int nuclearCharge, int principalQuantumNumber)
    {
        if (principalQuantumNumber <= 0) 
            throw new ArgumentException("Principal quantum number must be positive", nameof(principalQuantumNumber));
        if (nuclearCharge <= 0) 
            throw new ArgumentException("Nuclear charge must be positive", nameof(nuclearCharge));

        // E_n = -13.6 eV * Z² / n²
        var rydbergEnergy = 13.6 * 1.602176634e-19; // Convert eV to Joules
        return -rydbergEnergy * nuclearCharge * nuclearCharge / (principalQuantumNumber * principalQuantumNumber);
    }

    /// <summary>
    /// Calculates the orbital radius in a hydrogen-like atom.
    /// </summary>
    /// <param name="nuclearCharge">Nuclear charge (Z).</param>
    /// <param name="principalQuantumNumber">Principal quantum number (n).</param>
    /// <returns>Orbital radius in meters.</returns>
    public double CalculateHydrogenicRadius(int nuclearCharge, int principalQuantumNumber)
    {
        if (principalQuantumNumber <= 0) 
            throw new ArgumentException("Principal quantum number must be positive", nameof(principalQuantumNumber));
        if (nuclearCharge <= 0) 
            throw new ArgumentException("Nuclear charge must be positive", nameof(nuclearCharge));

        // r_n = a₀ * n² / Z
        return PhysicsConstants.BohrRadius * principalQuantumNumber * principalQuantumNumber / nuclearCharge;
    }

    #endregion

    #region Relativistic Methods

    /// <summary>
    /// Calculates the relativistic energy E = γmc².
    /// </summary>
    /// <param name="velocity">Velocity as a fraction of the speed of light.</param>
    /// <returns>Total relativistic energy in Joules.</returns>
    public double CalculateRelativisticEnergy(double velocity)
    {
        if (velocity >= PhysicsConstants.SpeedOfLight) 
            throw new ArgumentException("Velocity cannot exceed speed of light", nameof(velocity));

        var beta = velocity / PhysicsConstants.SpeedOfLight;
        var gamma = 1.0 / Math.Sqrt(1.0 - beta * beta);
        return gamma * Mass.Value * PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight;
    }

    /// <summary>
    /// Calculates the de Broglie wavelength for a given momentum.
    /// </summary>
    /// <param name="momentum">Momentum in kg⋅m/s.</param>
    /// <returns>de Broglie wavelength in meters.</returns>
    public double CalculateDeBroglieWavelength(double momentum)
    {
        if (momentum <= 0) 
            throw new ArgumentException("Momentum must be positive", nameof(momentum));

        return PhysicsConstants.PlanckConstant / momentum;
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Electrons interact electromagnetically with photons and all charged particles
        return other.Name == "Photon" || !other.Charge.Value.Equals(0.0);
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // All leptons participate in weak interactions
        return true;
    }

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Electrons do not participate in strong interactions
        return false;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        // Electromagnetic interaction dominates for electrons
        if (CanInteractElectromagnetically(other))
        {
            // Coulomb interaction
            var k = PhysicsConstants.CoulombConstant;
            var q1 = Charge.Value;
            var q2 = other.Charge.Value;
            return k * q1 * q2 / (distance * distance);
        }
        
        return 0.0; // No strong interaction
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Electron(IsPositron);
        clone.StateVector = StateVector; // This will trigger validation and events
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        return new Electron(!IsPositron);
    }

    #endregion

    #region Private Helper Methods

    private void InitializeElectronState()
    {
        // Initialize in a definite spin-up state
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a standard electron.
    /// </summary>
    /// <returns>A new electron instance.</returns>
    public static Electron CreateElectron() => new(false);

    /// <summary>
    /// Creates a positron (antielectron).
    /// </summary>
    /// <returns>A new positron instance.</returns>
    public static Electron CreatePositron() => new(true);

    #endregion
}
