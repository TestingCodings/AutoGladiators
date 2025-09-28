using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Logic;

namespace AutoGladiators.Core.StateMachine
{
    public enum GameStateId
    {
        Idle,
        Exploring,
        Dialogue,
        Battling,
        Inventory,
        Victory,
        Defeat,
        Fusion,
        Training,
        Capturing,
        CaptureSuccess,
        CaptureFailed,
        GameOver, // <-- Add this line
        // Add more states as needed
    }

    // Optional: pass data into states in a type-safe way
    public record StateArgs
    {
        public object? Payload { get; init; }     // e.g., Encounter info, NPC id, etc.
        public string? Reason { get; init; }      // e.g., "RandomEncounter", "UserOpenedBag"
    }

    // NOTE: StateTransition is defined in StateMachine/StateTransition.cs (single source of truth)

    public interface IGameState
    {
        GameStateId Id { get; }
        Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default);
        Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default);
        Task ExitAsync(GameStateContext ctx, CancellationToken ct = default);
    }

    // Everything states need access to (services, managers, UI signals)
    public sealed class GameStateContext
    {
        public GameStateService Game { get; }
        public BattleManager Battle { get; }
        public IMessageBus? Bus { get; }  // nullable so you can pass null for now
        public IUiBridge? Ui { get; }     // nullable so you can pass null for now

        // The machine will set this so legacy code can request transitions
        public System.Func<GameStateId, StateArgs?, CancellationToken, Task>? RequestTransitionAsync { get; set; }

        public GameStateContext(GameStateService game, BattleManager battle, IMessageBus? bus = null, IUiBridge? ui = null)
        {
            Game = game;
            Battle = battle;
            Bus = bus;
            Ui = ui;
        }
    }
}
