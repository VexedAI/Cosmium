using Cosmium.Engine.Physics.Mathematics;

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
}
