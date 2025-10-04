using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.StateMachine.States;
using AutoGladiators.Core.Core;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Client.ViewModels
{
    public class BattleViewModel : INotifyPropertyChanged
    {
        private GladiatorBot _playerBot = null!;
        private GladiatorBot _enemyBot = null!;
        private BattleManager _battleManager;
        private bool _isPlayerTurn = true;
        private bool _battleInProgress = true;

        public ObservableCollection<string> BattleLog { get; set; } = new();
        public event EventHandler<string>? DamageDealt;
        public event EventHandler? BattleEnded;

        // Always present the UI with Move objects; adapt from names if needed
        public ObservableCollection<Move> PlayerMoves => new(GetPlayerMoves(_playerBot));

        public GladiatorBot PlayerBot
        {
            get => _playerBot;
            set
            {
                _playerBot = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayerMoves));
            }
        }

        public GladiatorBot EnemyBot
        {
            get => _enemyBot;
            set
            {
                _enemyBot = value;
                OnPropertyChanged();
            }
        }

        // Battle state and UI properties
        private string _battleStateText = "Choose your move!";
        public string BattleStateText
        {
            get => _battleStateText;
            set { _battleStateText = value; OnPropertyChanged(); }
        }

        private bool _showBattleStatus = true;
        public bool ShowBattleStatus
        {
            get => _showBattleStatus;
            set { _showBattleStatus = value; OnPropertyChanged(); }
        }

        // Battle end screen properties
        private bool _showBattleEndScreen = false;
        public bool ShowBattleEndScreen
        {
            get => _showBattleEndScreen;
            set { _showBattleEndScreen = value; OnPropertyChanged(); }
        }

        private string _battleEndTitle = string.Empty;
        public string BattleEndTitle
        {
            get => _battleEndTitle;
            set { _battleEndTitle = value; OnPropertyChanged(); }
        }

        private string _battleEndMessage = string.Empty;
        public string BattleEndMessage
        {
            get => _battleEndMessage;
            set { _battleEndMessage = value; OnPropertyChanged(); }
        }

        private Color _battleEndColor = Colors.White;
        public Color BattleEndColor
        {
            get => _battleEndColor;
            set { _battleEndColor = value; OnPropertyChanged(); }
        }

        public bool IsPlayerTurn
        {
            get => _isPlayerTurn;
            set { _isPlayerTurn = value; OnPropertyChanged(); }
        }

        // Properties for XAML binding
        public double PlayerHpFraction => PlayerBot?.MaxHealth > 0 ? (double)PlayerBot.CurrentHealth / PlayerBot.MaxHealth : 0.0;
        public double EnemyHpFraction => EnemyBot?.MaxHealth > 0 ? (double)EnemyBot.CurrentHealth / EnemyBot.MaxHealth : 0.0;
        public string BattleLogText => string.Join("\n", BattleLog);

        public ICommand UseMoveCommand { get; }
        public ICommand ContinueCommand { get; }

        public BattleViewModel(GladiatorBot player, GladiatorBot enemy)
        {
            PlayerBot = player;
            EnemyBot = enemy;

            _battleManager = new BattleManager(PlayerBot, EnemyBot);

            UseMoveCommand = new Command<Move>(async (move) => await ExecutePlayerMove(move), 
                                             (move) => _battleInProgress && _isPlayerTurn);
            ContinueCommand = new Command(async () => await OnContinuePressed());

            Log($"🎯 Battle Arena: {player.Name} vs {enemy.Name}!");
            Log($"⚔️ {player.Name} enters the battle ready to fight!");
            BattleStateText = $"Your turn, {player.Name}!";
        }

        private async Task ExecutePlayerMove(Move move)
        {
            if (!_battleInProgress || !_isPlayerTurn)
                return;

            _isPlayerTurn = false;
            BattleStateText = $"{PlayerBot.Name} uses {move.Name}!";
            
            // Execute player move with enhanced feedback
            var oldEnemyHp = EnemyBot.CurrentHealth;
            var result = move.Use(PlayerBot, EnemyBot);
            var damage = oldEnemyHp - EnemyBot.CurrentHealth;
            
            Log($"⚔️ {PlayerBot.Name} used {move.Name}!");
            if (damage > 0)
            {
                Log($"💥 Dealt {damage} damage to {EnemyBot.Name}!");
                DamageDealt?.Invoke(this, damage.ToString());
            }
            else
            {
                Log(result);
            }
            
            UpdateAllProperties();
            
            // Check if enemy is defeated
            if (CheckBattleEnd())
                return;

            // Enemy turn
            await Task.Delay(1500); // Animation time
            await ExecuteEnemyTurn();
            
            // Check if player is defeated
            if (CheckBattleEnd())
                return;
                
            // Return to player turn
            _isPlayerTurn = true;
            BattleStateText = "Choose your next move!";
            UpdateAllProperties();
        }

        private async Task ExecuteEnemyTurn()
        {
            BattleStateText = $"{EnemyBot.Name} is choosing an attack...";
            await Task.Delay(1000); // Thinking time
            
            var enemyMove = _battleManager.ChooseMove(EnemyBot);
            if (enemyMove != null)
            {
                BattleStateText = $"{EnemyBot.Name} uses {enemyMove.Name}!";
                
                var oldPlayerHp = PlayerBot.CurrentHealth;
                var result = enemyMove.Use(EnemyBot, PlayerBot);
                var damage = oldPlayerHp - PlayerBot.CurrentHealth;
                
                Log($"🔴 {EnemyBot.Name} used {enemyMove.Name}!");
                if (damage > 0)
                {
                    Log($"💥 You took {damage} damage!");
                    DamageDealt?.Invoke(this, $"-{damage}");
                }
                else
                {
                    Log(result);
                }
            }
            
            UpdateAllProperties();
            await Task.Delay(1000); // Effect display time
        }

        private bool CheckBattleEnd()
        {
            if (PlayerBot.IsFainted)
            {
                _battleInProgress = false;
                ShowBattleStatus = false;
                
                BattleEndTitle = "😵 DEFEAT";
                BattleEndMessage = $"{PlayerBot.Name} was defeated by {EnemyBot.Name}...\nBetter luck next time!";
                BattleEndColor = Colors.Red;
                ShowBattleEndScreen = true;
                
                Log($"😵 {PlayerBot.Name} was defeated!");
                Log("Battle lost. Return to prepare for the next fight!");
                
                OnBattleEnded(false);
                return true;
            }
            else if (EnemyBot.IsFainted)
            {
                _battleInProgress = false;
                ShowBattleStatus = false;
                
                BattleEndTitle = "🏆 VICTORY!";
                BattleEndMessage = $"{PlayerBot.Name} defeated {EnemyBot.Name}!\nYou earned experience and rewards!";
                BattleEndColor = Colors.Gold;
                ShowBattleEndScreen = true;
                
                Log($"🏆 Victory! {PlayerBot.Name} defeated {EnemyBot.Name}!");
                Log("✨ Experience gained! Check your bot's progress.");
                
                OnBattleEnded(true);
                return true;
            }
            
            return false;
        }

        private void OnBattleEnded(bool victory)
        {
            BattleEnded?.Invoke(this, EventArgs.Empty);
            
            // Update game state based on battle outcome
            var gameState = GameStateService.Instance;
            if (victory)
            {
                // Award experience and handle victory rewards
                if (PlayerBot != null)
                {
                    // Simple experience gain - can be enhanced later
                    var expGained = EnemyBot?.Level ?? 1 * 100;
                    Log($"🎆 Gained {expGained} experience points!");
                }
            }
        }
        
        private async Task OnContinuePressed()
        {
            // Navigate back to previous screen or main menu
            if (Application.Current?.MainPage?.Navigation != null)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }
        
        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(PlayerBot));
            OnPropertyChanged(nameof(EnemyBot));
            OnPropertyChanged(nameof(PlayerMoves));
            OnPropertyChanged(nameof(PlayerHpFraction));
            OnPropertyChanged(nameof(EnemyHpFraction));
            OnPropertyChanged(nameof(BattleLogText));
            OnPropertyChanged(nameof(IsPlayerTurn));
        }

        private void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("mm:ss");
            BattleLog.Add($"[{timestamp}] {message}");
            
            // Keep log manageable (last 20 messages)
            while (BattleLog.Count > 20)
            {
                BattleLog.RemoveAt(0);
            }
            
            OnPropertyChanged(nameof(BattleLog));
            OnPropertyChanged(nameof(BattleLogText));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- Helpers ----

        private static IEnumerable<Move> GetPlayerMoves(GladiatorBot bot)
        {
            if (bot == null) return Enumerable.Empty<Move>();

            var moves = new List<Move>();
            
            // Get moves from Moveset property
            if (bot.Moveset != null && bot.Moveset.Any())
            {
                foreach (var moveName in bot.Moveset)
                {
                    var move = AutoGladiators.Core.Logic.MoveDatabase.GetMoveByName(moveName);
                    if (move != null)
                    {
                        moves.Add(move);
                    }
                }
            }
            
            // If no moves found, provide a basic attack
            if (!moves.Any())
            {
                var basicAttack = AutoGladiators.Core.Logic.MoveDatabase.GetMoveByName("Tackle");
                if (basicAttack != null)
                {
                    moves.Add(basicAttack);
                }
            }
            
            return moves;
        }
    }
}
