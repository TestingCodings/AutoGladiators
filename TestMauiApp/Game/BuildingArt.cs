using SkiaSharp;
using AutoGladiators.Core.Models.Exploration;

namespace TestMauiApp.Game;

/// <summary>
/// Draws procedural pixel buildings (healing centre, shop, gym) as tall
/// structures the player can walk behind. Anchored on a footprint rect in
/// screen space; the door sits at the bottom-centre tile.
/// </summary>
public static class BuildingArt
{
    private static readonly SKPaint P = new() { IsAntialias = false, Style = SKPaintStyle.Fill };

    private static readonly SKColor Wall     = new(0xE7, 0xDE, 0xCB);
    private static readonly SKColor WallShade = new(0xC6, 0xBB, 0xA2);
    private static readonly SKColor Outline   = new(0x2A, 0x24, 0x1C);
    private static readonly SKColor DoorCol    = new(0x33, 0x2A, 0x22);
    private static readonly SKColor WindowCol  = new(0x8F, 0xD6, 0xF5);

    private static (SKColor roof, SKColor roofDark, SKColor sign, string glyph) Theme(WorldObjectType kind) => kind switch
    {
        WorldObjectType.HealingCenter => (new(0xE0, 0x4B, 0x4B), new(0xA9, 0x2F, 0x2F), new(0xFF, 0xFF, 0xFF), "+"),
        WorldObjectType.Shop          => (new(0x3F, 0x8B, 0xE0), new(0x2C, 0x63, 0xA6), new(0xFF, 0xE0, 0x6A), "$"),
        WorldObjectType.Gym           => (new(0x8B, 0x5C, 0xF6), new(0x63, 0x3F, 0xB0), new(0xFF, 0xFF, 0xFF), "!"),
        _                             => (new(0x9A, 0x9A, 0x9A), new(0x6C, 0x6C, 0x6C), new(0xFF, 0xFF, 0xFF), "?"),
    };

    /// <param name="fp">footprint rect in screen space (whole building incl. door row)</param>
    public static void Draw(SKCanvas canvas, SKRect fp, WorldObjectType kind)
    {
        var (roof, roofDark, sign, glyph) = Theme(kind);
        float roofH = fp.Height * 0.42f;
        float wallTop = fp.Top + roofH;

        // Walls.
        P.Color = Outline; canvas.DrawRect(new SKRect(fp.Left - 2, wallTop - 2, fp.Right + 2, fp.Bottom + 2), P);
        P.Color = Wall;    canvas.DrawRect(new SKRect(fp.Left, wallTop, fp.Right, fp.Bottom), P);
        P.Color = WallShade; canvas.DrawRect(new SKRect(fp.Left, fp.Bottom - fp.Height * 0.10f, fp.Right, fp.Bottom), P);

        // Roof with overhang + eave shadow.
        float over = fp.Width * 0.08f;
        P.Color = Outline;  canvas.DrawRect(new SKRect(fp.Left - over - 2, fp.Top - 2, fp.Right + over + 2, wallTop + 2), P);
        P.Color = roof;     canvas.DrawRect(new SKRect(fp.Left - over, fp.Top, fp.Right + over, wallTop), P);
        P.Color = roofDark; canvas.DrawRect(new SKRect(fp.Left - over, wallTop - roofH * 0.22f, fp.Right + over, wallTop), P);

        // Windows.
        float wy = wallTop + (fp.Bottom - wallTop) * 0.22f;
        float ww = fp.Width * 0.14f, wh = ww;
        P.Color = Outline; canvas.DrawRect(new SKRect(fp.Left + fp.Width * 0.16f - 1, wy - 1, fp.Left + fp.Width * 0.16f + ww + 1, wy + wh + 1), P);
        P.Color = WindowCol; canvas.DrawRect(new SKRect(fp.Left + fp.Width * 0.16f, wy, fp.Left + fp.Width * 0.16f + ww, wy + wh), P);
        P.Color = Outline; canvas.DrawRect(new SKRect(fp.Right - fp.Width * 0.16f - ww - 1, wy - 1, fp.Right - fp.Width * 0.16f + 1, wy + wh + 1), P);
        P.Color = WindowCol; canvas.DrawRect(new SKRect(fp.Right - fp.Width * 0.16f - ww, wy, fp.Right - fp.Width * 0.16f, wy + wh), P);

        // Door (bottom centre).
        float dw = fp.Width * 0.22f, dh = fp.Height * 0.26f;
        float dx = fp.MidX - dw / 2f;
        P.Color = Outline; canvas.DrawRect(new SKRect(dx - 2, fp.Bottom - dh - 2, dx + dw + 2, fp.Bottom), P);
        P.Color = DoorCol; canvas.DrawRect(new SKRect(dx, fp.Bottom - dh, dx + dw, fp.Bottom), P);

        // Sign board above the door.
        float sw = fp.Width * 0.3f, sh = fp.Height * 0.14f;
        var sr = new SKRect(fp.MidX - sw / 2f, wallTop + 2, fp.MidX + sw / 2f, wallTop + 2 + sh);
        P.Color = Outline; canvas.DrawRect(new SKRect(sr.Left - 2, sr.Top - 2, sr.Right + 2, sr.Bottom + 2), P);
        P.Color = roofDark; canvas.DrawRect(sr, P);
        using var glyphPaint = new SKPaint { Color = sign, IsAntialias = true, TextSize = sh * 1.1f, TextAlign = SKTextAlign.Center, Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold) };
        canvas.DrawText(glyph, sr.MidX, sr.MidY + sh * 0.38f, glyphPaint);
    }
}
