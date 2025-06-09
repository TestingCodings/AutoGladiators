using NUnit.Framework;
using AutoGladiators.Client.StateMachines;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Tests.StateMachines
{
    public class BattleStateMachineTests
    {
        private BattleStateMachine _stateMachine;

        [SetUp]
        public void Setup()
        {
            var bot1 = new GladiatorBot { Name = "Alpha", MaxHealth = 100 };
            var bot2 = new GladiatorBot { Name = "Beta", MaxHealth = 100 };
            _stateMachine = new BattleStateMachine(bot1, bot2);
        }

        [Test]
        public void InitialState_ShouldBeIdle()
        {
            Assert.AreEqual("Idle", _stateMachine.CurrentState.ToString());
        }

        [Test]
        public void Attack_ShouldReduceOpponentHealth()
        {
            int initialHealth = _stateMachine.Enemy.MaxHealth;
            _stateMachine.PerformPlayerAction("PowerStrike");

            Assert.Less(_stateMachine.Enemy.MaxHealth, initialHealth);
        }
    }
}