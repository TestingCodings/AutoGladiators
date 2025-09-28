using AutoGladiators.Core.Services.Storage;
using Microsoft.Maui.Storage;

namespace AutoGladiators.Client.Services.Storage;

public sealed class AppStorage : IAppStorage
{
    public string AppDataPath => FileSystem.AppDataDirectory;
}
