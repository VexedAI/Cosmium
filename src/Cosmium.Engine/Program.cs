using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.Constants;

namespace Cosmium.Engine;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Cosmium Engine - Mathematical Foundation Demo ===\n");

        DemonstrateComplex();
        DemonstrateVector3D();
        DemonstrateMatrix();
        DemonstrateProbability();
        DemonstratePhysicsConstants();
        DemonstrateUnitConversions();
        DemonstratePrecisionHandling();

        Console.WriteLine("\n=== Demo Complete ===");
    }

    private static void DemonstrateComplex()
    {
        Console.WriteLine("--- Complex Number Operations ---");
        
        var z1 = new Complex(3, 4);  // 3 + 4i
        var z2 = new Complex(1, -2); // 1 - 2i
        
        Console.WriteLine($"z1 = {z1}");
        Console.WriteLine($"z2 = {z2}");
        Console.WriteLine($"z1 + z2 = {z1 + z2}");
        Console.WriteLine($"z1 * z2 = {z1 * z2}");
        Console.WriteLine($"|z1| = {z1.Magnitude:F3}");
        Console.WriteLine($"z1* = {z1.Conjugate}");
        Console.WriteLine($"e^(iπ) = {Complex.Exp(new Complex(0, Math.PI))}");
        Console.WriteLine();
    }

    private static void DemonstrateVector3D()
    {
        Console.WriteLine("--- 3D Vector Operations ---");
        
        var v1 = new Vector3D(1, 2, 3);
        var v2 = new Vector3D(4, 5, 6);
        
        Console.WriteLine($"v1 = {v1}");
        Console.WriteLine($"v2 = {v2}");
        Console.WriteLine($"v1 + v2 = {v1 + v2}");
        Console.WriteLine($"v1 · v2 = {v1.Dot(v2):F3}");
        Console.WriteLine($"v1 × v2 = {v1.Cross(v2)}");
        Console.WriteLine($"|v1| = {v1.Magnitude:F3}");
        Console.WriteLine($"v1̂ = {v1.Normalized}");
        
        var (r, theta, phi) = v1.ToSpherical();
        Console.WriteLine($"v1 in spherical: (r={r:F3}, θ={theta:F3}, φ={phi:F3})");
        Console.WriteLine();
    }

    private static void DemonstrateMatrix()
    {
        Console.WriteLine("--- Matrix Operations ---");
        
        // Create a 2x2 matrix
        var m1 = new Matrix(new Complex[,]
        {
            { new Complex(1, 0), new Complex(0, 1) },
            { new Complex(0, -1), new Complex(1, 0) }
        });
        
        Console.WriteLine($"Matrix m1:");
        Console.WriteLine(m1.ToString("F2"));
        
        Console.WriteLine($"m1 transpose:");
        Console.WriteLine(m1.Transpose().ToString("F2"));
        
        Console.WriteLine($"m1 conjugate transpose:");
        Console.WriteLine(m1.ConjugateTranspose().ToString("F2"));
        
        Console.WriteLine($"Trace(m1) = {m1.Trace()}");
        Console.WriteLine($"||m1||_F = {m1.FrobeniusNorm():F3}");
        Console.WriteLine($"Is Unitary: {m1.IsUnitary()}");
        
        // Create quantum state vectors
        var state1 = Matrix.ColumnVector(new Complex(1, 0), new Complex(0, 0));
        var state2 = Matrix.ColumnVector(new Complex(0, 0), new Complex(1, 0));
        
        Console.WriteLine($"⟨ψ₁|ψ₂⟩ = {state1.InnerProduct(state2)}");
        Console.WriteLine();
    }

    private static void DemonstrateProbability()
    {
        Console.WriteLine("--- Probability and Quantum Measurements ---");
        
        // Quantum amplitudes
        var amplitudes = new Complex[]
        {
            new Complex(1/Math.Sqrt(2), 0),
            new Complex(0, 1/Math.Sqrt(2))
        };
        
        var probabilities = Probability.FromAmplitudes(amplitudes);
        Console.WriteLine("Quantum amplitudes: [1/√2, i/√2]");
        Console.WriteLine($"Measurement probabilities: [{probabilities[0]:F3}, {probabilities[1]:F3}]");
        
        // Statistical calculations
        var values = new double[] { 0, 1 };
        Console.WriteLine($"Expected value: {Probability.ExpectedValue(values, probabilities):F3}");
        Console.WriteLine($"Variance: {Probability.Variance(values, probabilities):F3}");
        Console.WriteLine($"Entropy: {Probability.Entropy(probabilities):F3} bits");
        
        // Random sampling
        var random = new Random(42);
        var samples = Probability.SampleDiscrete(probabilities, 10, random);
        Console.WriteLine($"10 random samples: [{string.Join(", ", samples)}]");
        Console.WriteLine();
    }

    private static void DemonstratePhysicsConstants()
    {
        Console.WriteLine("--- Physics Constants ---");
        
        Console.WriteLine($"Speed of light: {PhysicsConstants.SpeedOfLight:E3} m/s");
        Console.WriteLine($"Planck constant: {PhysicsConstants.PlanckConstant:E3} J⋅s");
        Console.WriteLine($"Reduced Planck constant: {PhysicsConstants.ReducedPlanckConstant:E3} J⋅s");
        Console.WriteLine($"Elementary charge: {PhysicsConstants.ElementaryCharge:E3} C");
        Console.WriteLine($"Electron mass: {PhysicsConstants.ElectronMass:E3} kg");
        Console.WriteLine($"Fine structure constant: {PhysicsConstants.FineStructureConstant:F6}");
        Console.WriteLine($"Bohr radius: {PhysicsConstants.BohrRadius:E3} m");
        
        // Demonstrate derived constants
        Console.WriteLine($"Planck length: {PhysicsConstants.PlanckLength:E3} m");
        Console.WriteLine($"Planck time: {PhysicsConstants.PlanckTime:E3} s");
        
        // Validate fundamental relationships
        bool isValid = PhysicsConstants.ValidateConstantRelationships();
        Console.WriteLine($"Constant relationships valid: {isValid}");
        Console.WriteLine();
    }

    private static void DemonstrateUnitConversions()
    {
        Console.WriteLine("--- Unit Conversions ---");
        
        // Energy conversions
        double energyJoules = 1e-19; // Example energy
        double energyEV = UnitConversions.JoulesToElectronVolts(energyJoules);
        double energyHartree = UnitConversions.JoulesToHartree(energyJoules);
        
        Console.WriteLine($"Energy: {energyJoules:E3} J = {energyEV:F3} eV = {energyHartree:E3} Hartree");
        
        // Length conversions
        double lengthMeters = PhysicsConstants.BohrRadius * 2;
        double lengthBohr = UnitConversions.MetersToBohrRadii(lengthMeters);
        double lengthAngstrom = UnitConversions.MetersToAngstroms(lengthMeters);
        
        Console.WriteLine($"Length: {lengthMeters:E3} m = {lengthBohr:F3} a₀ = {lengthAngstrom:F3} Å");
        
        // Wavelength to energy conversion
        double wavelength = 500e-9; // 500 nm (green light)
        double photonEnergy = UnitConversions.WavelengthToPhotonEnergy(wavelength);
        double photonEnergyEV = UnitConversions.JoulesToElectronVolts(photonEnergy);
        
        Console.WriteLine($"Green light (λ = {wavelength*1e9:F0} nm): E = {photonEnergyEV:F2} eV");
        
        // Temperature to thermal energy
        double temperatureK = 300; // Room temperature
        double thermalEnergy = UnitConversions.TemperatureToThermalEnergy(temperatureK);
        double thermalEnergyEV = UnitConversions.JoulesToElectronVolts(thermalEnergy);
        
        Console.WriteLine($"Room temperature thermal energy: {thermalEnergyEV:F3} eV");
        Console.WriteLine();
    }

    private static void DemonstratePrecisionHandling()
    {
        Console.WriteLine("--- Precision Handling ---");
        
        // Demonstrate numerical stability checks
        double smallValue = 1e-15;
        bool isZero = PrecisionHandling.IsNumericallyZero(smallValue);
        Console.WriteLine($"Is {smallValue:E3} numerically zero? {isZero}");
        
        // Demonstrate safe operations
        double negativeValue = -1e-14;
        double safeSqrt = PrecisionHandling.SafeSqrt(negativeValue);
        Console.WriteLine($"Safe sqrt of {negativeValue:E3}: {safeSqrt:E3}");
        
        // Demonstrate high-precision summation
        var values = new double[] { 1.0, 1e-10, 1e-15, 1e-20 };
        double regularSum = values.Sum();
        double kahanSum = PrecisionHandling.KahanSum(values);
        
        Console.WriteLine($"Regular sum: {regularSum:G17}");
        Console.WriteLine($"Kahan sum: {kahanSum:G17}");
        Console.WriteLine($"Difference: {Math.Abs(kahanSum - regularSum):E3}");
        
        // Demonstrate quantum amplitude normalization
        var amplitudes = new Complex[]
        {
            new Complex(0.6, 0.1),
            new Complex(0.0, 0.7),
            new Complex(0.2, -0.1)
        };
        
        var normalized = PrecisionHandling.NormalizeQuantumAmplitudes(amplitudes);
        double normCheck = normalized.Sum(a => a.Real * a.Real + a.Imaginary * a.Imaginary);
        
        Console.WriteLine($"Normalized amplitudes sum |ψ|² = {normCheck:F10}");
        
        // Demonstrate numerical diagnostics
        var testValues = new double[] { 1e-10, 1.0, 1e10, double.Epsilon, 1e-5 };
        var diagnostics = PrecisionHandling.AnalyzeNumericalStability(testValues);
        
        Console.WriteLine($"Numerical health check: {diagnostics.IsNumericallyHealthy}");
        Console.WriteLine($"Dynamic range: {diagnostics.DynamicRange:F1} orders of magnitude");
        Console.WriteLine($"Estimated precision loss: {diagnostics.EstimatedPrecisionLoss:F1} digits");
        Console.WriteLine();
    }
}
