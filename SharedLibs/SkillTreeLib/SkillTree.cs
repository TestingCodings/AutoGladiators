using System.Collections.Generic;

namespace SkillTreeLib {

    public enum SkillCategory {
        Physical, Weaponry
    }

    public class Skill {
        public string Name { get; set; }
        public SkillCategory Category { get; set; }
        public string Description { get; set; }
        public Dictionary<string, float> Effects { get; set; }  // e.g., {"Speed": 1.2f, "Armor": 0.8f}

        public Skill(string name, SkillCategory category, string description, Dictionary<string, float> effects) {
            Name = name;
            Category = category;
            Description = description;
            Effects = effects;
        }
    }

    public class SkillTree {
        public List<Skill> PhysicalSkills { get; set; }
        public List<Skill> WeaponrySkills { get; set; }

        public SkillTree() {
            PhysicalSkills = new List<Skill> {
                new Skill("Enhanced Agility", SkillCategory.Physical, "Improves dodge and movement.", new Dictionary<string, float>{{"Agility",1.3f}}),
                new Skill("Reinforced Chassis", SkillCategory.Physical, "Adds extra durability.", new Dictionary<string, float>{{"Armor",1.5f}}),
                new Skill("Overdrive", SkillCategory.Physical, "Temporarily boosts speed.", new Dictionary<string, float>{{"Speed",1.5f}})
            };

            WeaponrySkills = new List<Skill> {
                new Skill("Plasma Sword", SkillCategory.Weaponry, "Melee combat weapon with high damage.", new Dictionary<string, float>{{"Damage",2.0f}}),
                new Skill("Energy Shield", SkillCategory.Weaponry, "Temporary damage reduction shield.", new Dictionary<string, float>{{"Defense",1.5f}}),
                new Skill("Laser Cannon", SkillCategory.Weaponry, "Ranged high-impact weapon.", new Dictionary<string, float>{{"RangeDamage",1.8f}})
            };
        }
    }
}
