using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Items;

namespace AutoGladiators.Core.Services
{
    /// <summary>
    /// Handles the capture mechanics for wild bots
    /// Based on Pokemon-style capture system with robot-specific elements
    /// </summary>
    public class CaptureService
    {
        private readonly Random _rng = new();
        
        /// <summary>
        /// Attempts to capture a wild bot using specified capture device
        /// </summary>
        /// <param name="wildBot">The bot to capture</param>
        /// <param name="CaptureGear">Device used for capture</param>
        /// <param name="playerLevel">Player's current level</param>
        /// <param name="environmentModifier">Environmental bonus (e.g., special locations)</param>
        /// <returns>Capture result with success status and details</returns>
        public CaptureResult AttemptCapture(GladiatorBot wildBot, CaptureGear CaptureGear, 
                                          int playerLevel, double environmentModifier = 1.0)
        {
            if (wildBot == null) throw new ArgumentNullException(nameof(wildBot));
            if (CaptureGear == null) throw new ArgumentNullException(nameof(CaptureGear));
            
            // Calculate capture chance using comprehensive formula
            double captureChance = CalculateCaptureChance(wildBot, CaptureGear, playerLevel, environmentModifier);
            
            // Roll for capture success
            bool success = _rng.NextDouble() < captureChance;
            
            // Create result
            var result = new CaptureResult
            {
                Success = success,
                CaptureChance = captureChance,
                BotCaptured = success ? wildBot : null,
                DeviceUsed = CaptureGear,
                AttemptNumber = 1, // TODO: Track multiple attempts
                ShakeCount = CalculateShakeCount(captureChance),
                Message = GenerateCaptureMessage(success, wildBot, CaptureGear)
            };
            
            // Apply capture success effects
            if (success)
            {
                ApplyCaptureSuccess(wildBot);
            }
            
            return result;
        }
        
        /// <summary>
        /// Calculates the probability of successful capture
        /// Formula: baseRate × (1 - currentHP/maxHP) × statusBonus × deviceBonus × rarityPenalty × levelBonus × environmentModifier
        /// </summary>
        private double CalculateCaptureChance(GladiatorBot bot, CaptureGear device, 
                                           int playerLevel, double environmentModifier)
        {
            // Base capture rate by bot class/rarity
            double baseRate = GetBaseCaptureRate(bot);
            
            // Health modifier - lower HP = higher capture chance
            double healthModifier = 1.0 - (double)bot.CurrentHealth / bot.MaxHealth;
            healthModifier = 0.1 + (healthModifier * 0.8); // Scale from 10% to 90%
            
            // Status effect bonuses
            double statusBonus = GetStatusBonus(bot);
            
            // Device effectiveness
            double deviceBonus = device.Effectiveness;
            
            // Rarity penalty - rarer bots are harder to capture
            double rarityPenalty = GetRarityPenalty(bot);
            
            // Player level bonus - higher level players have slight advantage
            double levelBonus = 1.0 + (playerLevel * 0.01); // 1% per level, capped
            levelBonus = Math.Min(levelBonus, 1.5); // Max 50% bonus
            
            // Calculate final chance
            double finalChance = baseRate * healthModifier * statusBonus * deviceBonus * 
                               rarityPenalty * levelBonus * environmentModifier;
            
            // Cap between 1% and 95%
            return Math.Clamp(finalChance, 0.01, 0.95);
        }
        
        private double GetBaseCaptureRate(GladiatorBot bot)
        {
            // Base rates by bot class - can be expanded with more granular system
            return bot.Level switch
            {
                <= 5 => 0.4,   // Low level bots easier to capture
                <= 10 => 0.3,
                <= 20 => 0.2,
                <= 30 => 0.15,
                _ => 0.1        // High level bots very difficult
            };
        }
        
        private double GetStatusBonus(GladiatorBot bot)
        {
            double bonus = 1.0;
            
            // Check for beneficial status effects
            if (bot.HasStatus(StatusEffectType.Stun) || bot.HasStatus(StatusEffectType.Paralysis))
                bonus += 0.5; // 50% bonus for immobilizing effects
            
            if (bot.HasStatus(StatusEffectType.Freeze))
                bonus += 0.3; // 30% bonus for frozen
                
            if (bot.HasStatus(StatusEffectType.Confusion))
                bonus += 0.2; // 20% bonus for confusion
                
            if (bot.HasStatus(StatusEffectType.Poison) || bot.HasStatus(StatusEffectType.Burn))
                bonus += 0.1; // 10% bonus for damage over time
            
            return bonus;
        }
        
        private double GetRarityPenalty(GladiatorBot bot)
        {
            // Determine rarity based on level and stats - can be expanded with explicit rarity system
            int totalStats = bot.MaxHealth + bot.AttackPower + bot.Defense + bot.Speed;
            int avgStat = totalStats / 4;
            
            return avgStat switch
            {
                <= 50 => 1.0,    // Common - no penalty
                <= 80 => 0.8,    // Uncommon - 20% penalty
                <= 120 => 0.6,   // Rare - 40% penalty
                <= 150 => 0.4,   // Epic - 60% penalty
                _ => 0.2          // Legendary - 80% penalty
            };
        }
        
        private int CalculateShakeCount(double captureChance)
        {
            // More shakes for higher capture chances (visual feedback)
            if (captureChance >= 0.8) return 4;
            if (captureChance >= 0.6) return 3;
            if (captureChance >= 0.4) return 2;
            if (captureChance >= 0.2) return 1;
            return 0;
        }
        
        private void ApplyCaptureSuccess(GladiatorBot bot)
        {
            // Reset any temporary battle effects
            bot.ResetStages();
            bot.Cleanse(); // Remove status effects
            
            // Heal to reasonable level for newly caught bots
            bot.CurrentHealth = Math.Max(bot.CurrentHealth, bot.MaxHealth / 4);
            
            // Set ownership flags
            bot.HasOwner = true;
        }
        
        private string GenerateCaptureMessage(bool success, GladiatorBot bot, CaptureGear device)
        {
            if (success)
            {
                return $"Success! {bot.Name} has been captured using {device.Name}!";
            }
            else
            {
                var messages = new[]
                {
                    $"{bot.Name} broke free from the {device.Name}!",
                    $"The {device.Name} failed to contain {bot.Name}!",
                    $"{bot.Name} resisted capture!",
                    $"So close! {bot.Name} escaped at the last second!"
                };
                return messages[_rng.Next(messages.Length)];
            }
        }
        
        /// <summary>
        /// Gets recommended capture device based on bot and situation
        /// </summary>
        public CaptureGear GetRecommendedDevice(GladiatorBot bot, List<CaptureGear> availableDevices)
        {
            if (availableDevices == null || !availableDevices.Any())
                return CaptureGear.BasicNet; // Default fallback
            
            // Simple recommendation logic - can be enhanced
            var bestDevice = availableDevices
                .OrderByDescending(d => d.Effectiveness)
                .FirstOrDefault();
            
            return bestDevice ?? CaptureGear.BasicNet;
        }
        
        /// <summary>
        /// Checks if a bot requires special capture conditions
        /// </summary>
        public bool RequiresSpecialCapture(GladiatorBot bot)
        {
            // Legendary or very high-level bots might need special conditions
            return bot.Level >= 25 || bot.MaxHealth >= 200;
        }
        
        /// <summary>
        /// Gets special capture requirements for a bot
        /// </summary>
        public List<string> GetSpecialRequirements(GladiatorBot bot)
        {
            var requirements = new List<string>();
            
            if (bot.Level >= 30)
                requirements.Add("Must be weakened to below 25% health");
            
            if (bot.MaxHealth >= 250)
                requirements.Add("Requires Master-grade capture device");
            
            // Berserk status would need to be added to StatusEffectType enum
            // if (bot.HasStatus(StatusEffectType.Berserk))
            //     requirements.Add("Must be calmed or stunned first");
            
            return requirements;
        }
    }
    
    /// <summary>
    /// Result of a capture attempt
    /// </summary>
    public class CaptureResult
    {
        public bool Success { get; set; }
        public double CaptureChance { get; set; }
        public GladiatorBot? BotCaptured { get; set; }
        public CaptureGear DeviceUsed { get; set; } = CaptureGear.BasicNet; // Default to avoid null
        public int AttemptNumber { get; set; }
        public int ShakeCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool EscapedAfterShakes => !Success && ShakeCount > 0;
    }
}
