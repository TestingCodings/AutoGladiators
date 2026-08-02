using NUnit.Framework;
using AutoGladiators.Core.Assets;

namespace AutoGladiators.Tests.Assets;

[TestFixture]
public class AssetManifestTests
{
    // -------------------------------------------------------------------------
    // Manifest loading
    // -------------------------------------------------------------------------

    [Test]
    public void LoadFromJson_ValidJson_ReturnsParsedManifest()
    {
        var json = """
            {
              "schemaVersion": "1.0",
              "description": "Test manifest",
              "assets": [
                {
                  "id": "npc_01",
                  "category": "npc",
                  "explorationSheet": "sprites/npc_01.png",
                  "frameWidth": 16,
                  "frameHeight": 16,
                  "columns": 3,
                  "rows": 4,
                  "sourceCategory": "placeholder",
                  "reviewStatus": "pending"
                }
              ]
            }
            """;

        var manifest = AssetManifestLoader.LoadFromJson(json);

        Assert.That(manifest.SchemaVersion, Is.EqualTo("1.0"));
        Assert.That(manifest.Assets, Has.Count.EqualTo(1));
        Assert.That(manifest.Assets[0].Id, Is.EqualTo("npc_01"));
        Assert.That(manifest.Assets[0].Category, Is.EqualTo("npc"));
        Assert.That(manifest.Assets[0].ExplorationSheet, Is.EqualTo("sprites/npc_01.png"));
        Assert.That(manifest.Assets[0].FrameWidth, Is.EqualTo(16));
        Assert.That(manifest.Assets[0].FrameHeight, Is.EqualTo(16));
        Assert.That(manifest.Assets[0].Columns, Is.EqualTo(3));
        Assert.That(manifest.Assets[0].Rows, Is.EqualTo(4));
    }

    [Test]
    public void LoadFromJson_OptionalFieldsAbsent_NullForOptionals()
    {
        var json = """
            {
              "schemaVersion": "1.0",
              "description": "",
              "assets": [
                {
                  "id": "npc_01",
                  "category": "npc",
                  "explorationSheet": "sprites/npc_01.png",
                  "frameWidth": 16,
                  "frameHeight": 16,
                  "columns": 3,
                  "rows": 4,
                  "sourceCategory": "placeholder",
                  "reviewStatus": "pending"
                }
              ]
            }
            """;

        var manifest = AssetManifestLoader.LoadFromJson(json);
        var entry = manifest.Assets[0];

        Assert.That(entry.Portrait, Is.Null);
        Assert.That(entry.BattleSprite, Is.Null);
        Assert.That(entry.PaletteId, Is.Null);
        Assert.That(entry.LicensingNotes, Is.Null);
    }

    [Test]
    public void LoadFromJson_WithOptionalPaths_ParsedCorrectly()
    {
        var json = """
            {
              "schemaVersion": "1.0",
              "description": "",
              "assets": [
                {
                  "id": "trainer_01",
                  "category": "trainer",
                  "explorationSheet": "sprites/trainer_01.png",
                  "frameWidth": 16,
                  "frameHeight": 16,
                  "columns": 3,
                  "rows": 4,
                  "portrait": "portraits/trainer_01.png",
                  "battleSprite": "battle/trainer_01.png",
                  "paletteId": "rival_red",
                  "sourceCategory": "ai-assisted-original",
                  "reviewStatus": "approved",
                  "licensingNotes": "Generated 2026-01-01"
                }
              ]
            }
            """;

        var manifest = AssetManifestLoader.LoadFromJson(json);
        var entry = manifest.Assets[0];

        Assert.That(entry.Portrait, Is.EqualTo("portraits/trainer_01.png"));
        Assert.That(entry.BattleSprite, Is.EqualTo("battle/trainer_01.png"));
        Assert.That(entry.PaletteId, Is.EqualTo("rival_red"));
        Assert.That(entry.LicensingNotes, Is.EqualTo("Generated 2026-01-01"));
    }

    [Test]
    public void LoadFromJson_EmptyAssets_ReturnsEmptyList()
    {
        var json = """{"schemaVersion":"1.0","description":"","assets":[]}""";
        var manifest = AssetManifestLoader.LoadFromJson(json);
        Assert.That(manifest.Assets, Is.Empty);
    }

    [Test]
    public void LoadFromJson_InvalidJson_Throws()
    {
        Assert.That(
            () => AssetManifestLoader.LoadFromJson("not valid json {{{"),
            Throws.InstanceOf<Exception>());
    }

    // -------------------------------------------------------------------------
    // Metadata validation — happy paths
    // -------------------------------------------------------------------------

    [Test]
    public void Validate_ValidEntry_NoErrors()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01",
            Category = "npc",
            ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16,
            FrameHeight = 16,
            Columns = 3,
            Rows = 4,
            SourceCategory = "placeholder",
            ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_ApprovedAiEntry_NoErrors()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "trainer_01",
            Category = "trainer",
            ExplorationSheet = "sprites/trainer_01.png",
            FrameWidth = 16,
            FrameHeight = 16,
            Columns = 3,
            Rows = 4,
            Portrait = "portraits/trainer_01.png",
            BattleSprite = "battle/trainer_01.png",
            SourceCategory = "ai-assisted-original",
            ReviewStatus = "approved",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors, Is.Empty);
    }

    // -------------------------------------------------------------------------
    // Required fields
    // -------------------------------------------------------------------------

    [Test]
    public void Validate_MissingId_ReturnsError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "",
            Category = "npc",
            ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "id"), Is.True);
    }

    [Test]
    public void Validate_MissingExplorationSheet_ReturnsError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01",
            Category = "npc",
            ExplorationSheet = "",
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "explorationSheet"), Is.True);
    }

    [Test]
    public void Validate_MissingOptionalPortrait_NoError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01",
            Category = "npc",
            ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            Portrait = null,      // optional — not required
            BattleSprite = null,  // optional — not required
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors, Is.Empty);
    }

    // -------------------------------------------------------------------------
    // Duplicate ID detection
    // -------------------------------------------------------------------------

    [Test]
    public void Validate_DuplicateIds_ReturnsError()
    {
        var manifest = MakeManifest(
            new AssetManifestEntry
            {
                Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
                FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
                SourceCategory = "placeholder", ReviewStatus = "pending",
            },
            new AssetManifestEntry
            {
                Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_02.png",
                FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
                SourceCategory = "placeholder", ReviewStatus = "pending",
            });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Message.Contains("Duplicate")), Is.True);
    }

    [Test]
    public void Validate_DuplicateIdsCaseInsensitive_ReturnsError()
    {
        var manifest = MakeManifest(
            new AssetManifestEntry
            {
                Id = "NPC_01", Category = "npc", ExplorationSheet = "sprites/a.png",
                FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
                SourceCategory = "placeholder", ReviewStatus = "pending",
            },
            new AssetManifestEntry
            {
                Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/b.png",
                FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
                SourceCategory = "placeholder", ReviewStatus = "pending",
            });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Message.Contains("Duplicate")), Is.True);
    }

    // -------------------------------------------------------------------------
    // Forbidden personal-name detection
    // -------------------------------------------------------------------------

    [Test]
    [TestCase("vince")]
    [TestCase("chad")]
    [TestCase("bradley")]
    [TestCase("colin")]
    [TestCase("john")]
    [TestCase("julian")]
    [TestCase("maria")]
    [TestCase("simon")]
    [TestCase("gary")]
    public void ContainsForbiddenName_PersonalName_ReturnsTrue(string name)
    {
        Assert.That(AssetManifestValidator.ContainsForbiddenName(name), Is.True);
    }

    [Test]
    [TestCase("npc_01")]
    [TestCase("trainer_rival_01")]
    [TestCase("sprites/npc_01.png")]
    [TestCase("boss_01")]
    public void ContainsForbiddenName_NeutralId_ReturnsFalse(string value)
    {
        Assert.That(AssetManifestValidator.ContainsForbiddenName(value), Is.False);
    }

    [Test]
    public void Validate_ForbiddenNameInId_ReturnsError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "vince",   // <-- personal name
            Category = "npc",
            ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "id" && e.Message.Contains("forbidden")), Is.True);
    }

    [Test]
    public void Validate_ForbiddenNameInExplorationSheet_ReturnsError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01",
            Category = "npc",
            ExplorationSheet = "sprites/vince.png",  // <-- personal name in path
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "explorationSheet" && e.Message.Contains("forbidden")), Is.True);
    }

    [Test]
    public void Validate_ForbiddenNameInPortrait_ReturnsError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01",
            Category = "npc",
            ExplorationSheet = "sprites/npc_01.png",
            Portrait = "portraits/gary.png",  // <-- personal name
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "portrait" && e.Message.Contains("forbidden")), Is.True);
    }

    // -------------------------------------------------------------------------
    // Frame geometry validation
    // -------------------------------------------------------------------------

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_ZeroOrNegativeFrameWidth_ReturnsError(int frameWidth)
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = frameWidth, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "frameWidth"), Is.True);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Validate_ZeroOrNegativeFrameHeight_ReturnsError(int frameHeight)
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = frameHeight, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "frameHeight"), Is.True);
    }

    [Test]
    public void Validate_ZeroColumns_ReturnsError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = 16, Columns = 0, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "columns"), Is.True);
    }

    [Test]
    public void Validate_ZeroRows_ReturnsError()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 0,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var errors = AssetManifestValidator.Validate(manifest);
        Assert.That(errors.Any(e => e.Field == "rows"), Is.True);
    }

    // -------------------------------------------------------------------------
    // AssetCatalog
    // -------------------------------------------------------------------------

    [Test]
    public void AssetCatalog_GetExplorationSheetPath_ReturnsCorrectPath()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var catalog = new AssetCatalog(manifest);
        Assert.That(catalog.GetExplorationSheetPath("npc_01"), Is.EqualTo("sprites/npc_01.png"));
    }

    [Test]
    public void AssetCatalog_GetExplorationSheetPath_UnknownId_ReturnsNull()
    {
        var catalog = AssetCatalog.Empty;
        Assert.That(catalog.GetExplorationSheetPath("npc_99"), Is.Null);
    }

    [Test]
    public void AssetCatalog_GetIdsByCategory_FiltersCorrectly()
    {
        var manifest = MakeManifest(
            new AssetManifestEntry
            {
                Id = "npc_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
                FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
                SourceCategory = "placeholder", ReviewStatus = "pending",
            },
            new AssetManifestEntry
            {
                Id = "trainer_01", Category = "trainer", ExplorationSheet = "sprites/trainer_01.png",
                FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
                SourceCategory = "placeholder", ReviewStatus = "pending",
            },
            new AssetManifestEntry
            {
                Id = "boss_01", Category = "boss", ExplorationSheet = "sprites/boss_01.png",
                FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
                SourceCategory = "placeholder", ReviewStatus = "pending",
            });

        var catalog = new AssetCatalog(manifest);
        var npcIds = catalog.GetIdsByCategory("npc");
        var trainerIds = catalog.GetIdsByCategory("trainer");
        var combined = catalog.GetIdsByCategory("npc", "trainer");

        Assert.That(npcIds, Has.Count.EqualTo(1));
        Assert.That(npcIds, Contains.Item("npc_01"));
        Assert.That(trainerIds, Has.Count.EqualTo(1));
        Assert.That(combined, Has.Count.EqualTo(2));
    }

    [Test]
    public void AssetCatalog_Empty_CountIsZero()
    {
        Assert.That(AssetCatalog.Empty.Count, Is.EqualTo(0));
    }

    [Test]
    public void AssetCatalog_LookupCaseInsensitive()
    {
        var manifest = MakeManifest(new AssetManifestEntry
        {
            Id = "NPC_01", Category = "npc", ExplorationSheet = "sprites/npc_01.png",
            FrameWidth = 16, FrameHeight = 16, Columns = 3, Rows = 4,
            SourceCategory = "placeholder", ReviewStatus = "pending",
        });

        var catalog = new AssetCatalog(manifest);
        Assert.That(catalog.GetExplorationSheetPath("npc_01"), Is.EqualTo("sprites/npc_01.png"));
        Assert.That(catalog.GetExplorationSheetPath("NPC_01"), Is.EqualTo("sprites/npc_01.png"));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static AssetManifest MakeManifest(params AssetManifestEntry[] entries) =>
        new() { SchemaVersion = "1.0", Assets = entries.ToList() };
}
