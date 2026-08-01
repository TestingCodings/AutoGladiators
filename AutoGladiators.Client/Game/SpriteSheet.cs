using SkiaSharp;
using Microsoft.Maui.Storage;

namespace AutoGladiators.Client.Game;

/// <summary>
/// A decoded sprite sheet sliced into a grid of equally-sized frames.
/// Frames are indexed row-major: index = row * Columns + col.
/// Pixel-art is drawn nearest-neighbour (no smoothing) for crisp GBA-style scaling.
/// </summary>
public sealed class SpriteSheet
{
    public SKBitmap Bitmap { get; }
    public int FrameW { get; }
    public int FrameH { get; }
    public int Columns { get; }
    public int Rows { get; }

    // Nearest-neighbour paint shared by every sprite draw.
    public static readonly SKPaint PixelPaint = new()
    {
        FilterQuality = SKFilterQuality.None,
        IsAntialias = false,
    };

    private SpriteSheet(SKBitmap bmp, int frameW, int frameH)
    {
        Bitmap = bmp;
        FrameW = frameW;
        FrameH = frameH;
        Columns = Math.Max(1, bmp.Width / frameW);
        Rows = Math.Max(1, bmp.Height / frameH);
    }

    /// <summary>Loads a sheet from a Resources/Raw asset (LogicalName path, e.g. "sprites/gary.png").</summary>
    public static async Task<SpriteSheet> LoadAsync(string rawLogicalPath, int frameW, int frameH)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(rawLogicalPath);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;
        var bmp = SKBitmap.Decode(ms)
                  ?? throw new InvalidOperationException($"Could not decode sprite sheet '{rawLogicalPath}'.");
        return new SpriteSheet(bmp, frameW, frameH);
    }

    public SKRectI SourceRect(int frameIndex)
    {
        int col = frameIndex % Columns;
        int row = frameIndex / Columns;
        int x = col * FrameW;
        int y = row * FrameH;
        return new SKRectI(x, y, x + FrameW, y + FrameH);
    }

    public void DrawFrame(SKCanvas canvas, int frameIndex, SKRect dest)
    {
        canvas.DrawBitmap(Bitmap, SourceRect(frameIndex), dest, PixelPaint);
    }
}
