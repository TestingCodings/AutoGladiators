using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Client.ViewModels
{
    public class EnhancedBattleViewModel : INotifyPropertyChanged
    {
        private readonly EnhancedInventoryService _inventoryService = new();
        private GladiatorBot _playerBot;
        private GladiatorBot _enemyBot;
        private bool _isBattleActive = false;
        private bool _isPlayerTurn = true;
        private string _battleLog = "";
        private string _currentBattleMessage = "";
        private bool _showVictoryOverlay = false;
        private bool _showDefeatOverlay = false;
        private string _lastUsedComboMove = "";
        private int _turnCounter = 0;

        public GladiatorBot PlayerBot
        {
            get => _playerBot;
            set
            {
                _playerBot = value;
                OnPropertyChanged(nameof(PlayerBot));
                OnPropertyChanged(nameof(PlayerHealthPercentage));
                OnPropertyChanged(nameof(PlayerMPPercentage));
                OnPropertyChanged(nameof(PlayerEnergyPercentage));
                LoadPlayerMoves();
            }
        }

        public GladiatorBot EnemyBot
        {
            get => _enemyBot;
            set
            {
                _enemyBot = value;
                OnPropertyChanged(nameof(EnemyBot));
                OnPropertyChanged(nameof(EnemyHealthPercentage));
                OnPropertyChanged(nameof(EnemyMPPercentage));
            }
        }

        public bool IsBattleActive
        {
            get => _isBattleActive;
            set
            {
                _isBattleActive = value;
                OnPropertyChanged(nameof(IsBattleActive));
            }
        }

        public bool IsPlayerTurn
        {
            get => _isPlayerTurn;
            set
            {
                _isPlayerTurn = value;
                OnPropertyChanged(nameof(IsPlayerTurn));
                OnPropertyChanged(nameof(TurnIndicator));
            }
        }

        public string BattleLog
        {
            get => _battleLog;
            set
            {
                _battleLog = value;
                OnPropertyChanged(nameof(BattleLog));
            }
        }

        public string CurrentBattleMessage
        {
            get => _currentBattleMessage;
            set
            {
                _currentBattleMessage = value;
                OnPropertyChanged(nameof(CurrentBattleMessage));
            }
        }

        public bool ShowVictoryOverlay
        {
            get => _showVictoryOverlay;
            set
            {
                _showVictoryOverlay = value;
                OnPropertyChanged(nameof(ShowVictoryOverlay));
            }
        }

        public bool ShowDefeatOverlay
        {
            get => _showDefeatOverlay;
            set
            {
                _showDefeatOverlay = value;
                OnPropertyChanged(nameof(ShowDefeatOverlay));
            }
        }

        public string TurnIndicator => IsPlayerTurn ? "Your Turn" : "Enemy Turn";

        public double PlayerHealthPercentage => PlayerBot != null ? 
            (double)PlayerBot.CurrentHealth / PlayerBot.MaxHealth * 100 : 0;

        public double PlayerMPPercentage => PlayerBot != null ? 
            (double)PlayerBot.CurrentMP / PlayerBot.MaxMP * 100 : 0;

        public double PlayerEnergyPercentage => PlayerBot != null ? 
            (double)PlayerBot.Energy / PlayerBot.MaxEnergy * 100 : 0;

        public double EnemyHealthPercentage => EnemyBot != null ? 
            (double)EnemyBot.CurrentHealth / EnemyBot.MaxHealth * 100 : 0;

        public double EnemyMPPercentage => EnemyBot != null ? 
            (double)EnemyBot.CurrentMP / EnemyBot.MaxMP * 100 : 0;

        public ObservableCollection<EnhancedMove> PlayerMoves { get; } = new();
        public ObservableCollection<BattleItem> AvailableItems { get; } = new();

        public ICommand MoveCommand { get; }
        public ICommand UseItemCommand { get; }
        public ICommand RunAwayCommand { get; }
        public ICommand ContinueBattleCommand { get; }

        public EnhancedBattleViewModel()
        {
            MoveCommand = new Command<EnhancedMove>(ExecuteMove);
            UseItemCommand = new Command<BattleItem>(UseItem);
            RunAwayCommand = new Command(RunAway);
            ContinueBattleCommand = new Command(ContinueBattle);
            
            LoadAvailableItems();
        }

        public void StartBattle(GladiatorBot playerBot, GladiatorBot enemyBot)
        {
            PlayerBot = playerBot;
            EnemyBot = enemyBot;
            
            // Reset battle state
            ResetBattleState();
            
            IsBattleActive = true;
            IsPlayerTurn = true;
            _turnCounter = 0;
            
            AddToBattleLog($"Battle begins! {PlayerBot.Name} vs {EnemyBot.Name}!");
            CurrentBattleMessage = "Choose your move!";
        }

        private void ResetBattleState()
        {
            BattleLog = "";
            ShowVictoryOverlay = false;
            ShowDefeatOverlay = false;
            _lastUsedComboMove = "";
            
            // Reset moves
            foreach (var move in PlayerMoves)
            {
                move.ResetBattleUses();
            }
            
            // Reset inventory cooldowns
            _inventoryService.UpdateBattleCooldowns();
        }

        private void LoadPlayerMoves()
        {
            PlayerMoves.Clear();
            
            var enhancedMoves = EnhancedMove.CreateEnhancedMoveSet()
                .Where(m => PlayerBot.Level >= m.MinLevel)
                .ToList();

            // Unlock moves based on progression
            foreach (var move in enhancedMoves)
            {
                if (move.RequiresComboSetup && move.ComboRequirement != null)
                {
                    // Ultimate moves start locked and get unlocked through gameplay
                    move.IsUnlocked = HasLearnedMove(move.ComboRequirement);
                }
                
                PlayerMoves.Add(move);
            }
        }

        private bool HasLearnedMove(string moveName)
        {
            // Simple check - in a full implementation, this would check the bot's learned moves
            return PlayerBot.Level >= 10; // Unlock ultimate moves at level 10+
        }

        private void LoadAvailableItems()
        {
            AvailableItems.Clear();
            var items = _inventoryService.GetAvailableBattleItems();
            
            foreach (var item in items)
            {
                AvailableItems.Add(item);
            }
        }

        private async void ExecuteMove(EnhancedMove move)
        {
            if (!IsPlayerTurn || !IsBattleActive || move == null)
                return;

            // Check if move can be used
            if (!move.CanUse(PlayerBot))
            {
                string reason = GetMoveRestrictionReason(move);
                CurrentBattleMessage = reason;
                await Task.Delay(1500);
                return;
            }

            // Check combo requirements
            if (move.RequiresComboSetup && move.ComboRequirement != _lastUsedComboMove)
            {
                CurrentBattleMessage = $"Must use {move.ComboRequirement} first to unlock {move.Name}!";
                await Task.Delay(2000);
                return;
            }

            // Execute player move
            string playerResult = move.Use(PlayerBot, EnemyBot);
            AddToBattleLog(playerResult);
            CurrentBattleMessage = playerResult;
            
            // Track last used move for combo system
            _lastUsedComboMove = move.Name;
            
            // Update UI
            RefreshStats();
            
            await Task.Delay(2000);
            
            // Check if enemy is defeated
            if (EnemyBot.CurrentHealth <= 0)
            {
                await HandleVictory();
                return;
            }
            
            // Enemy turn
            await ExecuteEnemyTurn();
        }

        private string GetMoveRestrictionReason(EnhancedMove move)
        {
            if (PlayerBot.CurrentMP < move.MpCost)
                return $"Not enough MP! Need {move.MpCost} MP (have {PlayerBot.CurrentMP})";
            
            if (!move.IsUnlocked)
                return $"{move.Name} is locked! Meet requirements to unlock.";
            
            if (PlayerBot.Level < move.MinLevel)
                return $"Need level {move.MinLevel} to use {move.Name}!";
            
            if (move.UsesPerBattle > 0 && move.RemainingUses <= 0)
                return $"{move.Name} has no uses left this battle!";
            
            if (PlayerBot.Energy < move.EnergyCost)
                return $"Not enough energy! Need {move.EnergyCost} energy.";
                
            return "Cannot use this move right now.";
        }

        private async Task ExecuteEnemyTurn()
        {
            IsPlayerTurn = false;
            CurrentBattleMessage = "Enemy is thinking...";
            
            await Task.Delay(1000);
            
            // Simple AI - choose a random available move
            var enemyMoves = EnhancedMove.CreateEnhancedMoveSet()
                .Where(m => m.CanUse(EnemyBot))
                .ToList();
                
            if (enemyMoves.Any())
            {
                var random = new Random();
                var selectedMove = enemyMoves[random.Next(enemyMoves.Count)];
                
                string enemyResult = selectedMove.Use(EnemyBot, PlayerBot);
                AddToBattleLog(enemyResult);
                CurrentBattleMessage = enemyResult;
                
                RefreshStats();
                
                await Task.Delay(2000);
                
                // Check if player is defeated
                if (PlayerBot.CurrentHealth <= 0)
                {
                    await HandleDefeat();
                    return;
                }
            }
            
            // End turn - regenerate MP and energy
            await EndTurn();
        }

        private async Task EndTurn()
        {
            _turnCounter++;
            
            // Natural MP regeneration
            PlayerBot.RegenerateMP(3);
            EnemyBot.RegenerateMP(3);
            
            // Update inventory cooldowns
            _inventoryService.UpdateBattleCooldowns();
            
            // Status effect processing
            PlayerBot.TickStatusEffects();
            EnemyBot.TickStatusEffects();
            
            RefreshStats();
            
            IsPlayerTurn = true;
            CurrentBattleMessage = "Choose your next move!";
        }

        private async void UseItem(BattleItem item)
        {
            if (!IsPlayerTurn || !IsBattleActive || item == null)
                return;

            var result = _inventoryService.UseBattleItem(item.BattleType, PlayerBot);
            
            if (result.Success)
            {
                AddToBattleLog($"Used {item.Name}! {result.Message}");
                CurrentBattleMessage = $"Used {item.Name}! {result.Message}";
                
                RefreshStats();
                
                await Task.Delay(1500);
                
                // Using an item ends the turn
                await ExecuteEnemyTurn();
            }
            else
            {
                CurrentBattleMessage = result.Message;
                await Task.Delay(1500);
            }
        }

        private void RunAway()
        {
            AddToBattleLog($"{PlayerBot.Name} ran away from battle!");
            CurrentBattleMessage = "You ran away safely...";
            EndBattle();
        }

        private async Task HandleVictory()
        {
            IsBattleActive = false;
            
            // Award experience
            int xpGained = EnemyBot.Level * 25 + 50;
            PlayerBot.GainExperience(xpGained);
            
            AddToBattleLog($"Victory! {PlayerBot.Name} gained {xpGained} XP!");
            
            if (PlayerBot.Level > 1) // Level up check
            {
                AddToBattleLog($"{PlayerBot.Name} reached level {PlayerBot.Level}!");
            }
            
            ShowVictoryOverlay = true;
        }

        private async Task HandleDefeat()
        {
            IsBattleActive = false;
            AddToBattleLog($"{PlayerBot.Name} was defeated...");
            ShowDefeatOverlay = true;
        }

        private void ContinueBattle()
        {
            ShowVictoryOverlay = false;
            ShowDefeatOverlay = false;
            // Navigate back to previous page or show battle results
        }

        private void EndBattle()
        {
            IsBattleActive = false;
            ShowVictoryOverlay = false;
            ShowDefeatOverlay = false;
        }

        private void AddToBattleLog(string message)
        {
            BattleLog += $"{message}\n";
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(PlayerHealthPercentage));
            OnPropertyChanged(nameof(PlayerMPPercentage));
            OnPropertyChanged(nameof(PlayerEnergyPercentage));
            OnPropertyChanged(nameof(EnemyHealthPercentage));
            OnPropertyChanged(nameof(EnemyMPPercentage));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}