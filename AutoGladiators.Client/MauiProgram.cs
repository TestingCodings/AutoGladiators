using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;
using AutoGladiators.Client; 

namespace AutoGladiators_MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
// This code sets up a .NET MAUI application with the CommunityToolkit.Maui library.
// It configures the application to use specific fonts and initializes the main application class.
// The `CreateMauiApp` method is the entry point for the application, where the builder is configured and the app is built.

