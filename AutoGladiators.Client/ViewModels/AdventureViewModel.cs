using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Core;
using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.ViewModels
{
    public partial class AdventureViewModel : ObservableObject
    {
        [ObservableProperty]
        private PlayerLocation currentLocation;

        private readonly PlayerMovementService movementService;

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }

        public AdventureViewModel(PlayerMovementService movementService)
        {
            this.movementService = movementService;
            this.movementService.OnPlayerMoved += OnPlayerMoved;

            MoveUpCommand = new RelayCommand(() => MovePlayer(0, -1));
            MoveDownCommand = new RelayCommand(() => MovePlayer(0, 1));
            MoveLeftCommand = new RelayCommand(() => MovePlayer(-1, 0));
            MoveRightCommand = new RelayCommand(() => MovePlayer(1, 0));

            CurrentLocation = movementService.GetCurrentLocation();
        }

        private void OnPlayerMoved(PlayerLocation newLocation)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentLocation = newLocation;
            });
        }

        private void MovePlayer(int dx, int dy)
        {
            movementService.MovePlayer((dx, dy));
        }
    }
}
// This ViewModel handles player movement in the adventure view, updating the current location
// and responding to movement commands. It uses the PlayerMovementService to manage the player's
// position and trigger encounters. The OnPlayerMoved event updates the UI with the new location.