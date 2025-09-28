using System.Collections.Generic;
using AutoGladiators.Core.Rng;

namespace AutoGladiators.Tests.Utils
{
    public sealed class FakeRandom : IRandomProvider, IRng
    {
        private readonly Queue<double> _doubles = new();
        private readonly Queue<int> _ints = new();

        public FakeRandom() { }
        public FakeRandom(params double[] doubles)
        {
            foreach (var d in doubles) _doubles.Enqueue(d);
        }

        public void EnqueueDouble(params double[] values)
        {
            foreach (var v in values) _doubles.Enqueue(v);
        }

        public void EnqueueInt(params int[] values)
        {
            foreach (var v in values) _ints.Enqueue(v);
        }

        // Required by IRandomProvider
        public double NextDouble()
            => _doubles.Count > 0 ? Normalize01(_doubles.Dequeue()) : 0.0;

        // Required by IRandomProvider
        public double NextDouble(double minInclusive, double maxExclusive)
        {
            var u = NextDouble(); // 0..1
            return minInclusive + u * (maxExclusive - minInclusive);
        }

        // Required by IRandomProvider
        public int Next(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            if (_ints.Count == 0) return minInclusive;
            var span = maxExclusive - minInclusive;
            var raw  = _ints.Dequeue();
            var mod  = ((raw % span) + span) % span; // non-negative modulo
            return minInclusive + mod;
        }

        // Optional convenience (safe if your prod code ever calls it)
        public int Next(int maxExclusive)
            => Next(0, maxExclusive);

        private static double Normalize01(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return 0.0;
            if (v < 0) return 0.0;
            if (v > 1) return v - System.Math.Floor(v); // wrap into [0,1)
            return v;
        }
    }
}
