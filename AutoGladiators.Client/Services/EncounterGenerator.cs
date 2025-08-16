using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services
{
    public static class EncounterGenerator
    {
        private static readonly IAppLogger Log = AppLog.For<EncounterGenerator>();

        private static readonly EncounterService _encounterService = new();

        public static bool ShouldTriggerEncounter(string regionKey)
        {
            var dummyLocation = new PlayerLocation { Region = regionKey, X = 0, Y = 0 };
            return _encounterService.encounterTriggered(dummyLocation);
        }

        public static GladiatorBot? GetRandomWildBot(string regionKey)
        {
            int playerLevel = GameStateService.Instance.CurrentPlayer.Level;
            return _encounterService.GenerateWildBot(regionKey, playerLevel);
        }

        public static bool TryGenerateWildEncounter(MapLocation location, out GladiatorBot? wildBot)
        {
            wildBot = null;

            if (string.IsNullOrEmpty(location.EncounterTableId))
                return false;

            if (!ShouldTriggerEncounter(location.EncounterTableId))
                return false;

            wildBot = GetRandomWildBot(location.EncounterTableId);
            return wildBot != null;
        }
    }
}

