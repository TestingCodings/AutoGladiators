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
        private ObservableCollection<string> battleLog;

        public ICommand SimulateBattleCommand { get; }

        public ICommand PowerStrikeCommand { get; }
        public ICommand RepairCommand { get; }
        public ICommand CaptureCommand { get; }

        public ICommand EvadeCommand => new RelayCommand(ExecuteEvade);


        public BattleViewModel()
        {
            SimulateBattleCommand = new RelayCommand(ExecuteSimulateBattle);
            battleLog = new ObservableCollection<string>();
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

        private void ExecutePowerStrike()
        {
            if (Bot1 == null || Bot2 == null)
            {
                BattleLog.Clear();
                BattleLog.Add("Please select both bots to perform a Power Strike.");
                return;
            }

            var result = BattleSimulator.PowerStrike(Bot1, Bot2);
            BattleLog.Clear();

            foreach (var line in result.Log)
            {
                BattleLog.Add(line);
            }

            BattleOutcome = $"Winner: {result.Winner} - Outcome: {result.Outcome}";
        }
        private void ExecuteEvade()
        {
            if (Bot1 == null || Bot2 == null)
            {
                BattleLog.Clear();
                BattleLog.Add("Please select both bots to perform an Evade.");
                return;
            }

            var result = BattleSimulator.Evade(Bot1, Bot2);
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

// Commands:
// - SimulateBattleCommand: Simulates a battle between the two selected bots and logs the outcome.
// - PowerStrikeCommand: Executes a Power Strike action between the two bots and logs the result.
// - EvadeCommand: Executes an Evade action between the two bots and logs the result.
