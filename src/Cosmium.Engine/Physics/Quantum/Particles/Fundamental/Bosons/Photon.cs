using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;

namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Bosons;

/// <summary>
/// Implementation of a photon following the Standard Model of particle physics.
/// The photon is the massless spin-1 boson that mediates electromagnetic interactions.
/// It travels at the speed of light and carries energy and momentum.
/// </summary>
public class Photon : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;
    private readonly ScalarMeasurableProperty _frequency;
    private readonly ScalarMeasurableProperty _wavelength;
    private readonly VectorMeasurableProperty _momentum;
    private VectorMeasurableProperty _electricField;
    private VectorMeasurableProperty _magneticField;

    #region Properties

    /// <summary>
    /// Gets the frequency of the photon in Hz.
    /// </summary>
    public double Frequency => _frequency.Value;

    /// <summary>
    /// Gets the wavelength of the photon in meters.
    /// </summary>
    public double Wavelength => _wavelength.Value;

    /// <summary>
    /// Gets the energy of the photon (E = hf).
    /// </summary>
    public double PhotonEnergy => PhysicsConstants.PlanckConstant * Frequency;

    /// <summary>
    /// Gets the momentum magnitude of the photon (p = E/c = h/λ).
    /// </summary>
    public double PhotonMomentum => PhotonEnergy / PhysicsConstants.SpeedOfLight;

    /// <summary>
    /// Gets the polarization state of the photon.
    /// </summary>
    public PhotonPolarization Polarization { get; private set; }

    /// <summary>
    /// Gets whether this photon is in the visible spectrum.
    /// </summary>
    public bool IsVisible => Wavelength >= 380e-9 && Wavelength <= 700e-9; // 380-700 nm

    /// <summary>
    /// Gets the color classification for visible light.
    /// </summary>
    public string ColorClassification
    {
        get
        {
            if (!IsVisible) return "Non-visible";
            
            return Wavelength switch
            {
                >= 620e-9 => "Red",
                >= 590e-9 => "Orange", 
                >= 570e-9 => "Yellow",
                >= 495e-9 => "Green",
                >= 450e-9 => "Blue",
                >= 380e-9 => "Violet",
                _ => "Non-visible"
            };
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new photon with specified frequency and polarization.
    /// </summary>
    /// <param name="frequency">Frequency in Hz.</param>
    /// <param name="polarization">Polarization state.</param>
    /// <param name="propagationDirection">Direction of propagation (normalized).</param>
    public Photon(double frequency, PhotonPolarization polarization = PhotonPolarization.Linear, 
                  double[]? propagationDirection = null) 
        : base("Photon", "γ", isElementary: true, hilbertSpaceDimension: 2) // Spin-1 but 2 helicity states
    {
        if (frequency <= 0)
            throw new ArgumentException("Frequency must be positive", nameof(frequency));

        Polarization = polarization;
        propagationDirection ??= new[] { 0.0, 0.0, 1.0 }; // Default: z-direction

        // Normalize propagation direction
        var magnitude = Math.Sqrt(propagationDirection.Sum(x => x * x));
        if (magnitude == 0) throw new ArgumentException("Propagation direction cannot be zero", nameof(propagationDirection));
        
        var normalizedDirection = propagationDirection.Select(x => x / magnitude).ToArray();

        // Calculate derived quantities
        var wavelength = PhysicsConstants.SpeedOfLight / frequency;
        var energy = PhysicsConstants.PlanckConstant * frequency;
        var momentum = energy / PhysicsConstants.SpeedOfLight;

        // Initialize physical properties
        _mass = new ScalarMeasurableProperty("Mass", "kg", 0.0); // Massless
        _charge = new ScalarMeasurableProperty("Charge", "C", 0.0); // Neutral
        _frequency = new ScalarMeasurableProperty("Frequency", "Hz", frequency);
        _wavelength = new ScalarMeasurableProperty("Wavelength", "m", wavelength);
        
        // Momentum vector
        var momentumVector = normalizedDirection.Select(x => x * momentum).ToArray();
        _momentum = new VectorMeasurableProperty("Momentum", "kg⋅m/s", momentumVector);

        // Initialize electromagnetic fields (simplified)
        InitializeElectromagneticFields(normalizedDirection);

        // Initialize quantum state based on polarization
        InitializePhotonState();
    }

    /// <summary>
    /// Creates a photon with specified wavelength.
    /// </summary>
    /// <param name="wavelength">Wavelength in meters.</param>
    /// <param name="polarization">Polarization state.</param>
    /// <param name="propagationDirection">Direction of propagation.</param>
    public static Photon FromWavelength(double wavelength, PhotonPolarization polarization = PhotonPolarization.Linear,
                                       double[]? propagationDirection = null)
    {
        if (wavelength <= 0)
            throw new ArgumentException("Wavelength must be positive", nameof(wavelength));

        var frequency = PhysicsConstants.SpeedOfLight / wavelength;
        return new Photon(frequency, polarization, propagationDirection);
    }

    /// <summary>
    /// Creates a photon with specified energy.
    /// </summary>
    /// <param name="energy">Energy in Joules.</param>
    /// <param name="polarization">Polarization state.</param>
    /// <param name="propagationDirection">Direction of propagation.</param>
    public static Photon FromEnergy(double energy, PhotonPolarization polarization = PhotonPolarization.Linear,
                                   double[]? propagationDirection = null)
    {
        if (energy <= 0)
            throw new ArgumentException("Energy must be positive", nameof(energy));

        var frequency = energy / PhysicsConstants.PlanckConstant;
        return new Photon(frequency, polarization, propagationDirection);
    }

    #endregion

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 1.0; // Spin-1 boson
    public override ParticleStatistics Statistics => ParticleStatistics.BoseEinstein; // Boson

    #endregion

    #region Photon-Specific Properties

    /// <summary>
    /// Gets the frequency as a measurable property.
    /// </summary>
    public IScalarMeasurable FrequencyMeasurable => _frequency;

    /// <summary>
    /// Gets the wavelength as a measurable property.
    /// </summary>
    public IScalarMeasurable WavelengthMeasurable => _wavelength;

    /// <summary>
    /// Gets the momentum as a vector measurable property.
    /// </summary>
    public new IVectorMeasurable Momentum => _momentum;

    /// <summary>
    /// Gets the electric field vector.
    /// </summary>
    public IVectorMeasurable ElectricField => _electricField;

    /// <summary>
    /// Gets the magnetic field vector.
    /// </summary>
    public IVectorMeasurable MagneticField => _magneticField;

    #endregion

    #region Electromagnetic Methods

    /// <summary>
    /// Calculates the intensity of the electromagnetic wave.
    /// </summary>
    /// <returns>Intensity in W/m².</returns>
    public double CalculateIntensity()
    {
        // I = (1/2) * c * ε₀ * E₀²
        var electricFieldMagnitude = _electricField.Magnitude;
        return 0.5 * PhysicsConstants.SpeedOfLight * PhysicsConstants.VacuumPermittivity * 
               electricFieldMagnitude * electricFieldMagnitude;
    }

    /// <summary>
    /// Calculates the radiation pressure exerted by the photon.
    /// </summary>
    /// <param name="reflectionCoefficient">Reflection coefficient (0 = absorption, 1 = reflection).</param>
    /// <returns>Radiation pressure in Pa.</returns>
    public double CalculateRadiationPressure(double reflectionCoefficient = 0.0)
    {
        if (reflectionCoefficient < 0 || reflectionCoefficient > 1)
            throw new ArgumentException("Reflection coefficient must be between 0 and 1", nameof(reflectionCoefficient));

        var intensity = CalculateIntensity();
        return intensity * (1.0 + reflectionCoefficient) / PhysicsConstants.SpeedOfLight;
    }

    /// <summary>
    /// Sets the polarization state of the photon.
    /// </summary>
    /// <param name="polarization">New polarization state.</param>
    public void SetPolarization(PhotonPolarization polarization)
    {
        Polarization = polarization;
        InitializePhotonState(); // Update quantum state
    }

    #endregion

    #region Optical Methods

    /// <summary>
    /// Calculates the refractive angle when the photon enters a medium (Snell's law).
    /// </summary>
    /// <param name="incidentAngle">Incident angle in radians.</param>
    /// <param name="refractiveIndex">Refractive index of the medium.</param>
    /// <returns>Refracted angle in radians.</returns>
    public double CalculateRefractionAngle(double incidentAngle, double refractiveIndex)
    {
        if (refractiveIndex <= 0)
            throw new ArgumentException("Refractive index must be positive", nameof(refractiveIndex));

        // Snell's law: n₁sin(θ₁) = n₂sin(θ₂)
        // Assuming initial medium is vacuum (n₁ = 1)
        var sinRefracted = Math.Sin(incidentAngle) / refractiveIndex;
        
        if (Math.Abs(sinRefracted) > 1.0)
            throw new InvalidOperationException("Total internal reflection occurs");

        return Math.Asin(sinRefracted);
    }

    /// <summary>
    /// Updates the photon's frequency and wavelength when entering a medium.
    /// </summary>
    /// <param name="refractiveIndex">Refractive index of the medium.</param>
    public void EnterMedium(double refractiveIndex)
    {
        if (refractiveIndex <= 0)
            throw new ArgumentException("Refractive index must be positive", nameof(refractiveIndex));

        // Frequency remains constant, wavelength changes
        var newWavelength = _wavelength.Value / refractiveIndex;
        _wavelength.SetValue(newWavelength);
        
        // Update momentum (p = h/λ)
        var newMomentumMagnitude = PhysicsConstants.PlanckConstant / newWavelength;
        var direction = _momentum.Direction;
        var newMomentumVector = direction.Select(x => x * newMomentumMagnitude).ToArray();
        _momentum.SetValue(newMomentumVector);
    }

    #endregion

    #region Quantum Optical Methods

    /// <summary>
    /// Calculates the probability of photoelectric emission from a material.
    /// </summary>
    /// <param name="workFunction">Work function of the material in Joules.</param>
    /// <returns>True if photoelectric effect is possible.</returns>
    public bool CanCausePhotoelectricEffect(double workFunction)
    {
        return PhotonEnergy >= workFunction;
    }

    /// <summary>
    /// Calculates the kinetic energy of emitted photoelectrons.
    /// </summary>
    /// <param name="workFunction">Work function in Joules.</param>
    /// <returns>Kinetic energy of photoelectrons in Joules.</returns>
    public double CalculatePhotoelectronKineticEnergy(double workFunction)
    {
        if (!CanCausePhotoelectricEffect(workFunction))
            return 0.0;

        return PhotonEnergy - workFunction;
    }

    /// <summary>
    /// Calculates the Compton scattering wavelength shift.
    /// </summary>
    /// <param name="scatteringAngle">Scattering angle in radians.</param>
    /// <returns>Wavelength shift in meters.</returns>
    public double CalculateComptonShift(double scatteringAngle)
    {
        // Δλ = (h/mₑc)(1 - cos θ)
        var comptonWavelength = PhysicsConstants.PlanckConstant / 
                               (PhysicsConstants.ElectronMass * PhysicsConstants.SpeedOfLight);
        return comptonWavelength * (1.0 - Math.Cos(scatteringAngle));
    }

    #endregion

    #region Interaction Overrides

    public override bool CanInteractElectromagnetically(IQuantumParticle other)
    {
        // Photons mediate electromagnetic interactions but don't self-interact at tree level
        return other.Charge.Value != 0.0;
    }

    public override bool CanInteractWeakly(IQuantumParticle other)
    {
        // Photons don't participate in weak interactions directly
        return false;
    }

    public override bool CanInteractStrongly(IQuantumParticle other)
    {
        // Photons don't participate in strong interactions
        return false;
    }

    public override double CalculateInteractionStrength(IQuantumParticle other, double distance)
    {
        // Photon interactions depend on the target particle's electromagnetic properties
        if (CanInteractElectromagnetically(other))
        {
            // Cross-section depends on target charge and photon energy
            var alpha = PhysicsConstants.FineStructureConstant;
            var crossSection = alpha * alpha / (PhotonEnergy / (PhysicsConstants.ElectronMass * 
                              PhysicsConstants.SpeedOfLight * PhysicsConstants.SpeedOfLight));
            return crossSection;
        }
        
        return 0.0;
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new Photon(_frequency.Value, Polarization, _momentum.Direction);
        clone.StateVector = StateVector;
        return clone;
    }

    public override IQuantumParticle? GetAntiparticle()
    {
        // Photons are their own antiparticles
        return Clone();
    }

    #endregion

    #region Private Helper Methods

    private void InitializeElectromagneticFields(double[] propagationDirection)
    {
        // Calculate electric field amplitude from energy
        // For a single photon, this is more of a classical field representation
        var intensity = PhotonEnergy / (PhysicsConstants.SpeedOfLight * 1e-15); // Over femtosecond pulse
        var electricFieldMagnitude = Math.Sqrt(2.0 * intensity / 
                                             (PhysicsConstants.SpeedOfLight * PhysicsConstants.VacuumPermittivity));

        // Electric field perpendicular to propagation (choose x-direction if propagating in z)
        var electricFieldDirection = GetPerpendicularDirection(propagationDirection);
        var electricField = electricFieldDirection.Select(x => x * electricFieldMagnitude).ToArray();
        
        // Magnetic field perpendicular to both E and k
        var magneticFieldDirection = CrossProduct(electricFieldDirection, propagationDirection);
        var magneticFieldMagnitude = electricFieldMagnitude / PhysicsConstants.SpeedOfLight;
        var magneticField = magneticFieldDirection.Select(x => x * magneticFieldMagnitude).ToArray();

        _electricField = new VectorMeasurableProperty("ElectricField", "V/m", electricField);
        _magneticField = new VectorMeasurableProperty("MagneticField", "T", magneticField);
    }

    private void InitializePhotonState()
    {
        // Initialize quantum state based on polarization
        StateVector = Polarization switch
        {
            PhotonPolarization.Linear => new Complex[] { new(1.0, 0.0), new(0.0, 0.0) },
            PhotonPolarization.RightCircular => new Complex[] { new(1.0/Math.Sqrt(2.0), 0.0), new(0.0, 1.0/Math.Sqrt(2.0)) },
            PhotonPolarization.LeftCircular => new Complex[] { new(1.0/Math.Sqrt(2.0), 0.0), new(0.0, -1.0/Math.Sqrt(2.0)) },
            _ => new Complex[] { new(1.0, 0.0), new(0.0, 0.0) }
        };
    }

    private static double[] GetPerpendicularDirection(double[] direction)
    {
        // Find a direction perpendicular to the given direction
        if (Math.Abs(direction[2]) < 0.9)
            return new[] { 0.0, 0.0, 1.0 }; // Use z if not parallel
        else
            return new[] { 1.0, 0.0, 0.0 }; // Use x if parallel to z
    }

    private static double[] CrossProduct(double[] a, double[] b)
    {
        return new[]
        {
            a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0]
        };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a red photon (wavelength ~650 nm).
    /// </summary>
    public static Photon CreateRedLight() => FromWavelength(650e-9);

    /// <summary>
    /// Creates a green photon (wavelength ~550 nm).
    /// </summary>
    public static Photon CreateGreenLight() => FromWavelength(550e-9);

    /// <summary>
    /// Creates a blue photon (wavelength ~450 nm).
    /// </summary>
    public static Photon CreateBlueLight() => FromWavelength(450e-9);

    /// <summary>
    /// Creates a gamma ray photon (high energy).
    /// </summary>
    /// <param name="energy">Energy in Joules.</param>
    public static Photon CreateGammaRay(double energy = 1e-13) => FromEnergy(energy);

    /// <summary>
    /// Creates an X-ray photon.
    /// </summary>
    /// <param name="energy">Energy in Joules (default ~10 keV).</param>
    public static Photon CreateXRay(double energy = 1.602e-15) => FromEnergy(energy);

    /// <summary>
    /// Creates a microwave photon.
    /// </summary>
    /// <param name="frequency">Frequency in Hz (default 2.45 GHz).</param>
    public static Photon CreateMicrowave(double frequency = 2.45e9) => new(frequency);

    #endregion
}

/// <summary>
/// Enumeration of photon polarization states.
/// </summary>
public enum PhotonPolarization
{
    /// <summary>
    /// Linear polarization.
    /// </summary>
    Linear,

    /// <summary>
    /// Right circular polarization.
    /// </summary>
    RightCircular,

    /// <summary>
    /// Left circular polarization.
    /// </summary>
    LeftCircular
}
