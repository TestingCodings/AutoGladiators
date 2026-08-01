#if WINDOWS
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using AutoGladiators.Client;

namespace AutoGladiators_MAUI;

public class AppHost : Microsoft.Maui.MauiWinUIApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    [STAThread]
    public static void Main(string[] args)
    {
        global::WinRT.ComWrappersSupport.InitializeComWrappers();
        global::Microsoft.UI.Xaml.Application.Start((p) =>
        {
            var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
            _ = new AppHost();
        });
    }
}
#endif
