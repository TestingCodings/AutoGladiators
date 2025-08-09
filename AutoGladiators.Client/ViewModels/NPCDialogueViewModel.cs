using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoGladiators.Client.StateMachine.States;
using DialogueNode = AutoGladiators.Client.Models.DialogueNode;
using DialogueOption = AutoGladiators.Client.Models.DialogueOption;


namespace AutoGladiators.Client.ViewModels
{
    public partial class NPCDialogueViewModel : ObservableObject
    {
        [ObservableProperty]
        private string nPCName;

        [ObservableProperty]
        private string currentText;

        [ObservableProperty]
        private ObservableCollection<DialogueOption> currentOptions;

        private Dictionary<string, DialogueNode> _nodeMap;

        public ICommand SelectOptionCommand { get; }

        public async Task InitializeFromFile(string filename)
        {
            // Ensure 'filename' is the relative path to JSON file, e.g. "Assets/Dialogues/lila_trader.json"
            // The file should be set to "Copy to Output Directory" in project properties.
            var dialogue = await NPCDialogueLoader.LoadDialogueAsync(filename);
            NPCName = dialogue.NPCName;
            _nodeMap = dialogue.DialogueNodes.ToDictionary(n => n.Id);
            LoadNode("start");
        }

        public NPCDialogueViewModel(string npcName)
        {
            NPCName = npcName;
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

        private void OnOptionSelected(DialogueOption option)
        {
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
        switch (key)
        {
            case "HealBot":
                // TODO: Replace with actual bot healing logic
                // Example: _currentBot.CurrentHealth = _currentBot.MaxHealth;
                break;
            case "GiveControlChip":
                // TODO: Replace with actual inventory logic
                // Example: _inventoryService.AddItem("ControlChip", 1);
                break;
            case "FlagIntroQuest":
                // TODO: Replace with actual quest flag logic
                // Example: _questFlags["IntroQuestStarted"] = true;
                break;
            // Add more here
        }
    }

    }
}
