namespace Cosmium.Engine.Physics.Quantum.Particles.Fundamental.Quarks;

/// <summary>
/// Enumeration of the three color charges in Quantum Chromodynamics (QCD).
/// Color charge is a fundamental property of quarks and gluons that governs strong interactions.
/// Unlike electric charge, color charge is confined and never observed in isolation.
/// </summary>
public enum QuarkColor
{
    /// <summary>
    /// Red color charge.
    /// Conventional designation for one of the three color states.
    /// </summary>
    Red,

    /// <summary>
    /// Green color charge.
    /// Conventional designation for one of the three color states.
    /// </summary>
    Green,

    /// <summary>
    /// Blue color charge.
    /// Conventional designation for one of the three color states.
    /// </summary>
    Blue
}

/// <summary>
/// Extension methods for QuarkColor enum to provide additional functionality.
/// </summary>
public static class QuarkColorExtensions
{
    /// <summary>
    /// Gets the conventional symbol used to represent the color charge.
    /// </summary>
    /// <param name="color">The color charge.</param>
    /// <returns>Single-letter symbol for the color.</returns>
    public static string GetSymbol(this QuarkColor color)
    {
        return color switch
        {
            QuarkColor.Red => "r",
            QuarkColor.Green => "g",
            QuarkColor.Blue => "b",
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unknown color charge")
        };
    }

    /// <summary>
    /// Gets the full name of the color charge.
    /// </summary>
    /// <param name="color">The color charge.</param>
    /// <returns>Full name of the color.</returns>
    public static string GetName(this QuarkColor color)
    {
        return color switch
        {
            QuarkColor.Red => "Red",
            QuarkColor.Green => "Green", 
            QuarkColor.Blue => "Blue",
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unknown color charge")
        };
    }

    /// <summary>
    /// Gets the RGB color representation for visualization purposes.
    /// Note: This is purely for display and has no physical significance.
    /// </summary>
    /// <param name="color">The color charge.</param>
    /// <returns>RGB tuple for visualization.</returns>
    public static (byte R, byte G, byte B) GetRgbColor(this QuarkColor color)
    {
        return color switch
        {
            QuarkColor.Red => (255, 0, 0),
            QuarkColor.Green => (0, 255, 0),
            QuarkColor.Blue => (0, 0, 255),
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unknown color charge")
        };
    }

    /// <summary>
    /// Gets the next color in cyclic order (for certain QCD calculations).
    /// Red → Green → Blue → Red
    /// </summary>
    /// <param name="color">The current color.</param>
    /// <returns>The next color in cyclic order.</returns>
    public static QuarkColor GetNextColor(this QuarkColor color)
    {
        return color switch
        {
            QuarkColor.Red => QuarkColor.Green,
            QuarkColor.Green => QuarkColor.Blue,
            QuarkColor.Blue => QuarkColor.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unknown color charge")
        };
    }

    /// <summary>
    /// Gets the previous color in cyclic order.
    /// Red ← Green ← Blue ← Red
    /// </summary>
    /// <param name="color">The current color.</param>
    /// <returns>The previous color in cyclic order.</returns>
    public static QuarkColor GetPreviousColor(this QuarkColor color)
    {
        return color switch
        {
            QuarkColor.Red => QuarkColor.Blue,
            QuarkColor.Green => QuarkColor.Red,
            QuarkColor.Blue => QuarkColor.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Unknown color charge")
        };
    }

    /// <summary>
    /// Gets all possible color charges.
    /// </summary>
    /// <returns>Array of all color charges.</returns>
    public static QuarkColor[] GetAllColors()
    {
        return new[] { QuarkColor.Red, QuarkColor.Green, QuarkColor.Blue };
    }

    /// <summary>
    /// Checks if a combination of three colors forms a color-neutral (white) state.
    /// This is fundamental to quark confinement - only color-neutral combinations can exist as free particles.
    /// </summary>
    /// <param name="color1">First color.</param>
    /// <param name="color2">Second color.</param>
    /// <param name="color3">Third color.</param>
    /// <returns>True if the combination is color-neutral.</returns>
    public static bool IsColorNeutral(QuarkColor color1, QuarkColor color2, QuarkColor color3)
    {
        // Color neutrality requires one of each color (RGB = white)
        return (color1 != color2 && color2 != color3 && color1 != color3);
    }

    /// <summary>
    /// Gets the anticolor corresponding to this color.
    /// Used for antiquarks and color-anticolor pairs in mesons.
    /// </summary>
    /// <param name="color">The color charge.</param>
    /// <returns>The corresponding anticolor (same as input for this simplified model).</returns>
    /// <remarks>
    /// In a full QCD implementation, anticolors would be represented as antired, antigreen, antiblue.
    /// For simplicity, we use the same enum but note that antiquarks carry anticolor charges.
    /// </remarks>
    public static QuarkColor GetAnticolor(this QuarkColor color)
    {
        // In this simplified model, we return the same color
        // In a full implementation, you might use a separate AntiQuarkColor enum
        return color;
    }
}
