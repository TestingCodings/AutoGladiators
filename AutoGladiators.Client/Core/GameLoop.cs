using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.Core
{
    public static class GameLoop
    {
        public static Task InitializeAsync(IUiBridge? ui = null, IMessageBus? bus = null, CancellationToken ct = default)
            => GameRuntime.Instance.InitializeAsync(ui, bus, ct);

        public static Task StartAsync(TimeSpan? interval = null, CancellationToken ct = default)
            => GameRuntime.Instance.StartAsync(interval, ct);

        public static Task StopAsync() => GameRuntime.Instance.StopAsync();

        public static Task GoToAsync(GameStateId id, StateArgs? args = null, CancellationToken ct = default)
            => GameRuntime.Instance.GoToAsync(id, args, ct);

        public static Task GoToAsync(string name, StateArgs? args = null, CancellationToken ct = default)
            => GameRuntime.Instance.GoToAsync(name, args, ct);
    }
}
