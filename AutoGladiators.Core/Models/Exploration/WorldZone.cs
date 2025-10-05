using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoGladiators.Core.Models.Exploration
{
    /// <summary>
    /// Represents a zone/area in the game world (like a route, city, or dungeon)
    /// </summary>
    public class WorldZone
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Width { get; }
        public int Height { get; }
        
        // 2D grid of tiles
        private MapTile[,] _tiles;
        
        // Zone connections
        public Dictionary<string, ZoneConnection> Connections { get; }
        
        // NPCs and interactive objects
        public List<WorldObject> Objects { get; }
        
        // Zone-specific settings
        public Dictionary<string, object> ZoneProperties { get; }
        
        public WorldZone(string id, string name, string description, int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Zone dimensions must be positive");
                
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? string.Empty;
            Width = width;
            Height = height;
            
            _tiles = new MapTile[width, height];
            Connections = new Dictionary<string, ZoneConnection>();
            Objects = new List<WorldObject>();
            ZoneProperties = new Dictionary<string, object>();
            
            // Initialize with default grass tiles
            InitializeDefaultTiles();
        }
        
        private void InitializeDefaultTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _tiles[x, y] = new MapTile(TileType.Grass, true, true, "grass", 0.05);
                }
            }
        }
        
        /// <summary>
        /// Gets the tile at the specified coordinates
        /// </summary>
        public MapTile? GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
                
            return _tiles[x, y];
        }
        
        /// <summary>
        /// Sets the tile at the specified coordinates
        /// </summary>
        public void SetTile(int x, int y, MapTile tile)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("Coordinates out of zone bounds");
                
            _tiles[x, y] = tile ?? throw new ArgumentNullException(nameof(tile));
        }
        
        /// <summary>
        /// Checks if a position is valid and passable
        /// </summary>
        public bool IsPassable(int x, int y)
        {
            var tile = GetTile(x, y);
            if (tile == null) return false;
            
            // Check if there's an object blocking this tile
            var blockingObject = Objects.FirstOrDefault(obj => obj.Position.X == x && obj.Position.Y == y && obj.BlocksMovement);
            
            return tile.IsPassable && blockingObject == null;
        }
        
        /// <summary>
        /// Adds a connection to another zone
        /// </summary>
        public void AddConnection(string direction, ZoneConnection connection)
        {
            Connections[direction] = connection;
        }
        
        /// <summary>
        /// Adds an interactive object to this zone
        /// </summary>
        public void AddObject(WorldObject obj)
        {
            Objects.Add(obj);
        }
        
        /// <summary>
        /// Gets all objects at a specific position
        /// </summary>
        public IEnumerable<WorldObject> GetObjectsAt(int x, int y)
        {
            return Objects.Where(obj => obj.Position.X == x && obj.Position.Y == y);
        }
    }
    
    /// <summary>
    /// Represents a connection between zones
    /// </summary>
    public class ZoneConnection
    {
        public string TargetZoneId { get; }
        public WorldPosition TargetPosition { get; }
        public WorldPosition SourcePosition { get; }
        public bool RequiresUnlock { get; }
        public string? UnlockCondition { get; }
        
        public ZoneConnection(string targetZoneId, WorldPosition targetPosition, 
                            WorldPosition sourcePosition, bool requiresUnlock = false, 
                            string? unlockCondition = null)
        {
            TargetZoneId = targetZoneId;
            TargetPosition = targetPosition;
            SourcePosition = sourcePosition;
            RequiresUnlock = requiresUnlock;
            UnlockCondition = unlockCondition;
        }
    }
}