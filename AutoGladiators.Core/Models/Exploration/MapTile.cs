using System;
using System.Collections.Generic;

namespace AutoGladiators.Core.Models.Exploration
{
    /// <summary>
    /// Represents different types of terrain/tiles in the world
    /// </summary>
    public enum TileType
    {
        Grass,
        Water,
        Mountain,
        Forest,
        Desert,
        Cave,
        Building,
        Road,
        Bridge,
        Obstacle
    }
    
    /// <summary>
    /// Represents a single tile/cell in the game world
    /// </summary>
    public class MapTile
    {
        public TileType Type { get; }
        public bool IsPassable { get; }
        public bool HasEncounters { get; }
        public string? SpriteId { get; }
        public Dictionary<string, object> Properties { get; }
        
        // Encounter configuration
        public double EncounterRate { get; }
        public List<string> AvailableGladiators { get; }
        
        public MapTile(TileType type, bool isPassable = true, bool hasEncounters = false, 
                      string? spriteId = null, double encounterRate = 0.0)
        {
            Type = type;
            IsPassable = isPassable;
            HasEncounters = hasEncounters;
            SpriteId = spriteId;
            EncounterRate = encounterRate;
            Properties = new Dictionary<string, object>();
            AvailableGladiators = new List<string>();
        }
        
        /// <summary>
        /// Adds a gladiator type that can be encountered on this tile
        /// </summary>
        public void AddEncounterGladiator(string gladiatorId)
        {
            if (!AvailableGladiators.Contains(gladiatorId))
            {
                AvailableGladiators.Add(gladiatorId);
            }
        }
        
        /// <summary>
        /// Sets a custom property for this tile
        /// </summary>
        public void SetProperty(string key, object value)
        {
            Properties[key] = value;
        }
        
        /// <summary>
        /// Gets a custom property for this tile
        /// </summary>
        public T? GetProperty<T>(string key)
        {
            return Properties.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;
        }
    }
}