namespace AutoGladiators.Core.Assets;

/// <summary>
/// Resolves neutral asset IDs to sprite file paths.
/// Used by the renderer to load sprite sheets without hard-coded filename lists.
///
/// The catalog is built from the <see cref="AssetManifest"/> loaded at startup.
/// If the manifest is unavailable the catalog falls back to an empty state and
/// the renderer uses its own convention-based filename fallback.
/// </summary>
public sealed class AssetCatalog
{
    private readonly Dictionary<string, AssetManifestEntry> _byId;

    /// <summary>A catalog with no entries — used when the manifest cannot be loaded.</summary>
    public static readonly AssetCatalog Empty = new(new AssetManifest());

    public AssetCatalog(AssetManifest manifest)
    {
        _byId = manifest.Assets.ToDictionary(
            e => e.Id,
            e => e,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Returns the manifest entry for <paramref name="id"/>, or null if not found.</summary>
    public AssetManifestEntry? GetById(string id)
        => _byId.TryGetValue(id, out var entry) ? entry : null;

    /// <summary>
    /// Returns the exploration-sheet path (relative to Resources/Raw/) for
    /// <paramref name="id"/>, or null if the ID is not registered.
    /// </summary>
    public string? GetExplorationSheetPath(string id)
        => GetById(id)?.ExplorationSheet;

    /// <summary>
    /// Returns all asset IDs whose category matches one of the given values.
    /// Category comparison is case-insensitive.
    /// </summary>
    public IReadOnlyList<string> GetIdsByCategory(params string[] categories)
    {
        var set = new HashSet<string>(categories, StringComparer.OrdinalIgnoreCase);
        return _byId.Values
            .Where(e => set.Contains(e.Category))
            .Select(e => e.Id)
            .ToList();
    }

    /// <summary>Total number of registered entries.</summary>
    public int Count => _byId.Count;
}
