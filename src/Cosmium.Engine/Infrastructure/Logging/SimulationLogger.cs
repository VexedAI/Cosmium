using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Cosmium.Engine.Infrastructure.Configuration;

namespace Cosmium.Engine.Infrastructure.Logging;

/// <summary>
/// Structured logging system for quantum simulation operations.
/// Provides context-aware logging with performance tracking and filtering.
/// </summary>
public class SimulationLogger
{
    private static SimulationLogger? _instance;
    private static readonly object _lock = new();
    private readonly ConcurrentQueue<LogEntry> _logQueue = new();
    private readonly Task _loggingTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<ILogTarget> _targets = new();

    /// <summary>
    /// Gets the singleton instance of the simulation logger.
    /// </summary>
    public static SimulationLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SimulationLogger();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Current minimum log level.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Whether the logger is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    private SimulationLogger()
    {
        // Initialize default targets based on configuration
        var config = EngineConfiguration.Instance.Logging;
        
        if (config.LogToConsole)
        {
            _targets.Add(new ConsoleLogTarget());
        }
        
        if (config.LogToFile)
        {
            _targets.Add(new FileLogTarget(config.LogFilePath, config.MaxFileSizeMB, config.RetainedFileCount));
        }

        MinimumLevel = config.MinimumLevel;

        // Start background logging task
        _loggingTask = Task.Run(ProcessLogEntries, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    public void Trace(string message, object? context = null) => Log(LogLevel.Trace, message, context);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    public void Debug(string message, object? context = null) => Log(LogLevel.Debug, message, context);

    /// <summary>
    /// Logs an information message.
    /// </summary>
    public void Information(string message, object? context = null) => Log(LogLevel.Information, message, context);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void Warning(string message, object? context = null) => Log(LogLevel.Warning, message, context);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public void Error(string message, Exception? exception = null, object? context = null)
    {
        Log(LogLevel.Error, message, context, exception);
    }

    /// <summary>
    /// Logs a critical error message.
    /// </summary>
    public void Critical(string message, Exception? exception = null, object? context = null)
    {
        Log(LogLevel.Critical, message, context, exception);
    }

    /// <summary>
    /// Logs a quantum operation with specific context.
    /// </summary>
    public void LogQuantumOperation(string operation, string systemId, object? parameters = null, TimeSpan? duration = null)
    {
        var context = new QuantumOperationContext
        {
            Operation = operation,
            SystemId = systemId,
            Parameters = parameters,
            Duration = duration,
            ThreadId = Environment.CurrentManagedThreadId,
            Timestamp = DateTime.UtcNow
        };

        Log(LogLevel.Information, $"Quantum operation: {operation}", context);
    }

    /// <summary>
    /// Logs a simulation step with detailed metrics.
    /// </summary>
    public void LogSimulationStep(int step, double time, object? state = null, object? metrics = null)
    {
        var context = new SimulationStepContext
        {
            Step = step,
            Time = time,
            State = state,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };

        Log(LogLevel.Debug, $"Simulation step {step} at t={time:E3}", context);
    }

    /// <summary>
    /// Creates a scoped logger for a specific operation.
    /// </summary>
    public IDisposable BeginScope(string operation, object? context = null)
    {
        return new LogScope(this, operation, context);
    }

    /// <summary>
    /// Adds a custom log target.
    /// </summary>
    public void AddTarget(ILogTarget target)
    {
        _targets.Add(target);
    }

    /// <summary>
    /// Removes a log target.
    /// </summary>
    public void RemoveTarget(ILogTarget target)
    {
        _targets.Remove(target);
    }

    /// <summary>
    /// Flushes all pending log entries.
    /// </summary>
    public async Task FlushAsync()
    {
        // Wait for queue to be empty
        while (!_logQueue.IsEmpty)
        {
            await Task.Delay(10);
        }

        // Flush all targets
        foreach (var target in _targets)
        {
            if (target is IAsyncLogTarget asyncTarget)
            {
                await asyncTarget.FlushAsync();
            }
        }
    }

    /// <summary>
    /// Disposes the logger and flushes remaining entries.
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        
        try
        {
            _loggingTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception)
        {
            // Ignore timeout exceptions during shutdown
        }

        foreach (var target in _targets)
        {
            target.Dispose();
        }

        _cancellationTokenSource.Dispose();
    }

    private void Log(LogLevel level, string message, object? context = null, Exception? exception = null)
    {
        if (!IsEnabled || level < MinimumLevel)
            return;

        var entry = new LogEntry
        {
            Level = level,
            Message = message,
            Context = context,
            Exception = exception,
            Timestamp = DateTime.UtcNow,
            ThreadId = Environment.CurrentManagedThreadId,
            StackTrace = level >= LogLevel.Error ? Environment.StackTrace : null
        };

        _logQueue.Enqueue(entry);
    }

    private async Task ProcessLogEntries()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (_logQueue.TryDequeue(out var entry))
                {
                    foreach (var target in _targets)
                    {
                        try
                        {
                            if (target is IAsyncLogTarget asyncTarget)
                            {
                                await asyncTarget.WriteAsync(entry);
                            }
                            else
                            {
                                target.Write(entry);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log target failed - write to console as fallback
                            Console.WriteLine($"Log target failed: {ex.Message}");
                        }
                    }
                }
                else
                {
                    await Task.Delay(10, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging error: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Represents a log entry with contextual information.
/// </summary>
public class LogEntry
{
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Context { get; set; }
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; }
    public int ThreadId { get; set; }
    public string? StackTrace { get; set; }
}

/// <summary>
/// Context for quantum operation logging.
/// </summary>
public class QuantumOperationContext
{
    public string Operation { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public object? Parameters { get; set; }
    public TimeSpan? Duration { get; set; }
    public int ThreadId { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Context for simulation step logging.
/// </summary>
public class SimulationStepContext
{
    public int Step { get; set; }
    public double Time { get; set; }
    public object? State { get; set; }
    public object? Metrics { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Scoped logger for operations with automatic disposal.
/// </summary>
public class LogScope : IDisposable
{
    private readonly SimulationLogger _logger;
    private readonly string _operation;
    private readonly Stopwatch _stopwatch;
    private readonly object? _context;

    public LogScope(SimulationLogger logger, string operation, object? context = null)
    {
        _logger = logger;
        _operation = operation;
        _context = context;
        _stopwatch = Stopwatch.StartNew();
        
        _logger.Debug($"BEGIN: {operation}", context);
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        
        var endContext = new
        {
            Duration = _stopwatch.Elapsed,
            InitialContext = _context
        };
        
        _logger.Debug($"END: {_operation} (Duration: {_stopwatch.Elapsed.TotalMilliseconds:F2} ms)", endContext);
    }
}

/// <summary>
/// Interface for log output targets.
/// </summary>
public interface ILogTarget : IDisposable
{
    void Write(LogEntry entry);
}

/// <summary>
/// Interface for asynchronous log output targets.
/// </summary>
public interface IAsyncLogTarget : ILogTarget
{
    Task WriteAsync(LogEntry entry);
    Task FlushAsync();
}

/// <summary>
/// Console log target implementation.
/// </summary>
public class ConsoleLogTarget : ILogTarget
{
    public void Write(LogEntry entry)
    {
        var color = GetColorForLevel(entry.Level);
        var originalColor = Console.ForegroundColor;
        
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{entry.Timestamp:HH:mm:ss.fff}] [{entry.Level}] {entry.Message}");
            
            if (entry.Exception != null)
            {
                Console.WriteLine($"Exception: {entry.Exception}");
            }
            
            if (entry.Context != null)
            {
                var contextJson = JsonSerializer.Serialize(entry.Context, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"Context: {contextJson}");
            }
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    private static ConsoleColor GetColorForLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Information => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };
    }

    public void Dispose()
    {
        // Console doesn't need disposal
    }
}

/// <summary>
/// File log target with rotation support.
/// </summary>
public class FileLogTarget : IAsyncLogTarget
{
    private readonly string _filePathPattern;
    private readonly int _maxFileSizeMB;
    private readonly int _retainedFileCount;
    private readonly SemaphoreSlim _writeSemaphore = new(1, 1);
    private string? _currentFilePath;
    private StreamWriter? _currentWriter;

    public FileLogTarget(string filePathPattern, int maxFileSizeMB, int retainedFileCount)
    {
        _filePathPattern = filePathPattern;
        _maxFileSizeMB = maxFileSizeMB;
        _retainedFileCount = retainedFileCount;
    }

    public void Write(LogEntry entry)
    {
        WriteAsync(entry).Wait();
    }

    public async Task WriteAsync(LogEntry entry)
    {
        await _writeSemaphore.WaitAsync();
        
        try
        {
            await EnsureCurrentWriter();
            
            if (_currentWriter != null)
            {
                var logLine = FormatLogEntry(entry);
                await _currentWriter.WriteLineAsync(logLine);
                await _currentWriter.FlushAsync();
                
                // Check if rotation is needed
                if (ShouldRotateFile())
                {
                    await RotateFile();
                }
            }
        }
        finally
        {
            _writeSemaphore.Release();
        }
    }

    public async Task FlushAsync()
    {
        await _writeSemaphore.WaitAsync();
        
        try
        {
            if (_currentWriter != null)
            {
                await _currentWriter.FlushAsync();
            }
        }
        finally
        {
            _writeSemaphore.Release();
        }
    }

    private async Task EnsureCurrentWriter()
    {
        var expectedPath = GetCurrentFilePath();
        
        if (_currentFilePath != expectedPath || _currentWriter == null)
        {
            if (_currentWriter != null)
            {
                await _currentWriter.DisposeAsync();
            }
            
            Directory.CreateDirectory(Path.GetDirectoryName(expectedPath)!);
            _currentWriter = new StreamWriter(expectedPath, append: true);
            _currentFilePath = expectedPath;
        }
    }

    private string GetCurrentFilePath()
    {
        return _filePathPattern.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd"));
    }

    private bool ShouldRotateFile()
    {
        if (_currentFilePath == null || !File.Exists(_currentFilePath))
            return false;
        
        var fileInfo = new FileInfo(_currentFilePath);
        return fileInfo.Length > _maxFileSizeMB * 1024 * 1024;
    }

    private async Task RotateFile()
    {
        if (_currentWriter != null)
        {
            await _currentWriter.DisposeAsync();
            _currentWriter = null;
        }

        // Clean up old files
        await CleanupOldFiles();
        
        // Writer will be recreated on next write
        _currentFilePath = null;
    }

    private async Task CleanupOldFiles()
    {
        try
        {
            var directory = Path.GetDirectoryName(_currentFilePath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return;

            var pattern = Path.GetFileNameWithoutExtension(_filePathPattern.Replace("{Date}", "*")) + "*" + Path.GetExtension(_filePathPattern);
            var files = Directory.GetFiles(directory, pattern)
                               .OrderByDescending(f => File.GetCreationTime(f))
                               .Skip(_retainedFileCount);

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore individual file deletion errors
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }

        await Task.CompletedTask;
    }

    private string FormatLogEntry(LogEntry entry)
    {
        var parts = new List<string>
        {
            entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            $"[{entry.Level}]",
            $"[Thread-{entry.ThreadId}]",
            entry.Message
        };

        if (entry.Exception != null)
        {
            parts.Add($"Exception: {entry.Exception}");
        }

        if (entry.Context != null)
        {
            var contextJson = JsonSerializer.Serialize(entry.Context);
            parts.Add($"Context: {contextJson}");
        }

        return string.Join(" | ", parts);
    }

    public void Dispose()
    {
        _currentWriter?.Dispose();
        _writeSemaphore.Dispose();
    }
}
