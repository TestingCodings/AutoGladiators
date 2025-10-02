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

namespace AutoGladiators.Client.ViewModels
{
    public class BattleViewModel : INotifyPropertyChanged
    {
        private GladiatorBot _playerBot = null!;
        private GladiatorBot _enemyBot = null!;
        private BattleManager _battleManager;

        public ObservableCollection<string> BattleLog { get; set; } = new();

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

        // Use a plain string for status text instead of mixing types with BattlingState class
        private string _battleStateText = string.Empty;
        public string BattleStateText
        {
            get => _battleStateText;
            set { _battleStateText = value; OnPropertyChanged(); }
        }

        // Properties for XAML binding
        public double PlayerHpFraction => PlayerBot?.MaxHealth > 0 ? (double)PlayerBot.CurrentHealth / PlayerBot.MaxHealth : 0.0;
        public double EnemyHpFraction => EnemyBot?.MaxHealth > 0 ? (double)EnemyBot.CurrentHealth / EnemyBot.MaxHealth : 0.0;
        public string BattleLogText => string.Join("\n", BattleLog);

        public ICommand UseMoveCommand { get; }

        public BattleViewModel(GladiatorBot player, GladiatorBot enemy)
        {
            PlayerBot = player;
            EnemyBot = enemy;

            _battleManager = new BattleManager(PlayerBot, EnemyBot);

            UseMoveCommand = new Command<Move>(async (move) => await ExecutePlayerMove(move));

            Log($"Battle started: {player.Name} vs {enemy.Name}");
        }

        private async Task ExecutePlayerMove(Move move)
        {
            if (_battleManager.IsBattleOver())
                return;

            var result = move.Use(PlayerBot, EnemyBot);
            Log(result);
            CheckBattleEnd();

            if (!_battleManager.IsBattleOver())
            {
                await Task.Delay(1000);
                EnemyTurn();
                CheckBattleEnd();
            }

            OnPropertyChanged(nameof(PlayerBot));
            OnPropertyChanged(nameof(EnemyBot));
            OnPropertyChanged(nameof(PlayerMoves));
            OnPropertyChanged(nameof(PlayerHpFraction));
            OnPropertyChanged(nameof(EnemyHpFraction));
            OnPropertyChanged(nameof(BattleLogText));
        }

        private void EnemyTurn()
        {
            var enemyMove = _battleManager.ChooseMove(EnemyBot);
            if (enemyMove != null)
            {
                var result = enemyMove.Use(EnemyBot, PlayerBot);
                Log(result);
            }
        }

        private void CheckBattleEnd()
        {
            if (PlayerBot.IsFainted)
            {
                BattleStateText = "Defeat...";
                Log("Your bot has been defeated!");
                // Let the state machine handle defeat transitions
                OnBattleEnded(false);
            }
            else if (EnemyBot.IsFainted)
            {
                BattleStateText = "Victory!";
                Log($"You defeated {EnemyBot.Name}!");
                // Let the state machine handle victory transitions
                OnBattleEnded(true);
            }
        }

        private void OnBattleEnded(bool victory)
        {
            // The state machine (BattlingState) will now handle the victory/defeat logic
            // This method just updates UI state to show the battle has ended
            
            // Set a flag or trigger state machine transition if needed
            // For now, we'll rely on the battle manager's state to determine if the battle is over
            
            if (victory)
            {
                Log("Battle won! Transitioning to victory state...");
            }
            else
            {
                Log("Battle lost! Transitioning to defeat state...");
            }
        }

        private void Log(string message)
        {
            BattleLog.Add(message);
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
