using System;
using System.Collections.Generic;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Core.Services.Analytics
{
    public interface IBattleAnalytics
    {
        void LogBattleStart(GladiatorBot player, GladiatorBot enemy);
        void LogBattleEnd(GladiatorBot player, GladiatorBot enemy, bool playerWon, TimeSpan duration);
        void LogMoveUsed(string playerId, string moveName, int damage, bool hit, bool critical);
        void LogRewardsEarned(string playerId, int xp, int gold, int enemyLevel);
        void LogPlayerLevelUp(string playerId, int newLevel, int totalXp);
    }

    public class BattleAnalytics : IBattleAnalytics
    {
        private static readonly IAppLogger _logger = AppLog.For<BattleAnalytics>();

        public void LogBattleStart(GladiatorBot player, GladiatorBot enemy)
        {
            var battleData = new Dictionary<string, object?>
            {
                ["PlayerLevel"] = player.Level,
                ["PlayerHealth"] = player.CurrentHealth,
                ["PlayerMaxHealth"] = player.MaxHealth,
                ["EnemyLevel"] = enemy.Level,
                ["EnemyHealth"] = enemy.CurrentHealth,
                ["EnemyMaxHealth"] = enemy.MaxHealth,
                ["EnemyName"] = enemy.Name,
                ["LevelDifference"] = enemy.Level - player.Level
            };

            _logger.LogBattleEvent("BattleStarted", player.Id.ToString(), enemy.Id.ToString(), battleData);
        }

        public void LogBattleEnd(GladiatorBot player, GladiatorBot enemy, bool playerWon, TimeSpan duration)
        {
            var battleData = new Dictionary<string, object?>
            {
                ["PlayerWon"] = playerWon,
                ["BattleDurationSeconds"] = duration.TotalSeconds,
                ["PlayerFinalHealth"] = player.CurrentHealth,
                ["EnemyFinalHealth"] = enemy.CurrentHealth,
                ["PlayerSurvivalRate"] = player.CurrentHealth / (double)player.MaxHealth,
                ["EnemyLevel"] = enemy.Level,
                ["LevelDifference"] = enemy.Level - player.Level
            };

            _logger.LogBattleEvent("BattleEnded", player.Id.ToString(), enemy.Id.ToString(), battleData);
        }

        public void LogMoveUsed(string playerId, string moveName, int damage, bool hit, bool critical)
        {
            var moveData = new Dictionary<string, object?>
            {
                ["MoveName"] = moveName,
                ["Damage"] = damage,
                ["Hit"] = hit,
                ["Critical"] = critical
            };

            _logger.LogBattleEvent("MoveUsed", playerId, "", moveData);
        }

        public void LogRewardsEarned(string playerId, int xp, int gold, int enemyLevel)
        {
            var rewardData = new Dictionary<string, object?>
            {
                ["XpEarned"] = xp,
                ["GoldEarned"] = gold,
                ["EnemyLevel"] = enemyLevel,
                ["XpPerLevel"] = xp / (double)enemyLevel
            };

            _logger.LogBattleEvent("RewardsEarned", playerId, "", rewardData);
        }

        public void LogPlayerLevelUp(string playerId, int newLevel, int totalXp)
        {
            var levelData = new Dictionary<string, object?>
            {
                ["NewLevel"] = newLevel,
                ["TotalXp"] = totalXp,
                ["XpPerLevel"] = totalXp / (double)newLevel
            };

            _logger.LogUserAction("LevelUp", playerId, levelData);
        }
    }
}