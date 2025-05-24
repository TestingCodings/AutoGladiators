
namespace StateMachineLib {
    public enum RobotState {
        Idle, Patrol, Attack, Flee, Recharge, Disabled
    }

    public class StateMachine {
        public RobotState CurrentState { get; private set; }

        public void TransitionTo(RobotState newState) {
            CurrentState = newState;
        }

        public void Update() {
            switch (CurrentState) {
                case RobotState.Idle:
                    // Idle logic
                    break;
                case RobotState.Attack:
                    // Attack logic
                    break;
                case RobotState.Flee:
                    // Flee logic
                    break;
            }
        }
    }
}
