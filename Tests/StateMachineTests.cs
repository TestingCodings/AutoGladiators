
using NUnit.Framework;
using StateMachineLib;

namespace AutoGladiators.Tests {
    public class StateMachineTests {
        [Test]
        public void Should_Transition_To_Attack() {
            var sm = new StateMachine();
            sm.TransitionTo(RobotState.Attack);
            Assert.AreEqual(RobotState.Attack, sm.CurrentState);
        }
    }
}
