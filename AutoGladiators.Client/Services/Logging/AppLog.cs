using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services.Logging
{
    public static class AppLog
    {
        private static ILoggerFactory? _factory;

        /// <summary>
        /// Call once at startup (see MauiProgram) to use MAUI's logging providers.
        /// </summary>
        public static void Initialize(ILoggerFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Fallback factory so logs still work even if Initialize() wasn't called.
        /// </summary>
        private static ILoggerFactory Factory =>
            _factory ??= LoggerFactory.Create(b =>
            {
                b.SetMinimumLevel(LogLevel.Information);
                b.AddDebug();
                // b.AddConsole(); // optional if console is desired/available
            });

        public static ILogger For<T>() => Factory.CreateLogger(typeof(T).FullName!);
        public static ILogger For(string category) => Factory.CreateLogger(category);
    }
}
