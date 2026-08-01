using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AutoGladiators.Client.Game;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models.Exploration;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Exploration;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Rng;
using WorldPosition = AutoGladiators.Core.Models.WorldPosition;
using EncounterService = AutoGladiators.Core.Services.Exploration.EncounterService;

namespace AutoGladiators.Client.Pages
{
    /// <summary>
    /// Open-world exploration built on the SkiaSharp tile renderer
    /// (<see cref="TileMapView"/>). Reads the live <see cref="WorldZone"/> from
    /// <see cref="WorldManager"/>, moves through the renderer, and closes the
    /// gameplay loop: steps trigger wild-encounter checks, encounters push the
    /// battle page, building doors heal/shop, and progress is saved.
    /// </summary>
    public class ExplorationPage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<ExplorationPage>();

        private readonly PlayerProfileService _playerProfileService;
        private readonly WorldManager _worldManager;
        private readonly MovementManager _movementManager;
        private readonly EncounterService _encounterService;
        private readonly IRng _rng;

        private PlayerProfile? _player;
        private WorldZone? _zone;
        private bool _initialised;
        private bool _inBattle;
        private bool _disposed;

        private readonly TileMapView _map;
        private readonly Label _locationLabel;
        private readonly Label _playerInfoLabel;

        public ExplorationPage(PlayerProfileService playerProfileService,
                               WorldManager worldManager,
                               MovementManager movementManager,
                               EncounterService encounterService,
                               IRng rng)
        {
            _playerProfileService = playerProfileService;
            _worldManager = worldManager;
            _movementManager = movementManager;
            _encounterService = encounterService;
            _rng = rng;

            Title = "Exploration";
            BackgroundColor = Color.FromArgb("#0C0E14");

            _map = new TileMapView { TileSize = 44 };
            _locationLabel = new Label { Text = "", TextColor = Colors.White, FontSize = 15, FontAttributes = FontAttributes.Bold };
            _playerInfoLabel = new Label { Text = "", TextColor = Color.FromArgb("#AEBAD0"), FontSize = 12 };

            Content = BuildLayout();

            _map.PlayerStepped += OnPlayerStepped;
            _map.BuildingEntered += OnBuildingEntered;
            _encounterService.OnWildEncounter += OnWildEncounter;
            Unloaded += OnUnloaded;
        }

        // ---- Layout -------------------------------------------------------

        private View BuildLayout()
        {
            var root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto },
                },
            };

            root.Add(_map, 0, 0);

            // Location / info banner overlay.
            var banner = new Border
            {
                Margin = new Thickness(14),
                Padding = new Thickness(14, 7),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb("#D91A1E28"),
                Stroke = Color.FromArgb("#5AC8FA"),
                StrokeThickness = 2,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = new VerticalStackLayout { Children = { _locationLabel, _playerInfoLabel } },
            };
            root.Add(banner, 0, 0);

            // Bottom controls: D-pad + action button + menu.
            var controls = new Grid
            {
                Padding = new Thickness(18),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                },
            };

            controls.Add(BuildDpad(), 0, 0);
            controls.Add(BuildActionColumn(), 2, 0);
            root.Add(controls, 0, 1);

            return root;
        }

        private View BuildDpad()
        {
            var pad = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 8,
                ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) },
                RowDefinitions = { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) },
            };
            pad.Add(DirButton("▲", Facing.Up), 1, 0);
            pad.Add(DirButton("◀", Facing.Left), 0, 1);
            pad.Add(DirButton("▶", Facing.Right), 2, 1);
            pad.Add(DirButton("▼", Facing.Down), 1, 2);
            return pad;
        }

        private Border DirButton(string glyph, Facing facing)
        {
            var border = new Border
            {
                WidthRequest = 62,
                HeightRequest = 62,
                BackgroundColor = Color.FromArgb("#232634"),
                Stroke = Color.FromArgb("#3A3F52"),
                StrokeThickness = 2,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Content = new Label { Text = glyph, TextColor = Color.FromArgb("#E8ECF5"), FontSize = 24, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
            };

            var pointer = new PointerGestureRecognizer();
            pointer.PointerPressed += (_, _) => { border.BackgroundColor = Color.FromArgb("#3A4056"); _map.HoldDirection(facing); };
            pointer.PointerReleased += (_, _) => { border.BackgroundColor = Color.FromArgb("#232634"); _map.ReleaseDirection(facing); };
            pointer.PointerExited += (_, _) => { border.BackgroundColor = Color.FromArgb("#232634"); _map.ReleaseDirection(facing); };
            border.GestureRecognizers.Add(pointer);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => _map.TryMove(facing);
            border.GestureRecognizers.Add(tap);
            return border;
        }

        private View BuildActionColumn()
        {
            var stack = new VerticalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.End };

            var action = new Border
            {
                WidthRequest = 82,
                HeightRequest = 82,
                BackgroundColor = Color.FromArgb("#274060"),
                Stroke = Color.FromArgb("#5AC8FA"),
                StrokeThickness = 3,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 41 },
                Content = new Label { Text = "A", TextColor = Color.FromArgb("#EAF6FF"), FontSize = 30, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
            };
            var actionTap = new TapGestureRecognizer();
            actionTap.Tapped += (_, _) => _map.Interact();
            action.GestureRecognizers.Add(actionTap);

            var menu = new Border
            {
                WidthRequest = 82,
                HeightRequest = 40,
                BackgroundColor = Color.FromArgb("#2B2B3A"),
                Stroke = Color.FromArgb("#3A3F52"),
                StrokeThickness = 2,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = new Label { Text = "MENU", TextColor = Color.FromArgb("#C6CEDD"), FontSize = 13, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center },
            };
            var menuTap = new TapGestureRecognizer();
            menuTap.Tapped += async (_, _) => await OnMenu();
            menu.GestureRecognizers.Add(menuTap);

            stack.Add(menu);
            stack.Add(action);
            return stack;
        }

        // ---- Lifecycle ----------------------------------------------------

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialised)
            {
                if (!await LoadPlayerAndWorld()) return;
                _initialised = true;
                await _map.InitAsync(_zone!, _player!.WorldPosition.X, _player!.WorldPosition.Y);
                HookKeyboard();
                UpdateBanner();
            }
            else
            {
                _inBattle = false;
                _map.StartLoop(); // resume after returning from battle/menu
                UpdateBanner();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _map.StopLoop();
        }

        private void OnUnloaded(object? sender, EventArgs e)
        {
            if (_disposed)
                return;

            _disposed = true;
            Unloaded -= OnUnloaded;
            _encounterService.OnWildEncounter -= OnWildEncounter;
            _map.PlayerStepped -= OnPlayerStepped;
            _map.BuildingEntered -= OnBuildingEntered;
            _map.Dispose();
        }

        private async Task<bool> LoadPlayerAndWorld()
        {
            try
            {
                _player = _playerProfileService.GetCurrentProfile();
                if (_player == null)
                {
                    var profiles = await _playerProfileService.GetSavedProfiles();
                    if (profiles.Any())
                        _player = await _playerProfileService.LoadProfile(profiles.OrderByDescending(p => p.LastPlayedDate).First().Id);
                }
                if (_player == null)
                {
                    await DisplayAlert("Error", "No player profile found. Create a character first.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return false;
                }

                if (_worldManager.CurrentZone == null)
                {
                    _worldManager.InitializeDefaultWorld();
                    _encounterService.InitializeEncounterTables(_worldManager);
                }

                _zone = _worldManager.GetZone(_player.WorldPosition.ZoneId);
                if (_zone == null)
                {
                    _player.WorldPosition = new WorldPosition("starter_town", 10, 10);
                    _zone = _worldManager.GetZone("starter_town");
                }
                if (_zone != null) _worldManager.ChangeZone(_zone.Id);
                return _zone != null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load world: {ex.Message}", "OK");
                return false;
            }
        }

        private void UpdateBanner()
        {
            if (_player == null || _zone == null) return;
            _locationLabel.Text = _zone.Name;
            _playerInfoLabel.Text = $"{_player.PlayerName}   •   Bots {_player.TotalBots}   •   Gold {_player.Gold}";
        }

        // ---- Loop wiring --------------------------------------------------

        private async void OnPlayerStepped(int tileX, int tileY)
        {
            if (_player == null || _zone == null || _inBattle) return;

            _player.WorldPosition = new WorldPosition(_zone.Id, tileX, tileY);
            try
            {
                await _encounterService.CheckForWildEncounterAsync(_player); // may raise OnWildEncounter
            }
            catch (Exception ex)
            {
                Log.Error("Failed to process exploration encounter step.", ex, new Dictionary<string, object?>
                {
                    ["ZoneId"] = _zone.Id,
                    ["TileX"] = tileX,
                    ["TileY"] = tileY
                });

                if (!_inBattle)
                    _map.StartLoop();
            }

            try
            {
                await _playerProfileService.SaveCurrentProfile();
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to save the player profile after movement.", ex, new Dictionary<string, object?>
                {
                    ["ZoneId"] = _zone.Id,
                    ["TileX"] = tileX,
                    ["TileY"] = tileY
                });

                if (!_inBattle)
                    _map.StartLoop();
            }
        }

        private async void OnWildEncounter(PlayerProfile player, GladiatorBot wild, string zoneName)
        {
            if (_inBattle) return;
            _inBattle = true;
            _map.StopLoop();
            bool navigatedToBattle = false;

            try
            {
                bool fight = await DisplayAlert("Wild Encounter!",
                    $"A wild {wild.Name} appeared!", "Battle", "Run");
                if (!fight)
                    return;

                var bot = player.BotRoster.FirstOrDefault(b => b.CurrentHealth > 0);
                if (bot == null)
                {
                    await DisplayAlert("No Battle-Ready Bots", "All your Gladiators are down! Visit the Repair Bay.", "OK");
                    return;
                }

                await Navigation.PushAsync(new BattlePage(bot, wild));
                navigatedToBattle = true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to enter battle from exploration.", ex, new Dictionary<string, object?>
                {
                    ["ZoneName"] = zoneName,
                    ["WildBot"] = wild.Name
                });
                await DisplayAlert("Battle Error", ex.Message, "OK");
            }
            finally
            {
                if (!navigatedToBattle)
                {
                    _inBattle = false;
                    _map.StartLoop();
                }
            }
        }

        private async void OnBuildingEntered(WorldObjectType kind)
        {
            if (_player == null) return;
            if (kind == WorldObjectType.HealingCenter)
            {
                foreach (var bot in _player.BotRoster) bot.CurrentHealth = bot.MaxHealth;
                try
                {
                    await _playerProfileService.SaveCurrentProfile();
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to save after using the healing center.", ex);
                }
                UpdateBanner();
            }
        }

        private async Task OnMenu()
        {
            var action = await DisplayActionSheet("Menu", "Cancel", null,
                "Gladiator Roster", "Inventory", "Save Game", "Return to Main Menu");
            switch (action)
            {
                case "Gladiator Roster": await Shell.Current.GoToAsync("//BotRoster"); break;
                case "Inventory": await Shell.Current.GoToAsync("//Inventory"); break;
                case "Save Game":
                    await _playerProfileService.SaveCurrentProfile();
                    await DisplayAlert("Saved", "Game saved.", "OK");
                    break;
                case "Return to Main Menu": await Shell.Current.GoToAsync("//MainMenu"); break;
            }
        }

        // ---- Keyboard (Windows dev head) ----------------------------------

#if WINDOWS
        private bool _confirmHeld;
        private void HookKeyboard()
        {
            if (Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Panel panel)
            {
                panel.IsTabStop = true;
                panel.KeyDown += (_, e) => { if (MapKey(e.Key, true)) e.Handled = true; };
                panel.KeyUp += (_, e) => { if (MapKey(e.Key, false)) e.Handled = true; };
                panel.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            }
        }

        private bool MapKey(Windows.System.VirtualKey k, bool down)
        {
            void Move(Facing f, bool d) { if (d) _map.HoldDirection(f); else _map.ReleaseDirection(f); }
            switch (k)
            {
                case Windows.System.VirtualKey.Up: case Windows.System.VirtualKey.W: Move(Facing.Up, down); return true;
                case Windows.System.VirtualKey.Down: case Windows.System.VirtualKey.S: Move(Facing.Down, down); return true;
                case Windows.System.VirtualKey.Left: case Windows.System.VirtualKey.A: Move(Facing.Left, down); return true;
                case Windows.System.VirtualKey.Right: case Windows.System.VirtualKey.D: Move(Facing.Right, down); return true;
                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.E:
                    if (down && !_confirmHeld) { _confirmHeld = true; _map.Interact(); }
                    else if (!down) _confirmHeld = false;
                    return true;
                default: return false;
            }
        }
#else
        private void HookKeyboard() { }
#endif
    }
}
