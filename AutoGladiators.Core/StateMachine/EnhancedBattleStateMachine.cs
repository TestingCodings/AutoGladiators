using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Items;
using AutoGladiators.Core.Services;

namespace AutoGladiators.Core.StateMachine
{
    /// <summary>
    /// Enhanced battle system with full turn-based mechanics
    /// Supports moves, items, swapping, and capture attempts
    /// </summary>
    public class EnhancedBattleStateMachine
    {
        private readonly Random _rng = new();
        private readonly CaptureService _captureService = new();
        
        // Battle state
        public BattleState CurrentState { get; private set; } = BattleState.Start;
        public BattleType BattleType { get; private set; }
        public int CurrentTurn { get; private set; } = 0;
        
        // Participants
        public List<GladiatorBot> PlayerTeam { get; private set; } = new();
        public List<GladiatorBot> EnemyTeam { get; private set; } = new();
        public GladiatorBot? CurrentPlayerBot => PlayerTeam.FirstOrDefault(b => !b.IsFainted);
        public GladiatorBot? CurrentEnemyBot => EnemyTeam.FirstOrDefault(b => !b.IsFainted);
        
        // Battle context
        public string Environment { get; set; } = "arena";
        public Dictionary<string, object> BattleConditions { get; } = new();
        public List<string> BattleLog { get; } = new();
        
        // Events
        public event Action<string>? OnBattleLogUpdated;
        public event Action<BattleState>? OnStateChanged;
        public event Action<BattleOutcome>? OnBattleEnded;
        public event Action<BotAction>? OnActionExecuted;
        public event Action? OnTurnStart;
        
        public EnhancedBattleStateMachine(BattleType battleType)
        {
            BattleType = battleType;
        }
        
        /// <summary>
        /// Starts a battle with the given teams
        /// </summary>
        public void StartBattle(List<GladiatorBot> playerTeam, List<GladiatorBot> enemyTeam, 
                               string environment = "arena")
        {
            PlayerTeam = new List<GladiatorBot>(playerTeam);
            EnemyTeam = new List<GladiatorBot>(enemyTeam);
            Environment = environment;
            CurrentTurn = 0;
            
            // Reset all bots for battle
            foreach (var bot in PlayerTeam.Concat(EnemyTeam))
            {
                bot.ResetStages();
                // Don't reset health - keep current state
            }
            
            var enemyName = BattleType == BattleType.Wild ? "wild " + CurrentEnemyBot?.Name : "enemy trainer";
            Log($"Battle started against {enemyName}!");
            
            if (!string.IsNullOrEmpty(environment) && environment != "arena")
            {
                Log($"Battle environment: {environment}");
                ApplyEnvironmentEffects();
            }
            
            ChangeState(BattleState.PlayerTurn);
            OnTurnStart?.Invoke();
        }
        
        /// <summary>
        /// Player chooses to attack with a specific move
        /// </summary>
        public void PlayerAttack(string moveName)
        {
            if (CurrentState != BattleState.PlayerTurn || CurrentPlayerBot == null) return;
            
            var action = new BotAction
            {
                Type = ActionType.Attack,
                ActingBot = CurrentPlayerBot,
                MoveName = moveName,
                Target = CurrentEnemyBot
            };
            
            ExecutePlayerAction(action);
        }
        
        /// <summary>
        /// Player uses an item
        /// </summary>
        public void PlayerUseItem(string itemId, GladiatorBot? targetBot = null)
        {
            if (CurrentState != BattleState.PlayerTurn) return;
            
            var action = new BotAction
            {
                Type = ActionType.UseItem,
                ActingBot = CurrentPlayerBot,
                ItemUsed = itemId,
                Target = targetBot ?? CurrentPlayerBot
            };
            
            ExecutePlayerAction(action);
        }
        
        /// <summary>
        /// Player attempts to capture (wild battles only)
        /// </summary>
        public void PlayerCapture(Models.Items.CaptureGear device)
        {
            if (CurrentState != BattleState.PlayerTurn || BattleType != BattleType.Wild) return;
            
            if (CurrentEnemyBot == null)
            {
                Log("No bot to capture!");
                return;
            }
            
            ChangeState(BattleState.CaptureAttempt);
            
            // TODO: Get player level from game state
            int playerLevel = 10; // Placeholder
            double envModifier = GetEnvironmentCaptureModifier();
            
            var result = _captureService.AttemptCapture(CurrentEnemyBot, device, playerLevel, envModifier);
            
            Log(result.Message);
            
            if (result.Success)
            {
                Log($"✓ {CurrentEnemyBot.Name} was successfully captured!");
                EndBattle(BattleResultType.Victory, result.BotCaptured);
            }
            else
            {
                Log("The capture attempt failed!");
                // Continue to enemy turn
                ChangeState(BattleState.EnemyTurn);
                ExecuteEnemyTurn();
            }
        }
        
        /// <summary>
        /// Player swaps to a different bot
        /// </summary>
        public void PlayerSwap(GladiatorBot newBot)
        {
            if (CurrentState != BattleState.PlayerTurn || newBot.IsFainted) return;
            
            var currentBot = CurrentPlayerBot;
            if (currentBot == newBot) return;
            
            // Move new bot to front of team
            PlayerTeam.Remove(newBot);
            PlayerTeam.Insert(0, newBot);
            
            Log($"{currentBot?.Name} is recalled! Go, {newBot.Name}!");
            
            // Swapping takes the turn
            ChangeState(BattleState.EnemyTurn);
            ExecuteEnemyTurn();
        }
        
        /// <summary>
        /// Player chooses to defend (reduces incoming damage)
        /// </summary>
        public void PlayerDefend()
        {
            if (CurrentState != BattleState.PlayerTurn || CurrentPlayerBot == null) return;
            
            var action = new BotAction
            {
                Type = ActionType.Defend,
                ActingBot = CurrentPlayerBot
            };
            
            // Apply defense boost for this turn
            CurrentPlayerBot.DefenseStage = Math.Min(6, CurrentPlayerBot.DefenseStage + 2);
            Log($"{CurrentPlayerBot.Name} takes a defensive stance!");
            
            OnActionExecuted?.Invoke(action);
            
            ChangeState(BattleState.EnemyTurn);
            ExecuteEnemyTurn();
        }
        
        private void ExecutePlayerAction(BotAction action)
        {
            switch (action.Type)
            {
                case ActionType.Attack:
                    ExecuteAttack(action.ActingBot, action.Target, action.MoveName);
                    break;
                    
                case ActionType.UseItem:
                    ExecuteItemUse(action.ActingBot, action.Target, action.ItemUsed);
                    break;
            }
            
            OnActionExecuted?.Invoke(action);
            
            // Check if battle ended
            if (CheckBattleEnd()) return;
            
            // Continue to enemy turn
            ChangeState(BattleState.EnemyTurn);
            ExecuteEnemyTurn();
        }
        
        private void ExecuteEnemyTurn()
        {
            if (CurrentEnemyBot == null || CurrentEnemyBot.IsFainted)
            {
                CheckBattleEnd();
                return;
            }
            
            // Simple AI - just attack for now
            // TODO: Implement more sophisticated AI
            var enemyAction = new BotAction
            {
                Type = ActionType.Attack,
                ActingBot = CurrentEnemyBot,
                Target = CurrentPlayerBot,
                MoveName = "Basic Attack"
            };
            
            ExecuteAttack(CurrentEnemyBot, CurrentPlayerBot, "Basic Attack");
            OnActionExecuted?.Invoke(enemyAction);
            
            // Check if battle ended
            if (CheckBattleEnd()) return;
            
            // Start new turn
            CurrentTurn++;
            TickStatusEffects();
            
            ChangeState(BattleState.PlayerTurn);
            OnTurnStart?.Invoke();
        }
        
        private void ExecuteAttack(GladiatorBot attacker, GladiatorBot target, string moveName)
        {
            if (attacker == null || target == null) return;
            
            // Calculate damage with enhanced formula
            int damage = CalculateDamage(attacker, target, moveName);
            
            // Apply type effectiveness and critical hits
            var (finalDamage, isCrit, effectiveness) = ApplyBattleModifiers(damage, attacker, target);
            
            // Deal damage
            target.ReceiveDamage(finalDamage);
            
            // Build attack message
            string message = $"{attacker.Name} used {moveName}!";
            if (isCrit) message += " Critical hit!";
            if (effectiveness > 1.0) message += " It's super effective!";
            if (effectiveness < 1.0) message += " It's not very effective...";
            
            Log($"{message} ({finalDamage} damage)");
            
            if (target.IsFainted)
            {
                Log($"{target.Name} fainted!");
            }
        }
        
        private int CalculateDamage(GladiatorBot attacker, GladiatorBot defender, string moveName)
        {
            // Enhanced damage formula
            int attack = attacker.GetEffectiveStat(attacker.AttackPower, attacker.AttackStage);
            int defense = defender.GetEffectiveStat(defender.Defense, defender.DefenseStage);
            
            // Base damage calculation
            int baseDamage = Math.Max(1, (attack * 2 - defense) / 2);
            
            // Move power modifier (TODO: implement move database)
            double movePower = GetMovePower(moveName);
            baseDamage = (int)(baseDamage * movePower);
            
            // Random variance (85-100%)
            double variance = 0.85 + (_rng.NextDouble() * 0.15);
            baseDamage = (int)(baseDamage * variance);
            
            return Math.Max(1, baseDamage);
        }
        
        private (int damage, bool isCrit, double effectiveness) ApplyBattleModifiers(
            int baseDamage, GladiatorBot attacker, GladiatorBot defender)
        {
            bool isCrit = _rng.NextDouble() < (attacker.CriticalHitChance / 100.0);
            if (isCrit) baseDamage = (int)(baseDamage * 1.5);
            
            // Type effectiveness (simplified)
            double effectiveness = GetTypeEffectiveness(attacker.ElementalCore, defender.ElementalCore);
            baseDamage = (int)(baseDamage * effectiveness);
            
            // Environment modifiers
            baseDamage = (int)(baseDamage * GetEnvironmentDamageModifier(attacker.ElementalCore));
            
            return (baseDamage, isCrit, effectiveness);
        }
        
        private double GetTypeEffectiveness(ElementalCore attackType, ElementalCore defendType)
        {
            // Simplified type chart - can be expanded
            if (attackType == ElementalCore.Electric && defendType == ElementalCore.Water) return 2.0;
            if (attackType == ElementalCore.Water && defendType == ElementalCore.Fire) return 2.0;
            if (attackType == ElementalCore.Fire && defendType == ElementalCore.Grass) return 2.0;
            if (attackType == ElementalCore.Grass && defendType == ElementalCore.Electric) return 2.0;
            
            // Reverse for weaknesses
            if (defendType == ElementalCore.Electric && attackType == ElementalCore.Water) return 0.5;
            if (defendType == ElementalCore.Water && attackType == ElementalCore.Fire) return 0.5;
            if (defendType == ElementalCore.Fire && attackType == ElementalCore.Grass) return 0.5;
            if (defendType == ElementalCore.Grass && attackType == ElementalCore.Electric) return 0.5;
            
            return 1.0; // Neutral
        }
        
        private double GetMovePower(string moveName)
        {
            // TODO: Implement proper move database
            return moveName switch
            {
                "Basic Attack" => 1.0,
                "Power Strike" => 1.2,
                "Heavy Slam" => 1.5,
                "Quick Jab" => 0.8,
                _ => 1.0
            };
        }
        
        private void ExecuteItemUse(GladiatorBot user, GladiatorBot target, string itemId)
        {
            // TODO: Implement item effects
            switch (itemId)
            {
                case "health_kit":
                    int healAmount = target.MaxHealth / 3;
                    target.Heal(healAmount);
                    Log($"{target.Name} was healed for {healAmount} HP!");
                    break;
                    
                case "energy_cell":
                    target.Energy = Math.Min(target.MaxEnergy, target.Energy + 50);
                    Log($"{target.Name} restored energy!");
                    break;
                    
                default:
                    Log($"Used {itemId} on {target.Name}");
                    break;
            }
        }
        
        private void TickStatusEffects()
        {
            foreach (var bot in PlayerTeam.Concat(EnemyTeam))
            {
                bot.TickStatusEffects();
                bot.TickStatus(); // Legacy status system
            }
        }
        
        private bool CheckBattleEnd()
        {
            bool playerDefeated = PlayerTeam.All(b => b.IsFainted);
            bool enemyDefeated = EnemyTeam.All(b => b.IsFainted);
            
            if (playerDefeated)
            {
                EndBattle(BattleResultType.Defeat);
                return true;
            }
            
            if (enemyDefeated)
            {
                EndBattle(BattleResultType.Victory);
                return true;
            }
            
            return false;
        }
        
        private void EndBattle(BattleResultType result, GladiatorBot? capturedBot = null)
        {
            ChangeState(result == BattleResultType.Victory ? BattleState.Win : BattleState.Lose);
            
            var battleResult = new BattleOutcome
            {
                Result = result,
                TurnsElapsed = CurrentTurn,
                CapturedBot = capturedBot,
                ExperienceGained = CalculateExperienceGain(result),
                RewardsEarned = CalculateRewards(result)
            };
            
            Log($"Battle ended: {result}");
            OnBattleEnded?.Invoke(battleResult);
        }
        
        private void ApplyEnvironmentEffects()
        {
            // TODO: Implement environment-specific effects
            switch (Environment)
            {
                case "electric_storm":
                    Log("Electric attacks are boosted in this storm!");
                    break;
                case "lava_field":
                    Log("Fire attacks are more powerful here!");
                    break;
            }
        }
        
        private double GetEnvironmentCaptureModifier()
        {
            return Environment switch
            {
                "research_lab" => 1.2, // Better capture rates in labs
                "wild_zone" => 0.9,    // Harder to capture in wild
                _ => 1.0
            };
        }
        
        private double GetEnvironmentDamageModifier(ElementalCore attackType)
        {
            return (Environment, attackType) switch
            {
                ("electric_storm", ElementalCore.Electric) => 1.3,
                ("lava_field", ElementalCore.Fire) => 1.3,
                ("frozen_tundra", ElementalCore.Ice) => 1.3,
                ("underwater", ElementalCore.Water) => 1.3,
                _ => 1.0
            };
        }
        
        private int CalculateExperienceGain(BattleResultType result)
        {
            int baseExp = result == BattleResultType.Victory ? 100 : 25;
            return baseExp * (CurrentTurn / 10 + 1); // Bonus for longer battles
        }
        
        private Dictionary<string, int> CalculateRewards(BattleResultType result)
        {
            var rewards = new Dictionary<string, int>();
            
            if (result == BattleResultType.Victory)
            {
                rewards["Credits"] = 50 + (CurrentTurn * 5);
                rewards["Scrap_Metal"] = _rng.Next(1, 4);
            }
            
            return rewards;
        }
        
        private void ChangeState(BattleState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
        
        private void Log(string message)
        {
            BattleLog.Add(message);
            OnBattleLogUpdated?.Invoke(message);
        }
    }
    
    /// <summary>
    /// Types of battles
    /// </summary>
    public enum BattleType
    {
        Wild,       // Against wild bots (can capture)
        Trainer,    // Against NPC trainers
        Player,     // PvP battles
        Arena,      // Tournament/gym battles
        Raid        // Co-op raid battles
    }
    
    /// <summary>
    /// Represents an action taken by a bot in battle
    /// </summary>
    public class BotAction
    {
        public ActionType Type { get; set; }
        public GladiatorBot ActingBot { get; set; }
        public GladiatorBot Target { get; set; }
        public string MoveName { get; set; } = string.Empty;
        public string ItemUsed { get; set; } = string.Empty;
        public int Priority { get; set; } = 0; // For speed calculations
    }
    
    /// <summary>
    /// Types of actions in battle
    /// </summary>
    public enum ActionType
    {
        Attack,
        Defend,
        UseItem,
        Swap,
        Capture,
        Special
    }
    
    /// <summary>
    /// Result of a completed battle
    /// </summary>
    public class BattleOutcome
    {
        public BattleResultType Result { get; set; }
        public int TurnsElapsed { get; set; }
        public GladiatorBot? CapturedBot { get; set; }
        public int ExperienceGained { get; set; }
        public Dictionary<string, int> RewardsEarned { get; set; } = new();
    }
    
    /// <summary>
    /// Battle outcome types
    /// </summary>
    public enum BattleResultType
    {
        Victory,
        Defeat,
        Draw,
        Fled,
        Captured
    }
}