namespace AutoGladiators.Core.Rng
{
    public interface IRng
    {
        double NextDouble();
        double NextDouble(double min, double max);
        int Next(int max);
        int Next(int min, int max);
    }
}
