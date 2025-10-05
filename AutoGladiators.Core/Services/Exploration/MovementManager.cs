using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Exploration;
using AutoGladiators.Core.Core;

namespace AutoGladiators.Core.Services.Exploration
{
    /// <summary>
    /// Directions for movement
    /// </summary>
    public enum MovementDirection
    {
        North,
        South,
        East,
        West
    }
    
    /// <summary>
    /// Manages player movement and navigation in the world
    /// </summary>
    public class MovementManager
    {
        private readonly WorldManager _worldManager;
        
        public event Action<WorldPosition, WorldPosition>? OnMovementStarted;
        public event Action<WorldPosition>? OnMovementCompleted;
        public event Action<WorldPosition, string>? OnMovementBlocked;
        
        public MovementManager(WorldManager worldManager)
        {
            _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        }
        
        /// <summary>
        /// Attempts to move the player in a specific direction
        /// </summary>
        public async Task<bool> MovePlayerAsync(PlayerProfile player, MovementDirection direction)
        {
            var currentPosition = player.WorldPosition;
            var newPosition = GetPositionInDirection(currentPosition, direction);
            
            OnMovementStarted?.Invoke(currentPosition, newPosition);
            
            // Check if movement is valid
            if (!_worldManager.CanMoveTo(currentPosition, newPosition))
            {
                OnMovementBlocked?.Invoke(newPosition, GetBlockedReason(newPosition));
                return false;
            }
            
            // Perform the movement
            var success = await _worldManager.MovePlayerAsync(player, newPosition);
            
            if (success)
            {
                OnMovementCompleted?.Invoke(newPosition);
            }
            else
            {
                OnMovementBlocked?.Invoke(newPosition, "Movement failed");
            }
            
            return success;
        }
        
        /// <summary>
        /// Gets the position one step in the specified direction
        /// </summary>
        private WorldPosition GetPositionInDirection(WorldPosition current, MovementDirection direction)
        {
            return direction switch
            {
                MovementDirection.North => new WorldPosition(current.ZoneId, current.X, current.Y - 1),
                MovementDirection.South => new WorldPosition(current.ZoneId, current.X, current.Y + 1),
                MovementDirection.East => new WorldPosition(current.ZoneId, current.X + 1, current.Y),
                MovementDirection.West => new WorldPosition(current.ZoneId, current.X - 1, current.Y),
                _ => current
            };
        }
        
        /// <summary>
        /// Gets a human-readable reason why movement was blocked
        /// </summary>
        private string GetBlockedReason(WorldPosition position)
        {
            var zone = _worldManager.GetZone(position.ZoneId);
            if (zone == null)
                return "Cannot access that area";
                
            var tile = zone.GetTile(position.X, position.Y);
            if (tile == null)
                return "You cannot go that way";
                
            if (!tile.IsPassable)
            {
                return tile.Type switch
                {
                    TileType.Water => "You need a way to cross the water",
                    TileType.Mountain => "The mountain is too steep to climb",
                    TileType.Obstacle => "Your path is blocked",
                    _ => "You cannot pass through here"
                };
            }
            
            // Check for blocking objects
            var blockingObjects = zone.GetObjectsAt(position.X, position.Y);
            foreach (var obj in blockingObjects)
            {
                if (obj.BlocksMovement)
                    return $"Your path is blocked by {obj.Name.ToLower()}";
            }
            
            return "Cannot move there";
        }
        
        /// <summary>
        /// Checks if the player can move in a specific direction
        /// </summary>
        public bool CanMove(PlayerProfile player, MovementDirection direction)
        {
            var currentPosition = player.WorldPosition;
            var newPosition = GetPositionInDirection(currentPosition, direction);
            
            return _worldManager.CanMoveTo(currentPosition, newPosition);
        }
        
        /// <summary>
        /// Gets all valid movement directions from current position
        /// </summary>
        public MovementDirection[] GetValidDirections(PlayerProfile player)
        {
            var validDirections = new List<MovementDirection>();
            
            foreach (MovementDirection direction in Enum.GetValues<MovementDirection>())
            {
                if (CanMove(player, direction))
                {
                    validDirections.Add(direction);
                }
            }
            
            return validDirections.ToArray();
        }
        
        /// <summary>
        /// Teleports player to a specific position (bypassing movement restrictions)
        /// </summary>
        public async Task<bool> TeleportPlayerAsync(PlayerProfile player, WorldPosition position)
        {
            var zone = _worldManager.GetZone(position.ZoneId);
            if (zone == null)
                return false;
                
            var tile = zone.GetTile(position.X, position.Y);
            if (tile == null)
                return false;
            
            var oldPosition = player.WorldPosition;
            player.WorldPosition = position;
            
            // Change zone if needed
            if (oldPosition.ZoneId != position.ZoneId)
            {
                _worldManager.ChangeZone(position.ZoneId);
            }
            
            OnMovementCompleted?.Invoke(position);
            return true;
        }
    }
}