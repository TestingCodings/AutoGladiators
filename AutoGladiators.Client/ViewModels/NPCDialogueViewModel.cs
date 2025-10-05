using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueNode = AutoGladiators.Core.Models.DialogueNode;
using DialogueOption = AutoGladiators.Core.Models.DialogueOption;


namespace AutoGladiators.Client.ViewModels
{
    public partial class NPCDialogueViewModel : ObservableObject
    {
        [ObservableProperty]
        private string nPCName;

        [ObservableProperty]
        private string currentText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<DialogueOption> currentOptions;

        private Dictionary<string, DialogueNode> _nodeMap;
        private readonly NPCDialogueLoader _dialogueLoader;

        public ICommand SelectOptionCommand { get; }

        public async Task InitializeFromFile(string filename)
        {
            // Ensure 'filename' is the relative path to JSON file, e.g. "Assets/Dialogues/lila_trader.json"
            // The file should be set to "Copy to Output Directory" in project properties.
            var dialogue = await _dialogueLoader.LoadDialogueAsync(filename);
            NPCName = dialogue.NPCName;
            _nodeMap = dialogue.DialogueNodes.ToDictionary(n => n.Id);
            LoadNode("start");
        }

        public NPCDialogueViewModel(string npcName, NPCDialogueLoader dialogueLoader)
        {
            NPCName = npcName;
            _dialogueLoader = dialogueLoader ?? throw new ArgumentNullException(nameof(dialogueLoader));
            _nodeMap = new Dictionary<string, DialogueNode>();
            CurrentOptions = new ObservableCollection<DialogueOption>();
            SelectOptionCommand = new RelayCommand<DialogueOption>(OnOptionSelected);
        }


        private void LoadNode(string nodeId)
        {
            if (_nodeMap.TryGetValue(nodeId, out var node))
            {
                CurrentText = node.Text;
                CurrentOptions = new ObservableCollection<DialogueOption>(node.Options);
            }
        }

        private void OnOptionSelected(DialogueOption? option)
        {
            if (option == null) return;
            
            var actionKey = option.GetActionKey(); // extension method
            if (!string.IsNullOrEmpty(actionKey))
            {
                HandleAction(actionKey);
            }

            // Next node id: handle both NextNodeId and Next gracefully
            var nextProp = option.GetType().GetProperty("NextNodeId") ?? option.GetType().GetProperty("Next");
            var nextNodeId = nextProp?.GetValue(option) as string;
            LoadNode(nextNodeId ?? "start");
        }


    private void HandleAction(string key)
    {
        var gameState = GameStateService.Instance;
        
        switch (key.ToLowerInvariant())
        {
            case "healbot":
            case "heal":
                // Heal the current bot
                var currentBot = gameState.GetCurrentBot();
                if (currentBot != null)
                {
                    currentBot.CurrentHealth = currentBot.MaxHealth;
                    // You might want to show a message or update UI here
                }
                break;
                
            case "givecontrolchip":
            case "givechip":
                // Add a control chip to inventory
                var controlChip = new AutoGladiators.Core.Models.Item
                {
                    Name = "Control Chip",
                    Description = "A device used to capture and control bots",
                    Type = "Capture",
                    Effect = "Increases capture rate by 25%",
                    Quantity = 1
                };
                gameState.AddItem(controlChip);
                break;
                
            case "flagintroquest":
            case "startquest":
                // Set a quest flag
                gameState.SetFlag("IntroQuestStarted", true);
                break;
                
            case "givegold":
                // Give player some gold
                if (gameState.CurrentPlayer != null)
                {
                    gameState.CurrentPlayer.AddItem("Gold", 100);
                }
                break;
                
            case "shophealing":
                // Add healing items to inventory
                var healingPotion = new AutoGladiators.Core.Models.Item
                {
                    Name = "Healing Potion",
                    Description = "Restores 50 HP to a bot",
                    Type = "Healing",
                    Effect = "Restores 50 HP",
                    Quantity = 1
                };
                gameState.AddItem(healingPotion);
                break;
                
            case "startbattle":
                // This could trigger a battle - might need to be handled differently
                // as it requires navigation which the ViewModel shouldn't handle directly
                gameState.SetFlag("StartBattleRequested", "true");
                break;
                
            default:
                // Log unknown action for debugging
                System.Diagnostics.Debug.WriteLine($"Unknown dialogue action: {key}");
                break;
        }
    }

    }
}