using System;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;

namespace AutoGladiators.Core.StateMachine
{
    public interface IMessageBus
    {
        void Publish(string topic, object? data = null);
        void Subscribe(string topic, Action<object?> handler);
        void Unsubscribe(string topic, Action<object?> handler);
    }

    /// <summary>
    /// Thin UI contract used by states. Implementations may no-op most methods.
    /// </summary>
    public interface IUiBridge
    {
        // ---- Generic helpers ----
        void SetStatus(string status);
        void ShowToast(string message);

        // ---- Overworld / exploring ----
        void ShowOverworld();
        void HideOverworld();

        // ---- Battle HUD ----
        void ShowBattleHud(GladiatorBot player, GladiatorBot enemy);
        void HideBattleHud();

        // ---- Victory / Defeat ----
        void ShowVictoryScreen(object? rewards);
        void HideVictoryScreen();
        void ShowDefeatScreen(object? details);
        void HideDefeatScreen();

        // ---- Inventory ----
        void ShowInventory(object inventory);
        void RefreshInventory(object inventory);
        void HideInventory();

        // Requested actions (polled by states). Return defaults if not supported.
        bool RequestedCloseInventory();            // false by default
        bool RequestedOpenInventory();             // false by default
        (Guid? itemId, int? quantity) GetItemUseRequest(); // (null,null) by default
        Guid? RequestedDiscardItemId();            // null by default
        void ShowItemUseResult(object result);     // optional feedback

        // ---- Dialogue ----
        // Basic show/hide
        void ShowDialogueForNpc(string npcId, bool modal = true);
        void HideDialogue();
        // Optional overload if you want to pass a file/key explicitly
        void ShowDialogueForNpc(string npcId, string dialogueFile);

        bool DialogueRequestedBattle();            // false by default
        bool DialogueCompleted();                  // false by default

        // ---- Idle ----
        void ShowIdleScreen();
        void HideIdleScreen();
        bool RequestedStart();                     // false by default
        string? RequestedDialogueNpcId();          // null by default

        // ---- Training ----
        void ShowTrainingScreen();
        void HideTrainingScreen();
        void ShowTrainingProgress(int current, int total);
        void HideTrainingProgress();
        void TrainingCancelled();
        bool TrainingContinueRequested();          // false by default
        void ShowTrainingResult(object result);
        void TrainingCompleted();

        // ---- Fusion ----
        void ShowFusionScreen();
        void HideFusionScreen();
        void FusionCancelled();
        // Return (null,null) by default
        (Guid? leftBotId, Guid? rightBotId) GetFusionSelection();
        void ShowFusionResult(object result);

        // ---- Capturing (throwing chip/ball etc.) ----
        void ShowCaptureAnimation();
        void UpdateCaptureAnimationOutcome(bool success);
        void HideCaptureAnimation();

        void ShowCaptureSuccess(object? info = null);
        void HideCaptureSuccess();

        void ShowCaptureFailed(object? info = null);
        void HideCaptureFailed();
    }

    // Legacy context left as-is if you still need it:
    public interface IGameStateContext
    {
        GladiatorBot Self { get; }
        GladiatorBot Enemy { get; }

        bool IsBattleOver { get; }

        void TransitionTo(IGameState newState);

        void Log(string message);

        void ApplyPostBattleEffects();
        void ResetTurnState();
        void AwardXP(int amount);
    }
}
