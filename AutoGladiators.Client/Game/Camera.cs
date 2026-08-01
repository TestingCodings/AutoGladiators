using SkiaSharp;

namespace AutoGladiators.Client.Game;

/// <summary>
/// A world-pixel camera. Its (X,Y) is the world-pixel coordinate shown at the
/// centre of the viewport. Converts world pixels to screen pixels.
/// </summary>
public sealed class Camera
{
    public float X { get; set; }
    public float Y { get; set; }
    public float ViewportW { get; set; }
    public float ViewportH { get; set; }

    /// <summary>Snap the camera centre to a world-pixel position.</summary>
    public void CenterOn(float worldX, float worldY)
    {
        X = worldX;
        Y = worldY;
    }

    public SKPoint WorldToScreen(float worldX, float worldY)
        => new(worldX - X + ViewportW / 2f, worldY - Y + ViewportH / 2f);
}
