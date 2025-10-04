using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Core.Services
{
    public class WorldMapService
    {
        private static readonly IAppLogger Log = AppLog.For<WorldMapService>();
        private static readonly WorldMapService _instance = new();
        public static WorldMapService Instance => _instance;

        public const int MAP_SIZE = 6;
        private MapTile[,] _mapGrid;
        private (int X, int Y) _playerPosition;
        
        public MapTile[,] MapGrid => _mapGrid;
        public (int X, int Y) PlayerPosition => _playerPosition;

        private WorldMapService()
        {
            _mapGrid = new MapTile[MAP_SIZE, MAP_SIZE];
            _playerPosition = (0, 0); // Start at top-left
            GenerateWorldMap();
        }

        private void GenerateWorldMap()
        {
            Log.Info("Generating world map...");
            
            // Initialize all tiles as empty first
            for (int x = 0; x < MAP_SIZE; x++)
            {
                for (int y = 0; y < MAP_SIZE; y++)
                {
                    _mapGrid[x, y] = new MapTile
                    {
                        X = x,
                        Y = y,
                        Terrain = TerrainType.Empty,
                        Icon = "⬜",
                        Name = "Unexplored",
                        Description = "This area hasn't been mapped yet."
                    };
                }
            }

            // Place starting area
            SetTile(0, 0, new MapTile
            {
                X = 0, Y = 0,
                Terrain = TerrainType.StartingArea,
                Icon = "🏠",
                Name = "Base Camp",
                Description = "Your safe starting point. No enemies here.",
                Difficulty = DifficultyLevel.Safe,
                IsExplored = true
            });

            // Place Scrap Yards (easy areas)
            SetTile(1, 0, CreateTile(1, 0, TerrainType.ScrapYards, "🏭", "Rusty Outskirts", DifficultyLevel.Easy));
            SetTile(0, 1, CreateTile(0, 1, TerrainType.ScrapYards, "🏭", "Scrap Plains", DifficultyLevel.Easy));
            SetTile(2, 1, CreateTile(2, 1, TerrainType.ScrapYards, "🏭", "Junk Valley", DifficultyLevel.Easy));

            // Place Electric Wastes (medium areas)
            SetTile(3, 0, CreateTile(3, 0, TerrainType.ElectricWastes, "⚡", "Static Fields", DifficultyLevel.Medium));
            SetTile(1, 2, CreateTile(1, 2, TerrainType.ElectricWastes, "⚡", "Thunder Ridge", DifficultyLevel.Medium));
            SetTile(4, 1, CreateTile(4, 1, TerrainType.ElectricWastes, "⚡", "Voltage Zone", DifficultyLevel.Medium));

            // Place Volcanic Depths (hard areas)
            SetTile(2, 3, CreateTile(2, 3, TerrainType.VolcanicDepths, "🌋", "Lava Pools", DifficultyLevel.Hard));
            SetTile(4, 2, CreateTile(4, 2, TerrainType.VolcanicDepths, "🌋", "Magma Caverns", DifficultyLevel.Hard));
            SetTile(3, 4, CreateTile(3, 4, TerrainType.VolcanicDepths, "🌋", "Fire Mountain", DifficultyLevel.Hard));

            // Place Crystal Caverns (extreme areas)
            SetTile(5, 3, CreateTile(5, 3, TerrainType.CrystalCaverns, "🔮", "Crystal Depths", DifficultyLevel.Extreme));
            SetTile(4, 5, CreateTile(4, 5, TerrainType.CrystalCaverns, "🔮", "Frozen Palace", DifficultyLevel.Extreme));
            SetTile(5, 5, CreateTile(5, 5, TerrainType.CrystalCaverns, "🔮", "Diamond Core", DifficultyLevel.Extreme));

            Log.Info($"World map generated with {CountExploredTiles()} explored areas");
        }

        private MapTile CreateTile(int x, int y, TerrainType terrain, string icon, string name, DifficultyLevel difficulty)
        {
            return new MapTile
            {
                X = x, Y = y,
                Terrain = terrain,
                Icon = icon,
                Name = name,
                Description = GetTerrainDescription(terrain, difficulty),
                Difficulty = difficulty,
                IsExplored = false
            };
        }

        private string GetTerrainDescription(TerrainType terrain, DifficultyLevel difficulty) => terrain switch
        {
            TerrainType.ScrapYards => "Abandoned machinery and rust. Perfect for beginners.",
            TerrainType.ElectricWastes => "Crackling with energy. Electric bots thrive here.",
            TerrainType.VolcanicDepths => "Dangerous lava flows. Fire bots guard rare treasures.",
            TerrainType.CrystalCaverns => "Mysterious crystals pulse with power. Legendary encounters await.",
            _ => "An unknown area waiting to be explored."
        };

        private void SetTile(int x, int y, MapTile tile)
        {
            if (IsValidPosition(x, y))
            {
                _mapGrid[x, y] = tile;
            }
        }

        public bool CanMoveTo(int x, int y)
        {
            if (!IsValidPosition(x, y)) return false;
            
            // Can only move to adjacent tiles (not diagonal)
            var (currentX, currentY) = _playerPosition;
            var distance = Math.Abs(x - currentX) + Math.Abs(y - currentY);
            return distance == 1;
        }

        public bool MovePlayerTo(int x, int y)
        {
            if (!CanMoveTo(x, y)) return false;

            _playerPosition = (x, y);
            
            // Mark tile as explored
            var tile = GetTile(x, y);
            if (tile != null)
            {
                tile.IsExplored = true;
                Log.Info($"Player moved to {tile.Name} at ({x}, {y})");
            }
            
            return true;
        }

        public MapTile? GetTile(int x, int y)
        {
            return IsValidPosition(x, y) ? _mapGrid[x, y] : null;
        }

        public MapTile GetCurrentTile()
        {
            return GetTile(_playerPosition.X, _playerPosition.Y) ?? _mapGrid[0, 0];
        }

        public List<MapTile> GetAdjacentTiles()
        {
            var adjacent = new List<MapTile>();
            var (x, y) = _playerPosition;
            
            // Check all 4 directions
            var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            
            foreach (var (dx, dy) in directions)
            {
                var tile = GetTile(x + dx, y + dy);
                if (tile != null)
                {
                    adjacent.Add(tile);
                }
            }
            
            return adjacent;
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < MAP_SIZE && y >= 0 && y < MAP_SIZE;
        }

        private int CountExploredTiles()
        {
            int count = 0;
            for (int x = 0; x < MAP_SIZE; x++)
            {
                for (int y = 0; y < MAP_SIZE; y++)
                {
                    if (_mapGrid[x, y].IsExplored) count++;
                }
            }
            return count;
        }

        public void SavePlayerPosition()
        {
            // TODO: Integrate with save system
            Log.Info($"Saving player position: ({_playerPosition.X}, {_playerPosition.Y})");
        }

        public void LoadPlayerPosition(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                _playerPosition = (x, y);
                Log.Info($"Loaded player position: ({x}, {y})");
            }
        }
    }
}