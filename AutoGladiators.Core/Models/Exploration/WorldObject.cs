using System;
using System.Collections.Generic;
using AutoGladiators.Core.Core;

namespace AutoGladiators.Core.Models.Exploration
{
    /// <summary>
    /// Types of interactive objects in the world
    /// </summary>
    public enum WorldObjectType
    {
        NPC,
        Trainer,
        Item,
        Shop,
        HealingCenter,
        Obstacle,
        Decoration,
        QuestGiver,
        Gym,
        Teleporter
    }
    
    /// <summary>
    /// Represents an interactive object in the world (NPCs, items, buildings, etc.)
    /// </summary>
    public class WorldObject
    {
        public string Id { get; }
        public WorldObjectType Type { get; }
        public string Name { get; }
        public string Description { get; }
        public WorldPosition Position { get; set; }
        public string? SpriteId { get; }
        public bool BlocksMovement { get; }
        public bool IsInteractable { get; }
        public Dictionary<string, object> Properties { get; }
        
        // Interaction data
        public string? DialogueId { get; set; }
        public string? ItemId { get; set; }
        public List<string> RequiredItems { get; }
        public List<string> QuestIds { get; }
        
        // Event handlers
        public event Action<PlayerProfile>? OnInteraction;
        public event Action<PlayerProfile>? OnApproach;
        
        public WorldObject(string id, WorldObjectType type, string name, 
                          WorldPosition position, string? spriteId = null, 
                          bool blocksMovement = true, bool isInteractable = true)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = string.Empty;
            Position = position;
            SpriteId = spriteId;
            BlocksMovement = blocksMovement;
            IsInteractable = isInteractable;
            Properties = new Dictionary<string, object>();
            RequiredItems = new List<string>();
            QuestIds = new List<string>();
        }
        
        /// <summary>
        /// Triggers when player interacts with this object
        /// </summary>
        public virtual void Interact(PlayerProfile player)
        {
            if (!IsInteractable) return;
            
            OnInteraction?.Invoke(player);
        }
        
        /// <summary>
        /// Triggers when player approaches this object
        /// </summary>
        public virtual void Approach(PlayerProfile player)
        {
            OnApproach?.Invoke(player);
        }
        
        /// <summary>
        /// Sets a custom property for this object
        /// </summary>
        public void SetProperty(string key, object value)
        {
            Properties[key] = value;
        }
        
        /// <summary>
        /// Gets a custom property for this object
        /// </summary>
        public T? GetProperty<T>(string key)
        {
            return Properties.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;
        }
        
        /// <summary>
        /// Checks if player meets requirements to interact with this object
        /// </summary>
        public bool CanInteract(PlayerProfile player)
        {
            if (!IsInteractable) return false;
            
            // Check required items
            foreach (var requiredItem in RequiredItems)
            {
                if (!player.Inventory.ContainsKey(requiredItem))
                    return false;
            }
            
            return true;
        }
    }
}