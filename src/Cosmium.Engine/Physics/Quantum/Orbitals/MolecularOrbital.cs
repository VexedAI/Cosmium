using Cosmium.Engine.Infrastructure.Logging;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;
using Complex = Cosmium.Engine.Physics.Mathematics.Complex;

namespace Cosmium.Engine.Physics.Quantum.Orbitals;

/// <summary>
/// Represents a molecular orbital constructed from Linear Combination of Atomic Orbitals (LCAO).
/// Implements basic molecular orbital theory for simple diatomic and polyatomic molecules.
/// </summary>
public class MolecularOrbital : Orbital
{
    #region Fields

    private static readonly SimulationLogger Logger = SimulationLogger.Instance;
    private readonly object _calculationLock = new();
    private readonly List<AtomicOrbitalContribution> _atomicOrbitals;
    private double? _cachedEnergy;
    private double? _cachedNormalization;
    private readonly bool _isBonding;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the molecular orbital energy in Joules.
    /// Calculated using the LCAO method with overlap and resonance integrals.
    /// </summary>
    public override double Energy
    {
        get
        {
            lock (_calculationLock)
            {
                if (_cachedEnergy.HasValue)
                    return _cachedEnergy.Value;

                _cachedEnergy = CalculateMolecularOrbitalEnergy();
                return _cachedEnergy.Value;
            }
        }
    }

    /// <summary>
    /// Gets whether this molecular orbital is normalized.
    /// </summary>
    public override bool IsNormalized => Math.Abs(CalculateNormalizationIntegral() - 1.0) < PrecisionHandling.ProbabilityNormalizationTolerance;

    /// <summary>
    /// Gets whether this is a bonding orbital (true) or antibonding orbital (false).
    /// </summary>
    public bool IsBonding => _isBonding;

    /// <summary>
    /// Gets the type of molecular orbital (bonding or antibonding).
    /// </summary>
    public string OrbitalType => IsBonding ? "Bonding" : "Antibonding";

    /// <summary>
    /// Gets the number of atomic orbitals contributing to this molecular orbital.
    /// </summary>
    public int NumberOfContributingOrbitals => _atomicOrbitals.Count;

    /// <summary>
    /// Gets the bond order contribution from this molecular orbital.
    /// </summary>
    public double BondOrderContribution => IsBonding ? 0.5 : -0.5;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new molecular orbital from atomic orbital contributions.
    /// </summary>
    /// <param name="atomicOrbitals">The list of atomic orbitals and their coefficients.</param>
    /// <param name="isBonding">Whether this is a bonding (true) or antibonding (false) orbital.</param>
    public MolecularOrbital(IEnumerable<AtomicOrbitalContribution> atomicOrbitals, bool isBonding = true)
        : base(
            atomicOrbitals.First().Orbital.PrincipalQuantumNumber,
            atomicOrbitals.First().Orbital.OrbitalAngularMomentumQuantumNumber,
            0, // Molecular orbitals don't have a simple ml value
            1.0) // Effective charge is averaged
    {
        if (atomicOrbitals == null)
            throw new ArgumentNullException(nameof(atomicOrbitals));

        _atomicOrbitals = atomicOrbitals.ToList();
        _isBonding = isBonding;

        if (_atomicOrbitals.Count < 2)
            throw new ArgumentException("Molecular orbital must be constructed from at least 2 atomic orbitals", nameof(atomicOrbitals));

        // Validate that all atomic orbitals have the same l quantum number for proper symmetry
        var firstL = _atomicOrbitals.First().Orbital.OrbitalAngularMomentumQuantumNumber;
        if (!_atomicOrbitals.All(ao => ao.Orbital.OrbitalAngularMomentumQuantumNumber == firstL))
        {
            Logger.Warning("Molecular orbital constructed from atomic orbitals with different l quantum numbers");
        }

        Logger.Debug($"Created {OrbitalType} molecular orbital from {NumberOfContributingOrbitals} atomic orbitals",
            new { IsBonding = isBonding, NumOrbitals = NumberOfContributingOrbitals });
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a simple diatomic molecular orbital from two hydrogen-like atomic orbitals.
    /// </summary>
    /// <param name="orbital1">The first atomic orbital.</param>
    /// <param name="orbital2">The second atomic orbital.</param>
    /// <param name="isBonding">Whether to create a bonding (true) or antibonding (false) combination.</param>
    /// <param name="position1">Position of the first atom.</param>
    /// <param name="position2">Position of the second atom.</param>
    /// <returns>A diatomic molecular orbital.</returns>
    public static MolecularOrbital CreateDiatomic(HydrogenicOrbital orbital1, HydrogenicOrbital orbital2, 
        bool isBonding, Vector3D position1, Vector3D position2)
    {
        if (orbital1 == null) throw new ArgumentNullException(nameof(orbital1));
        if (orbital2 == null) throw new ArgumentNullException(nameof(orbital2));

        // Calculate overlap integral between the two orbitals
        double overlap = CalculateOverlapIntegral(orbital1, orbital2, position1, position2);
        
        // For bonding: ψ = N(ψA + ψB), for antibonding: ψ = N(ψA - ψB)
        double coeff1 = 1.0;
        double coeff2 = isBonding ? 1.0 : -1.0;
        
        // Normalize coefficients
        double norm = Math.Sqrt(2.0 * (1.0 + (isBonding ? overlap : -overlap)));
        coeff1 /= norm;
        coeff2 /= norm;

        var contributions = new List<AtomicOrbitalContribution>
        {
            new(orbital1, coeff1, position1),
            new(orbital2, coeff2, position2)
        };

        return new MolecularOrbital(contributions, isBonding);
    }

    /// <summary>
    /// Creates a molecular orbital for a linear H₂⁺ ion.
    /// </summary>
    /// <param name="bondLength">The bond length in meters.</param>
    /// <param name="isBonding">Whether to create the bonding (1sσ) or antibonding (1sσ*) orbital.</param>
    /// <returns>A H₂⁺ molecular orbital.</returns>
    public static MolecularOrbital CreateH2Plus(double bondLength, bool isBonding = true)
    {
        var bondValidation = ParameterValidator.ValidatePositive(bondLength, nameof(bondLength));
        bondValidation.ThrowIfInvalid();

        var h1s_A = HydrogenicOrbital.Create1s(1.0);
        var h1s_B = HydrogenicOrbital.Create1s(1.0);

        var positionA = new Vector3D(-bondLength / 2.0, 0, 0);
        var positionB = new Vector3D(bondLength / 2.0, 0, 0);

        return CreateDiatomic(h1s_A, h1s_B, isBonding, positionA, positionB);
    }

    /// <summary>
    /// Creates a simple π orbital from two p orbitals.
    /// </summary>
    /// <param name="pOrbital1">The first p orbital.</param>
    /// <param name="pOrbital2">The second p orbital.</param>
    /// <param name="isBonding">Whether to create a bonding π or antibonding π* orbital.</param>
    /// <param name="position1">Position of the first atom.</param>
    /// <param name="position2">Position of the second atom.</param>
    /// <returns>A π molecular orbital.</returns>
    public static MolecularOrbital CreatePiOrbital(HydrogenicOrbital pOrbital1, HydrogenicOrbital pOrbital2, 
        bool isBonding, Vector3D position1, Vector3D position2)
    {
        if (pOrbital1.OrbitalAngularMomentumQuantumNumber != 1 || pOrbital2.OrbitalAngularMomentumQuantumNumber != 1)
            throw new ArgumentException("π orbitals must be constructed from p atomic orbitals (l=1)");

        return CreateDiatomic(pOrbital1, pOrbital2, isBonding, position1, position2);
    }

    #endregion

    #region Wave Function Calculations

    /// <summary>
    /// Calculates the radial component of the molecular orbital.
    /// For molecular orbitals, this is an approximation based on the contributing atomic orbitals.
    /// </summary>
    /// <param name="r">The radial distance from the molecular center.</param>
    /// <returns>An approximate radial component.</returns>
    public override double RadialWaveFunction(double r)
    {
        var rValidation = ParameterValidator.ValidatePositive(r, nameof(r), allowZero: true);
        rValidation.ThrowIfInvalid();

        // For molecular orbitals, the radial wave function is not well-defined
        // Return an average of the atomic radial functions
        double sum = 0.0;
        double totalCoeff = 0.0;

        foreach (var contribution in _atomicOrbitals)
        {
            var atomicRadial = contribution.Orbital.RadialWaveFunction(r);
            sum += Math.Abs(contribution.Coefficient) * atomicRadial;
            totalCoeff += Math.Abs(contribution.Coefficient);
        }

        return totalCoeff > 0 ? sum / totalCoeff : 0.0;
    }

    /// <summary>
    /// Calculates the angular component of the molecular orbital.
    /// For molecular orbitals, this is simplified and may not be physically accurate.
    /// </summary>
    /// <param name="theta">The polar angle in radians.</param>
    /// <param name="phi">The azimuthal angle in radians.</param>
    /// <returns>An approximate angular component.</returns>
    public override Complex AngularWaveFunction(double theta, double phi)
    {
        var thetaValidation = ParameterValidator.ValidateRange(theta, nameof(theta), 0.0, Math.PI);
        thetaValidation.ThrowIfInvalid();

        var phiValidation = ParameterValidator.ValidateRange(phi, nameof(phi), 0.0, 2.0 * Math.PI);
        phiValidation.ThrowIfInvalid();

        // For molecular orbitals, return an average angular function
        Complex sum = Complex.Zero;
        double totalCoeff = 0.0;

        foreach (var contribution in _atomicOrbitals)
        {
            var atomicAngular = contribution.Orbital.AngularWaveFunction(theta, phi);
            sum += contribution.Coefficient * atomicAngular;
            totalCoeff += Math.Abs(contribution.Coefficient);
        }

        return totalCoeff > 0 ? sum / totalCoeff : Complex.Zero;
    }

    /// <summary>
    /// Calculates the complete molecular orbital wave function at a given position.
    /// Uses the LCAO approximation: ψ_MO = Σ c_i ψ_i
    /// </summary>
    /// <param name="position">The position vector in Cartesian coordinates.</param>
    /// <returns>The complex value of the molecular orbital wave function.</returns>
    public override Complex WaveFunction(Vector3D position)
    {
        Complex totalWaveFunction = Complex.Zero;

        foreach (var contribution in _atomicOrbitals)
        {
            // Translate position relative to the atomic orbital center
            var relativePosition = position - contribution.Position;
            var atomicWaveFunction = contribution.Orbital.WaveFunction(relativePosition);
            totalWaveFunction += contribution.Coefficient * atomicWaveFunction;
        }

        return totalWaveFunction;
    }

    #endregion

    #region Quantum Mechanical Properties

    /// <summary>
    /// Calculates the expectation value of the radial position for the molecular orbital.
    /// This is an approximation based on the contributing atomic orbitals.
    /// </summary>
    /// <returns>The expectation value of r in meters.</returns>
    public override double ExpectationValueRadius()
    {
        double sum = 0.0;
        double normalization = 0.0;

        foreach (var contribution in _atomicOrbitals)
        {
            var atomicExpectation = contribution.Orbital.ExpectationValueRadius();
            var coeffSquared = contribution.Coefficient * contribution.Coefficient;
            sum += coeffSquared * atomicExpectation;
            normalization += coeffSquared;
        }

        return normalization > 0 ? sum / normalization : 0.0;
    }

    /// <summary>
    /// Calculates the expectation value of r² for the molecular orbital.
    /// </summary>
    /// <returns>The expectation value of r² in m².</returns>
    public override double ExpectationValueRadiusSquared()
    {
        double sum = 0.0;
        double normalization = 0.0;

        foreach (var contribution in _atomicOrbitals)
        {
            var atomicExpectation = contribution.Orbital.ExpectationValueRadiusSquared();
            var coeffSquared = contribution.Coefficient * contribution.Coefficient;
            sum += coeffSquared * atomicExpectation;
            normalization += coeffSquared;
        }

        return normalization > 0 ? sum / normalization : 0.0;
    }

    /// <summary>
    /// Calculates the most probable radius for the molecular orbital.
    /// </summary>
    /// <returns>The most probable radius in meters.</returns>
    public override double MostProbableRadius()
    {
        // For molecular orbitals, this is an approximation
        return Math.Sqrt(ExpectationValueRadiusSquared());
    }

    /// <summary>
    /// Calculates the bond order for a diatomic molecule based on this molecular orbital.
    /// </summary>
    /// <param name="electronCount">Number of electrons in this molecular orbital.</param>
    /// <returns>The bond order contribution.</returns>
    public double CalculateBondOrder(int electronCount)
    {
        var electronValidation = ParameterValidator.ValidateIntegerRange(electronCount, nameof(electronCount), 0, 2);
        electronValidation.ThrowIfInvalid();

        return electronCount * BondOrderContribution;
    }

    /// <summary>
    /// Calculates the overlap population between atoms in this molecular orbital.
    /// </summary>
    /// <returns>The overlap population (positive for bonding, negative for antibonding).</returns>
    public double CalculateOverlapPopulation()
    {
        if (_atomicOrbitals.Count != 2)
            throw new InvalidOperationException("Overlap population calculation requires exactly 2 atomic orbitals");

        var ao1 = _atomicOrbitals[0];
        var ao2 = _atomicOrbitals[1];

        double overlap = CalculateOverlapIntegral(ao1.Orbital, ao2.Orbital, ao1.Position, ao2.Position);
        return 2.0 * ao1.Coefficient * ao2.Coefficient * overlap;
    }

    #endregion

    #region Energy Calculations

    /// <summary>
    /// Calculates the molecular orbital energy using the LCAO method.
    /// </summary>
    /// <returns>The molecular orbital energy in Joules.</returns>
    private double CalculateMolecularOrbitalEnergy()
    {
        if (_atomicOrbitals.Count == 2)
        {
            // Simple two-center case
            var ao1 = _atomicOrbitals[0];
            var ao2 = _atomicOrbitals[1];

            double alpha1 = ao1.Orbital.Energy; // Coulomb integral for orbital 1
            double alpha2 = ao2.Orbital.Energy; // Coulomb integral for orbital 2
            double beta = CalculateResonanceIntegral(ao1.Orbital, ao2.Orbital, ao1.Position, ao2.Position);
            double overlap = CalculateOverlapIntegral(ao1.Orbital, ao2.Orbital, ao1.Position, ao2.Position);

            // Secular equation solution for homonuclear diatomic
            if (Math.Abs(alpha1 - alpha2) < PrecisionHandling.QuantumCalculationTolerance)
            {
                // Homonuclear case: E = α ± β
                return alpha1 + (IsBonding ? beta : -beta);
            }
            else
            {
                // Heteronuclear case: solve quadratic equation
                double avgAlpha = (alpha1 + alpha2) / 2.0;
                double deltaAlpha = (alpha1 - alpha2) / 2.0;
                double discriminant = deltaAlpha * deltaAlpha + beta * beta;
                
                double energyShift = Math.Sqrt(discriminant);
                return avgAlpha + (IsBonding ? -energyShift : energyShift);
            }
        }
        else
        {
            // Multi-center case - simplified approximation
            double weightedEnergy = 0.0;
            double totalWeight = 0.0;

            foreach (var contribution in _atomicOrbitals)
            {
                var weight = contribution.Coefficient * contribution.Coefficient;
                weightedEnergy += weight * contribution.Orbital.Energy;
                totalWeight += weight;
            }

            return totalWeight > 0 ? weightedEnergy / totalWeight : 0.0;
        }
    }

    /// <summary>
    /// Calculates the resonance integral (β) between two atomic orbitals.
    /// </summary>
    /// <param name="orbital1">The first atomic orbital.</param>
    /// <param name="orbital2">The second atomic orbital.</param>
    /// <param name="position1">Position of the first atom.</param>
    /// <param name="position2">Position of the second atom.</param>
    /// <returns>The resonance integral in Joules.</returns>
    private static double CalculateResonanceIntegral(Orbital orbital1, Orbital orbital2, Vector3D position1, Vector3D position2)
    {
        // Simplified approximation: β ≈ k * S * (α₁ + α₂) / 2
        // where k is an empirical parameter (~1.75 for hydrogen)
        double overlap = CalculateOverlapIntegral(orbital1, orbital2, position1, position2);
        double avgEnergy = (orbital1.Energy + orbital2.Energy) / 2.0;
        
        // Empirical scaling factor
        double k = 1.75;
        
        return k * overlap * avgEnergy;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculates the overlap integral between two atomic orbitals.
    /// </summary>
    /// <param name="orbital1">The first atomic orbital.</param>
    /// <param name="orbital2">The second atomic orbital.</param>
    /// <param name="position1">Position of the first atom.</param>
    /// <param name="position2">Position of the second atom.</param>
    /// <returns>The overlap integral.</returns>
    private static double CalculateOverlapIntegral(Orbital orbital1, Orbital orbital2, Vector3D position1, Vector3D position2)
    {
        // For hydrogen-like orbitals, we can use analytical expressions
        if (orbital1 is HydrogenicOrbital h1 && orbital2 is HydrogenicOrbital h2)
        {
            return CalculateHydrogenicOverlap(h1, h2, position1, position2);
        }

        // For general orbitals, return a simplified approximation
        double distance = position1.DistanceTo(position2);
        double characteristic_length = PhysicsConstants.BohrRadius / Math.Max(orbital1.EffectiveNuclearCharge, orbital2.EffectiveNuclearCharge);
        
        // Exponential decay approximation
        return Math.Exp(-distance / characteristic_length);
    }

    /// <summary>
    /// Calculates the overlap integral between two hydrogen-like orbitals.
    /// </summary>
    /// <param name="h1">The first hydrogen-like orbital.</param>
    /// <param name="h2">The second hydrogen-like orbital.</param>
    /// <param name="pos1">Position of the first atom.</param>
    /// <param name="pos2">Position of the second atom.</param>
    /// <returns>The overlap integral.</returns>
    private static double CalculateHydrogenicOverlap(HydrogenicOrbital h1, HydrogenicOrbital h2, Vector3D pos1, Vector3D pos2)
    {
        // Simplified analytical expression for 1s-1s overlap
        if (h1.PrincipalQuantumNumber == 1 && h1.OrbitalAngularMomentumQuantumNumber == 0 &&
            h2.PrincipalQuantumNumber == 1 && h2.OrbitalAngularMomentumQuantumNumber == 0)
        {
            double distance = pos1.DistanceTo(pos2);
            double zeta = (h1.EffectiveNuclearCharge + h2.EffectiveNuclearCharge) / 2.0;
            double rho = zeta * distance / PhysicsConstants.BohrRadius;
            
            // Analytical 1s-1s overlap: S = (1 + ρ + ρ²/3) * exp(-ρ)
            return (1.0 + rho + rho * rho / 3.0) * Math.Exp(-rho);
        }

        // For other cases, use exponential approximation
        double dist = pos1.DistanceTo(pos2);
        double avgZeta = (h1.EffectiveNuclearCharge + h2.EffectiveNuclearCharge) / 2.0;
        double rhoApprox = avgZeta * dist / PhysicsConstants.BohrRadius;
        
        return Math.Exp(-rhoApprox / 2.0);
    }

    /// <summary>
    /// Calculates the normalization integral for this molecular orbital.
    /// </summary>
    /// <returns>The normalization integral ⟨ψ|ψ⟩.</returns>
    private double CalculateNormalizationIntegral()
    {
        double norm = 0.0;

        // Diagonal terms
        foreach (var contribution in _atomicOrbitals)
        {
            norm += contribution.Coefficient * contribution.Coefficient;
        }

        // Off-diagonal terms (cross terms)
        for (int i = 0; i < _atomicOrbitals.Count; i++)
        {
            for (int j = i + 1; j < _atomicOrbitals.Count; j++)
            {
                var ao1 = _atomicOrbitals[i];
                var ao2 = _atomicOrbitals[j];
                double overlap = CalculateOverlapIntegral(ao1.Orbital, ao2.Orbital, ao1.Position, ao2.Position);
                norm += 2.0 * ao1.Coefficient * ao2.Coefficient * overlap;
            }
        }

        return norm;
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a detailed string representation of the molecular orbital.
    /// </summary>
    public override string ToDetailedString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Molecular Orbital: {OrbitalType}");
        sb.AppendLine($"Energy: {QuantumMath.JoulesToElectronVolts(Energy):F6} eV");
        sb.AppendLine($"Number of contributing orbitals: {NumberOfContributingOrbitals}");
        sb.AppendLine($"Is Normalized: {IsNormalized}");
        sb.AppendLine($"Bond Order Contribution: {BondOrderContribution:F2}");
        sb.AppendLine("Atomic Orbital Contributions:");

        for (int i = 0; i < _atomicOrbitals.Count; i++)
        {
            var ao = _atomicOrbitals[i];
            sb.AppendLine($"  [{i + 1}] {ao.Orbital.OrbitalDesignation}: {ao.Coefficient:F4} at {ao.Position}");
        }

        sb.AppendLine($"Created: {CreationTime:yyyy-MM-dd HH:mm:ss} UTC");
        return sb.ToString();
    }

    #endregion
}

/// <summary>
/// Represents an atomic orbital contribution to a molecular orbital.
/// </summary>
public class AtomicOrbitalContribution
{
    /// <summary>
    /// Gets the atomic orbital.
    /// </summary>
    public Orbital Orbital { get; }

    /// <summary>
    /// Gets the coefficient (weight) of this atomic orbital in the molecular orbital.
    /// </summary>
    public double Coefficient { get; }

    /// <summary>
    /// Gets the position of the atom containing this orbital.
    /// </summary>
    public Vector3D Position { get; }

    /// <summary>
    /// Initializes a new atomic orbital contribution.
    /// </summary>
    /// <param name="orbital">The atomic orbital.</param>
    /// <param name="coefficient">The coefficient in the linear combination.</param>
    /// <param name="position">The position of the atom.</param>
    public AtomicOrbitalContribution(Orbital orbital, double coefficient, Vector3D position)
    {
        Orbital = orbital ?? throw new ArgumentNullException(nameof(orbital));
        Coefficient = coefficient;
        Position = position;
    }

    /// <summary>
    /// Returns a string representation of this contribution.
    /// </summary>
    public override string ToString()
    {
        return $"{Coefficient:F4} * {Orbital.OrbitalDesignation} at {Position}";
    }
}
