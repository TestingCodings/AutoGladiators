using SkiaSharp;
using AutoGladiators.Core.Models.Exploration;

namespace AutoGladiators.Client.Game;

/// <summary>
/// A world character: a moving <see cref="GameActor"/> plus presentation and
/// dialogue. Wanders randomly within a radius of its home tile, respecting
/// terrain and other occupants. Rendered either from a sprite sheet (humans)
/// or procedurally as a robot (<see cref="CharacterArt"/>).
/// </summary>
public sealed class Npc
{
    public GameActor Actor { get; }
    public string Name { get; }
    public string[] Dialogue { get; }
    public WorldObjectType Kind { get; }

    public bool IsRobot => Actor.Sheet is null;
    public SKColor RobotCore { get; }

    private readonly int _homeX, _homeY, _radius;
    private readonly Random _rng;
    private float _cooldown;

    public int TileX => Actor.TileX;
    public int TileY => Actor.TileY;

    public Npc(GameActor actor, string name, string[] dialogue, WorldObjectType kind,
               int radius, SKColor robotCore, Random rng)
    {
        Actor = actor;
        Name = name;
        Dialogue = dialogue;
        Kind = kind;
        _homeX = actor.TileX;
        _homeY = actor.TileY;
        _radius = radius;
        RobotCore = robotCore;
        _rng = rng;
        _cooldown = 0.5f + (float)_rng.NextDouble() * 2f;
    }

    /// <summary>Turn to face a given direction (used when the player talks to them).</summary>
    public void Face(Facing f) => Actor.Facing = f;

    public void Update(float dt, WorldZone zone, Func<int, int, bool> occupied)
    {
        Actor.Update(dt);
        if (Actor.IsMoving || _radius == 0) return;

        _cooldown -= dt;
        if (_cooldown > 0) return;
        _cooldown = 1.4f + (float)_rng.NextDouble() * 2.6f;

        // Idle most of the time so the world feels calm, not twitchy.
        if (_rng.NextDouble() < 0.45) return;

        var dir = (Facing)_rng.Next(4);
        var (dx, dy) = dir.Delta();
        int nx = Actor.TileX + dx, ny = Actor.TileY + dy;

        if (Math.Abs(nx - _homeX) > _radius || Math.Abs(ny - _homeY) > _radius)
        {
            Actor.Facing = dir; // turn but stay home-bound
            return;
        }
        if (!zone.IsPassable(nx, ny) || occupied(nx, ny))
        {
            Actor.Facing = dir;
            return;
        }
        Actor.StartMoveTo(nx, ny, dir);
    }
}
