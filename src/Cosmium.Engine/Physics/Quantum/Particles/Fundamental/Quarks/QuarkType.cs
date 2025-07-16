namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

/// <summary>
/// Enumeration of the six types of quarks in the Standard Model.
/// Each quark type has distinct mass, charge, and quantum properties.
/// </summary>
public enum QuarkType
{
    /// <summary>
    /// Up quark (u) - First generation, electric charge +2/3.
    /// Mass: ~2.2 MeV/c²
    /// </summary>
    Up,

    /// <summary>
    /// Down quark (d) - First generation, electric charge -1/3.
    /// Mass: ~4.7 MeV/c²
    /// </summary>
    Down,

    /// <summary>
    /// Charm quark (c) - Second generation, electric charge +2/3.
    /// Mass: ~1.27 GeV/c²
    /// </summary>
    Charm,

    /// <summary>
    /// Strange quark (s) - Second generation, electric charge -1/3.
    /// Mass: ~95 MeV/c²
    /// </summary>
    Strange,

    /// <summary>
    /// Top quark (t) - Third generation, electric charge +2/3.
    /// Mass: ~172.8 GeV/c² (heaviest known elementary particle)
    /// </summary>
    Top,

    /// <summary>
    /// Bottom quark (b) - Third generation, electric charge -1/3.
    /// Mass: ~4.18 GeV/c²
    /// </summary>
    Bottom
}

/// <summary>
/// Extension methods for QuarkType enum to provide physical properties.
/// </summary>
public static class QuarkTypeExtensions
{
    /// <summary>
    /// Gets the electric charge of the quark in units of elementary charge.
    /// </summary>
    /// <param name="quarkType">The type of quark.</param>
    /// <returns>The electric charge as a fraction of elementary charge.</returns>
    public static double GetElectricCharge(this QuarkType quarkType)
    {
        return quarkType switch
        {
            QuarkType.Up => 2.0 / 3.0,
            QuarkType.Down => -1.0 / 3.0,
            QuarkType.Charm => 2.0 / 3.0,
            QuarkType.Strange => -1.0 / 3.0,
            QuarkType.Top => 2.0 / 3.0,
            QuarkType.Bottom => -1.0 / 3.0,
            _ => throw new ArgumentOutOfRangeException(nameof(quarkType), quarkType, "Unknown quark type")
        };
    }

    /// <summary>
    /// Gets the rest mass of the quark in kg.
    /// Values from Particle Data Group (PDG) 2022.
    /// </summary>
    /// <param name="quarkType">The type of quark.</param>
    /// <returns>The rest mass in kg.</returns>
    public static double GetRestMass(this QuarkType quarkType)
    {
        // Convert from MeV/c² to kg using E = mc²
        // 1 MeV = 1.602176634e-13 J, c = 2.99792458e8 m/s
        const double MeVToKg = 1.602176634e-13 / (2.99792458e8 * 2.99792458e8);
        
        return quarkType switch
        {
            QuarkType.Up => 2.2e6 * MeVToKg,        // ~2.2 MeV/c²
            QuarkType.Down => 4.7e6 * MeVToKg,      // ~4.7 MeV/c²
            QuarkType.Charm => 1.27e9 * MeVToKg,    // ~1.27 GeV/c²
            QuarkType.Strange => 95e6 * MeVToKg,    // ~95 MeV/c²
            QuarkType.Top => 172.8e9 * MeVToKg,     // ~172.8 GeV/c²
            QuarkType.Bottom => 4.18e9 * MeVToKg,   // ~4.18 GeV/c²
            _ => throw new ArgumentOutOfRangeException(nameof(quarkType), quarkType, "Unknown quark type")
        };
    }

    /// <summary>
    /// Gets the generation (family) number of the quark.
    /// </summary>
    /// <param name="quarkType">The type of quark.</param>
    /// <returns>The generation number (1, 2, or 3).</returns>
    public static int GetGeneration(this QuarkType quarkType)
    {
        return quarkType switch
        {
            QuarkType.Up or QuarkType.Down => 1,
            QuarkType.Charm or QuarkType.Strange => 2,
            QuarkType.Top or QuarkType.Bottom => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(quarkType), quarkType, "Unknown quark type")
        };
    }

    /// <summary>
    /// Gets the conventional symbol used to represent the quark.
    /// </summary>
    /// <param name="quarkType">The type of quark.</param>
    /// <returns>The single-letter symbol for the quark.</returns>
    public static string GetSymbol(this QuarkType quarkType)
    {
        return quarkType switch
        {
            QuarkType.Up => "u",
            QuarkType.Down => "d",
            QuarkType.Charm => "c",
            QuarkType.Strange => "s",
            QuarkType.Top => "t",
            QuarkType.Bottom => "b",
            _ => throw new ArgumentOutOfRangeException(nameof(quarkType), quarkType, "Unknown quark type")
        };
    }

    /// <summary>
    /// Gets the weak isospin quantum number for the quark.
    /// </summary>
    /// <param name="quarkType">The type of quark.</param>
    /// <returns>The weak isospin value (+1/2 for up-type, -1/2 for down-type).</returns>
    public static double GetWeakIsospin(this QuarkType quarkType)
    {
        return quarkType switch
        {
            QuarkType.Up or QuarkType.Charm or QuarkType.Top => 0.5,
            QuarkType.Down or QuarkType.Strange or QuarkType.Bottom => -0.5,
            _ => throw new ArgumentOutOfRangeException(nameof(quarkType), quarkType, "Unknown quark type")
        };
    }

    /// <summary>
    /// Determines if this is an up-type quark (positive weak isospin).
    /// </summary>
    /// <param name="quarkType">The type of quark.</param>
    /// <returns>True if up-type, false if down-type.</returns>
    public static bool IsUpType(this QuarkType quarkType)
    {
        return quarkType is QuarkType.Up or QuarkType.Charm or QuarkType.Top;
    }

    /// <summary>
    /// Determines if this is a down-type quark (negative weak isospin).
    /// </summary>
    /// <param name="quarkType">The type of quark.</param>
    /// <returns>True if down-type, false if up-type.</returns>
    public static bool IsDownType(this QuarkType quarkType)
    {
        return !IsUpType(quarkType);
    }
}
