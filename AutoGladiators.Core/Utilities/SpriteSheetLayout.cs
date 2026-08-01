namespace AutoGladiators.Core.Utilities;

/// <summary>
/// Validates a sprite sheet grid and computes row-major frame bounds.
/// </summary>
public readonly record struct SpriteSheetLayout
{
    public int BitmapWidth { get; }
    public int BitmapHeight { get; }
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    public int Columns { get; }
    public int Rows { get; }
    public int FrameCount => Columns * Rows;

    private SpriteSheetLayout(int bitmapWidth, int bitmapHeight, int frameWidth, int frameHeight)
    {
        BitmapWidth = bitmapWidth;
        BitmapHeight = bitmapHeight;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        Columns = bitmapWidth / frameWidth;
        Rows = bitmapHeight / frameHeight;
    }

    public static SpriteSheetLayout Create(int bitmapWidth, int bitmapHeight, int frameWidth, int frameHeight, string context)
    {
        if (bitmapWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(bitmapWidth), $"{context}: bitmap width must be positive.");
        if (bitmapHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(bitmapHeight), $"{context}: bitmap height must be positive.");
        if (frameWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameWidth), $"{context}: frame width must be positive.");
        if (frameHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameHeight), $"{context}: frame height must be positive.");
        if (bitmapWidth < frameWidth || bitmapHeight < frameHeight)
            throw new InvalidOperationException(
                $"{context}: bitmap size {bitmapWidth}x{bitmapHeight} is smaller than frame size {frameWidth}x{frameHeight}.");
        if (bitmapWidth % frameWidth != 0 || bitmapHeight % frameHeight != 0)
            throw new InvalidOperationException(
                $"{context}: bitmap size {bitmapWidth}x{bitmapHeight} is not evenly divisible by frame size {frameWidth}x{frameHeight}.");

        return new SpriteSheetLayout(bitmapWidth, bitmapHeight, frameWidth, frameHeight);
    }

    public (int Left, int Top, int Right, int Bottom) GetFrameBounds(int frameIndex, string context)
    {
        if (frameIndex < 0 || frameIndex >= FrameCount)
            throw new ArgumentOutOfRangeException(
                nameof(frameIndex),
                $"{context}: frame index {frameIndex} is outside the valid range 0-{FrameCount - 1}.");

        int col = frameIndex % Columns;
        int row = frameIndex / Columns;
        int x = col * FrameWidth;
        int y = row * FrameHeight;
        return (x, y, x + FrameWidth, y + FrameHeight);
    }
}
