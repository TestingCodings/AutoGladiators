using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Core;
using AutoGladiators.Client.Pages;
using AutoGladiators.Client.ViewModels;

namespace AutoGladiators.Client.Services
{
    public class MauiUiBridge : IUiBridge
    {
        // ---- Generic helpers ----
        public void SetStatus(string status)
        {
            // Could show in a status bar or toast
            Console.WriteLine($"Status: {status}");
        }

        public void ShowToast(string message)
        {
            // For now, just log - could implement with Community Toolkit
            Console.WriteLine($"Toast: {message}");
        }

        // ---- Overworld / exploring ----
        public void ShowOverworld() { }
        public void HideOverworld() { }

        // ---- Battle HUD ----
        public void ShowBattleHud(GladiatorBot player, GladiatorBot enemy)
        {
            // Battle HUD is handled by BattlePage
        }

        public void HideBattleHud() { }

        // ---- Victory / Defeat ----
        public void ShowVictoryScreen(object? rewards)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Extract reward data from the dynamic object
                    var rewardData = rewards as dynamic;
                    if (rewardData != null)
                    {
                        var enemyName = rewardData.enemyName?.ToString() ?? "Enemy";
                        var enemyLevel = (int)(rewardData.enemyLevel ?? 1);
                        var xp = (int)(rewardData.xp ?? 0);
                        var gold = (int)(rewardData.gold ?? 0);

                        var victoryViewModel = new VictoryViewModel(enemyName, enemyLevel, xp, gold);
                        var victoryPage = new VictoryPage(victoryViewModel);
                        
                        await Shell.Current.Navigation.PushAsync(victoryPage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error showing victory screen: {ex.Message}");
                    // Fallback to simple alert
                    await Application.Current?.MainPage?.DisplayAlert("Victory!", 
                        "You won the battle!", "Continue");
                }
            });
        }

        public void HideVictoryScreen()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Pop the victory page if it's on the navigation stack
                    if (Shell.Current.Navigation.NavigationStack.Any())
                    {
                        var currentPage = Shell.Current.Navigation.NavigationStack.LastOrDefault();
                        if (currentPage is VictoryPage)
                        {
                            await Shell.Current.Navigation.PopAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error hiding victory screen: {ex.Message}");
                }
            });
        }

        public void ShowDefeatScreen(object? details)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current?.MainPage?.DisplayAlert("Defeat", 
                    "Your bot was defeated, but you can try again!", "Continue");
            });
        }

        public void HideDefeatScreen() { }

        // ---- Inventory ---- (Stub implementations for now)
        public void ShowInventory(object inventory) { }
        public void RefreshInventory(object inventory) { }
        public void HideInventory() { }
        public bool RequestedCloseInventory() => false;
        public bool RequestedOpenInventory() => false;
        public (Guid? itemId, int? quantity) GetItemUseRequest() => (null, null);
        public Guid? RequestedDiscardItemId() => null;
        public void ShowItemUseResult(object result) { }

        // ---- Dialogue ---- (Stub implementations)
        public void ShowDialogueForNpc(string npcId, bool modal = true) { }
        public void HideDialogue() { }
        public void ShowDialogueForNpc(string npcId, string dialogueFile) { }
        public bool DialogueRequestedBattle() => false;
        public bool DialogueCompleted() => false;

        // ---- Idle ---- (Stub implementations)
        public void ShowIdleScreen() { }
        public void HideIdleScreen() { }
        public bool RequestedStart() => false;
        public string? RequestedDialogueNpcId() => null;

        // ---- Training ---- (Stub implementations)
        public void ShowTrainingScreen() { }
        public void HideTrainingScreen() { }
        public void ShowTrainingProgress(int current, int total) { }
        public void HideTrainingProgress() { }
        public void TrainingCancelled() { }
        public bool TrainingContinueRequested() => false;
        public void ShowTrainingResult(object result) { }
        public void TrainingCompleted() { }

        // ---- Fusion ---- (Stub implementations)
        public void ShowFusionScreen() { }
        public void HideFusionScreen() { }
        public void FusionCancelled() { }
        public (Guid? leftBotId, Guid? rightBotId) GetFusionSelection() => (null, null);
        public void ShowFusionResult(object result) { }

        // ---- Capturing ---- (Stub implementations)
        public void ShowCaptureAnimation() { }
        public void UpdateCaptureAnimationOutcome(bool success) { }
        public void HideCaptureAnimation() { }
        public void ShowCaptureSuccess(object? info = null) { }
        public void HideCaptureSuccess() { }
        public void ShowCaptureFailed(object? info = null) { }
        public void HideCaptureFailed() { }
    }
}