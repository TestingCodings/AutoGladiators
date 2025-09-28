namespace AutoGladiators.Core.Rng
{
    public class DefaultRng : IRng
    {
        private readonly Random _random = new();

        public double NextDouble() => _random.NextDouble();
        public double NextDouble(double min, double max) => _random.NextDouble() * (max - min) + min;
        public int Next(int max) => _random.Next(max);
        public int Next(int min, int max) => _random.Next(min, max);
    }
}
