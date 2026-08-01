namespace AutoGladiators.Client.Game;

public enum Facing
{
    // Values match the row order of the GBA-style sheet: down/up/left/right.
    Down = 0,
    Up = 1,
    Left = 2,
    Right = 3,
}

public static class FacingExtensions
{
    public static (int dx, int dy) Delta(this Facing f) => f switch
    {
        Facing.Up => (0, -1),
        Facing.Down => (0, 1),
        Facing.Left => (-1, 0),
        Facing.Right => (1, 0),
        _ => (0, 0),
    };

    public static Facing Opposite(this Facing f) => f switch
    {
        Facing.Up => Facing.Down,
        Facing.Down => Facing.Up,
        Facing.Left => Facing.Right,
        _ => Facing.Left,
    };
}

/// <summary>
/// A sprite-sheet character that lives on the tile grid and animates its walk
/// between tiles. Logical tile position updates immediately when a move starts;
/// the render pixel position eases toward the destination over StepDuration.
/// </summary>
public sealed class GameActor
{
    /// <summary>Optional sprite sheet. Null for actors drawn procedurally (e.g. the robot player).</summary>
    public SpriteSheet? Sheet { get; }
    public int TileX { get; private set; }
    public int TileY { get; private set; }
    public float PixelX { get; private set; }
    public float PixelY { get; private set; }
    public Facing Facing { get; set; } = Facing.Down;
    public bool IsMoving { get; private set; }

    /// <summary>Seconds elapsed in the current walk, for driving procedural animation.</summary>
    public float AnimTime => _animTime;

    private readonly int _tileSize;
    private float _startX, _startY, _destX, _destY;
    private float _moveT;
    private float _animTime;

    public const float StepDuration = 0.18f; // seconds to cross one tile
    private const int WalkFps = 8;

    // Walk cycle within a facing row: centre, left-foot, centre, right-foot.
    private static readonly int[] WalkCycle = { 1, 0, 1, 2 };

    public GameActor(SpriteSheet? sheet, int tileX, int tileY, int tileSize)
    {
        if (tileSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be positive.");

        Sheet = sheet;
        _tileSize = tileSize;
        TileX = tileX;
        TileY = tileY;
        PixelX = tileX * tileSize;
        PixelY = tileY * tileSize;
        _startX = PixelX;
        _startY = PixelY;
        _destX = PixelX;
        _destY = PixelY;
    }

    /// <summary>Begin easing to an adjacent tile. Caller has already validated passability.</summary>
    public void StartMoveTo(int tileX, int tileY, Facing facing)
    {
        Facing = facing;

        if (tileX == TileX && tileY == TileY)
        {
            IsMoving = false;
            _moveT = 0f;
            _animTime = 0f;
            return;
        }

        TileX = tileX;
        TileY = tileY;
        _startX = PixelX;
        _startY = PixelY;
        _destX = tileX * _tileSize;
        _destY = tileY * _tileSize;
        _moveT = 0f;
        IsMoving = true;
    }

    public void Update(float dt)
    {
        if (!IsMoving)
        {
            _animTime = 0f;
            return;
        }

        if (dt <= 0f)
            return;

        _moveT += dt;
        _animTime += dt;
        float p = Math.Clamp(_moveT / StepDuration, 0f, 1f);
        PixelX = Lerp(_startX, _destX, p);
        PixelY = Lerp(_startY, _destY, p);

        if (p >= 1f)
        {
            IsMoving = false;
            PixelX = _destX;
            PixelY = _destY;
        }
    }

    public int FrameIndex()
    {
        int rowStart = (int)Facing * 3;
        if (!IsMoving)
            return rowStart + 1; // idle = centre frame
        int step = WalkCycle[(int)(_animTime * WalkFps) % WalkCycle.Length];
        return rowStart + step;
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
