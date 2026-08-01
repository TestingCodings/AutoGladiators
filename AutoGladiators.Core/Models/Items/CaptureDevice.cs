using System;
using System.Collections.Generic;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Core;

namespace AutoGladiators.Core.Models.Items
{
    /// <summary>
    /// Represents capture devices used to catch wild bots
    /// </summary>
    public class CaptureGear
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public double Effectiveness { get; } // Base capture multiplier
        public int Cost { get; }
        public CaptureGearType Type { get; }
        public Dictionary<string, double> SpecialModifiers { get; }
        
        public CaptureGear(string id, string name, string description, double effectiveness, 
                           int cost, CaptureGearType type)
        {
            Id = id;
            Name = name;
            Description = description;
            Effectiveness = effectiveness;
            Cost = cost;
            Type = type;
            SpecialModifiers = new Dictionary<string, double>();
        }
        
        // Predefined capture devices
        public static readonly CaptureGear BasicNet = new(
            "basic_net", "Basic Net", 
            "A simple containment net. Cheap but not very effective.", 
            1.0, 50, CaptureGearType.Net);
            
        public static readonly CaptureGear ReinforcedNet = new(
            "reinforced_net", "Reinforced Net",
            "A stronger net with metal fibers. Better success rate.",
            1.3, 120, CaptureGearType.Net);
            
        public static readonly CaptureGear EMPCage = new(
            "emp_cage", "EMP Cage",
            "Electromagnetic cage that disrupts bot systems. Very effective on electronic types.",
            1.5, 200, CaptureGearType.EMP);
            
        public static readonly CaptureGear StasisPod = new(
            "stasis_pod", "Stasis Pod",
            "Advanced containment using temporal stasis fields. High success rate.",
            2.0, 500, CaptureGearType.Stasis);
            
        public static readonly CaptureGear MasterTrap = new(
            "master_trap", "Master Trap",
            "Military-grade capture device. Can contain even legendary bots.",
            2.5, 1000, CaptureGearType.Master);
            
        // Specialized devices
        public static readonly CaptureGear CryoTrap = new(
            "cryo_trap", "Cryo Trap",
            "Freezes the target while capturing. Bonus effectiveness against overheated bots.",
            1.4, 300, CaptureGearType.Cryo);
            
        public static readonly CaptureGear MagneticTrap = new(
            "magnetic_trap", "Magnetic Trap",
            "Uses powerful magnets. Super effective against metal-based bots.",
            1.6, 350, CaptureGearType.Magnetic);
        
        // Get all available capture devices
        public static List<CaptureGear> GetAllDevices()
        {
            return new List<CaptureGear>
            {
                BasicNet, ReinforcedNet, EMPCage, StasisPod, 
                MasterTrap, CryoTrap, MagneticTrap
            };
        }
        
        /// <summary>
        /// Gets effectiveness against specific bot type
        /// </summary>
        public double GetEffectivenessAgainst(GladiatorBot bot)
        {
            double baseEffectiveness = Effectiveness;
            
            // Apply type-specific modifiers
            switch (Type)
            {
                case CaptureGearType.EMP:
                    if (bot.ElementalCore == ElementalCore.Electric || bot.ElementalCore == ElementalCore.Metal)
                        baseEffectiveness *= 1.5; // 50% bonus against electronic bots
                    break;
                    
                case CaptureGearType.Cryo:
                    if (bot.HasStatus(StatusEffectType.Burn) || bot.ElementalCore == ElementalCore.Fire)
                        baseEffectiveness *= 1.3; // 30% bonus against fire/heated bots
                    break;
                    
                case CaptureGearType.Magnetic:
                    if (bot.ElementalCore == ElementalCore.Metal || bot.Name.Contains("Steel"))
                        baseEffectiveness *= 1.4; // 40% bonus against metal bots
                    break;
            }
            
            return baseEffectiveness;
        }
        
        /// <summary>
        /// Checks if device can be used in current situation
        /// </summary>
        public bool CanUse(GladiatorBot bot, string environment = "")
        {
            switch (Type)
            {
                case CaptureGearType.EMP:
                    // EMP devices don't work in electromagnetic storms
                    return environment != "electromagnetic_storm";
                    
                case CaptureGearType.Cryo:
                    // Cryo devices don't work in extreme heat
                    return environment != "lava_field" && environment != "thermal_plant";
                    
                default:
                    return true;
            }
        }
    }
    
    /// <summary>
    /// Types of capture devices
    /// </summary>
    public enum CaptureGearType
    {
        Net,        // Physical containment
        EMP,        // Electromagnetic disruption
        Stasis,     // Temporal/energy containment
        Master,     // Advanced multi-type
        Cryo,       // Freezing-based
        Magnetic,   // Magnetic containment
        Sonic,      // Sound-based disruption
        Plasma      // Energy-based containment
    }
}
