using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Client.Services
{
    public static class MoveAdapter
    {
        // Minimal "wrap names as Moves" to satisfy the UI/VM. Fill real fields later.
        public static IEnumerable<Move> FromNames(IEnumerable<string> names) =>
            names.Select(n => new Move { Name = n });
    }
}
            