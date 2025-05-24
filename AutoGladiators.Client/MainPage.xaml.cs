
---

## 🖥️ **Advanced AutoGladiators.Client** (Xamarin.Forms App)

**File:** `AutoGladiators.Client/MainPage.xaml.cs`

```csharp
using Xamarin.Forms;
using StateMachineLib;
using BehaviorEngineLib;
using SkillTreeLib;

namespace AutoGladiators.Client {
    public partial class MainPage : ContentPage {
        private StateMachine robotStateMachine;
        private BehaviorProfile currentBehaviorProfile;
        private SkillTree currentSkillTree;

        public MainPage() {
            InitializeComponent();
            SetupRobot();
        }

        private void SetupRobot() {
            robotStateMachine = new StateMachine();
            currentBehaviorProfile = new BehaviorProfile {
                Intelligence = 85,
                ReactionTimeMs = 100,
                Aggression = 0.75f,
                AwarenessRadius = 15f,
                Patience = 0.5f,
                HasLearningEnabled = true
            };

            currentSkillTree = new SkillTree();
        }

        private void StartMatchButton_Clicked(object sender, EventArgs e) {
            robotStateMachine.TransitionTo(RobotState.Patrol);
            // Further logic to initiate match simulations
        }
    }
}
