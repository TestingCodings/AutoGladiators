using System.Text.Json;

namespace AutoGladiators.Core.Assets;

/// <summary>
/// Loads an <see cref="AssetManifest"/> from JSON.
/// </summary>
public static class AssetManifestLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Deserialises a manifest from a raw JSON string.</summary>
    /// <exception cref="InvalidOperationException">Thrown if the JSON is null or invalid.</exception>
    public static AssetManifest LoadFromJson(string json)
    {
        return JsonSerializer.Deserialize<AssetManifest>(json, Options)
               ?? throw new InvalidOperationException("Asset manifest JSON deserialised to null.");
    }

    /// <summary>Reads and deserialises a manifest from a file path (for local tools and tests).</summary>
    public static async Task<AssetManifest> LoadFromFileAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return LoadFromJson(json);
    }

    /// <summary>Synchronous version of <see cref="LoadFromFileAsync"/> for use in console tools.</summary>
    public static AssetManifest LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return LoadFromJson(json);
    }
}
