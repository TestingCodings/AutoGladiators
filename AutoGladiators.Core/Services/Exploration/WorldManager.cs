using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Exploration;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Rng;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Services.Exploration
{
    /// <summary>
    /// Enhanced world manager supporting Pokemon-style exploration with time/weather systems
    /// Manages the game world, zones, NPCs, shops, and dynamic encounters
    /// </summary>
    public class WorldManager
    {
        private readonly Dictionary<string, WorldZone> _zones = new();
        private readonly Dictionary<string, NPC> _npcs = new();
        private readonly Dictionary<string, Shop> _shops = new();
        private readonly IRng _rng;
        private readonly EncounterService _encounterService;
        
        public WorldZone? CurrentZone { get; private set; }
        
        // Enhanced world state
        public TimeOfDay CurrentTimeOfDay { get; private set; } = TimeOfDay.Day;
        public WeatherCondition CurrentWeather { get; private set; } = WeatherCondition.Clear;
        public Dictionary<string, bool> GlobalFlags { get; } = new();
        
        // Enhanced events
        public event Action<WorldZone, WorldZone>? OnZoneChanged;
        public event Action<WorldPosition>? OnPlayerMoved;
        public event Action<PlayerProfile, string>? OnWildEncounter;
        public event Action<TimeOfDay>? OnTimeChanged;
        public event Action<WeatherCondition>? OnWeatherChanged;
        public event Action<string>? OnWorldEvent;
        
        public WorldManager(IRng rng)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _encounterService = new EncounterService();
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
        private Task TriggerWildEncounter(PlayerProfile player, string gladiatorId)
        {
            // This will be implemented when we add the encounter system
            // For now, just raise an event
            OnWildEncounter?.Invoke(player, gladiatorId);
            return Task.CompletedTask;
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
            
            // Generate town terrain with roads and buildings
            GenerateStarterTownTerrain(starterTown);
            
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
            
            // Generate varied route terrain
            GenerateRoute1Terrain(route1);
            
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
        
        /// <summary>
        /// Gets an NPC by ID
        /// </summary>
        public NPC? GetNPC(string npcId)
        {
            return _npcs.TryGetValue(npcId, out var npc) ? npc : null;
        }
        
        /// <summary>
        /// Gets a shop by ID
        /// </summary>
        public Shop? GetShop(string shopId)
        {
            return _shops.TryGetValue(shopId, out var shop) ? shop : null;
        }
        
        /// <summary>
        /// Updates the time of day and triggers related events
        /// </summary>
        public void UpdateTimeOfDay(TimeOfDay newTime)
        {
            if (CurrentTimeOfDay != newTime)
            {
                CurrentTimeOfDay = newTime;
                _encounterService.UpdateConditions(CurrentTimeOfDay.ToString(), CurrentWeather.ToString());
                OnTimeChanged?.Invoke(newTime);
            }
        }
        
        /// <summary>
        /// Updates weather and triggers related events
        /// </summary>
        public void UpdateWeather(WeatherCondition newWeather)
        {
            if (CurrentWeather != newWeather)
            {
                CurrentWeather = newWeather;
                _encounterService.UpdateConditions(CurrentTimeOfDay.ToString(), CurrentWeather.ToString());
                OnWeatherChanged?.Invoke(newWeather);
            }
        }
        
        /// <summary>
        /// Sets a global flag
        /// </summary>
        public void SetGlobalFlag(string flag, bool value)
        {
            GlobalFlags[flag] = value;
        }
        
        /// <summary>
        /// Gets a global flag value
        /// </summary>
        public bool GetGlobalFlag(string flag)
        {
            return GlobalFlags.TryGetValue(flag, out var value) && value;
        }
        
        /// <summary>
        /// Generates terrain for starter town with roads and building areas
        /// </summary>
        private void GenerateStarterTownTerrain(WorldZone zone)
        {
            // Create horizontal road through middle
            int roadY = zone.Height / 2;
            for (int x = 0; x < zone.Width; x++)
            {
                zone.SetTile(x, roadY, new Models.Exploration.MapTile(TileType.Road, true, false, "road", 0.0));
            }
            
            // Create vertical road through middle
            int roadX = zone.Width / 2;
            for (int y = 0; y < zone.Height; y++)
            {
                zone.SetTile(roadX, y, new Models.Exploration.MapTile(TileType.Road, true, false, "road", 0.0));
            }
            
            // Add some building tiles around the center
            for (int x = 5; x < 15; x++)
            {
                for (int y = 3; y < 17; y++)
                {
                    // Skip roads
                    if (x == roadX || y == roadY) continue;
                    
                    // Random chance for building vs grass
                    if (_rng.NextDouble() < 0.3) // 30% chance for building
                    {
                        zone.SetTile(x, y, new Models.Exploration.MapTile(TileType.Building, false, false, "building", 0.0));
                    }
                }
            }
            
            // Add some decorative water features
            // Small pond in corners
            for (int x = 2; x < 5; x++)
            {
                for (int y = 2; y < 5; y++)
                {
                    if (_rng.NextDouble() < 0.7) // 70% chance for water in pond area
                    {
                        zone.SetTile(x, y, new Models.Exploration.MapTile(TileType.Water, false, false, "water", 0.0));
                    }
                }
            }
        }
        
        /// <summary>
        /// Generates varied terrain for Route 1 with different biomes
        /// </summary>
        private void GenerateRoute1Terrain(WorldZone zone)
        {
            // Create main path through the route
            int pathY = zone.Height / 2;
            for (int x = 0; x < zone.Width; x++)
            {
                zone.SetTile(x, pathY, new Models.Exploration.MapTile(TileType.Road, true, false, "path", 0.0));
                // Also make path tiles above and below for wider road
                if (pathY > 0)
                    zone.SetTile(x, pathY - 1, new Models.Exploration.MapTile(TileType.Road, true, false, "path", 0.0));
                if (pathY < zone.Height - 1)
                    zone.SetTile(x, pathY + 1, new Models.Exploration.MapTile(TileType.Road, true, false, "path", 0.0));
            }
            
            // Add forest area on the north side
            for (int x = 5; x < 25; x++)
            {
                for (int y = 0; y < pathY - 2; y++)
                {
                    if (_rng.NextDouble() < 0.6) // 60% chance for forest
                    {
                        zone.SetTile(x, y, new Models.Exploration.MapTile(TileType.Forest, true, true, "forest", 0.08));
                    }
                }
            }
            
            // Add water features
            // River on south side
            for (int x = 8; x < 22; x++)
            {
                for (int y = pathY + 3; y < zone.Height; y++)
                {
                    if (_rng.NextDouble() < 0.4) // 40% chance for water
                    {
                        zone.SetTile(x, y, new Models.Exploration.MapTile(TileType.Water, false, false, "river", 0.0));
                    }
                }
            }
            
            // Add some mountain/hill areas
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    if (y >= pathY - 1 && y <= pathY + 1) continue; // Skip main path
                    
                    if (_rng.NextDouble() < 0.3) // 30% chance for mountain
                    {
                        zone.SetTile(x, y, new Models.Exploration.MapTile(TileType.Mountain, false, false, "mountain", 0.0));
                    }
                }
            }
            
            // Add some obstacles and interesting features
            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    var tile = zone.GetTile(x, y);
                    if (tile?.Type == TileType.Grass)
                    {
                        // Small chance for obstacles or special terrain
                        double roll = _rng.NextDouble();
                        if (roll < 0.05) // 5% chance for obstacle
                        {
                            zone.SetTile(x, y, new Models.Exploration.MapTile(TileType.Obstacle, false, false, "rock", 0.0));
                        }
                        else if (roll < 0.1) // Additional 5% chance for desert patches
                        {
                            zone.SetTile(x, y, new Models.Exploration.MapTile(TileType.Desert, true, true, "sand", 0.03));
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Represents an NPC in the world
    /// </summary>
    public class NPC
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public WorldPosition Position { get; set; }
        public List<Dialogue> Dialogues { get; set; } = new();
        public List<string> QuestsAvailable { get; set; } = new();
        public string? ShopId { get; set; }
        public bool IsTrainer { get; set; } = false;
        public List<GladiatorBot> TrainerTeam { get; set; } = new();
    }
    
    /// <summary>
    /// Represents a dialogue option
    /// </summary>
    public class Dialogue
    {
        public string Id { get; }
        public string Text { get; }
        public List<string> RequiredFlags { get; }
        public Dictionary<string, bool> FlagsToSet { get; }
        
        public Dialogue(string id, string text)
        {
            Id = id;
            Text = text;
            RequiredFlags = new List<string>();
            FlagsToSet = new Dictionary<string, bool>();
        }
    }
    
    /// <summary>
    /// Represents a shop in the world
    /// </summary>
    public class Shop
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public WorldPosition Position { get; set; }
        public Dictionary<string, ShopItem> Items { get; set; } = new();
        public string? RequiredFlag { get; set; }
    }
    
    /// <summary>
    /// Represents an item for sale in a shop
    /// </summary>
    public class ShopItem
    {
        public string ItemId { get; }
        public string DisplayName { get; }
        public int Price { get; }
        public int Stock { get; set; }
        
        public ShopItem(string itemId, string displayName, int price, int stock)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Price = price;
            Stock = stock;
        }
    }
}