using Cosmium.Engine.Infrastructure.Validation;
using Cosmium.Engine.Physics.Quantum.Particles.Abstract;
using Cosmium.Engine.Simulation.Core;
using Cosmium.Engine.Simulation.Events;

namespace Cosmium.Engine.Simulation.Engine;

/// <summary>
/// Main simulation orchestrator that coordinates time evolution, state management, and particle interactions.
/// Implements the primary simulation loop and manages all simulation components.
/// </summary>
public class SimulationEngine : ISimulationEngine, IDisposable
{
    private readonly IParameterValidator _parameterValidator;
    private readonly ITimeStepManager _timeStepManager;
    private readonly IStateEvolutionEngine _stateEvolutionEngine;
    private readonly object _lockObject = new();

    private Guid _id;
    private string _name;
    private string _version;
    private SimulationEngineStatus _status;
    private SimulationParameters? _parameters;
    private SimulationContext? _context;
    private ISimulation? _currentSimulation;
    private CancellationTokenSource? _cancellationTokenSource;
    
    // Timing
    private DateTime _creationTime;
    private DateTime? _startTime;
    private double _currentSimulationTime;
    private long _currentStepNumber;
    private double _currentTimeStep;
    private bool _useCustomTimeStep;
    private double _customTimeStep;
    
    // Simulation state
    private readonly List<IQuantumParticle> _particles;
    private readonly Dictionary<Guid, object> _activeComponents;
    
    // Performance tracking
    private DateTime _simulationStartTime;
    private TimeSpan _totalExecutionTime;
    private readonly SimulationEngineMetrics _metrics;
    private SimulationPerformanceMetrics _performanceMetrics;
    
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the SimulationEngine.
    /// </summary>
    public SimulationEngine(
        IParameterValidator parameterValidator,
        ITimeStepManager timeStepManager,
        IStateEvolutionEngine stateEvolutionEngine)
    {
        _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        _timeStepManager = timeStepManager ?? throw new ArgumentNullException(nameof(timeStepManager));
        _stateEvolutionEngine = stateEvolutionEngine ?? throw new ArgumentNullException(nameof(stateEvolutionEngine));

        _id = Guid.NewGuid();
        _name = "Quantum Simulation Engine";
        _version = "1.0.0";
        _status = SimulationEngineStatus.Created;
        _creationTime = DateTime.UtcNow;
        _particles = new List<IQuantumParticle>();
        _activeComponents = new Dictionary<Guid, object>();
        
        _metrics = new SimulationEngineMetrics
        {
            TotalSteps = 0,
            AverageStepTimeMs = 0.0,
            TotalExecutionTimeMs = 0.0,
            ParticleCount = 0,
            ComponentCount = 0,
            MemoryUsageMB = 0.0,
            CpuUsagePercent = 0.0,
            ThroughputStepsPerSecond = 0.0,
            EfficiencyRating = 1.0
        };

        _performanceMetrics = new SimulationPerformanceMetrics
        {
            StepsPerSecond = 0.0,
            AverageStepTimeMs = 0.0,
            MemoryUsageMB = 0.0,
            CpuUsagePercent = 0.0,
            ActiveThreads = 1,
            GarbageCollections = 0,
            TotalExecutionTime = TimeSpan.Zero,
            Efficiency = 1.0
        };
    }

    #region Basic Properties

    public Guid Id
    {
        get
        {
            lock (_lockObject)
            {
                return _id;
            }
        }
    }

    public string Name
    {
        get
        {
            lock (_lockObject)
            {
                return _name;
            }
        }
    }

    public string Version
    {
        get
        {
            lock (_lockObject)
            {
                return _version;
            }
        }
    }

    public SimulationEngineStatus Status
    {
        get
        {
            lock (_lockObject)
            {
                return _status;
            }
        }
    }

    public ISimulation? CurrentSimulation
    {
        get
        {
            lock (_lockObject)
            {
                return _currentSimulation;
            }
        }
    }

    public DateTime CreationTime
    {
        get
        {
            lock (_lockObject)
            {
                return _creationTime;
            }
        }
    }

    public DateTime? StartTime
    {
        get
        {
            lock (_lockObject)
            {
                return _startTime;
            }
        }
    }

    #endregion

    #region Configuration and Capabilities

    public IReadOnlyList<string> SupportedSimulationTypes { get; } = new[]
    {
        "Atomic",
        "Molecular",
        "ParticleCollision",
        "QuantumField",
        "Generic"
    };

    public bool SupportsParallelExecution => true;

    public bool SupportsAdaptiveTimeStep => true;

    public bool SupportsRealTimeMonitoring => true;

    public int MaxParticleCount => 100000;

    public int RecommendedMemoryLimitMB => 8192;

    #endregion

    #region Engine Lifecycle

    public async Task InitializeAsync(SimulationParameters parameters)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        var validationResult = await _parameterValidator.ValidateAsync(parameters);
        if (!validationResult.IsValid)
            throw new ArgumentException($"Invalid simulation parameters: {validationResult.FirstError}");

        var previousStatus = _status;
        
        try
        {
            // Initialize components
            _timeStepManager.Initialize(parameters);
            await _stateEvolutionEngine.InitializeAsync(parameters);

            lock (_lockObject)
            {
                _parameters = parameters;
                _context = new SimulationContext();
                _currentSimulationTime = 0.0;
                _currentStepNumber = 0;
                _cancellationTokenSource = new CancellationTokenSource();
            }

            await SetStatusAsync(SimulationEngineStatus.Initialized);
        }
        catch (Exception ex)
        {
            await SetStatusAsync(SimulationEngineStatus.Error);
            throw new InvalidOperationException($"Failed to initialize simulation engine: {ex.Message}", ex);
        }
    }

    public async Task StartSimulationAsync(ISimulation simulation)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        if (simulation == null)
            throw new ArgumentNullException(nameof(simulation));

        if (_status != SimulationEngineStatus.Initialized && _status != SimulationEngineStatus.Idle)
            throw new InvalidOperationException($"Cannot start simulation in status: {_status}");

        lock (_lockObject)
        {
            _currentSimulation = simulation;
            _startTime = DateTime.UtcNow;
        }

        await SetStatusAsync(SimulationEngineStatus.Running);
        
        // Start the simulation execution
        _ = Task.Run(async () =>
        {
            try
            {
                await simulation.StartAsync();
                await SetStatusAsync(SimulationEngineStatus.Idle);
            }
            catch (OperationCanceledException)
            {
                await SetStatusAsync(SimulationEngineStatus.Paused);
            }
            catch (Exception ex)
            {
                await SetStatusAsync(SimulationEngineStatus.Error);
                await RaiseErrorEventAsync(ex);
            }
        });
    }

    public async Task PauseSimulationAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        if (_status == SimulationEngineStatus.Running)
        {
            _cancellationTokenSource?.Cancel();
            await SetStatusAsync(SimulationEngineStatus.Paused);
        }
    }

    public async Task ResumeSimulationAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        if (_status == SimulationEngineStatus.Paused && _currentSimulation != null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await SetStatusAsync(SimulationEngineStatus.Running);
            
            // Resume simulation execution
            _ = Task.Run(async () =>
            {
                try
                {
                await _currentSimulation.StartAsync();
                    await SetStatusAsync(SimulationEngineStatus.Idle);
                }
                catch (OperationCanceledException)
                {
                    await SetStatusAsync(SimulationEngineStatus.Paused);
                }
                catch (Exception ex)
                {
                    await SetStatusAsync(SimulationEngineStatus.Error);
                    await RaiseErrorEventAsync(ex);
                }
            });
        }
    }

    public async Task StopSimulationAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        await SetStatusAsync(SimulationEngineStatus.Stopping);
        _cancellationTokenSource?.Cancel();
        
        lock (_lockObject)
        {
            _currentSimulation = null;
        }
        
        await SetStatusAsync(SimulationEngineStatus.Idle);
    }

    public async Task ResetAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        lock (_lockObject)
        {
            _currentSimulationTime = 0.0;
            _currentStepNumber = 0;
            _particles.Clear();
            _activeComponents.Clear();
            _currentSimulation = null;
            _startTime = null;
        }

        await SetStatusAsync(SimulationEngineStatus.Initialized);
    }

    public async Task ShutdownAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        await SetStatusAsync(SimulationEngineStatus.ShuttingDown);
        _cancellationTokenSource?.Cancel();
        
        lock (_lockObject)
        {
            _currentSimulation = null;
        }
        
        await SetStatusAsync(SimulationEngineStatus.Shutdown);
    }

    #endregion

    #region Execution Control

    public async Task ExecuteStepAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimulationEngine));

        if (_currentSimulation == null)
            throw new InvalidOperationException("No simulation is currently loaded");

        var stepStartTime = DateTime.UtcNow;

        try
        {
            // Calculate next time step
            var timeStep = _useCustomTimeStep ? _customTimeStep : _timeStepManager.CalculateOptimalTimeStep(
                _particles.AsReadOnly(),
                _currentSimulationTime);

            // Evolve quantum states
            if (_particles.Count > 0)
            {
                var evolvedParticles = await _stateEvolutionEngine.EvolveParticlesAsync(
                    _particles.AsReadOnly(),
                    timeStep);

                // Update particle list
                lock (_lockObject)
                {
                    _particles.Clear();
                    _particles.AddRange(evolvedParticles);
                }
            }

            // Update simulation state
            lock (_lockObject)
            {
                _currentSimulationTime += timeStep;
                _currentStepNumber++;
                _currentTimeStep = timeStep;
            }

            var stepTime = DateTime.UtcNow - stepStartTime;

            // Raise step completed event
            var stepArgs = new SimulationStepCompletedEventArgs(
                _id,
                _currentSimulationTime,
                _currentStepNumber,
                timeStep,
                stepTime.TotalMilliseconds,
                _particles.Count,
                0.0, // totalEnergy - would calculate actual value
                0.0, // energyError - would calculate actual value
                true // isSuccessful
            );

            StepCompleted?.Invoke(this, stepArgs);
        }
        catch (Exception ex)
        {
            await RaiseErrorEventAsync(ex);
            throw;
        }
    }

    public async Task ExecuteStepsAsync(long stepCount)
    {
        for (long i = 0; i < stepCount; i++)
        {
            await ExecuteStepAsync();
        }
    }

    public async Task ExecuteForTimeAsync(double duration)
    {
        var targetTime = _currentSimulationTime + duration;
        
        while (_currentSimulationTime < targetTime)
        {
            await ExecuteStepAsync();
        }
    }

    public async Task ExecuteUntilConvergenceAsync(long maxSteps, double tolerance)
    {
        var previousEnergy = 0.0;
        var convergenceSteps = 0;
        const int requiredConvergenceSteps = 10;

        for (long step = 0; step < maxSteps; step++)
        {
            await ExecuteStepAsync();

            // Check convergence (simplified)
            var currentEnergy = 0.0; // Would calculate actual energy
            var energyChange = Math.Abs(currentEnergy - previousEnergy);

            if (energyChange < tolerance)
            {
                convergenceSteps++;
                if (convergenceSteps >= requiredConvergenceSteps)
                {
                    // Raise convergence event
                    var convergenceArgs = new ConvergenceDetectedEventArgs(
                        _id,
                        _currentSimulationTime,
                        _currentStepNumber,
                        ConvergenceType.Energy,
                        tolerance,
                        energyChange,
                        "Energy convergence detected",
                        convergenceSteps,
                        DateTime.UtcNow - _simulationStartTime,
                        currentEnergy
                    );

                    ConvergenceDetected?.Invoke(this, convergenceArgs);
                    break;
                }
            }
            else
            {
                convergenceSteps = 0;
            }

            previousEnergy = currentEnergy;
        }
    }

    #endregion

    #region Time Evolution Management

    public ITimeStepManager TimeStepManager => _timeStepManager;

    public IStateEvolutionEngine StateEvolutionEngine => _stateEvolutionEngine;

    public double CurrentTime
    {
        get
        {
            lock (_lockObject)
            {
                return _currentSimulationTime;
            }
        }
    }

    public long CurrentStep
    {
        get
        {
            lock (_lockObject)
            {
                return _currentStepNumber;
            }
        }
    }

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

    public void SetCustomTimeStep(double timeStep)
    {
        if (timeStep <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(timeStep), "Time step must be positive");

        lock (_lockObject)
        {
            _customTimeStep = timeStep;
            _useCustomTimeStep = true;
        }
    }

    public void ResetToAutomaticTimeStep()
    {
        lock (_lockObject)
        {
            _useCustomTimeStep = false;
        }
    }

    #endregion

    #region Performance and Monitoring

    public SimulationPerformanceMetrics PerformanceMetrics
    {
        get
        {
            lock (_lockObject)
            {
                return _performanceMetrics;
            }
        }
    }

    public double StepsPerSecond
    {
        get
        {
            lock (_lockObject)
            {
                return _performanceMetrics.StepsPerSecond;
            }
        }
    }

    public double MemoryUsageMB
    {
        get
        {
            lock (_lockObject)
            {
                return _performanceMetrics.MemoryUsageMB;
            }
        }
    }

    public bool IsPerformingOptimally
    {
        get
        {
            lock (_lockObject)
            {
                return _performanceMetrics.Efficiency > 0.8;
            }
        }
    }

    public IReadOnlyList<string> GetPerformanceRecommendations()
    {
        var recommendations = new List<string>();

        if (_performanceMetrics.MemoryUsageMB > RecommendedMemoryLimitMB * 0.9)
        {
            recommendations.Add("Memory usage is high. Consider reducing particle count or implementing memory optimization.");
        }

        if (_performanceMetrics.StepsPerSecond < 10.0)
        {
            recommendations.Add("Execution speed is low. Consider using adaptive time stepping or parallel execution.");
        }

        if (_performanceMetrics.Efficiency < 0.5)
        {
            recommendations.Add("Overall efficiency is low. Review simulation parameters and consider optimization.");
        }

        return recommendations.AsReadOnly();
    }

    #endregion

    #region Error Handling and Diagnostics

    public bool IsHealthy
    {
        get
        {
            lock (_lockObject)
            {
                return _status != SimulationEngineStatus.Error && IsPerformingOptimally;
            }
        }
    }

    public async Task<EngineHealthReport> PerformHealthCheckAsync()
    {
        var issues = new List<string>();
        var recommendations = new List<string>();
        var diagnosticData = new Dictionary<string, object>();

        // Check engine status
        if (_status == SimulationEngineStatus.Error)
        {
            issues.Add("Engine is in error state");
        }

        // Check performance
        if (!IsPerformingOptimally)
        {
            issues.Add("Performance is below optimal levels");
            recommendations.AddRange(GetPerformanceRecommendations());
        }

        // Check memory usage
        var memoryUsage = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        diagnosticData["MemoryUsageMB"] = memoryUsage;
        diagnosticData["ParticleCount"] = _particles.Count;
        diagnosticData["ComponentCount"] = _activeComponents.Count;

        var healthScore = issues.Count == 0 ? 1.0 : Math.Max(0.0, 1.0 - (issues.Count * 0.2));

        return new EngineHealthReport
        {
            IsHealthy = issues.Count == 0,
            HealthScore = healthScore,
            Issues = issues.AsReadOnly(),
            Recommendations = recommendations.AsReadOnly(),
            DiagnosticData = diagnosticData,
            CheckTimestamp = DateTime.UtcNow
        };
    }

    public IReadOnlyList<string> GetCurrentIssues()
    {
        var issues = new List<string>();

        if (_status == SimulationEngineStatus.Error)
        {
            issues.Add("Engine is in error state");
        }

        if (!IsPerformingOptimally)
        {
            issues.Add("Performance is below optimal levels");
        }

        return issues.AsReadOnly();
    }

    public async Task<bool> AttemptRecoveryAsync()
    {
        if (_status != SimulationEngineStatus.Error)
            return true;

        try
        {
            // Attempt to reset to a stable state
            await ResetAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Events

    public event EventHandler<EngineStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<SimulationStepCompletedEventArgs>? StepCompleted;
    public event EventHandler<PerformanceMetricsUpdatedEventArgs>? PerformanceUpdated;
    public event EventHandler<EngineErrorEventArgs>? ErrorOccurred;
    public event EventHandler<ConvergenceDetectedEventArgs>? ConvergenceDetected;

    #endregion

    #region Private Helper Methods

    private async Task SetStatusAsync(SimulationEngineStatus newStatus)
    {
        SimulationEngineStatus previousStatus;

        lock (_lockObject)
        {
            previousStatus = _status;
            _status = newStatus;
        }

        if (previousStatus != newStatus)
        {
            var args = new EngineStatusChangedEventArgs(
                _id,
                _currentSimulationTime,
                _currentStepNumber,
                _id,
                _name,
                previousStatus,
                newStatus,
                $"Status changed from {previousStatus} to {newStatus}"
            );

            StatusChanged?.Invoke(this, args);
        }
    }

    private async Task RaiseErrorEventAsync(Exception exception)
    {
        var errorArgs = new EngineErrorEventArgs(
            _id,
            _currentSimulationTime,
            _currentStepNumber,
            _id,
            _name,
            exception,
            ErrorSeverity.Major,
            true, // isRecoverable
            "SimulationEngine"
        );

        ErrorOccurred?.Invoke(this, errorArgs);
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            
            if (_stateEvolutionEngine is IDisposable disposableEngine)
                disposableEngine.Dispose();
            
            if (_timeStepManager is IDisposable disposableManager)
                disposableManager.Dispose();

            _status = SimulationEngineStatus.Shutdown;
            _disposed = true;
        }
    }

    #endregion
}
