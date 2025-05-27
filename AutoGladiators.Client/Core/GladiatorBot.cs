using System;
using System.Collections.Generic;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.StateMachine.States;
using AutoGladiators.Client.Core.Behaviors;

namespace AutoGladiators.Client.Core
{
    public class GladiatorBot
    {
        // Identity
        public string Name { get; set; }

        // Vital stats
        public int Health { get; set; }
        public int Energy { get; set; }
        public int MaxHealth { get; private set; }
        public int MaxEnergy { get; private set; }

        // Traits and progression
        public double Speed { get; set; }
        public double Agility { get; set; }
        public double Strength { get; set; }
        public double Intelligence { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public double LastDamageTaken { get; set; }

        // Modifiers and dynamic battle environment
        public List<string> Modifiers { get; private set; } = new();

        // State and AI
        public GladiatorAction LastAction { get; private set; } = GladiatorAction.None;
        public bool IsCharging { get; private set; } = false;
        public bool IsDefending { get; private set; } = false;
        public IBehaviorProfile Behavior { get; set; }
        public GameStateMachine StateMachine { get; set; }

        // Historical outcomes
        public object LastRaceResult { get; set; }
        public object LastBattleResult { get; set; }

        public GladiatorBot(string name, IBehaviorProfile behavior, int maxHealth, int startEnergy, List<string>? modifiers = null)
        {
            Name = name;
            Behavior = behavior;
            MaxHealth = maxHealth;
            MaxEnergy = startEnergy;
            Health = maxHealth;
            Energy = startEnergy;

            if (modifiers != null)
                Modifiers.AddRange(modifiers);

            ApplyModifiers();

            var initialState = new IdleState();
            StateMachine = new GameStateMachine(initialState);
            StateMachine.Initialize(this);

        }

        private void ApplyModifiers()
        {
            foreach (var mod in Modifiers)
            {
                switch (mod)
                {
                    case "HealthBoost":
                        Health += 20;
                        MaxHealth += 20;
                        break;
                    case "EnergyBoost":
                        Energy += 20;
                        MaxEnergy += 20;
                        break;
                    case "EnergyDrain":
                        Energy -= 10;
                        break;
                    case "EvadeBoost":
                        Console.WriteLine($"{Name} gains higher evade ability.");
                        break;
                    case "LavaHazard":
                        Health -= 5;
                        break;
                    case "SlowRegeneration":
                        Console.WriteLine($"{Name} regenerates slower.");
                        break;
                    case "Stealth":
                        Console.WriteLine($"{Name} gains stealth advantage.");
                        break;
                    case "ZeroGravity":
                        Energy += 5;
                        break;
                }
            }

            Health = Math.Min(Health, MaxHealth);
            Energy = Math.Clamp(Energy, 0, MaxEnergy);
        }

        public void TakeTurn(GladiatorBot opponent)
        {
            Behavior.ExecuteStrategy(this, opponent);
        }

        public void Attack(GladiatorBot target)
        {
            LastAction = GladiatorAction.Attack;
            IsCharging = false;
            IsDefending = false;
            target.ReceiveDamage(10);
        }

        public void PowerStrike(GladiatorBot target)
        {
            LastAction = GladiatorAction.PowerStrike;
            IsCharging = false;
            IsDefending = false;

            if (Energy >= 20)
            {
                target.ReceiveDamage(25);
                Energy -= 20;
            }
        }

        public void Defend()
        {
            LastAction = GladiatorAction.Defend;
            IsDefending = true;
            IsCharging = false;
        }

        public void Evade() => LastAction = GladiatorAction.Evade;

        public void Parry() => LastAction = GladiatorAction.Parry;

        public void CounterAttack(GladiatorBot target)
        {
            LastAction = GladiatorAction.CounterAttack;
            target.ReceiveDamage(15);
        }

        public void Charge()
        {
            LastAction = GladiatorAction.Charge;
            IsCharging = true;
            Energy += 15;
        }

        public void ReceiveDamage(int amount)
        {
            if (IsDefending)
                amount /= 2;

            Health -= amount;
            LastDamageTaken = amount;
        }

        public void Train()
        {
            // TODO: Training logic based on current behavior
        }

        public void LevelUp()
        {
            Level++;
            Experience = 0;
            MaxHealth += 10;
            MaxEnergy += 5;
        }

        public void ResetTempStats()
        {
            // TODO: Reset temporary buffs like IsCharging or turn-limited effects
        }

        public void ResetBattleState()
        {
            IsCharging = false;
            IsDefending = false;
            LastAction = GladiatorAction.None;
        }

        public void RecordDefeat()
        {
            // TODO: Log defeat stats or penalties
        }

        public bool IsAlive => Health > 0;
    }
}
