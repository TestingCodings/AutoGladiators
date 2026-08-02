using System.Text;
using AutoGladiators.Core.Assets;

/// <summary>
/// AutoGladiators Asset Validator
///
/// Validates the asset manifest and (optionally) the referenced sprite files.
///
/// Usage:
///   dotnet run --project Tools/AssetValidator -- [OPTIONS]
///
/// Options:
///   --manifest &lt;path&gt;   Path to AssetManifest.json (default: AutoGladiators.Client/Config/AssetManifest.json)
///   --assets-root &lt;path&gt; Root directory that asset paths are relative to (default: AutoGladiators.Client/Resources/Raw)
///   --skip-files        Skip file-system checks (metadata-only validation)
///   --help              Show this help text
///
/// Exit codes:
///   0 — All checks passed (warnings are printed but do not fail)
///   1 — One or more validation errors were found
///   2 — Manifest file not found or could not be parsed
/// </summary>

int Run(string[] args)
{
    string manifestPath = Path.Combine("AutoGladiators.Client", "Config", "AssetManifest.json");
    string assetsRoot = Path.Combine("AutoGladiators.Client", "Resources", "Raw");
    bool skipFiles = false;

    // ---- Parse arguments ----
    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--manifest" when i + 1 < args.Length:
                manifestPath = args[++i];
                break;
            case "--assets-root" when i + 1 < args.Length:
                assetsRoot = args[++i];
                break;
            case "--skip-files":
                skipFiles = true;
                break;
            case "--help":
            case "-h":
                PrintHelp();
                return 0;
            default:
                Console.Error.WriteLine($"Unknown argument: {args[i]}");
                PrintHelp();
                return 2;
        }
    }

    // ---- Locate the manifest relative to the working directory or repo root ----
    if (!File.Exists(manifestPath))
    {
        // Try relative to the repo root (useful when running from the Tools/ directory).
        string repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
        if (!string.IsNullOrEmpty(repoRoot))
        {
            manifestPath = Path.Combine(repoRoot, manifestPath);
            assetsRoot = Path.Combine(repoRoot, assetsRoot);
        }
    }

    // ---- Load the manifest ----
    AssetManifest manifest;
    try
    {
        if (!File.Exists(manifestPath))
        {
            Console.Error.WriteLine($"[ERROR] Manifest not found: {manifestPath}");
            return 2;
        }
        manifest = AssetManifestLoader.LoadFromFile(manifestPath);
        Console.WriteLine($"Loaded manifest: {manifestPath}");
        Console.WriteLine($"  Schema version : {manifest.SchemaVersion}");
        Console.WriteLine($"  Asset count    : {manifest.Assets.Count}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[ERROR] Could not parse manifest: {ex.Message}");
        return 2;
    }

    int errorCount = 0;
    int warningCount = 0;

    // ---- Metadata validation ----
    var metaErrors = AssetManifestValidator.Validate(manifest);
    if (metaErrors.Count > 0)
    {
        Console.WriteLine($"\n--- Metadata validation ({metaErrors.Count} issue(s)) ---");
        foreach (var e in metaErrors)
        {
            // Forbidden-name errors on paths are expected while anonymization is pending.
            bool isPendingAnon = manifest.Assets
                .Any(a => a.Id == e.AssetId && a.ReviewStatus == "pending-anonymization");

            if (isPendingAnon && e.Field != "id")
            {
                Console.WriteLine($"  [WARN]  {e.AssetId}.{e.Field}: {e.Message}");
                warningCount++;
            }
            else
            {
                Console.WriteLine($"  [ERROR] {e.AssetId}.{e.Field}: {e.Message}");
                errorCount++;
            }
        }
    }
    else
    {
        Console.WriteLine("\nMetadata validation: OK");
    }

    // ---- File-system validation ----
    if (!skipFiles)
    {
        Console.WriteLine($"\n--- File validation (root: {assetsRoot}) ---");
        foreach (var entry in manifest.Assets)
        {
            var fileErrors = ValidateFiles(entry, assetsRoot);
            foreach (var msg in fileErrors)
            {
                // Missing files are errors; dimension checks require PNG to exist first.
                Console.WriteLine($"  [ERROR] {entry.Id}: {msg}");
                errorCount++;
            }
        }
        if (errorCount == 0 && warningCount == 0)
            Console.WriteLine("File validation: OK (all referenced files found)");
    }

    // ---- Summary ----
    Console.WriteLine();
    if (errorCount > 0)
    {
        Console.WriteLine($"FAILED — {errorCount} error(s), {warningCount} warning(s).");
        if (manifest.Assets.Any(a => a.ReviewStatus == "pending-anonymization"))
        {
            Console.WriteLine("NOTE: Some warnings are expected — anonymization of personal-name sprite");
            Console.WriteLine("      files is pending. Update ExplorationSheet paths in the manifest once");
            Console.WriteLine("      the anonymization PR merges.");
        }
        return 1;
    }

    if (warningCount > 0)
        Console.WriteLine($"PASSED with {warningCount} warning(s) — see above.");
    else
        Console.WriteLine("PASSED — no issues found.");

    return 0;
}

static List<string> ValidateFiles(AssetManifestEntry entry, string assetsRoot)
{
    var errors = new List<string>();

    // Check exploration sheet (required).
    if (!string.IsNullOrWhiteSpace(entry.ExplorationSheet))
    {
        var fullPath = Path.Combine(assetsRoot, entry.ExplorationSheet.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            errors.Add($"ExplorationSheet file not found: {fullPath}");
        }
        else
        {
            var dimErrors = ValidatePngDimensions(
                fullPath, entry.Id, "explorationSheet",
                entry.FrameWidth, entry.FrameHeight,
                entry.Columns, entry.Rows);
            errors.AddRange(dimErrors);
        }
    }

    // Check portrait (optional).
    if (!string.IsNullOrWhiteSpace(entry.Portrait))
    {
        var fullPath = Path.Combine(assetsRoot, entry.Portrait!.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            errors.Add($"Portrait file not found: {fullPath}");
    }

    // Check battle sprite (optional).
    if (!string.IsNullOrWhiteSpace(entry.BattleSprite))
    {
        var fullPath = Path.Combine(assetsRoot, entry.BattleSprite!.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            errors.Add($"BattleSprite file not found: {fullPath}");
    }

    return errors;
}

/// <summary>
/// Reads the PNG IHDR chunk to extract width and height without a full decode,
/// then validates divisibility and expected frame counts.
/// </summary>
static List<string> ValidatePngDimensions(
    string path, string id, string fieldName,
    int frameW, int frameH, int columns, int rows)
{
    var errors = new List<string>();
    try
    {
        int width, height;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            // PNG signature: 8 bytes. IHDR chunk: 4 (length) + 4 (type) + 4 (width) + 4 (height) = 16 bytes.
            var header = new byte[24];
            if (fs.Read(header, 0, 24) < 24)
            {
                errors.Add($"{fieldName}: File too small to be a valid PNG — {path}");
                return errors;
            }

            // Verify PNG signature: 137 80 78 71 13 10 26 10
            if (header[0] != 137 || header[1] != 80 || header[2] != 78 || header[3] != 71
                || header[4] != 13 || header[5] != 10 || header[6] != 26 || header[7] != 10)
            {
                errors.Add($"{fieldName}: File does not have a valid PNG signature — {path}");
                return errors;
            }

            // IHDR: bytes 16–23 contain big-endian width (4) + height (4).
            width  = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
            height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
        }

        // Expected sheet dimensions.
        int expectedW = frameW * columns;
        int expectedH = frameH * rows;

        if (width != expectedW || height != expectedH)
        {
            errors.Add($"{fieldName}: Expected {expectedW}×{expectedH} px " +
                       $"({columns} cols × {rows} rows of {frameW}×{frameH} frames), " +
                       $"but file is {width}×{height} px — {Path.GetFileName(path)}");
        }
        else
        {
            // Belt-and-braces: also check divisibility.
            if (width % frameW != 0)
                errors.Add($"{fieldName}: Width {width} is not divisible by frameWidth {frameW}.");
            if (height % frameH != 0)
                errors.Add($"{fieldName}: Height {height} is not divisible by frameHeight {frameH}.");
        }
    }
    catch (Exception ex)
    {
        errors.Add($"{fieldName}: Could not read PNG dimensions — {ex.Message}");
    }
    return errors;
}

static string FindRepoRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return "";
}

static void PrintHelp()
{
    Console.WriteLine(@"AutoGladiators Asset Validator

Usage:
  dotnet run --project Tools/AssetValidator -- [OPTIONS]

Options:
  --manifest <path>     Path to AssetManifest.json
                        Default: AutoGladiators.Client/Config/AssetManifest.json
  --assets-root <path>  Root directory that asset paths are relative to
                        Default: AutoGladiators.Client/Resources/Raw
  --skip-files          Skip file-system checks (metadata-only validation)
  --help                Show this help text

Exit codes:
  0  All checks passed
  1  One or more validation errors found
  2  Manifest not found or could not be parsed

Examples:
  dotnet run --project Tools/AssetValidator
  dotnet run --project Tools/AssetValidator -- --skip-files
  dotnet run --project Tools/AssetValidator -- --manifest path/to/AssetManifest.json
");
}

return Run(args);
