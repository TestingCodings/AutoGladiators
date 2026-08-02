using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using AutoGladiators.Core.Assets;
using AutoGladiators.Core.Models.Exploration;
using AutoGladiators.Core.Utilities;

namespace AutoGladiators.Client.Game;

/// <summary>
/// SkiaSharp tile-map renderer. Reads terrain and objects from a Core
/// <see cref="WorldZone"/>, follows the player with an easing camera, draws
/// detailed procedural terrain (<see cref="TileArt"/>), wandering NPCs, and a
/// GBA-style dialogue overlay.
///
/// Efficiency: the surface is only invalidated while something is animating
/// (walking, camera catching up, ambient water, or active dialogue). At full
/// rest the loop idles at zero draw cost.
/// </summary>
public sealed class TileMapView : SKCanvasView, IDisposable
{
    public int TileSize { get; set; } = 48;

    private readonly Camera _camera = new();
    private WorldZone? _zone;
    private GameActor? _player;
    private readonly List<Npc> _npcs = new();
    private readonly record struct Building(int DoorX, int DoorY, WorldObjectType Kind, string Name);
    private readonly List<Building> _buildings = new();
    private readonly DialogueBox _dialogue = new();
    private readonly Dictionary<string, SpriteSheet> _sheetCache = new();
    private readonly Random _rng = new();

    private IDispatcherTimer? _timer;
    private readonly Stopwatch _clock = new();
    private double _lastSeconds;

    private Facing? _heldDir;
    private bool _cameraInitialised;

    // Lightweight footstep-dust particles (world-space).
    private struct Particle { public float X, Y, Vx, Vy, Life, Max; }
    private readonly List<Particle> _particles = new();
    private readonly SKColor _playerCore = new(0xFF, 0x8C, 0x2A);
    private float _ambientTime;
    private int _ambientFrame;

    // Neutral NPC sprite IDs matching entries in Config/AssetManifest.json.
    // The catalog resolves each ID to an actual file path; see BuildDefaultCatalog()
    // for the current transitional mapping (pending anonymization).
    private static readonly string[] NpcSpriteIds =
        { "npc_01", "npc_02", "npc_03", "npc_04", "npc_05", "npc_06", "npc_07", "npc_08", "npc_09" };

    // Resolved at InitAsync time; defaults to Empty if the manifest cannot be loaded.
    private AssetCatalog _catalog = AssetCatalog.Empty;

    private static readonly SKPaint ShadowPaint = new()
    { IsAntialias = true, Style = SKPaintStyle.Fill, Color = new SKColor(0, 0, 0, 70) };
    private static readonly SKColor Backdrop = new(0x10, 0x12, 0x18);

    private const float CameraLerp = 10f;

    public TileMapView()
    {
        EnableTouchEvents = false;
        PaintSurface += OnPaint;
    }

    public async Task InitAsync(WorldZone zone, int playerTileX, int playerTileY)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(zone);

        StopLoop();
        _zone = zone;
        _player = new GameActor(null, playerTileX, playerTileY, TileSize); // procedural robot
        _cameraInitialised = false;
        _catalog = await TryLoadCatalogAsync();
        _heldDir = null;
        _particles.Clear();
        _dialogue.Close();
        _ambientTime = 0f;
        _ambientFrame = 0;
        await SpawnNpcsAsync(zone);
        StartLoop();
        InvalidateSurface();
    }

    private async Task SpawnNpcsAsync(WorldZone zone)
    {
        _npcs.Clear();
        _buildings.Clear();
        int humanIndex = 0;

        foreach (var obj in zone.Objects)
        {
            if (obj.Type is WorldObjectType.HealingCenter or WorldObjectType.Shop or WorldObjectType.Gym)
            {
                _buildings.Add(new Building(obj.Position.X, obj.Position.Y, obj.Type, obj.Name));
                continue;
            }

            bool isCharacter = obj.Type is WorldObjectType.NPC or WorldObjectType.Trainer or WorldObjectType.QuestGiver;
            if (!isCharacter) continue;

            var lines = obj.GetProperty<string[]>("lines") ?? new[] { obj.Name + ": ...", "Nothing to say." };
            bool asRobot = obj.Type == WorldObjectType.Trainer && (humanIndex % 2 == 0);

            SpriteSheet? sheet = null;
            if (!asRobot)
            {
                string id = NpcSpriteIds[humanIndex % NpcSpriteIds.Length];
                sheet = await TryLoadSheetAsync(id);
                humanIndex++;
            }
            else humanIndex++;

            var core = obj.Type == WorldObjectType.Trainer
                ? new SKColor(0xFF, 0x3B, 0x3B)   // enemy trainers glow red
                : new SKColor(0x4A, 0xC8, 0xFF);
            int radius = obj.Type == WorldObjectType.QuestGiver ? 0 : 3;

            var actor = new GameActor(sheet, obj.Position.X, obj.Position.Y, TileSize);
            _npcs.Add(new Npc(actor, obj.Name, lines, obj.Type, radius, core, _rng));
        }
    }

    private bool IsBlockedByBuildingFootprint(int x, int y) =>
        _buildings.Any(building => BuildingFootprint.ContainsBlockingTile(building.DoorX, building.DoorY, x, y));

    private async Task<SpriteSheet> GetSheetAsync(string key)
    {
        if (_sheetCache.TryGetValue(key, out var s)) return s;
        s = await SpriteSheet.LoadAsync($"sprites/{key}.png", 16, 16);
        _sheetCache[key] = s;
        return s;
    }

    /// <summary>
    /// Resolves a neutral asset ID to a sprite sheet using the catalog, with a
    /// convention-based fallback. Returns null if no file is found so the caller
    /// can fall back to procedural drawing.
    /// </summary>
    private async Task<SpriteSheet?> TryLoadSheetAsync(string id)
    {
        if (_sheetCache.TryGetValue(id, out var cached)) return cached;

        // 1. Manifest catalog (populated from AssetManifest.json at startup).
        var path = _catalog.GetExplorationSheetPath(id);

        // 2. Convention fallback: sprites/{id}.png (the expected post-anonymization name).
        path ??= $"sprites/{id}.png";

        try
        {
            var sheet = await SpriteSheet.LoadAsync(path, 16, 16);
            _sheetCache[id] = sheet;
            return sheet;
        }
        catch
        {
            // Sheet not found — NPC will be drawn as a procedural robot.
            return null;
        }
    }

    /// <summary>
    /// Attempts to load the asset catalog from Resources/Raw/config/AssetManifest.json.
    /// Falls back to the compiled-in default catalog if the file is unavailable.
    /// </summary>
    private async Task<AssetCatalog> TryLoadCatalogAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("config/AssetManifest.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            return new AssetCatalog(AssetManifestLoader.LoadFromJson(json));
        }
        catch
        {
            return BuildDefaultCatalog();
        }
    }

    /// <summary>
    /// Transitional default catalog used when the manifest file is not available at
    /// runtime. Maps neutral IDs to the current personal-name sprite files.
    /// Once the anonymization PR lands and files are renamed to npc_01.png etc.,
    /// this method should be removed — the manifest file will be the sole source.
    /// </summary>
    private static AssetCatalog BuildDefaultCatalog()
    {
        var manifest = new AssetManifest
        {
            Assets = new List<AssetManifestEntry>
            {
                MakeEntry("npc_01", "sprites/vince.png"),
                MakeEntry("npc_02", "sprites/chad.png"),
                MakeEntry("npc_03", "sprites/blue.png"),
                MakeEntry("npc_04", "sprites/bradley.png"),
                MakeEntry("npc_05", "sprites/colin.png"),
                MakeEntry("npc_06", "sprites/john.png"),
                MakeEntry("npc_07", "sprites/julian.png"),
                MakeEntry("npc_08", "sprites/maria.png"),
                MakeEntry("npc_09", "sprites/simon.png"),
            }
        };
        return new AssetCatalog(manifest);
    }

    private static AssetManifestEntry MakeEntry(string id, string path) => new()
    {
        Id = id, Category = "npc", ExplorationSheet = path,
        FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
        SourceCategory = "pending-anonymization", ReviewStatus = "pending-anonymization",
    };

    // ---- Loop -------------------------------------------------------------

    public void StartLoop()
    {
        if (_timer is not null) return;
        _clock.Restart();
        _lastSeconds = _clock.Elapsed.TotalSeconds;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void StopLoop()
    {
        if (_timer is not null)
        {
            _timer.Tick -= OnTick;
            _timer.Stop();
            _timer = null;
        }

        _clock.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        double now = _clock.Elapsed.TotalSeconds;
        float dt = (float)Math.Min(0.05, now - _lastSeconds);
        _lastSeconds = now;
        if (_player is null || _zone is null) return;

        bool dialogueActive = _dialogue.IsActive;
        if (dialogueActive)
        {
            _player.Update(dt); // finish any in-progress step, but no new input
            _dialogue.Update(dt);
            InvalidateSurface();
            return;
        }

        if (!_player.IsMoving && _heldDir is { } dir) TryStep(dir);
        bool moved = _player.IsMoving;
        _player.Update(dt);

        foreach (var npc in _npcs)
            npc.Update(dt, _zone, (x, y) => IsOccupiedExcept(npc, x, y));

        bool cameraSettling = UpdateCamera(dt);
        UpdateParticles(dt);

        _ambientTime += dt;
        int ambientFrame = (int)(_ambientTime * 6f);
        bool ambientTick = ambientFrame != _ambientFrame;
        _ambientFrame = ambientFrame;
        bool npcsMoving = _npcs.Any(n => n.Actor.IsMoving);

        if (moved || _player.IsMoving || cameraSettling || ambientTick || npcsMoving || _particles.Count > 0)
            InvalidateSurface();
    }

    private bool UpdateCamera(float dt)
    {
        if (_player is null) return false;
        float targetX = _player.PixelX + TileSize / 2f;
        float targetY = _player.PixelY + TileSize / 2f;
        if (!_cameraInitialised)
        {
            _camera.CenterOn(targetX, targetY);
            _cameraInitialised = true;
            return true;
        }
        float t = Math.Clamp(CameraLerp * dt, 0f, 1f);
        _camera.X += (targetX - _camera.X) * t;
        _camera.Y += (targetY - _camera.Y) * t;
        return Math.Abs(targetX - _camera.X) > 0.3f || Math.Abs(targetY - _camera.Y) > 0.3f;
    }

    // ---- Occupancy / collision -------------------------------------------

    private bool IsOccupied(int x, int y)
    {
        if (IsBlockedByBuildingFootprint(x, y)) return true;
        if (_player is { } p && p.TileX == x && p.TileY == y) return true;
        return _npcs.Any(n => n.TileX == x && n.TileY == y);
    }

    private bool IsOccupiedExcept(Npc self, int x, int y)
    {
        if (IsBlockedByBuildingFootprint(x, y)) return true;
        if (_player is { } p && p.TileX == x && p.TileY == y) return true;
        return _npcs.Any(n => n != self && n.TileX == x && n.TileY == y);
    }

    // ---- Input ------------------------------------------------------------

    public void HoldDirection(Facing facing)
    {
        if (_dialogue.IsActive) return;
        _heldDir = facing;
        if (_player is { IsMoving: false }) TryStep(facing);
    }

    public void ReleaseDirection(Facing facing)
    {
        if (_heldDir == facing) _heldDir = null;
    }

    public bool TryMove(Facing facing) => !_dialogue.IsActive && TryStep(facing);

    /// <summary>Confirm/interact: advance dialogue, or talk to whoever the player faces.</summary>
    public void Interact()
    {
        if (_dialogue.IsActive) { _dialogue.Advance(); InvalidateSurface(); return; }
        if (_player is null || _player.IsMoving) return;

        var (dx, dy) = _player.Facing.Delta();
        int fx = _player.TileX + dx, fy = _player.TileY + dy;

        var npc = _npcs.FirstOrDefault(n => n.TileX == fx && n.TileY == fy);
        if (npc is not null)
        {
            npc.Face(_player.Facing.Opposite());   // look back at the player
            _dialogue.Open(npc.Name, npc.Dialogue, npc.IsRobot, npc.RobotCore);
            InvalidateSurface();
            return;
        }

        // Enter a building by facing its door, or by standing on the door tile.
        var door = _buildings.FirstOrDefault(b =>
            (b.DoorX == fx && b.DoorY == fy) || (b.DoorX == _player.TileX && b.DoorY == _player.TileY));
        if (door.Name is not null)
        {
            var (name, lines, colour) = door.Kind switch
            {
                WorldObjectType.HealingCenter => (door.Name, new[] { "Welcome! Let's tend to your bots.", "Your team is fully restored." }, new SKColor(0xE0, 0x4B, 0x4B)),
                WorldObjectType.Shop          => (door.Name, new[] { "Welcome to the shop!", "Buy something and I'll upgrade your rig." }, new SKColor(0x3F, 0x8B, 0xE0)),
                _                             => (door.Name, new[] { "The gym leader isn't ready for you yet." }, new SKColor(0x8B, 0x5C, 0xF6)),
            };
            BuildingEntered?.Invoke(door.Kind);
            _dialogue.Open(name, lines, robot: false, colour);
            InvalidateSurface();
        }
    }

    /// <summary>Raised when the player enters a building door (heal, shop, gym). Wired to real services on port.</summary>
    public event Action<WorldObjectType>? BuildingEntered;

    private bool TryStep(Facing facing)
    {
        if (_zone is null || _player is null || _player.IsMoving) return false;
        var (dx, dy) = facing.Delta();
        int nx = _player.TileX + dx, ny = _player.TileY + dy;
        if (!_zone.IsPassable(nx, ny) || IsOccupied(nx, ny))
        {
            _player.Facing = facing;
            InvalidateSurface();
            return false;
        }
        // Kick up dust from the tile being left (skip on water).
        if (_zone.GetTile(_player.TileX, _player.TileY)?.Type != TileType.Water)
            SpawnDust(_player.PixelX + TileSize / 2f, _player.PixelY + TileSize * 0.9f);

        _player.StartMoveTo(nx, ny, facing);
        PlayerStepped?.Invoke(nx, ny);
        return true;
    }

    /// <summary>Raised when the player commits a step to a new tile (logical position updated).</summary>
    public event Action<int, int>? PlayerStepped;

    /// <summary>True once a zone and player have been initialised.</summary>
    public bool IsReady => _player is not null && _zone is not null;

    private void SpawnDust(float wx, float wy)
    {
        int n = 2 + _rng.Next(2);
        for (int i = 0; i < n; i++)
        {
            float ang = (float)(_rng.NextDouble() * Math.PI * 2);
            _particles.Add(new Particle
            {
                X = wx, Y = wy,
                Vx = (float)Math.Cos(ang) * 8f,
                Vy = -6f - (float)_rng.NextDouble() * 8f,
                Life = 0, Max = 0.35f + (float)_rng.NextDouble() * 0.2f,
            });
        }
    }

    private void UpdateParticles(float dt)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Life += dt;
            if (p.Life >= p.Max) { _particles.RemoveAt(i); continue; }
            p.X += p.Vx * dt;
            p.Y += p.Vy * dt;
            p.Vy += 20f * dt; // gentle gravity settle
            _particles[i] = p;
        }
    }

    private void DrawParticles(SKCanvas canvas)
    {
        if (_particles.Count == 0) return;
        using var paint = new SKPaint { IsAntialias = false };
        foreach (var p in _particles)
        {
            float k = 1f - p.Life / p.Max;
            paint.Color = new SKColor(0xD8, 0xCE, 0xB4, (byte)(150 * k));
            var s = _camera.WorldToScreen(p.X, p.Y);
            float r = TileSize * 0.06f * (0.6f + k);
            canvas.DrawRect(new SKRect(s.X - r, s.Y - r, s.X + r, s.Y + r), paint);
        }
    }

    // ---- Rendering --------------------------------------------------------

    private void OnPaint(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(Backdrop);
        if (_zone is null || _player is null) return;

        float scale = (float)(e.Info.Width / Width);
        if (float.IsNaN(scale) || scale <= 0) scale = 1f;
        canvas.Scale(scale);

        _camera.ViewportW = (float)Width;
        _camera.ViewportH = (float)Height;

        TileArt.AnimPhase = _ambientTime;
        DrawTiles(canvas);
        DrawEdges(canvas);
        DrawParticles(canvas);
        DrawCharacters(canvas);
        DrawDayTint(canvas);
        DrawLighting(canvas);
        if (_dialogue.IsActive) DrawDialogue(canvas);
    }

    /// <summary>
    /// Edge pass: foam where water meets land and cliff faces below mountains,
    /// so terrain reads as layered depth rather than flat coloured squares.
    /// </summary>
    private void DrawEdges(SKCanvas canvas)
    {
        if (_zone is null) return;
        float left = _camera.X - _camera.ViewportW / 2f;
        float top = _camera.Y - _camera.ViewportH / 2f;
        int startX = Math.Max(0, (int)(left / TileSize) - 1);
        int startY = Math.Max(0, (int)(top / TileSize) - 1);
        int endX = Math.Min(_zone.Width - 1, (int)((left + _camera.ViewportW) / TileSize) + 1);
        int endY = Math.Min(_zone.Height - 1, (int)((top + _camera.ViewportH) / TileSize) + 1);

        using var foam = new SKPaint { Color = new SKColor(0xCF, 0xEC, 0xFF), IsAntialias = false };
        using var cliff = new SKPaint { Color = new SKColor(0x5C, 0x52, 0x44), IsAntialias = false };
        using var cliffLip = new SKPaint { Color = new SKColor(0xB2, 0xA8, 0x94), IsAntialias = false };
        float e = TileSize * 0.16f;

        TileType TypeAt(int x, int y) => _zone!.GetTile(x, y)?.Type ?? TileType.Grass;

        for (int ty = startY; ty <= endY; ty++)
            for (int tx = startX; tx <= endX; tx++)
            {
                var t = TypeAt(tx, ty);
                var s = _camera.WorldToScreen(tx * TileSize, ty * TileSize);

                if (t == TileType.Water)
                {
                    // Foam rim on edges that touch land.
                    if (TypeAt(tx, ty - 1) != TileType.Water) canvas.DrawRect(new SKRect(s.X, s.Y, s.X + TileSize, s.Y + e), foam);
                    if (TypeAt(tx, ty + 1) != TileType.Water) canvas.DrawRect(new SKRect(s.X, s.Y + TileSize - e, s.X + TileSize, s.Y + TileSize), foam);
                    if (TypeAt(tx - 1, ty) != TileType.Water) canvas.DrawRect(new SKRect(s.X, s.Y, s.X + e, s.Y + TileSize), foam);
                    if (TypeAt(tx + 1, ty) != TileType.Water) canvas.DrawRect(new SKRect(s.X + TileSize - e, s.Y, s.X + TileSize, s.Y + TileSize), foam);
                }
                else if (t == TileType.Mountain && TypeAt(tx, ty + 1) != TileType.Mountain)
                {
                    // Cliff face on the exposed south edge for a sense of height.
                    canvas.DrawRect(new SKRect(s.X, s.Y + TileSize - e * 2, s.X + TileSize, s.Y + TileSize), cliff);
                    canvas.DrawRect(new SKRect(s.X, s.Y + TileSize - e * 2, s.X + TileSize, s.Y + TileSize - e * 2 + 2), cliffLip);
                }
            }
    }

    private static readonly SKColor DayTint = new(0x3A, 0x40, 0x66, 26); // subtle cool wash
    private void DrawDayTint(SKCanvas canvas)
    {
        using var p = new SKPaint { Color = DayTint, IsAntialias = false };
        canvas.DrawRect(new SKRect(0, 0, _camera.ViewportW, _camera.ViewportH), p);
    }

    private void DrawTiles(SKCanvas canvas)
    {
        if (_zone is null) return;
        float left = _camera.X - _camera.ViewportW / 2f;
        float top = _camera.Y - _camera.ViewportH / 2f;
        int startX = Math.Max(0, (int)(left / TileSize) - 1);
        int startY = Math.Max(0, (int)(top / TileSize) - 1);
        int endX = Math.Min(_zone.Width - 1, (int)((left + _camera.ViewportW) / TileSize) + 1);
        int endY = Math.Min(_zone.Height - 1, (int)((top + _camera.ViewportH) / TileSize) + 1);

        for (int ty = startY; ty <= endY; ty++)
            for (int tx = startX; tx <= endX; tx++)
            {
                var type = _zone.GetTile(tx, ty)?.Type ?? TileType.Grass;
                var s = _camera.WorldToScreen(tx * TileSize, ty * TileSize);
                TileArt.Draw(canvas, type, new SKRect(s.X, s.Y, s.X + TileSize, s.Y + TileSize), tx, ty);
            }
    }

    /// <summary>Draw buildings + player + NPCs back-to-front (y-sorted by ground line).</summary>
    private void DrawCharacters(SKCanvas canvas)
    {
        if (_player is null) return;
        var drawables = new List<(float y, Action draw)>(_npcs.Count + _buildings.Count + 1)
        {
            (_player.PixelY + TileSize, () => DrawCharacter(canvas, _player, robot: true, _playerCore))
        };
        foreach (var n in _npcs)
        {
            var npc = n;
            drawables.Add((npc.Actor.PixelY + TileSize, () => DrawCharacter(canvas, npc.Actor, npc.IsRobot, npc.RobotCore)));
        }
        foreach (var b in _buildings)
        {
            var bld = b;
            float baseWorldY = (bld.DoorY + 1) * TileSize;
            drawables.Add((baseWorldY, () => DrawBuilding(canvas, bld)));
        }
        foreach (var d in drawables.OrderBy(d => d.y)) d.draw();
    }

    private void DrawBuilding(SKCanvas canvas, Building b)
    {
        var s = _camera.WorldToScreen((b.DoorX - 1) * TileSize, (b.DoorY - 2) * TileSize);
        var fp = new SKRect(s.X, s.Y, s.X + TileSize * 3, s.Y + TileSize * 3);
        BuildingArt.Draw(canvas, fp, b.Kind);
    }

    private void DrawCharacter(SKCanvas canvas, GameActor actor, bool robot, SKColor core)
    {
        var s = _camera.WorldToScreen(actor.PixelX, actor.PixelY);
        float footX = s.X + TileSize / 2f;
        float footY = s.Y + TileSize * 0.94f;

        canvas.DrawOval(new SKRect(footX - TileSize * 0.28f, footY - TileSize * 0.09f,
                                   footX + TileSize * 0.28f, footY + TileSize * 0.09f), ShadowPaint);

        if (robot || actor.Sheet is null)
            CharacterArt.DrawRobot(canvas, footX, footY, TileSize / 16f, actor.Facing, actor.AnimTime, actor.IsMoving, core);
        else
        {
            float lift = TileSize * 0.12f;
            var dest = new SKRect(s.X, s.Y - lift, s.X + TileSize, s.Y + TileSize - lift);
            actor.Sheet.DrawFrame(canvas, actor.FrameIndex(), dest);
        }
    }

    private SKPaint? _vignette;
    private SKSize _vignetteFor;

    private void DrawLighting(SKCanvas canvas)
    {
        float w = _camera.ViewportW, h = _camera.ViewportH;
        if (w <= 0 || h <= 0) return;
        if (_vignette is null || _vignetteFor.Width != w || _vignetteFor.Height != h)
        {
            _vignette?.Dispose();
            var center = new SKPoint(w / 2f, h / 2f);
            float radius = Math.Max(w, h) * 0.72f;
            var shader = SKShader.CreateRadialGradient(center, radius,
                new[] { new SKColor(0, 0, 0, 0), new SKColor(0, 0, 0, 0), new SKColor(0x08, 0x0A, 0x12, 150) },
                new[] { 0f, 0.62f, 1f }, SKShaderTileMode.Clamp);
            _vignette = new SKPaint { Shader = shader, IsAntialias = false };
            _vignetteFor = new SKSize(w, h);
        }
        canvas.DrawRect(new SKRect(0, 0, w, h), _vignette!);
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        StopLoop();
        PaintSurface -= OnPaint;
        _dialogue.Close();
        _particles.Clear();
        _buildings.Clear();
        _npcs.Clear();

        foreach (var sheet in _sheetCache.Values)
            sheet.Dispose();
        _sheetCache.Clear();

        _vignette?.Dispose();
        _vignette = null;

        BuildingEntered = null;
        PlayerStepped = null;
        _zone = null;
        _player = null;
        _disposed = true;
    }

    // ---- Dialogue overlay -------------------------------------------------

    private static readonly SKPaint BodyText = new()
    { Color = SKColors.White, IsAntialias = true, TextSize = 18, Typeface = SKTypeface.Default };
    private static readonly SKPaint NameText = new()
    { Color = new SKColor(0x8F, 0xD6, 0xF5), IsAntialias = true, TextSize = 16, Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold) };

    private void DrawDialogue(SKCanvas canvas)
    {
        float w = _camera.ViewportW, h = _camera.ViewportH;
        float margin = 16, boxH = Math.Min(150, h * 0.32f);
        var box = new SKRect(margin, h - boxH - margin, w - margin, h - margin);

        using var panel = new SKPaint { Color = new SKColor(0x12, 0x16, 0x24, 0xF2), IsAntialias = true };
        using var border = new SKPaint { Color = new SKColor(0x5A, 0xC8, 0xFA), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
        canvas.DrawRoundRect(box, 12, 12, panel);
        canvas.DrawRoundRect(box, 12, 12, border);

        // Portrait swatch (mini robot glyph or coloured initial).
        float pad = 14;
        var portrait = new SKRect(box.Left + pad, box.Top + pad, box.Left + pad + 56, box.Top + pad + 56);
        using var pfill = new SKPaint { Color = new SKColor(0x1E, 0x24, 0x36), IsAntialias = true };
        canvas.DrawRoundRect(portrait, 8, 8, pfill);
        if (_dialogue.SpeakerIsRobot)
            CharacterArt.DrawRobot(canvas, portrait.MidX, portrait.Bottom - 8, 2.6f, Facing.Down, 0, false, _dialogue.SpeakerCore);
        else
        {
            using var initP = new SKPaint { Color = _dialogue.SpeakerCore, IsAntialias = true, TextSize = 30, Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold) };
            string initial = _dialogue.SpeakerName.Length > 0 ? _dialogue.SpeakerName[..1] : "?";
            canvas.DrawText(initial, portrait.MidX - 9, portrait.MidY + 10, initP);
        }

        float textLeft = portrait.Right + pad;
        float textTop = box.Top + pad + 14;

        canvas.DrawText(_dialogue.SpeakerName, textLeft, box.Top + pad + 4, NameText);

        float maxW = box.Right - pad - textLeft;
        float lineH = 24;
        int i = 0;
        foreach (var line in WrapText(_dialogue.VisibleText, BodyText, maxW))
            canvas.DrawText(line, textLeft, textTop + 18 + lineH * i++, BodyText);

        // Blinking continue arrow once the line is fully revealed.
        if (_dialogue.LineFullyShown && ((int)(_ambientTime * 3) & 1) == 0)
        {
            using var arrow = new SKPaint { Color = new SKColor(0x5A, 0xC8, 0xFA), IsAntialias = true };
            float ax = box.Right - 26, ay = box.Bottom - 20;
            using var path = new SKPath();
            path.MoveTo(ax, ay); path.LineTo(ax + 12, ay); path.LineTo(ax + 6, ay + 8); path.Close();
            canvas.DrawPath(path, arrow);
        }
    }

    private static IEnumerable<string> WrapText(string text, SKPaint font, float maxWidth)
    {
        var words = text.Split(' ');
        var line = "";
        foreach (var word in words)
        {
            var trial = line.Length == 0 ? word : line + " " + word;
            if (font.MeasureText(trial) > maxWidth && line.Length > 0)
            {
                yield return line;
                line = word;
            }
            else line = trial;
        }
        if (line.Length > 0) yield return line;
    }
}
