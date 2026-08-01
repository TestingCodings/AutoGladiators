using SkiaSharp;

namespace AutoGladiators.Client.Game;

/// <summary>
/// Draws the player as an original robot-gladiator directly on the canvas — no
/// sprite sheet, so the character is unique to AutoGladiators and animates
/// freely (walk bob, swinging limbs, pulsing energy core). Everything is aliased
/// blocky pixel work with dark outlines for a crisp, readable-at-distance look.
/// </summary>
public static class CharacterArt
{
    private static readonly SKPaint P = new() { IsAntialias = false, Style = SKPaintStyle.Fill };

    // Palette — steel body with a cyan visor and a warm energy core.
    private static readonly SKColor Outline  = new(0x0E, 0x12, 0x1A);
    private static readonly SKColor Steel     = new(0x74, 0x82, 0x92);
    private static readonly SKColor SteelDark = new(0x4A, 0x55, 0x62);
    private static readonly SKColor SteelLite = new(0xA6, 0xB3, 0xC0);
    private static readonly SKColor Visor     = new(0x2C, 0xE8, 0xF5);
    private static readonly SKColor VisorDim  = new(0x12, 0x9A, 0xB0);

    /// <param name="footX">screen x of the character's ground point (centre)</param>
    /// <param name="footY">screen y of the character's ground point (feet)</param>
    /// <param name="u">art-pixel size in screen units (TileSize / 16)</param>
    public static void DrawRobot(SKCanvas canvas, float footX, float footY, float u,
                                 Facing facing, float animTime, bool isMoving, SKColor core)
    {
        // 4-phase walk: centre, step, centre, step. Alternate feet + gentle bob.
        int phase = isMoving ? (int)(animTime * 8f) % 4 : 0;
        int bob = isMoving ? (phase == 1 || phase == 3 ? -1 : 0) : 0;
        int stepA = phase == 1 ? 1 : (phase == 3 ? -1 : 0); // left leg / lead arm
        int stepB = -stepA;                                  // right leg / trail arm

        void R(int ax, int ay, int aw, int ah, SKColor c)
        {
            float l = footX + ax * u;
            float t = footY + (ay + bob) * u;
            P.Color = c;
            canvas.DrawRect(new SKRect(l, t, l + aw * u, t + ah * u), P);
        }

        bool side = facing is Facing.Left or Facing.Right;
        int dir = facing == Facing.Left ? -1 : 1; // for side facing, which way it leans

        // ---- Legs (drawn first, behind the torso) ----
        R(-5, -6 + stepA, 4, 7, Outline);
        R(-4, -6 + stepA, 2, 6, SteelDark);
        R(1, -6 + stepB, 4, 7, Outline);
        R(2, -6 + stepB, 2, 6, SteelDark);

        // ---- Torso ----
        R(-7, -16, 14, 11, Outline);
        R(-6, -15, 12, 9, Steel);
        R(-6, -15, 12, 2, SteelLite);       // top bevel highlight
        R(-6, -8, 12, 2, SteelDark);        // bottom shade

        // ---- Energy core (pulses) ----
        float pulse = 0.6f + 0.4f * (float)Math.Sin(animTime * 6f);
        var coreLit = new SKColor(core.Red, core.Green, core.Blue, (byte)(160 + 95 * pulse));
        R(-2, -13, 4, 4, Outline);
        R(-1, -12, 2, 2, coreLit);

        // ---- Arms (swing opposite the legs) ----
        R(-9, -15 + stepB, 3, 8, Outline);
        R(-8, -14 + stepB, 1, 6, Steel);
        R(6, -15 + stepA, 3, 8, Outline);
        R(7, -14 + stepA, 1, 6, Steel);

        // ---- Head ----
        R(-5, -23, 10, 8, Outline);
        R(-4, -22, 8, 6, Steel);
        R(-4, -22, 8, 1, SteelLite);
        // Antenna with glowing tip.
        R(0, -26, 1, 3, Outline);
        R(0, -27, 1, 1, Visor);

        // ---- Visor / face, direction-dependent ----
        switch (facing)
        {
            case Facing.Down:
                R(-3, -21, 6, 3, VisorDim);
                R(-3, -21, 6, 2, Visor);
                break;
            case Facing.Up: // back of head — cooling vents, no visor
                R(-3, -21, 6, 3, SteelDark);
                R(-2, -21, 1, 3, Outline);
                R(1, -21, 1, 3, Outline);
                break;
            default: // side profile — visor pushed to the leading edge
                int vx = side ? (dir < 0 ? -3 : 1) : -3;
                R(vx, -21, 3, 3, VisorDim);
                R(vx, -21, 3, 2, Visor);
                break;
        }
    }
}
