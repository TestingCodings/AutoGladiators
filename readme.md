🤖 AutoGladiators
AutoGladiators is a cross-platform, turn-based robot battle RPG built with .NET MAUI.
Players explore locations, encounter wild bots, and engage in strategic battles to earn rewards, upgrade their team, and progress through the game world.

🎯 Current Focus (MVP)
The current development branch is building a core gameplay loop:

Adventure – Navigate locations and trigger wild encounters.

Encounter – A wild GladiatorBot appears.

Battle – Turn-based combat using your bot’s moves.

Victory – Earn XP and gold, then return to Adventure.

🔹 Features in Progress
Starter Bot & Roster – Begin with a basic GladiatorBot in your team.

Wild Encounter Generation – Spawn random enemy bots based on location.

Battle Manager – Handles turn resolution, damage calculation, and victory/defeat detection.

State Machine Architecture – Game flow is managed by modular states (ExploringState, BattlingState, VictoryState, etc.).

Cross-Platform UI – Shared logic across Android and Windows builds.

🛠 Tech Stack
Framework: .NET MAUI (Android + Windows targets)

Architecture: State machine-driven game flow

Language: C#

Data: Simple in-memory game state (SQLite planned)

UI Binding: MVVM pattern (ViewModels + XAML)

📅 Roadmap
Phase 1 (MVP Loop)

Adventure → Encounter → Battle → Victory → Back to Adventure.

Stable, crash-free gameplay with minimal UI.

Phase 2

Expand locations and encounter variety.

Add multiple player bots and roster management.

Introduce more moves, status effects, and AI variation.

Phase 3

NPC interactions, shops, quests, and story progression.

Persistent player saves.

Animation, polish, and balance pass.

🚀 Development Notes
The old behaviour profiles, races, and pure simulation mode have been removed from the core focus.

The codebase still contains some legacy files, which are being refactored or removed in stages.

A logging branch is in progress to aid debugging and track state transitions.

