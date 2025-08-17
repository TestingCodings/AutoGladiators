using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using AutoGladiators.Client;

namespace AutoGladiators_MAUI;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
