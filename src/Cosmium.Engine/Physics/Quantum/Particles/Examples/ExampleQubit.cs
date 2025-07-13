using System.Numerics;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using static Cosmium.Engine.Physics.Quantum.Particles.Abstract.ParticleStatistics;

namespace Cosmium.Engine.Physics.Quantum.Particles.Examples;

/// <summary>
/// Example implementation of a simple quantum particle for demonstration purposes.
/// This shows how to extend QuantumParticleBase to create specific particle types.
/// </summary>
public class ExampleQubit : QuantumParticleBase
{
    private readonly ScalarMeasurableProperty _mass;
    private readonly ScalarMeasurableProperty _charge;

    /// <summary>
    /// Initializes a new qubit (2-level quantum system).
    /// </summary>
    public ExampleQubit() : base("Qubit", "q", isElementary: true, hilbertSpaceDimension: 2)
    {
        // Initialize as massless, neutral particle
        _mass = new ScalarMeasurableProperty("Mass", "kg", 0.0);
        _charge = new ScalarMeasurableProperty("Charge", "C", 0.0);
    }

    #region Abstract Implementation

    public override IScalarMeasurable Mass => _mass;
    public override IScalarMeasurable Charge => _charge;
    public override double SpinQuantumNumber => 0.5; // Spin-1/2 particle
    public override ParticleStatistics Statistics => ParticleStatistics.FermiDirac;

    #endregion

    #region Qubit-Specific Operations

    /// <summary>
    /// Sets the qubit to the |0⟩ state.
    /// </summary>
    public void SetGroundState()
    {
        StateVector = new Complex[] { new(1.0, 0.0), new(0.0, 0.0) };
    }

    /// <summary>
    /// Sets the qubit to the |1⟩ state.
    /// </summary>
    public void SetExcitedState()
    {
        StateVector = new Complex[] { new(0.0, 0.0), new(1.0, 0.0) };
    }

    /// <summary>
    /// Sets the qubit to a superposition state α|0⟩ + β|1⟩.
    /// </summary>
    /// <param name="alpha">Amplitude for |0⟩ state.</param>
    /// <param name="beta">Amplitude for |1⟩ state.</param>
    public void SetSuperposition(Complex alpha, Complex beta)
    {
        // Normalize the amplitudes
        var norm = Math.Sqrt(alpha.Real * alpha.Real + alpha.Imaginary * alpha.Imaginary +
                            beta.Real * beta.Real + beta.Imaginary * beta.Imaginary);
        
        StateVector = new Complex[] { alpha / norm, beta / norm };
    }

    /// <summary>
    /// Applies a Pauli-X (NOT) gate to the qubit.
    /// </summary>
    public void ApplyPauliX()
    {
        var pauliX = new Complex[,]
        {
            { new(0.0, 0.0), new(1.0, 0.0) },
            { new(1.0, 0.0), new(0.0, 0.0) }
        };
        
        ApplyUnitaryOperation(pauliX);
    }

    /// <summary>
    /// Applies a Pauli-Y gate to the qubit.
    /// </summary>
    public void ApplyPauliY()
    {
        var pauliY = new Complex[,]
        {
            { new(0.0, 0.0), new(0.0, -1.0) },
            { new(0.0, 1.0), new(0.0, 0.0) }
        };
        
        ApplyUnitaryOperation(pauliY);
    }

    /// <summary>
    /// Applies a Pauli-Z gate to the qubit.
    /// </summary>
    public void ApplyPauliZ()
    {
        var pauliZ = new Complex[,]
        {
            { new(1.0, 0.0), new(0.0, 0.0) },
            { new(0.0, 0.0), new(-1.0, 0.0) }
        };
        
        ApplyUnitaryOperation(pauliZ);
    }

    /// <summary>
    /// Applies a Hadamard gate to create superposition.
    /// </summary>
    public void ApplyHadamard()
    {
        var sqrt2Inv = 1.0 / Math.Sqrt(2);
        var hadamard = new Complex[,]
        {
            { new(sqrt2Inv, 0.0), new(sqrt2Inv, 0.0) },
            { new(sqrt2Inv, 0.0), new(-sqrt2Inv, 0.0) }
        };
        
        ApplyUnitaryOperation(hadamard);
    }

    /// <summary>
    /// Measures the qubit in the computational basis {|0⟩, |1⟩}.
    /// </summary>
    /// <returns>0 or 1 corresponding to |0⟩ or |1⟩.</returns>
    public int MeasureComputationalBasis()
    {
        var measurementOperator = new Complex[,]
        {
            { new(0.0, 0.0), new(0.0, 0.0) },
            { new(0.0, 0.0), new(1.0, 0.0) }
        };
        
        var result = MeasureObservable(measurementOperator);
        return result > 0.5 ? 1 : 0;
    }

    /// <summary>
    /// Gets the probability of measuring |0⟩.
    /// </summary>
    public double GetZeroProbability()
    {
        var state = StateVector;
        return state[0].Real * state[0].Real + state[0].Imaginary * state[0].Imaginary;
    }

    /// <summary>
    /// Gets the probability of measuring |1⟩.
    /// </summary>
    public double GetOneProbability()
    {
        var state = StateVector;
        return state[1].Real * state[1].Real + state[1].Imaginary * state[1].Imaginary;
    }

    #endregion

    #region Private Helpers

    private void ApplyUnitaryOperation(Complex[,] unitary)
    {
        var currentState = StateVector;
        var newState = new Complex[2];
        
        for (int i = 0; i < 2; i++)
        {
            newState[i] = Complex.Zero;
            for (int j = 0; j < 2; j++)
            {
                newState[i] += unitary[i, j] * currentState[j];
            }
        }
        
        StateVector = newState;
    }

    #endregion

    #region Clone Implementation

    public override IQuantumParticle Clone()
    {
        var clone = new ExampleQubit();
        clone.StateVector = StateVector; // This will trigger validation and events
        return clone;
    }

    #endregion
}
