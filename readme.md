# 🤖 AutoGladiators

**AutoGladiators** is a cross-platform AI-driven arena game where customizable robots ("GladiatorBots") battle, race, or train autonomously — powered by dynamic state machines, behavior profiles, and configurable environments.

---

## 🎮 Game Summary

AutoGladiators combines tactical bot building, stat management, and simulated battles to challenge both players' strategic thinking and AI programming logic.

Each bot has attributes and skills such as:

- ⚔️ Strength, Agility, Endurance
- 🧠 Intelligence, Adaptability
- 🛡️ Armor, Weapons (e.g., sword, gun, cannon)
- ⚡ Energy and Health Pools

Bots operate autonomously using **behavior profiles** and **state transitions**, determined by:

- Current Level
- Battle Conditions
- Victory Objectives
- Adaptive AI logic

---

## 🧠 Core Architecture

### ✅ State Machine Driven
Every GladiatorBot operates via a modular state machine:

- `IdleState`
- `TrainingState`
- `RacingState`
- `BattlingState`
- `VictoryState` / `DefeatState`

Transitions between states are handled through rules (e.g., timers, win conditions, defeat, user actions).

---

### 🧠 Behavior Profiles

Each bot's personality and tactics are defined by a `BehaviorProfile` interface:

```csharp
int Intelligence { get; }
int Adaptability { get; }
string DecideAction(GladiatorBot self, GladiatorBot opponent, string currentState);
Profiles include:

AggressiveBehavior – Always seeks to attack

DefensiveBehavior – Prioritizes evasion and self-preservation

BalancedBehavior – Adapts based on energy, stats, and game state

ReactiveBehavior – Counterattacks and pivots based on observed patterns

⚙️ Level Configuration
Levels are JSON-driven and define:

Mode: Battle, Race, or Training

Modifiers: PowerStrikes, EnergyDrain, EvadeBoost, etc.

BotSettings: Initial energy, health

VictoryCondition: LastBotStanding, FirstToFinish, TimeSurvival, etc.

Example:

json
Copy
Edit
{
  "LevelName": "Arena Alpha",
  "Mode": "Battle",
  "Environment": "Desert",
  "Modifiers": ["PowerStrikes", "Evade"],
  "Rules": {
    "VictoryCondition": "LastBotStanding"
  }
}
📲 Mobile and Cross-Platform
Built with .NET MAUI, the game is:

✔️ Deployable to Android and Windows

✔️ Extensible to iOS and macOS

✔️ CI/CD-enabled (Jenkins support in progress)

🔮 Future Functionality
Planned features:

🧩 Game Mechanics
Stat-based crafting & upgrades

Unlockable parts (e.g., new chassis, boosters)

Shop economy using earned coins

AI training via persistent evolution (ML-style leveling)

🧠 AI Enhancements
AI learns from defeat history

Bots develop skill trees over time

Multiplayer: Bots fight other players’ designs asynchronously

🖼 UI & UX
Drag-and-drop bot customization

Animated battle replays

Simulation viewer with bot telemetry overlays

🌍 Online Features (Stretch)
Leaderboards and Bot Ratings

Shareable behavior profiles

Bot tournaments with procedural levels

🚀 How to Build and Run
Clone the Repo

bash
Copy
Edit
git clone https://github.com/YourUser/AutoGladiators.git
cd AutoGladiators
Build the Solution

Open with Visual Studio 2022 or later (with .NET MAUI workloads)

Or use CLI:

bash
Copy
Edit
dotnet build -t:Run -f net8.0-android
Deploy to Android Emulator or Device

bash
Copy
Edit
dotnet build -t:Install -f net8.0-android
👨‍💻 Contributing
This project is in active development. Contributions welcome via pull requests, especially in the following areas:

Battle logic optimization

Visual simulation views

Audio feedback

New AI profiles