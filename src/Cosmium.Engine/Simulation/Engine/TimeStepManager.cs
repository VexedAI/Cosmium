using System;
using System.Collections.Generic;
using System.Linq;
using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Simulation.Core;

namespace Cosmium.Engine.Simulation.Engine
{
    /// <summary>
    /// Manages time evolution and adaptive time stepping for quantum mechanical simulations.
    /// Implements sophisticated algorithms for stable time evolution with error control.
    /// </summary>
    public class TimeStepManager : ITimeStepManager
    {
        private readonly object _lockObject = new object();
        
        private double _currentTimeStep;
        private double _minTimeStep;
        private double _maxTimeStep;
        private bool _isAdaptiveSteppingEnabled;
        private double _errorTolerance;
        private double _stabilityFactor;
        private bool _isCurrentStepStable;
        private long _totalAdjustments;
        private long _stepIncreases;
        private long _stepDecreases;
        private bool _isInitialized;
        
        // Statistics tracking
        private long _totalSteps;
        private double _averageTimeStep;
        private double _minTimeStepUsed;
        private double _maxTimeStepUsed;
        private long _rejectedSteps;
        private double _efficiency;
        private TimeSpan _timeStepCalculationTime;
        private readonly List<double> _timeStepHistory;
        
        /// <summary>
        /// Initializes a new instance of the TimeStepManager.
        /// </summary>
        public TimeStepManager()
        {
            _timeStepHistory = new List<double>();
            
            // Initialize with safe defaults
            _currentTimeStep = 1e-15; // 1 femtosecond default
            _minTimeStep = 1e-18; // 1 attosecond minimum
            _maxTimeStep = 1e-12; // 1 picosecond maximum
            _errorTolerance = 1e-6;
            _stabilityFactor = 1.0;
            _isAdaptiveSteppingEnabled = true;
            _isCurrentStepStable = true;
            _minTimeStepUsed = double.MaxValue;
            _maxTimeStepUsed = double.MinValue;
            _efficiency = 1.0;
        }

        #region Properties

        public double CurrentTimeStep
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentTimeStep;
                }
            }
        }

        public double MinTimeStep
        {
            get
            {
                lock (_lockObject)
                {
                    return _minTimeStep;
                }
            }
        }

        public double MaxTimeStep
        {
            get
            {
                lock (_lockObject)
                {
                    return _maxTimeStep;
                }
            }
        }

        public bool IsAdaptiveSteppingEnabled
        {
            get
            {
                lock (_lockObject)
                {
                    return _isAdaptiveSteppingEnabled;
                }
            }
        }

        public double ErrorTolerance
        {
            get
            {
                lock (_lockObject)
                {
                    return _errorTolerance;
                }
            }
        }

        public double StabilityFactor
        {
            get
            {
                lock (_lockObject)
                {
                    return _stabilityFactor;
                }
            }
        }

        public bool IsCurrentStepStable
        {
            get
            {
                lock (_lockObject)
                {
                    return _isCurrentStepStable;
                }
            }
        }

        public long TotalAdjustments
        {
            get
            {
                lock (_lockObject)
                {
                    return _totalAdjustments;
                }
            }
        }

        public long StepIncreases
        {
            get
            {
                lock (_lockObject)
                {
                    return _stepIncreases;
                }
            }
        }

        public long StepDecreases
        {
            get
            {
                lock (_lockObject)
                {
                    return _stepDecreases;
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<TimeStepAdjustedEventArgs>? TimeStepAdjusted;
        public event EventHandler<StabilityIssueDetectedEventArgs>? StabilityIssueDetected;

        #endregion

        #region Initialization

        public void Initialize(SimulationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            lock (_lockObject)
            {
                // Extract time step configuration from simulation parameters
                _currentTimeStep = parameters.TimeStep > 0 ? parameters.TimeStep : 1e-15;
                _minTimeStep = _currentTimeStep / 1000.0;
                _maxTimeStep = _currentTimeStep * 1000.0;
                _errorTolerance = 1e-6;
                _isAdaptiveSteppingEnabled = true;
                
                // Reset statistics
                ResetStatisticsInternal();
                
                _isInitialized = true;
            }
        }

        public void ConfigureAdaptiveStepping(bool enable, double errorTolerance, double minTimeStep, double maxTimeStep)
        {
            if (errorTolerance <= 0)
                throw new ArgumentException("Error tolerance must be positive", nameof(errorTolerance));
            if (minTimeStep <= 0)
                throw new ArgumentException("Minimum time step must be positive", nameof(minTimeStep));
            if (maxTimeStep <= minTimeStep)
                throw new ArgumentException("Maximum time step must be greater than minimum", nameof(maxTimeStep));

            lock (_lockObject)
            {
                _isAdaptiveSteppingEnabled = enable;
                _errorTolerance = errorTolerance;
                _minTimeStep = minTimeStep;
                _maxTimeStep = maxTimeStep;
                
                // Ensure current time step is within bounds
                _currentTimeStep = Math.Max(_minTimeStep, Math.Min(_maxTimeStep, _currentTimeStep));
            }
        }

        public void SetFixedTimeStep(double timeStep)
        {
            if (timeStep <= 0)
                throw new ArgumentException("Time step must be positive", nameof(timeStep));

            lock (_lockObject)
            {
                _currentTimeStep = timeStep;
                _isAdaptiveSteppingEnabled = false;
            }
        }

        #endregion

        #region Time Step Calculation

        public double CalculateOptimalTimeStep(IReadOnlyList<IQuantumParticle> particles, double currentTime)
        {
            if (particles == null)
                throw new ArgumentNullException(nameof(particles));

            var startTime = DateTime.UtcNow;
            
            try
            {
                lock (_lockObject)
                {
                    if (!_isAdaptiveSteppingEnabled)
                        return _currentTimeStep;

                    // Calculate based on stability criteria
                    double stableTimeStep = CalculateStableTimeStepInternal(particles);
                    
                    // Calculate based on CFL condition
                    double cflTimeStep = CalculateCFLTimeStepInternal(particles);
                    
                    // Take the minimum for safety
                    double optimalTimeStep = Math.Min(stableTimeStep, cflTimeStep);
                    
                    // Ensure within bounds
                    optimalTimeStep = Math.Max(_minTimeStep, Math.Min(_maxTimeStep, optimalTimeStep));
                    
                    return optimalTimeStep;
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    _timeStepCalculationTime = _timeStepCalculationTime.Add(DateTime.UtcNow - startTime);
                }
            }
        }

        public double CalculateStableTimeStep(IReadOnlyList<IQuantumParticle> particles, double maxEigenvalue)
        {
            if (particles == null)
                throw new ArgumentNullException(nameof(particles));

            // For quantum systems, stability requires dt < 2/|max eigenvalue|
            if (maxEigenvalue > 0)
            {
                return Math.Min(_maxTimeStep, 2.0 / maxEigenvalue);
            }
            
            return _currentTimeStep;
        }

        public double CalculateCFLTimeStep(double maxVelocity, double minSpatialStep)
        {
            if (maxVelocity <= 0 || minSpatialStep <= 0)
                return _currentTimeStep;

            // CFL condition: dt <= C * dx / v_max, where C is the CFL number (typically 0.5)
            const double cflNumber = 0.5;
            return cflNumber * minSpatialStep / maxVelocity;
        }

        public double EstimateLocalError(IReadOnlyList<IQuantumParticle> particles, double proposedTimeStep)
        {
            if (particles == null)
                throw new ArgumentNullException(nameof(particles));

            // Simplified local error estimation
            // In a full implementation, this would involve comparing solutions
            // of different orders or using embedded methods
            
            double currentError = 0.0;
            foreach (var particle in particles)
            {
                // Estimate error based on particle energy and time step
                var energy = particle.Energy.Value;
                var timeStepError = Math.Abs(energy * proposedTimeStep * proposedTimeStep);
                currentError = Math.Max(currentError, timeStepError);
            }
            
            return currentError;
        }

        #endregion

        #region Time Step Adaptation

        public bool UpdateTimeStep(IReadOnlyList<IQuantumParticle> particles, double localError, double currentTime)
        {
            if (particles == null)
                throw new ArgumentNullException(nameof(particles));

            if (!_isAdaptiveSteppingEnabled)
                return false;

            lock (_lockObject)
            {
                double suggestedTimeStep = SuggestNewTimeStep(_currentTimeStep, localError, _errorTolerance);
                
                if (Math.Abs(suggestedTimeStep - _currentTimeStep) / _currentTimeStep > 0.1) // 10% change threshold
                {
                    double previousTimeStep = _currentTimeStep;
                    _currentTimeStep = suggestedTimeStep;
                    _totalAdjustments++;
                    
                    if (suggestedTimeStep > previousTimeStep)
                    {
                        _stepIncreases++;
                    }
                    else
                    {
                        _stepDecreases++;
                    }
                    
                    // Update statistics
                    _timeStepHistory.Add(_currentTimeStep);
                    UpdateStatisticsInternal();
                    
                    // Raise event
                    var eventArgs = new TimeStepAdjustedEventArgs(
                        previousTimeStep, 
                        _currentTimeStep, 
                        "Adaptive step adjustment", 
                        localError, 
                        currentTime);
                    
                    TimeStepAdjusted?.Invoke(this, eventArgs);
                    
                    return true;
                }
                
                return false;
            }
        }

        public double SuggestNewTimeStep(double currentTimeStep, double localError, double targetError)
        {
            if (currentTimeStep <= 0 || targetError <= 0)
                return currentTimeStep;

            // Safety factor to prevent oscillation
            const double safetyFactor = 0.9;
            
            // Error ratio determines scaling
            double errorRatio = targetError / Math.Max(localError, 1e-15);
            
            // Use conservative scaling for stability
            double scalingFactor = safetyFactor * Math.Pow(errorRatio, 0.2);
            
            // Limit the change to prevent dramatic adjustments
            scalingFactor = Math.Max(0.5, Math.Min(2.0, scalingFactor));
            
            return currentTimeStep * scalingFactor;
        }

        public bool IncreaseTimeStep(double factor = 1.5)
        {
            if (factor <= 1.0)
                return false;

            lock (_lockObject)
            {
                double newTimeStep = _currentTimeStep * factor;
                if (newTimeStep <= _maxTimeStep)
                {
                    _currentTimeStep = newTimeStep;
                    _stepIncreases++;
                    _totalAdjustments++;
                    return true;
                }
                return false;
            }
        }

        public bool DecreaseTimeStep(double factor = 0.5)
        {
            if (factor >= 1.0)
                return false;

            lock (_lockObject)
            {
                double newTimeStep = _currentTimeStep * factor;
                if (newTimeStep >= _minTimeStep)
                {
                    _currentTimeStep = newTimeStep;
                    _stepDecreases++;
                    _totalAdjustments++;
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region Stability Analysis

        public TimeStepStabilityResult AnalyzeStability(IReadOnlyList<IQuantumParticle> particles)
        {
            if (particles == null)
                throw new ArgumentNullException(nameof(particles));

            // Calculate maximum eigenvalue (simplified)
            double maxEigenvalue = CalculateMaxEigenvalue(particles);
            
            // Calculate stability factor
            double stabilityLimit = 2.0 / maxEigenvalue;
            double currentStabilityFactor = stabilityLimit / _currentTimeStep;
            
            // Calculate CFL number
            double maxVelocity = CalculateMaxVelocity(particles);
            double minSpatialStep = 1e-10; // Simplified spatial discretization
            double cflNumber = _currentTimeStep * maxVelocity / minSpatialStep;
            
            bool isStable = currentStabilityFactor > 1.0 && cflNumber < 1.0;
            
            var warnings = new List<string>();
            double recommendedTimeStep = _currentTimeStep;
            
            if (!isStable)
            {
                warnings.Add("Current time step may cause numerical instability");
                recommendedTimeStep = Math.Min(stabilityLimit * 0.9, CalculateCFLTimeStep(maxVelocity, minSpatialStep));
            }
            
            lock (_lockObject)
            {
                _isCurrentStepStable = isStable;
                _stabilityFactor = currentStabilityFactor;
            }
            
            return new TimeStepStabilityResult
            {
                IsStable = isStable,
                StabilityFactor = currentStabilityFactor,
                MaxEigenvalue = maxEigenvalue,
                CFLNumber = cflNumber,
                Warnings = warnings.AsReadOnly(),
                RecommendedTimeStep = recommendedTimeStep
            };
        }

        public bool IsTimeStepStable(IReadOnlyList<IQuantumParticle> particles, double proposedTimeStep)
        {
            if (particles == null)
                throw new ArgumentNullException(nameof(particles));

            double maxEigenvalue = CalculateMaxEigenvalue(particles);
            double stabilityLimit = 2.0 / maxEigenvalue;
            
            return proposedTimeStep < stabilityLimit;
        }

        public double GetMaxStableTimeStep(IReadOnlyList<IQuantumParticle> particles)
        {
            if (particles == null)
                throw new ArgumentNullException(nameof(particles));

            double maxEigenvalue = CalculateMaxEigenvalue(particles);
            double stabilityLimit = 2.0 / maxEigenvalue;
            
            return Math.Min(_maxTimeStep, stabilityLimit * 0.9); // 90% of theoretical limit for safety
        }

        #endregion

        #region Validation

        public ValidationResult ValidateConfiguration()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            
            if (_minTimeStep <= 0)
                errors.Add("Minimum time step must be positive");
            
            if (_maxTimeStep <= _minTimeStep)
                errors.Add("Maximum time step must be greater than minimum time step");
            
            if (_errorTolerance <= 0)
                errors.Add("Error tolerance must be positive");
            
            if (_currentTimeStep < _minTimeStep || _currentTimeStep > _maxTimeStep)
                errors.Add("Current time step is outside allowed bounds");
            
            if (_errorTolerance > 1e-3)
                warnings.Add("Error tolerance may be too large for accurate results");
            
            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        public ValidationResult ValidateTimeStep(double timeStep)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            
            if (timeStep <= 0)
                errors.Add("Time step must be positive");
            
            if (timeStep < _minTimeStep)
                errors.Add($"Time step {timeStep:E6} is below minimum {_minTimeStep:E6}");
            
            if (timeStep > _maxTimeStep)
                errors.Add($"Time step {timeStep:E6} exceeds maximum {_maxTimeStep:E6}");
            
            if (timeStep > _maxTimeStep * 0.9)
                warnings.Add("Time step is close to maximum limit");
            
            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        #endregion

        #region Statistics and Diagnostics

        public TimeStepStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                return new TimeStepStatistics
                {
                    TotalSteps = _totalSteps,
                    AverageTimeStep = _averageTimeStep,
                    MinTimeStepUsed = _minTimeStepUsed != double.MaxValue ? _minTimeStepUsed : 0.0,
                    MaxTimeStepUsed = _maxTimeStepUsed != double.MinValue ? _maxTimeStepUsed : 0.0,
                    TotalAdjustments = _totalAdjustments,
                    Increases = _stepIncreases,
                    Decreases = _stepDecreases,
                    RejectedSteps = _rejectedSteps,
                    Efficiency = _efficiency,
                    TimeStepCalculationTime = _timeStepCalculationTime
                };
            }
        }

        public void ResetStatistics()
        {
            lock (_lockObject)
            {
                ResetStatisticsInternal();
            }
        }

        public Dictionary<string, object> GetDiagnostics()
        {
            lock (_lockObject)
            {
                return new Dictionary<string, object>
                {
                    { "CurrentTimeStep", _currentTimeStep },
                    { "IsAdaptive", _isAdaptiveSteppingEnabled },
                    { "IsStable", _isCurrentStepStable },
                    { "StabilityFactor", _stabilityFactor },
                    { "ErrorTolerance", _errorTolerance },
                    { "MinTimeStep", _minTimeStep },
                    { "MaxTimeStep", _maxTimeStep },
                    { "TotalAdjustments", _totalAdjustments },
                    { "SuccessRate", _efficiency },
                    { "IsInitialized", _isInitialized }
                };
            }
        }

        #endregion

        #region Private Methods

        private double CalculateStableTimeStepInternal(IReadOnlyList<IQuantumParticle> particles)
        {
            double maxEigenvalue = CalculateMaxEigenvalue(particles);
            return maxEigenvalue > 0 ? Math.Min(_maxTimeStep, 2.0 / maxEigenvalue * 0.9) : _currentTimeStep;
        }

        private double CalculateCFLTimeStepInternal(IReadOnlyList<IQuantumParticle> particles)
        {
            double maxVelocity = CalculateMaxVelocity(particles);
            double minSpatialStep = 1e-10; // Simplified spatial discretization
            return CalculateCFLTimeStep(maxVelocity, minSpatialStep);
        }

        private double CalculateMaxEigenvalue(IReadOnlyList<IQuantumParticle> particles)
        {
            // Simplified calculation - in practice, this would involve
            // calculating the eigenvalues of the system Hamiltonian
            double maxEigenvalue = 0.0;
            
            foreach (var particle in particles)
            {
                // Use energy as a proxy for eigenvalue
                double eigenvalue = Math.Abs(particle.Energy.Value) / (1.054571817e-34); // ħ
                maxEigenvalue = Math.Max(maxEigenvalue, eigenvalue);
            }
            
            return maxEigenvalue > 0 ? maxEigenvalue : 1e12; // Default fallback
        }

        private double CalculateMaxVelocity(IReadOnlyList<IQuantumParticle> particles)
        {
            double maxVelocity = 0.0;
            
            foreach (var particle in particles)
            {
                // Use momentum to estimate velocity
                if (particle.Mass.Value > 0)
                {
                    var momentum = particle.Momentum;
                    double velocity = momentum.Magnitude / particle.Mass.Value;
                    maxVelocity = Math.Max(maxVelocity, velocity);
                }
            }
            
            return maxVelocity > 0 ? maxVelocity : 299792458.0; // Speed of light as fallback
        }

        private void UpdateStatisticsInternal()
        {
            _totalSteps++;
            
            if (_timeStepHistory.Count > 0)
            {
                _averageTimeStep = _timeStepHistory.Average();
                _minTimeStepUsed = Math.Min(_minTimeStepUsed, _timeStepHistory.Last());
                _maxTimeStepUsed = Math.Max(_maxTimeStepUsed, _timeStepHistory.Last());
            }
            
            if (_totalSteps > 0)
            {
                _efficiency = 1.0 - (double)_rejectedSteps / _totalSteps;
            }
        }

        private void ResetStatisticsInternal()
        {
            _totalSteps = 0;
            _totalAdjustments = 0;
            _stepIncreases = 0;
            _stepDecreases = 0;
            _rejectedSteps = 0;
            _averageTimeStep = 0.0;
            _minTimeStepUsed = double.MaxValue;
            _maxTimeStepUsed = double.MinValue;
            _efficiency = 1.0;
            _timeStepCalculationTime = TimeSpan.Zero;
            _timeStepHistory.Clear();
        }

        #endregion
    }
}
