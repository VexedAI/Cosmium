using Cosmium.Engine.Simulation.Engine;

namespace Cosmium.Engine.Simulation.Core;

/// <summary>
/// Represents a complete snapshot of the simulation state at a specific point in time.
/// Used for checkpointing and state restoration.
/// </summary>
public class SimulationSnapshot
{
    /// <summary>
    /// Unique identifier for this snapshot.
    /// </summary>
    public Guid SnapshotId { get; set; }

    /// <summary>
    /// UTC timestamp when the snapshot was created.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Current simulation time when snapshot was taken.
    /// </summary>
    public double SimulationTime { get; set; }

    /// <summary>
    /// Current step number when snapshot was taken.
    /// </summary>
    public long StepNumber { get; set; }

    /// <summary>
    /// Current engine status when snapshot was taken.
    /// </summary>
    public SimulationEngineStatus Status { get; set; }

    /// <summary>
    /// Number of particles in the simulation.
    /// </summary>
    public int ParticleCount { get; set; }

    /// <summary>
    /// Number of active components in the simulation.
    /// </summary>
    public int ComponentCount { get; set; }

    /// <summary>
    /// Additional metadata about the snapshot.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Serialized particle data (implementation would include actual particle states).
    /// </summary>
    public byte[]? ParticleData { get; set; }

    /// <summary>
    /// Serialized component data (implementation would include actual component states).
    /// </summary>
    public byte[]? ComponentData { get; set; }

    /// <summary>
    /// Serialized engine configuration data.
    /// </summary>
    public byte[]? ConfigurationData { get; set; }

    /// <summary>
    /// Size of the snapshot in bytes.
    /// </summary>
    public long SizeBytes => 
        (ParticleData?.Length ?? 0) + 
        (ComponentData?.Length ?? 0) + 
        (ConfigurationData?.Length ?? 0);

    /// <summary>
    /// Creates a copy of this snapshot.
    /// </summary>
    public SimulationSnapshot Clone()
    {
        return new SimulationSnapshot
        {
            SnapshotId = SnapshotId,
            Timestamp = Timestamp,
            SimulationTime = SimulationTime,
            StepNumber = StepNumber,
            Status = Status,
            ParticleCount = ParticleCount,
            ComponentCount = ComponentCount,
            Metadata = new Dictionary<string, object>(Metadata),
            ParticleData = ParticleData?.ToArray(),
            ComponentData = ComponentData?.ToArray(),
            ConfigurationData = ConfigurationData?.ToArray()
        };
    }

    /// <summary>
    /// Validates that the snapshot contains consistent data.
    /// </summary>
    public bool IsValid()
    {
        return SnapshotId != Guid.Empty &&
               Timestamp != default &&
               SimulationTime >= 0 &&
               StepNumber >= 0 &&
               ParticleCount >= 0 &&
               ComponentCount >= 0;
    }

    /// <summary>
    /// Returns a string representation of the snapshot.
    /// </summary>
    public override string ToString()
    {
        return $"Snapshot {SnapshotId:D} at t={SimulationTime:F6}, step={StepNumber}, particles={ParticleCount}";
    }
}
