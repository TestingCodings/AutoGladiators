namespace AutoGladiators.Core.Assets;

/// <summary>
/// A single validation finding from <see cref="AssetManifestValidator"/>.
/// </summary>
/// <param name="AssetId">The manifest entry ID the error relates to (may be empty for schema-level errors).</param>
/// <param name="Field">The field or context the error relates to.</param>
/// <param name="Message">Human-readable description of the problem.</param>
public sealed record ValidationError(string AssetId, string Field, string Message);

/// <summary>
/// Validates an <see cref="AssetManifest"/> for structural correctness, forbidden
/// personal-name patterns, frame-geometry integrity, and duplicate IDs.
///
/// This validator operates on metadata only — it does not perform file-system I/O.
/// File-existence and PNG-decode checks are handled by the AssetValidator console tool.
/// </summary>
public static class AssetManifestValidator
{
    /// <summary>
    /// Personal names that must not appear in asset IDs or paths.
    /// This list covers the original personal-name sprites that are being
    /// replaced as part of the anonymization effort.
    /// </summary>
    public static readonly IReadOnlyList<string> ForbiddenPatterns = new[]
    {
        "vince", "chad", "bradley", "colin", "john", "julian",
        "maria", "simon", "gary", "priya", "mike", "juliet",
        "sarah", "barry", "doyle", "derek", "claus", "clive",
        "lulu", "hrlady",
    };

    private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "npc", "trainer", "player", "enemy", "boss", "item", "ui", "effect",
    };

    private static readonly HashSet<string> ValidSourceCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "original", "ai-assisted-original", "ai-assisted-edited",
        "licensed-pack", "placeholder", "pending-anonymization",
    };

    private static readonly HashSet<string> ValidReviewStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "approved", "pending", "pending-anonymization", "draft",
    };

    /// <summary>
    /// Validates the entire manifest. Returns all detected errors.
    /// An empty list means the manifest is structurally valid.
    /// </summary>
    public static IReadOnlyList<ValidationError> Validate(AssetManifest manifest)
    {
        var errors = new List<ValidationError>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in manifest.Assets)
        {
            ValidateEntry(entry, seenIds, errors);
        }

        return errors;
    }

    /// <summary>
    /// Returns true if <paramref name="value"/> contains any forbidden personal-name
    /// pattern (case-insensitive).
    /// </summary>
    public static bool ContainsForbiddenName(string value)
    {
        var lower = value.ToLowerInvariant();
        return ForbiddenPatterns.Any(p => lower.Contains(p));
    }

    // -------------------------------------------------------------------------

    private static void ValidateEntry(
        AssetManifestEntry entry,
        HashSet<string> seenIds,
        List<ValidationError> errors)
    {
        // ---- Required: Id ----
        if (string.IsNullOrWhiteSpace(entry.Id))
        {
            errors.Add(new("", "id", "Asset ID is required and must not be empty."));
            // Cannot continue with meaningful per-field checks without an ID.
            return;
        }

        // ---- Duplicate IDs ----
        if (!seenIds.Add(entry.Id))
            errors.Add(new(entry.Id, "id", "Duplicate asset ID."));

        // ---- Forbidden names in ID ----
        if (ContainsForbiddenName(entry.Id))
            errors.Add(new(entry.Id, "id", "Asset ID contains a forbidden personal name."));

        // ---- Category ----
        if (string.IsNullOrWhiteSpace(entry.Category))
            errors.Add(new(entry.Id, "category", "Category is required."));
        else if (!ValidCategories.Contains(entry.Category))
            errors.Add(new(entry.Id, "category",
                $"Unknown category '{entry.Category}'. Valid values: {string.Join(", ", ValidCategories)}."));

        // ---- ExplorationSheet (required) ----
        if (string.IsNullOrWhiteSpace(entry.ExplorationSheet))
        {
            errors.Add(new(entry.Id, "explorationSheet", "ExplorationSheet path is required."));
        }
        else
        {
            if (ContainsForbiddenName(entry.ExplorationSheet))
                errors.Add(new(entry.Id, "explorationSheet",
                    "ExplorationSheet path contains a forbidden personal name. Update this path once the anonymization PR lands."));

            if (!entry.ExplorationSheet.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                errors.Add(new(entry.Id, "explorationSheet",
                    "ExplorationSheet path should end with .png."));
        }

        // ---- Frame geometry ----
        if (entry.FrameWidth <= 0)
            errors.Add(new(entry.Id, "frameWidth", $"FrameWidth must be positive, got {entry.FrameWidth}."));

        if (entry.FrameHeight <= 0)
            errors.Add(new(entry.Id, "frameHeight", $"FrameHeight must be positive, got {entry.FrameHeight}."));

        if (entry.Columns <= 0)
            errors.Add(new(entry.Id, "columns", $"Columns must be positive, got {entry.Columns}."));

        if (entry.Rows <= 0)
            errors.Add(new(entry.Id, "rows", $"Rows must be positive, got {entry.Rows}."));

        // ---- Optional paths — forbidden-name check ----
        if (entry.Portrait != null && ContainsForbiddenName(entry.Portrait))
            errors.Add(new(entry.Id, "portrait",
                "Portrait path contains a forbidden personal name."));

        if (entry.BattleSprite != null && ContainsForbiddenName(entry.BattleSprite))
            errors.Add(new(entry.Id, "battleSprite",
                "BattleSprite path contains a forbidden personal name."));

        // ---- SourceCategory ----
        if (string.IsNullOrWhiteSpace(entry.SourceCategory))
            errors.Add(new(entry.Id, "sourceCategory", "SourceCategory is required."));
        else if (!ValidSourceCategories.Contains(entry.SourceCategory))
            errors.Add(new(entry.Id, "sourceCategory",
                $"Unknown sourceCategory '{entry.SourceCategory}'."));

        // ---- ReviewStatus ----
        if (string.IsNullOrWhiteSpace(entry.ReviewStatus))
            errors.Add(new(entry.Id, "reviewStatus", "ReviewStatus is required."));
        else if (!ValidReviewStatuses.Contains(entry.ReviewStatus))
            errors.Add(new(entry.Id, "reviewStatus",
                $"Unknown reviewStatus '{entry.ReviewStatus}'."));
    }
}
