using SkiaSharp;
using AutoGladiators.Core.Models.Exploration;

namespace TestMauiApp.Game;

/// <summary>
/// Draws detailed, retro-style pixel terrain procedurally — no external tileset
/// required, so the art is original and copyright-safe. Detail is deterministic
/// per tile (hashed from tile coords) so the world looks stable between frames.
/// Everything is drawn aliased (no smoothing) for a crisp 2D-console look.
/// </summary>
public static class TileArt
{
    private static readonly SKPaint Fill = new() { IsAntialias = false, Style = SKPaintStyle.Fill };

    /// <summary>Ambient animation clock (seconds), set by the view each frame. Drives water shimmer.</summary>
    public static float AnimPhase;

    // Base colours per terrain type.
    private static readonly Dictionary<TileType, SKColor> Base = new()
    {
        [TileType.Grass]    = new(0x5F, 0xB8, 0x4B),
        [TileType.Water]    = new(0x3C, 0x8B, 0xE0),
        [TileType.Mountain] = new(0x93, 0x8A, 0x7A),
        [TileType.Forest]   = new(0x4F, 0xA8, 0x40),
        [TileType.Desert]   = new(0xE3, 0xCE, 0x87),
        [TileType.Cave]     = new(0x3A, 0x3A, 0x44),
        [TileType.Building] = new(0x9C, 0x6B, 0x3F),
        [TileType.Road]     = new(0xCB, 0xC2, 0xA8),
        [TileType.Bridge]   = new(0xB8, 0x8A, 0x52),
        [TileType.Obstacle] = new(0x6B, 0x2E, 0x2E),
    };

    public static SKColor BaseColor(TileType t) => Base.TryGetValue(t, out var c) ? c : Base[TileType.Grass];

    private static uint Hash(int x, int y)
    {
        unchecked
        {
            uint h = (uint)(x * 73856093) ^ (uint)(y * 19349663);
            h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15;
            return h;
        }
    }

    /// <summary>Draw one terrain tile with detail into a screen-space rect.</summary>
    public static void Draw(SKCanvas canvas, TileType type, SKRect r, int tx, int ty)
    {
        // Base fill.
        Fill.Color = BaseColor(type);
        canvas.DrawRect(r, Fill);

        float u = r.Width / 16f; // one art-pixel in screen units (16 art px per tile)
        uint h = Hash(tx, ty);

        switch (type)
        {
            case TileType.Grass:
                DrawSpeckles(canvas, r, u, h, new SKColor(0x4C, 0xA0, 0x3C), 3);
                DrawSpeckles(canvas, r, u, h >> 8, new SKColor(0x74, 0xCB, 0x5A), 2);
                break;

            case TileType.Forest:
                DrawSpeckles(canvas, r, u, h, new SKColor(0x3C, 0x8A, 0x30), 2);
                DrawTree(canvas, r, u);
                break;

            case TileType.Water:
                DrawWaves(canvas, r, u, ty);
                break;

            case TileType.Mountain:
                DrawRock(canvas, r, u, h);
                break;

            case TileType.Desert:
                DrawSpeckles(canvas, r, u, h, new SKColor(0xCF, 0xB8, 0x6E), 4);
                break;

            case TileType.Road:
                DrawRoad(canvas, r, u, h);
                break;

            case TileType.Cave:
                DrawSpeckles(canvas, r, u, h, new SKColor(0x2C, 0x2C, 0x36), 4);
                break;
        }
    }

    private static void Px(SKCanvas c, SKRect r, float u, int px, int py, int w, int hgt, SKColor col)
    {
        Fill.Color = col;
        c.DrawRect(new SKRect(r.Left + px * u, r.Top + py * u, r.Left + (px + w) * u, r.Top + (py + hgt) * u), Fill);
    }

    private static void DrawSpeckles(SKCanvas c, SKRect r, float u, uint h, SKColor col, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int px = (int)((h >> (i * 3)) & 0xF);
            int py = (int)((h >> (i * 3 + 4)) & 0xF);
            Px(c, r, u, px, py, 2, 1, col);
        }
    }

    private static void DrawWaves(SKCanvas c, SKRect r, float u, int ty)
    {
        var light = new SKColor(0x6F, 0xB0, 0xEE);
        var dark = new SKColor(0x2E, 0x74, 0xC8);
        // Crests drift horizontally over time for a living surface.
        int t = (int)(AnimPhase * 3f) + ty;
        int a = (t % 8);
        int b = 7 - ((t + 3) % 8);
        Px(c, r, u, Math.Clamp(a, 0, 12), 4, 4, 1, light);
        Px(c, r, u, Math.Clamp(b, 0, 12), 9, 4, 1, light);
        Px(c, r, u, Math.Clamp((t + 5) % 10, 0, 13), 12, 3, 1, dark);
    }

    private static void DrawTree(SKCanvas c, SKRect r, float u)
    {
        var trunk = new SKColor(0x6B, 0x45, 0x22);
        var canopy = new SKColor(0x2E, 0x7D, 0x36);
        var canopyLt = new SKColor(0x3E, 0x9A, 0x46);
        Px(c, r, u, 7, 11, 2, 4, trunk);          // trunk
        Px(c, r, u, 4, 3, 8, 8, canopy);          // canopy body
        Px(c, r, u, 5, 2, 6, 2, canopy);          // canopy top
        Px(c, r, u, 5, 4, 3, 3, canopyLt);        // highlight
    }

    private static void DrawRock(SKCanvas c, SKRect r, float u, uint h)
    {
        var shade = new SKColor(0x77, 0x6E, 0x60);
        var light = new SKColor(0xB0, 0xA6, 0x94);
        int off = (int)(h & 0x3);
        Px(c, r, u, 2 + off, 8, 12 - off, 6, shade);   // lower shadow mass
        Px(c, r, u, 5, 3, 6, 6, light);                // peak
        Px(c, r, u, 6, 2, 4, 2, light);
        Px(c, r, u, 4, 9, 2, 2, shade);
    }

    private static void DrawRoad(SKCanvas c, SKRect r, float u, uint h)
    {
        var edge = new SKColor(0xB1, 0xA7, 0x8A);
        var speck = new SKColor(0xDD, 0xD6, 0xC2);
        Px(c, r, u, 0, 0, 16, 1, edge);
        Px(c, r, u, 0, 15, 16, 1, edge);
        Px(c, r, u, (int)(h & 0xF), (int)((h >> 4) & 0xF), 1, 1, speck);
    }
}
