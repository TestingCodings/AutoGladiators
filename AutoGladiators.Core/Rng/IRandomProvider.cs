namespace AutoGladiators.Core.Rng;

public interface IRandomProvider
{
    int Next(int minInclusive, int maxExclusive);
    double NextDouble();
}

public sealed class RealRandomProvider : IRandomProvider
{
    private readonly Random _r = new();
    public int Next(int minInclusive, int maxExclusive) => _r.Next(minInclusive, maxExclusive);
    public double NextDouble() => _r.NextDouble();
}
