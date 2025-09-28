using AutoGladiators.Core.Services.Storage;

namespace AutoGladiators.Tests.Utils;

public sealed class FakeAppStorage : IAppStorage
{
    public string AppDataPath { get; }
    public FakeAppStorage(string? root = null)
    {
        AppDataPath = root ?? Path.Combine(Path.GetTempPath(), "AutoGladiatorsTests");
        Directory.CreateDirectory(AppDataPath);
    }
}
