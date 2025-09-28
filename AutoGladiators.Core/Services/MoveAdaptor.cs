using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public static class MoveAdapter
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For("MoveAdaptor");
        // Minimal "wrap names as Moves" to satisfy the UI/VM. Fill real fields later.
        public static IEnumerable<Move> FromNames(IEnumerable<string> names) =>
            names.Select(n => new Move { Name = n });
    }
}

