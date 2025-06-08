using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoGladiators.Client.ViewModels
{
    public partial class BattleViewModel : ObservableObject
    {
        [ObservableProperty]
        private GladiatorBot bot1;

        [ObservableProperty]
        private GladiatorBot bot2;

        [ObservableProperty]
        private string battleOutcome;

        [ObservableProperty]
        private ObservableCollection<string> battleLog = new();

        public ICommand SimulateBattleCommand { get; }

        public BattleViewModel()
        {
            SimulateBattleCommand = new RelayCommand(ExecuteSimulateBattle);
        }

        private void ExecuteSimulateBattle()
        {
            if (Bot1 == null || Bot2 == null)
            {
                BattleLog.Clear();
                BattleLog.Add("Please select both bots to simulate a battle.");
                return;
            }

            var result = BattleSimulator.SimulateBattle(Bot1, Bot2);
            BattleLog.Clear();

            foreach (var line in result.Log)
            {
                BattleLog.Add(line);
            }

            BattleOutcome = $"Winner: {result.Winner} - Outcome: {result.Outcome}";
        }

        public void SetBots(GladiatorBot selectedBot1, GladiatorBot selectedBot2)
        {
            Bot1 = selectedBot1;
            Bot2 = selectedBot2;
        }
    }
}
// This ViewModel handles the battle simulation logic, including bot selection and battle outcome display.
// It uses the BattleSimulator to run the simulation and updates the battle log and outcome accordingly.
