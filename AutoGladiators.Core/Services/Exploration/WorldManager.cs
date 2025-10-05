using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Exploration;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Rng;

namespace AutoGladiators.Core.Services.Exploration
{
    /// <summary>
    /// Manages the game world, zones, and navigation between them
    /// </summary>
    public class WorldManager
    {
        private readonly Dictionary<string, WorldZone> _zones = new();
        private readonly IRng _rng;
        
        public WorldZone? CurrentZone { get; private set; }
        public event Action<WorldZone, WorldZone>? OnZoneChanged;
        public event Action<WorldPosition>? OnPlayerMoved;
        public event Action<PlayerProfile, string>? OnWildEncounter;
        
        public WorldManager(IRng rng)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }
        
        /// <summary>
        /// Adds a zone to the world
        /// </summary>
        public void AddZone(WorldZone zone)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));
            
            _zones[zone.Id] = zone;
        }
        
        /// <summary>
        /// Gets a zone by ID
        /// </summary>
        public WorldZone? GetZone(string zoneId)
        {
            return _zones.TryGetValue(zoneId, out var zone) ? zone : null;
        }
        
        /// <summary>
        /// Changes the current zone
        /// </summary>
        public bool ChangeZone(string zoneId)
        {
            var newZone = GetZone(zoneId);
            if (newZone == null) return false;
            
            var oldZone = CurrentZone;
            CurrentZone = newZone;
            
            if (oldZone != null)
            {
                OnZoneChanged?.Invoke(oldZone, newZone);
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if movement from one position to another is valid
        /// </summary>
        public bool CanMoveTo(WorldPosition from, WorldPosition to)
        {
            // Same zone movement
            if (from.ZoneId == to.ZoneId)
            {
                var zone = GetZone(from.ZoneId);
                if (zone == null) return false;
                
                return zone.IsPassable(to.X, to.Y);
            }
            
            // Zone transition
            var sourceZone = GetZone(from.ZoneId);
            if (sourceZone == null) return false;
            
            // Check if there's a connection at the source position
            var connections = sourceZone.Connections.Values;
            var validConnection = connections.FirstOrDefault(c => 
                c.SourcePosition.X == from.X && c.SourcePosition.Y == from.Y && 
                c.TargetZoneId == to.ZoneId);
            
            return validConnection != null;
        }
        
        /// <summary>
        /// Attempts to move player to a new position
        /// </summary>
        public async Task<bool> MovePlayerAsync(PlayerProfile player, WorldPosition newPosition)
        {
            if (!CanMoveTo(player.WorldPosition, newPosition))
                return false;
            
            var oldPosition = player.WorldPosition;
            player.WorldPosition = newPosition;
            
            // Change zone if needed
            if (oldPosition.ZoneId != newPosition.ZoneId)
            {
                ChangeZone(newPosition.ZoneId);
            }
            
            OnPlayerMoved?.Invoke(newPosition);
            
            // Check for encounters after movement
            await CheckForRandomEncounter(player);
            
            return true;
        }
        
        /// <summary>
        /// Checks for random wild gladiator encounters
        /// </summary>
        private async Task CheckForRandomEncounter(PlayerProfile player)
        {
            var zone = GetZone(player.WorldPosition.ZoneId);
            if (zone == null) return;
            
            var tile = zone.GetTile(player.WorldPosition.X, player.WorldPosition.Y);
            if (tile == null || !tile.HasEncounters) return;
            
            var encounterChance = _rng.NextDouble();
            if (encounterChance <= tile.EncounterRate && tile.AvailableGladiators.Count > 0)
            {
                var gladiatorId = tile.AvailableGladiators[_rng.Next(tile.AvailableGladiators.Count)];
                await TriggerWildEncounter(player, gladiatorId);
            }
        }
        
        /// <summary>
        /// Triggers a wild gladiator encounter
        /// </summary>
        private async Task TriggerWildEncounter(PlayerProfile player, string gladiatorId)
        {
            // This will be implemented when we add the encounter system
            // For now, just raise an event
            OnWildEncounter?.Invoke(player, gladiatorId);
        }
        
        /// <summary>
        /// Gets all zones in the world
        /// </summary>
        public IEnumerable<WorldZone> GetAllZones()
        {
            return _zones.Values;
        }
        
        /// <summary>
        /// Initializes the world with default zones
        /// </summary>
        public void InitializeDefaultWorld()
        {
            // Create starter town
            var starterTown = new WorldZone("starter_town", "New Gladiator Town", 
                "A peaceful town where new gladiator trainers begin their journey.", 20, 20);
            
            // Add some buildings and NPCs
            var healingCenter = new WorldObject("healing_center", WorldObjectType.HealingCenter, 
                "Gladiator Healing Center", new WorldPosition("starter_town", 10, 8), "healing_center");
            
            var shop = new WorldObject("item_shop", WorldObjectType.Shop, 
                "Item Shop", new WorldPosition("starter_town", 8, 10), "shop");
            
            var professor = new WorldObject("professor_npc", WorldObjectType.NPC, 
                "Professor Gladius", new WorldPosition("starter_town", 10, 15), "professor");
            
            starterTown.AddObject(healingCenter);
            starterTown.AddObject(shop);
            starterTown.AddObject(professor);
            
            AddZone(starterTown);
            
            // Create Route 1
            var route1 = new WorldZone("route_1", "Route 1", 
                "A grassy route filled with wild gladiators.", 30, 15);
            
            // Set encounter gladiators for grass tiles
            for (int x = 0; x < route1.Width; x++)
            {
                for (int y = 0; y < route1.Height; y++)
                {
                    var tile = route1.GetTile(x, y);
                    if (tile?.Type == TileType.Grass)
                    {
                        tile.AddEncounterGladiator("warrior");
                        tile.AddEncounterGladiator("guardian");
                    }
                }
            }
            
            // Add connection between town and route
            var townToRoute = new ZoneConnection("route_1", 
                new WorldPosition("route_1", 0, 7), 
                new WorldPosition("starter_town", 19, 10));
            
            var routeToTown = new ZoneConnection("starter_town", 
                new WorldPosition("starter_town", 19, 10), 
                new WorldPosition("route_1", 0, 7));
            
            starterTown.AddConnection("east", townToRoute);
            route1.AddConnection("west", routeToTown);
            
            AddZone(route1);
            
            // Set starter town as current zone
            ChangeZone("starter_town");
        }
    }
}