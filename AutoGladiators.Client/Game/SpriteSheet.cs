using AutoGladiators.Core.Utilities;
using Microsoft.Maui.Storage;
using SkiaSharp;

namespace AutoGladiators.Client.Game;

/// <summary>
/// A decoded sprite sheet sliced into a grid of equally-sized frames.
/// Frames are indexed row-major: index = row * Columns + col.
/// Pixel-art is drawn nearest-neighbour (no smoothing) for crisp GBA-style scaling.
/// </summary>
public sealed class SpriteSheet : IDisposable
{
    private readonly SpriteSheetLayout _layout;
    private readonly string _source;
    private bool _disposed;

    public SKBitmap Bitmap { get; }
    public int FrameW => _layout.FrameWidth;
    public int FrameH => _layout.FrameHeight;
    public int Columns => _layout.Columns;
    public int Rows => _layout.Rows;
    public int FrameCount => _layout.FrameCount;

    // Nearest-neighbour paint shared by every sprite draw.
    public static readonly SKPaint PixelPaint = new()
    {
        FilterQuality = SKFilterQuality.None,
        IsAntialias = false,
    };

    private SpriteSheet(SKBitmap bmp, int frameW, int frameH, string source)
    {
        Bitmap = bmp ?? throw new ArgumentNullException(nameof(bmp));
        _source = string.IsNullOrWhiteSpace(source) ? "sprite sheet" : source;
        _layout = SpriteSheetLayout.Create(bmp.Width, bmp.Height, frameW, frameH, _source);
    }

    /// <summary>Loads a sheet from a Resources/Raw asset (LogicalName path, e.g. "sprites/npc_01.png").</summary>
    public static async Task<SpriteSheet> LoadAsync(string rawLogicalPath, int frameW, int frameH)
    {
        if (string.IsNullOrWhiteSpace(rawLogicalPath))
            throw new ArgumentException("A raw asset path is required.", nameof(rawLogicalPath));

        await using var stream = await FileSystem.OpenAppPackageFileAsync(rawLogicalPath);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;
        var bmp = SKBitmap.Decode(ms)
                  ?? throw new InvalidOperationException($"Could not decode sprite sheet '{rawLogicalPath}'.");

        try
        {
            return new SpriteSheet(bmp, frameW, frameH, rawLogicalPath);
        }
        catch
        {
            bmp.Dispose();
            throw;
        }
    }

    public SKRectI SourceRect(int frameIndex)
    {
        ThrowIfDisposed();
        var bounds = _layout.GetFrameBounds(frameIndex, _source);
        return new SKRectI(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
    }

    public void DrawFrame(SKCanvas canvas, int frameIndex, SKRect dest)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(canvas);
        canvas.DrawBitmap(Bitmap, SourceRect(frameIndex), dest, PixelPaint);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Bitmap.Dispose();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SpriteSheet), $"Sprite sheet '{_source}' has already been disposed.");
    }
}
