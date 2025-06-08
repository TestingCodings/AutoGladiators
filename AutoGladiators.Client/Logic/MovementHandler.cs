using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services;
using System;
using System.Timers; 
namespace AutoGladiators.Client.Logic 
{
    public class MovementHandler
    {
        private EncounterService _encounterService;
        private PlayerLocation _currentLocation;
        private System.Timers.Timer _movementTimer;

        public event Action<PlayerLocation> OnPlayerMoved;
        public event Action OnEncounterTriggered;

        public MovementHandler(PlayerLocation startingLocation, EncounterService encounterService)
        {
            _currentLocation = startingLocation;
            _encounterService = encounterService;
            _movementTimer = new Timer(100); // 100ms between moves
            _movementTimer.Elapsed += OnTimedMove;
        }

        public void MovePlayer((int deltaX, int deltaY) direction)
        {
            var newLocation = _currentLocation + direction;

            if (!newLocation.Equals(_currentLocation))
            {
                _currentLocation.X = newLocation.X;
                _currentLocation.Y = newLocation.Y;
                OnPlayerMoved?.Invoke(_currentLocation);

                // Check for random encounter at the new location
                if (_encounterService.CheckForEncounter(_currentLocation))
                {
                    OnEncounterTriggered?.Invoke();
                }
            }
        }

        private void OnTimedMove(object sender, ElapsedEventArgs e)
        {
            _movementTimer.Stop(); // Stop the timer to prevent multiple triggers
            MovePlayer((1, 0)); // Example: move right by default
            _movementTimer.Start(); // Restart the timer for continuous movement
        }
        public void StartMovement()
        {
            _movementTimer.Start();
        }
        public void StopMovement()
        {
            _movementTimer.Stop();
        }
        public PlayerLocation GetCurrentLocation()
        {
            return _currentLocation.Clone();
        }
        public void TeleportTo(PlayerLocation newLocation)
        {
            _currentLocation.Region = newLocation.Region;
            _currentLocation.X = newLocation.X;
            _currentLocation.Y = newLocation.Y;
            OnPlayerMoved?.Invoke(_currentLocation);
        }
        public void ResetToStartingLocation(PlayerLocation startingLocation)
        {
            _currentLocation.Region = startingLocation.Region;
            _currentLocation.X = startingLocation.X;
            _currentLocation.Y = startingLocation.Y;
            OnPlayerMoved?.Invoke(_currentLocation);
        }
        public void SetEncounterService(EncounterService encounterService)
        {
            _encounterService = encounterService;
        }
        public void SetCurrentLocation(PlayerLocation newLocation)
        {
            if (newLocation != null && !newLocation.Equals(_currentLocation))
            {
                _currentLocation.Region = newLocation.Region;
                _currentLocation.X = newLocation.X;
                _currentLocation.Y = newLocation.Y;
                OnPlayerMoved?.Invoke(_currentLocation);
            }
        }
        public void SetMovementInterval(int intervalMs)
        {
            if (intervalMs > 0)
            {
                _movementTimer.Interval = intervalMs;
            }
        }
        public void Dispose()
        {
            _movementTimer?.Dispose();
        }
    }
}
