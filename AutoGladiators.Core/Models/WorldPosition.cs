using System;

namespace AutoGladiators.Core.Models
{
    /// <summary>
    /// Represents a position in the game world with zone and coordinate information
    /// </summary>
    public struct WorldPosition : IEquatable<WorldPosition>
    {
        public string ZoneId { get; }
        public int X { get; }
        public int Y { get; }
        
        public WorldPosition(string zoneId, int x, int y)
        {
            ZoneId = zoneId ?? throw new ArgumentNullException(nameof(zoneId));
            X = x;
            Y = y;
        }
        
        public bool Equals(WorldPosition other)
        {
            return ZoneId == other.ZoneId && X == other.X && Y == other.Y;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is WorldPosition other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(ZoneId, X, Y);
        }
        
        public static bool operator ==(WorldPosition left, WorldPosition right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(WorldPosition left, WorldPosition right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"Zone:{ZoneId} ({X},{Y})";
        }
    }
}