namespace AutoGladiators.Client.Models
{
    public class PlayerLocation 
    {
        public string Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public override string ToString()
        {
            return $"{Region} ({X}, {Y})";
        }
        public PlayerLocation Clone()
        {
            return new PlayerLocation
            {
                Region = this.Region,
                X = this.X,
                Y = this.Y
            };
        }
        interface IEquatable<PlayerLocation>
        {
            public bool IsEqual(PlayerLocation other)
            {
                if (other == null) return false;
                return this.Region == other.Region && this.X == other.X && this.Y == other.Y;
            }
            public override bool Equals(object obj)
            {
                if (obj is PlayerLocation other)
                {
                    return IsEqual(other);
                }
                return false;
            }
        }

            public override int GetHashCode()
            {
                return HashCode.Combine(Region, X, Y);
            }
            public static bool operator ==(PlayerLocation left, PlayerLocation right)
            {
                if (left is null) return right is null;
                return left.Equals(right);
            }
            public static bool operator !=(PlayerLocation left, PlayerLocation right)
            {
                return !(left == right);
            }
        
        public static PlayerLocation operator +(PlayerLocation loc, (int deltaX, int deltaY) delta)
        {
            return new PlayerLocation
            {
                Region = loc.Region,
                X = loc.X + delta.deltaX,
                Y = loc.Y + delta.deltaY
            };
        }
        public static PlayerLocation operator -(PlayerLocation loc, (int deltaX, int deltaY) delta)
        {
            return new PlayerLocation
            {
                Region = loc.Region,
                X = loc.X - delta.deltaX,
                Y = loc.Y - delta.deltaY
            };
        }
    }
}