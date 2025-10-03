using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;
using AutoGladiators.Core.Models;

namespace AutoGladiators.Core.Services
{
    public class PlayerMovementService : IDisposable
    {
        private static readonly IAppLogger Log = AppLog.For<PlayerMovementService>();

        private PlayerLocation _currentLocation;
        private System.Timers.Timer _movementTimer;
        private EncounterService _encounterService;

        public event Action<PlayerLocation> OnPlayerMoved;

        public PlayerMovementService(PlayerLocation startingLocation, EncounterService encounterService, int intervalMs = 1000)
        {
            _currentLocation = startingLocation.Clone();
            _encounterService = encounterService;
            _movementTimer = new System.Timers.Timer(intervalMs);
            _movementTimer.Elapsed += OnTimedMove;
            _movementTimer.AutoReset = false;
        }

        private void OnTimedMove(object sender, ElapsedEventArgs e)
        {
            _movementTimer.Stop();
            MovePlayer((1, 0)); // Example default right move
            _movementTimer.Start();
        }

        public void StartMovement() => _movementTimer.Start();
        public void StopMovement() => _movementTimer.Stop();

        public PlayerLocation GetCurrentLocation() => _currentLocation.Clone();

        public void UpdatePlayerPosition(PlayerLocation newLocation, bool isTeleport = false)
        {
            if (newLocation == null || newLocation.Equals(_currentLocation)) return;

            _currentLocation.Region = newLocation.Region;
            _currentLocation.X = newLocation.X;
            _currentLocation.Y = newLocation.Y;

            OnPlayerMoved?.Invoke(_currentLocation);

            if (!isTeleport)
            {
                AutoGladiators.Core.Core.GladiatorBot encounterTriggered;
                _encounterService?.TryTriggerEncounter(_currentLocation, out encounterTriggered);
            }
        }

        public void ResetToStartingLocation(PlayerLocation startingLocation)
        {
            UpdatePlayerPosition(startingLocation, isTeleport: true);
        }

        public void MovePlayer((int deltaX, int deltaY) delta)
        {
            var newLoc = _currentLocation + delta;
            UpdatePlayerPosition(newLoc);
        }

        public void SetMovementInterval(int intervalMs)
        {
            if (intervalMs > 0)
            {
                _movementTimer.Interval = intervalMs;
            }
        }

        public void Dispose() => _movementTimer?.Dispose();
    }
}
