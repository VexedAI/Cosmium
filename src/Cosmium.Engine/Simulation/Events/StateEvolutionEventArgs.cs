using System;
using Cosmium.Engine.Physics.Mathematics;
using Cosmium.Engine.Physics.Quantum.States;

namespace Cosmium.Engine.Simulation.Events
{
    /// <summary>
    /// Event arguments for state evolution events in quantum simulations.
    /// Provides information about quantum state changes, evolution algorithms, and measurements.
    /// </summary>
    public class StateEvolutionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the unique identifier of the simulation that generated this event.
        /// </summary>
        public Guid SimulationId { get; }

        /// <summary>
        /// Gets the current simulation time when the event occurred.
        /// </summary>
        public double SimulationTime { get; }

        /// <summary>
        /// Gets the current simulation step number.
        /// </summary>
        public long SimulationStep { get; }

        /// <summary>
        /// Gets the quantum state before evolution.
        /// </summary>
        public QuantumState PreviousState { get; }

        /// <summary>
        /// Gets the quantum state after evolution.
        /// </summary>
        public QuantumState CurrentState { get; }

        /// <summary>
        /// Gets the time step used for the evolution.
        /// </summary>
        public double TimeStep { get; }

        /// <summary>
        /// Gets the evolution algorithm used.
        /// </summary>
        public string EvolutionAlgorithm { get; }

        /// <summary>
        /// Gets the Hamiltonian matrix used for evolution.
        /// </summary>
        public Matrix Hamiltonian { get; }

        /// <summary>
        /// Gets the evolution error estimate, if available.
        /// </summary>
        public double? EvolutionError { get; }

        /// <summary>
        /// Gets whether the evolution preserved unitarity.
        /// </summary>
        public bool IsUnitary { get; }

        /// <summary>
        /// Gets whether the evolution preserved normalization.
        /// </summary>
        public bool IsNormalized { get; }

        /// <summary>
        /// Gets the change in state norm during evolution.
        /// </summary>
        public double NormChange { get; }

        /// <summary>
        /// Gets the overlap between previous and current states.
        /// </summary>
        public Complex StateOverlap { get; }

        /// <summary>
        /// Gets whether a measurement was performed during this evolution step.
        /// </summary>
        public bool MeasurementPerformed { get; }

        /// <summary>
        /// Gets the measurement result, if a measurement was performed.
        /// </summary>
        public double? MeasurementResult { get; }

        /// <summary>
        /// Gets the observable that was measured, if applicable.
        /// </summary>
        public string? MeasuredObservable { get; }

        /// <summary>
        /// Gets whether the state collapsed due to measurement.
        /// </summary>
        public bool StateCollapsed { get; }

        /// <summary>
        /// Gets additional diagnostic information about the evolution.
        /// </summary>
        public string? DiagnosticInfo { get; }

        /// <summary>
        /// Gets the timestamp when the event was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Initializes a new instance of the StateEvolutionEventArgs class.
        /// </summary>
        /// <param name="simulationId">The unique identifier of the simulation.</param>
        /// <param name="simulationTime">The current simulation time.</param>
        /// <param name="simulationStep">The current simulation step number.</param>
        /// <param name="previousState">The quantum state before evolution.</param>
        /// <param name="currentState">The quantum state after evolution.</param>
        /// <param name="timeStep">The time step used for evolution.</param>
        /// <param name="evolutionAlgorithm">The evolution algorithm used.</param>
        /// <param name="hamiltonian">The Hamiltonian matrix used.</param>
        /// <param name="evolutionError">The evolution error estimate, if available.</param>
        /// <param name="isUnitary">Whether the evolution preserved unitarity.</param>
        /// <param name="isNormalized">Whether the evolution preserved normalization.</param>
        /// <param name="measurementPerformed">Whether a measurement was performed.</param>
        /// <param name="measurementResult">The measurement result, if applicable.</param>
        /// <param name="measuredObservable">The observable that was measured, if applicable.</param>
        /// <param name="stateCollapsed">Whether the state collapsed due to measurement.</param>
        /// <param name="diagnosticInfo">Additional diagnostic information.</param>
        public StateEvolutionEventArgs(
            Guid simulationId,
            double simulationTime,
            long simulationStep,
            QuantumState previousState,
            QuantumState currentState,
            double timeStep,
            string evolutionAlgorithm,
            Matrix hamiltonian,
            double? evolutionError = null,
            bool isUnitary = true,
            bool isNormalized = true,
            bool measurementPerformed = false,
            double? measurementResult = null,
            string? measuredObservable = null,
            bool stateCollapsed = false,
            string? diagnosticInfo = null)
        {
            SimulationId = simulationId;
            SimulationTime = simulationTime;
            SimulationStep = simulationStep;
            PreviousState = previousState ?? throw new ArgumentNullException(nameof(previousState));
            CurrentState = currentState ?? throw new ArgumentNullException(nameof(currentState));
            TimeStep = timeStep;
            EvolutionAlgorithm = evolutionAlgorithm ?? throw new ArgumentNullException(nameof(evolutionAlgorithm));
            Hamiltonian = hamiltonian ?? throw new ArgumentNullException(nameof(hamiltonian));
            EvolutionError = evolutionError;
            IsUnitary = isUnitary;
            IsNormalized = isNormalized;
            MeasurementPerformed = measurementPerformed;
            MeasurementResult = measurementResult;
            MeasuredObservable = measuredObservable;
            StateCollapsed = stateCollapsed;
            DiagnosticInfo = diagnosticInfo;
            Timestamp = DateTime.UtcNow;

            // Calculate derived properties
            NormChange = Math.Abs(currentState.Norm - previousState.Norm);
            StateOverlap = previousState.InnerProduct(currentState);
        }

        /// <summary>
        /// Creates a StateEvolutionEventArgs for a simple state evolution without measurement.
        /// </summary>
        /// <param name="simulationId">The simulation identifier.</param>
        /// <param name="simulationTime">The current simulation time.</param>
        /// <param name="simulationStep">The current step number.</param>
        /// <param name="previousState">The previous quantum state.</param>
        /// <param name="currentState">The current quantum state.</param>
        /// <param name="timeStep">The evolution time step.</param>
        /// <param name="algorithm">The evolution algorithm.</param>
        /// <param name="hamiltonian">The Hamiltonian matrix.</param>
        /// <returns>A new StateEvolutionEventArgs instance.</returns>
        public static StateEvolutionEventArgs CreateEvolutionEvent(
            Guid simulationId,
            double simulationTime,
            long simulationStep,
            QuantumState previousState,
            QuantumState currentState,
            double timeStep,
            string algorithm,
            Matrix hamiltonian)
        {
            return new StateEvolutionEventArgs(
                simulationId, simulationTime, simulationStep,
                previousState, currentState, timeStep, algorithm, hamiltonian);
        }

        /// <summary>
        /// Creates a StateEvolutionEventArgs for a measurement event.
        /// </summary>
        /// <param name="simulationId">The simulation identifier.</param>
        /// <param name="simulationTime">The current simulation time.</param>
        /// <param name="simulationStep">The current step number.</param>
        /// <param name="previousState">The state before measurement.</param>
        /// <param name="currentState">The state after measurement (collapsed).</param>
        /// <param name="measurementResult">The measurement result.</param>
        /// <param name="observable">The measured observable.</param>
        /// <param name="hamiltonian">The Hamiltonian matrix.</param>
        /// <returns>A new StateEvolutionEventArgs instance for measurement.</returns>
        public static StateEvolutionEventArgs CreateMeasurementEvent(
            Guid simulationId,
            double simulationTime,
            long simulationStep,
            QuantumState previousState,
            QuantumState currentState,
            double measurementResult,
            string observable,
            Matrix hamiltonian)
        {
            return new StateEvolutionEventArgs(
                simulationId, simulationTime, simulationStep,
                previousState, currentState, 0.0, "Measurement", hamiltonian,
                measurementPerformed: true,
                measurementResult: measurementResult,
                measuredObservable: observable,
                stateCollapsed: true);
        }

        /// <summary>
        /// Returns a string representation of the state evolution event.
        /// </summary>
        /// <returns>A formatted string describing the event.</returns>
        public override string ToString()
        {
            var info = $"StateEvolution[Step={SimulationStep}, Time={SimulationTime:E3}s, Algorithm={EvolutionAlgorithm}";
            
            if (MeasurementPerformed)
            {
                info += $", Measurement={MeasuredObservable}={MeasurementResult}";
            }
            
            if (EvolutionError.HasValue)
            {
                info += $", Error={EvolutionError:E3}";
            }
            
            info += $", Unitary={IsUnitary}, Normalized={IsNormalized}]";
            
            return info;
        }

        /// <summary>
        /// Gets detailed information about the state evolution for debugging purposes.
        /// </summary>
        /// <returns>A detailed string representation.</returns>
        public string GetDetailedInfo()
        {
            var details = $"State Evolution Event Details:\n";
            details += $"  Simulation ID: {SimulationId}\n";
            details += $"  Time: {SimulationTime:E6} s (Step {SimulationStep})\n";
            details += $"  Algorithm: {EvolutionAlgorithm}\n";
            details += $"  Time Step: {TimeStep:E6} s\n";
            details += $"  Previous State Norm: {PreviousState.Norm:F6}\n";
            details += $"  Current State Norm: {CurrentState.Norm:F6}\n";
            details += $"  Norm Change: {NormChange:E6}\n";
            details += $"  State Overlap: |⟨ψ_prev|ψ_curr⟩| = {StateOverlap.Magnitude:F6}\n";
            details += $"  Unitarity Preserved: {IsUnitary}\n";
            details += $"  Normalization Preserved: {IsNormalized}\n";
            
            if (EvolutionError.HasValue)
            {
                details += $"  Evolution Error: {EvolutionError:E6}\n";
            }
            
            if (MeasurementPerformed)
            {
                details += $"  Measurement Performed: {MeasuredObservable}\n";
                details += $"  Measurement Result: {MeasurementResult}\n";
                details += $"  State Collapsed: {StateCollapsed}\n";
            }
            
            if (!string.IsNullOrEmpty(DiagnosticInfo))
            {
                details += $"  Diagnostics: {DiagnosticInfo}\n";
            }
            
            details += $"  Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC";
            
            return details;
        }
    }
}
