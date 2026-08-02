namespace AutoGladiators.Core.Assets;

/// <summary>
/// A single entry in the asset manifest, describing one character sprite set.
/// Maps a neutral asset ID to its file paths and records provenance metadata.
/// </summary>
public sealed class AssetManifestEntry
{
    /// <summary>Unique neutral identifier — must not contain personal names.</summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Asset category. Valid values: npc, trainer, player, enemy, boss, item, ui, effect.
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// Path relative to Resources/Raw/ for the exploration sprite sheet PNG.
    /// Required. Sheet must conform to the 3-column × 4-row, 16×16-frame contract
    /// described in ART_STYLE_GUIDE.md.
    /// </summary>
    public string ExplorationSheet { get; set; } = "";

    /// <summary>Frame width in pixels (16 for the current sprite-sheet contract).</summary>
    public int FrameWidth { get; set; } = 16;

    /// <summary>Frame height in pixels (16 for the current sprite-sheet contract).</summary>
    public int FrameHeight { get; set; } = 16;

    /// <summary>Number of columns in the exploration sheet (3 for the current contract).</summary>
    public int Columns { get; set; } = 3;

    /// <summary>Number of rows in the exploration sheet (4 for the current contract).</summary>
    public int Rows { get; set; } = 4;

    /// <summary>Optional path relative to Resources/Raw/ for a dialogue portrait PNG.</summary>
    public string? Portrait { get; set; }

    /// <summary>Optional path relative to Resources/Raw/ for a battle sprite PNG.</summary>
    public string? BattleSprite { get; set; }

    /// <summary>Named palette identifier for this character.</summary>
    public string? PaletteId { get; set; }

    /// <summary>
    /// Provenance category. Valid values: original, ai-assisted-original,
    /// ai-assisted-edited, licensed-pack, placeholder, pending-anonymization.
    /// </summary>
    public string SourceCategory { get; set; } = "placeholder";

    /// <summary>
    /// Review status. Valid values: approved, pending, pending-anonymization, draft.
    /// </summary>
    public string ReviewStatus { get; set; } = "pending";

    /// <summary>Ownership, licensing, or provenance notes.</summary>
    public string? LicensingNotes { get; set; }
}

/// <summary>
/// Root object for AssetManifest.json.
/// </summary>
public sealed class AssetManifest
{
    /// <summary>Schema version — increment when the schema changes.</summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>Human-readable description of the manifest.</summary>
    public string Description { get; set; } = "";

    /// <summary>All registered asset entries.</summary>
    public List<AssetManifestEntry> Assets { get; set; } = new();
}
