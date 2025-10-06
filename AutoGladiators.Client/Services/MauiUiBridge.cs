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

        // ---- Inventory ---- (Production implementations)
        private bool _inventoryVisible = false;
        private bool _requestedCloseInventory = false;
        private bool _requestedOpenInventory = false;
        private (string? itemId, int? quantity) _itemUseRequest = (null, null);
        private string? _requestedDiscardItemId = null;
        
        public void ShowInventory(object inventory) 
        {
            if (!_inventoryVisible)
            {
                _inventoryVisible = true;
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync("//InventoryPage");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error navigating to inventory: {ex.Message}");
                        // Fallback: just set the flag
                        _inventoryVisible = true;
                    }
                });
            }
        }
        
        public void RefreshInventory(object inventory) 
        {
            // Trigger refresh through messaging system (modern approach would use WeakReferenceMessenger)
            // For now, just mark that refresh is needed
            Console.WriteLine("Inventory refresh requested");
        }
        
        public void HideInventory() 
        {
            if (_inventoryVisible)
            {
                _inventoryVisible = false;
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (Shell.Current.Navigation.NavigationStack.Count > 1)
                        {
                            await Shell.Current.Navigation.PopAsync();
                        }
                        else
                        {
                            await Shell.Current.GoToAsync("//AdventurePage");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error hiding inventory: {ex.Message}");
                        _inventoryVisible = false;
                    }
                });
            }
        }
        
        public bool RequestedCloseInventory() 
        {
            bool requested = _requestedCloseInventory;
            _requestedCloseInventory = false; // Reset after checking
            return requested;
        }
        
        public bool RequestedOpenInventory() 
        {
            bool requested = _requestedOpenInventory;
            _requestedOpenInventory = false; // Reset after checking
            return requested;
        }
        
        public (Guid? itemId, int? quantity) GetItemUseRequest() 
        {
            var request = _itemUseRequest;
            _itemUseRequest = (null, null); // Reset after reading
            
            // Convert string itemId to Guid if possible
            if (Guid.TryParse(request.itemId, out Guid guid))
            {
                return (guid, request.quantity);
            }
            return (null, null);
        }
        
        public Guid? RequestedDiscardItemId() 
        {
            var itemId = _requestedDiscardItemId;
            _requestedDiscardItemId = null; // Reset after reading
            
            if (Guid.TryParse(itemId, out Guid guid))
            {
                return guid;
            }
            return null;
        }
        
        public void ShowItemUseResult(object result) 
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (result != null)
                {
                    string message = result.ToString() ?? "Item used successfully.";
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Item Used", message, "OK");
                    }
                }
            });
        }
        
        // Public methods for UI to trigger inventory actions
        public void RequestCloseInventory() => _requestedCloseInventory = true;
        public void RequestOpenInventory() => _requestedOpenInventory = true;
        public void RequestUseItem(string itemId, int quantity) => _itemUseRequest = (itemId, quantity);
        public void RequestDiscardItem(string itemId) => _requestedDiscardItemId = itemId;

        // ---- Dialogue ---- (Production implementations)
        private bool _dialogueVisible = false;
        private bool _dialogueRequestedBattle = false;
        private bool _dialogueCompleted = false;
        private string? _currentNpcId = null;
        
        public void ShowDialogueForNpc(string npcId, bool modal = true) 
        {
            if (!_dialogueVisible && !string.IsNullOrEmpty(npcId))
            {
                _dialogueVisible = true;
                _currentNpcId = npcId;
                _dialogueCompleted = false;
                _dialogueRequestedBattle = false;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Navigate to dialogue page with NPC ID as parameter
                        await Shell.Current.GoToAsync($"//NPCDialoguePage?npcId={npcId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error showing dialogue for NPC {npcId}: {ex.Message}");
                        _dialogueVisible = false;
                    }
                });
            }
        }
        
        public void HideDialogue() 
        {
            if (_dialogueVisible)
            {
                _dialogueVisible = false;
                _currentNpcId = null;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (Shell.Current.Navigation.NavigationStack.Count > 1)
                        {
                            await Shell.Current.Navigation.PopAsync();
                        }
                        else
                        {
                            // Return to exploration page
                            await Shell.Current.GoToAsync("//ExplorationPage");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error hiding dialogue: {ex.Message}");
                        _dialogueVisible = false;
                    }
                });
            }
        }
        
        public void ShowDialogueForNpc(string npcId, string dialogueFile) 
        {
            // Enhanced version with specific dialogue file
            if (!_dialogueVisible && !string.IsNullOrEmpty(npcId))
            {
                _dialogueVisible = true;
                _currentNpcId = npcId;
                _dialogueCompleted = false;
                _dialogueRequestedBattle = false;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Navigate with both NPC ID and dialogue file
                        await Shell.Current.GoToAsync($"//NPCDialoguePage?npcId={npcId}&dialogueFile={dialogueFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error showing dialogue for NPC {npcId} with file {dialogueFile}: {ex.Message}");
                        _dialogueVisible = false;
                    }
                });
            }
        }
        
        public bool DialogueRequestedBattle() 
        {
            bool requested = _dialogueRequestedBattle;
            _dialogueRequestedBattle = false; // Reset after checking
            return requested;
        }
        
        public bool DialogueCompleted() 
        {
            bool completed = _dialogueCompleted;
            if (completed)
            {
                _dialogueCompleted = false; // Reset after checking
                _dialogueVisible = false;
                _currentNpcId = null;
            }
            return completed;
        }
        
        // Public methods for dialogue system to signal events
        public void SignalDialogueBattleRequest() => _dialogueRequestedBattle = true;
        public void SignalDialogueCompleted() => _dialogueCompleted = true;
        public string? GetCurrentNpcId() => _currentNpcId;

        // ---- Idle ---- (Stub implementations)
        public void ShowIdleScreen() { }
        public void HideIdleScreen() { }
        public bool RequestedStart() => false;
        public string? RequestedDialogueNpcId() => null;

        // ---- Training ---- (Production implementations)
        private bool _trainingVisible = false;
        private bool _trainingProgressVisible = false;
        private bool _trainingCancelled = false;
        private bool _trainingContinueRequested = false;
        private bool _trainingCompleted = false;
        
        public void ShowTrainingScreen() 
        {
            if (!_trainingVisible)
            {
                _trainingVisible = true;
                _trainingCompleted = false;
                _trainingCancelled = false;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync("//TrainingPage");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error showing training screen: {ex.Message}");
                        _trainingVisible = false;
                    }
                });
            }
        }
        
        public void HideTrainingScreen() 
        {
            if (_trainingVisible)
            {
                _trainingVisible = false;
                _trainingProgressVisible = false;
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (Shell.Current.Navigation.NavigationStack.Count > 1)
                        {
                            await Shell.Current.Navigation.PopAsync();
                        }
                        else
                        {
                            await Shell.Current.GoToAsync("//AdventurePage");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error hiding training screen: {ex.Message}");
                        _trainingVisible = false;
                    }
                });
            }
        }
        
        public void ShowTrainingProgress(int current, int total) 
        {
            _trainingProgressVisible = true;
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                double progress = total > 0 ? (double)current / total : 0;
                string message = $"Training Progress: {current}/{total} ({progress:P0})";
                
                if (Application.Current?.MainPage != null)
                {
                    // You could show a progress bar or update UI here
                    Console.WriteLine(message);
                }
            });
        }
        
        public void HideTrainingProgress() 
        {
            _trainingProgressVisible = false;
            // Hide progress UI elements
            Console.WriteLine("Training progress hidden");
        }
        
        public void TrainingCancelled() 
        {
            _trainingCancelled = true;
            _trainingVisible = false;
            _trainingProgressVisible = false;
        }
        
        public bool TrainingContinueRequested() 
        {
            bool requested = _trainingContinueRequested;
            _trainingContinueRequested = false; // Reset after checking
            return requested;
        }
        
        public void ShowTrainingResult(object result) 
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (result != null && Application.Current?.MainPage != null)
                {
                    string message = result.ToString() ?? "Training completed successfully!";
                    await Application.Current.MainPage.DisplayAlert("Training Complete", message, "OK");
                }
            });
        }
        
        public void TrainingCompleted() 
        {
            _trainingCompleted = true;
            _trainingVisible = false;
            _trainingProgressVisible = false;
        }
        
        // Public methods for training system to signal events
        public void RequestTrainingContinue() => _trainingContinueRequested = true;
        public bool IsTrainingVisible() => _trainingVisible;
        public bool IsTrainingProgressVisible() => _trainingProgressVisible;

        // ---- Fusion ---- (Production implementations)
        private bool _fusionVisible = false;
        private bool _fusionCancelled = false;
        private (int? leftBotId, int? rightBotId) _fusionSelection = (null, null);
        
        public void ShowFusionScreen() 
        {
            if (!_fusionVisible)
            {
                _fusionVisible = true;
                _fusionCancelled = false;
                _fusionSelection = (null, null);
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync("//FusionPage");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error showing fusion screen: {ex.Message}");
                        _fusionVisible = false;
                    }
                });
            }
        }
        
        public void HideFusionScreen() 
        {
            if (_fusionVisible)
            {
                _fusionVisible = false;
                _fusionSelection = (null, null);
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (Shell.Current.Navigation.NavigationStack.Count > 1)
                        {
                            await Shell.Current.Navigation.PopAsync();
                        }
                        else
                        {
                            await Shell.Current.GoToAsync("//AdventurePage");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error hiding fusion screen: {ex.Message}");
                        _fusionVisible = false;
                    }
                });
            }
        }
        
        public void FusionCancelled() 
        {
            _fusionCancelled = true;
            _fusionVisible = false;
            _fusionSelection = (null, null);
        }
        
        public (Guid? leftBotId, Guid? rightBotId) GetFusionSelection() 
        {
            var selection = _fusionSelection;
            _fusionSelection = (null, null); // Reset after reading
            
            // Convert int IDs to GUIDs if possible
            Guid? leftGuid = null, rightGuid = null;
            
            if (selection.leftBotId.HasValue)
            {
                // For demo purposes, create deterministic GUIDs from int IDs
                leftGuid = new Guid($"00000000-0000-0000-0000-{selection.leftBotId.Value:D12}");
            }
            
            if (selection.rightBotId.HasValue)
            {
                rightGuid = new Guid($"00000000-0000-0000-0000-{selection.rightBotId.Value:D12}");
            }
            
            return (leftGuid, rightGuid);
        }
        
        public void ShowFusionResult(object result) 
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (result != null && Application.Current?.MainPage != null)
                {
                    string message = result.ToString() ?? "Fusion completed successfully!";
                    await Application.Current.MainPage.DisplayAlert("Fusion Complete", message, "OK");
                }
            });
        }
        
        // Public methods for fusion system
        public void SetFusionSelection(int? leftBotId, int? rightBotId) => 
            _fusionSelection = (leftBotId, rightBotId);
        public bool IsFusionVisible() => _fusionVisible;

        // ---- Capturing ---- (Production implementations)
        private bool _captureAnimationVisible = false;
        private bool _captureSuccess = false;
        
        public void ShowCaptureAnimation() 
        {
            _captureAnimationVisible = true;
            _captureSuccess = false;
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    // Show capture animation - could be a popup or overlay
                    await Application.Current.MainPage.DisplayAlert("Capture Attempt", 
                        "Attempting to capture the bot...", "Watch");
                }
            });
        }
        
        public void UpdateCaptureAnimationOutcome(bool success) 
        {
            _captureSuccess = success;
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                string message = success ? "The bot is being captured!" : "The bot is resisting capture...";
                Console.WriteLine($"Capture Update: {message}");
            });
        }
        
        public void HideCaptureAnimation() 
        {
            if (_captureAnimationVisible)
            {
                _captureAnimationVisible = false;
                Console.WriteLine("Capture animation hidden");
            }
        }
        
        public void ShowCaptureSuccess(object? info = null) 
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    string message = info?.ToString() ?? "Bot captured successfully!";
                    await Application.Current.MainPage.DisplayAlert("Capture Success!", message, "Great!");
                }
            });
        }
        
        // Public methods for capture system
        public bool IsCaptureAnimationVisible() => _captureAnimationVisible;
        public void HideCaptureSuccess() { }
        public void ShowCaptureFailed(object? info = null) { }
        public void HideCaptureFailed() { }
    }
}