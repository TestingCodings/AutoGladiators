using SkiaSharp;

namespace AutoGladiators.Client.Game;

/// <summary>
/// State for the GBA-style conversation overlay: a queue of lines revealed with
/// a typewriter effect. Advance() completes the current line if still typing,
/// otherwise moves to the next line (closing after the last).
/// </summary>
public sealed class DialogueBox
{
    public bool IsActive { get; private set; }
    public string SpeakerName { get; private set; } = "";
    public bool SpeakerIsRobot { get; private set; }
    public SKColor SpeakerCore { get; private set; }

    private string[] _lines = Array.Empty<string>();
    private int _lineIndex;
    private int _charsShown;
    private float _charTimer;

    private const float CharsPerSecond = 42f;

    public void Open(string speaker, string[] lines, bool robot, SKColor core)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var sanitizedLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        if (sanitizedLines.Length == 0)
        {
            Close();
            return;
        }

        SpeakerName = speaker ?? string.Empty;
        SpeakerIsRobot = robot;
        SpeakerCore = core;
        _lines = sanitizedLines;
        _lineIndex = 0;
        _charsShown = 0;
        _charTimer = 0;
        IsActive = true;
    }

    public string CurrentLine => _lines.Length == 0 || _lineIndex >= _lines.Length ? "" : _lines[_lineIndex];
    public bool LineFullyShown => _charsShown >= CurrentLine.Length;
    public bool OnLastLine => _lineIndex >= _lines.Length - 1;
    public string VisibleText => CurrentLine[..Math.Min(_charsShown, CurrentLine.Length)];

    public void Update(float dt)
    {
        if (!IsActive || LineFullyShown || dt <= 0f)
            return;

        _charTimer += dt;
        _charsShown = Math.Min(CurrentLine.Length, (int)(_charTimer * CharsPerSecond));
    }

    /// <summary>Confirm pressed: reveal-all, then next line, then close.</summary>
    public void Advance()
    {
        if (!IsActive) return;
        if (!LineFullyShown)
        {
            _charsShown = CurrentLine.Length;
            return;
        }

        if (OnLastLine)
        {
            Close();
            return;
        }

        _lineIndex++;
        _charsShown = 0;
        _charTimer = 0;
    }

    public void Close()
    {
        IsActive = false;
        _lines = Array.Empty<string>();
        _lineIndex = 0;
        _charsShown = 0;
        _charTimer = 0f;
        SpeakerName = string.Empty;
        SpeakerIsRobot = false;
        SpeakerCore = default;
    }
}
