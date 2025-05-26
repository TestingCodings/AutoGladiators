#if WINDOWS
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace AutoGladiators_MAUI;

public class AppHost : Microsoft.Maui.MauiWinUIApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    [STAThread]
    public static void Main(string[] args)
    {
        var app = new AppHost();
        app.Run(args);
    }
}
#endif
