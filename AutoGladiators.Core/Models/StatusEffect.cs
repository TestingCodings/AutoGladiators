using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Models
{
    public class StatusEffect
    {
        public StatusEffectType Type { get; set; }
        public int Duration { get; set; }
        public int Intensity { get; set; } = 1; // For effects that stack in power
        public string? Description { get; set; }

        public StatusEffect()
        {
        }

        public StatusEffect(StatusEffectType type, int duration, int intensity = 1)
        {
            Type = type;
            Duration = duration;
            Intensity = intensity;
        }

        public void Tick()
        {
            if (Duration > 0)
                Duration--;
        }

        public bool IsExpired => Duration <= 0;

        public StatusEffect Clone()
        {
            return new StatusEffect
            {
                Type = Type,
                Duration = Duration,
                Intensity = Intensity,
                Description = Description
            };
        }
    }
}