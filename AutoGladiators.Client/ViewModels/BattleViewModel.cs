using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AutoGladiators.Client.Logic;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.StateMachine.States;
using AutoGladiators.Client.Core;
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
            if (_battleManager.IsBattleOver)
                return;

            var result = move.Use(PlayerBot, EnemyBot);
            Log(result);
            CheckBattleEnd();

            if (!_battleManager.IsBattleOver)
            {
                await Task.Delay(1000);
                EnemyTurn();
                CheckBattleEnd();
            }

            OnPropertyChanged(nameof(PlayerBot));
            OnPropertyChanged(nameof(EnemyBot));
            OnPropertyChanged(nameof(PlayerMoves));
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

        private async void CheckBattleEnd()
        {
            if (PlayerBot.IsFainted)
            {
                BattleStateText = "You lost...";
                Log("Your bot has fainted, but you can continue.");
                await OnBattleEnded();
            }
            else if (EnemyBot.IsFainted)
            {
                BattleStateText = "You won!";
                Log($"You defeated {EnemyBot.Name}!");
                await OnBattleEnded();
            }
        }

        private async Task OnBattleEnded()
        {
            // Delay to show win/loss message
            await Task.Delay(2000);

            // === TODO SECTION ===
            // Award XP, drop loot, update quests, persist, etc.

            // Return to previous page
            await Shell.Current.GoToAsync("..");
        }

        private void Log(string message)
        {
            BattleLog.Add(message);
            OnPropertyChanged(nameof(BattleLog));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ---- Helpers ----

        private static IEnumerable<Move> GetPlayerMoves(GladiatorBot bot)
        {
            if (bot == null) return Enumerable.Empty<Move>();

            // Try common shapes in your models with minimal assumptions
            // 1) LearnableMoves already a sequence of Move
            var learnableMovesProp = bot.GetType().GetProperty("LearnableMoves");
            var learnable = learnableMovesProp?.GetValue(bot);
            if (learnable is IEnumerable<Move> mv) return mv;

            // 2) LearnableMoves is a sequence of names
            if (learnable is IEnumerable<string> names1) return names1.Select(n => new Move { Name = n });

            // 3) Moves (equipped/known)
            var movesProp = bot.GetType().GetProperty("Moves");
            var movesVal = movesProp?.GetValue(bot);
            if (movesVal is IEnumerable<Move> mv2) return mv2;
            if (movesVal is IEnumerable<string> names2) return names2.Select(n => new Move { Name = n });

            // 4) MoveNames
            var moveNamesProp = bot.GetType().GetProperty("MoveNames");
            var moveNamesVal = moveNamesProp?.GetValue(bot) as IEnumerable<string>;
            if (moveNamesVal != null) return moveNamesVal.Select(n => new Move { Name = n });

            // Nothing found
            return Enumerable.Empty<Move>();
        }
    }
}
