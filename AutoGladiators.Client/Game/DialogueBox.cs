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
        if (lines.Length == 0) return;
        SpeakerName = speaker;
        SpeakerIsRobot = robot;
        SpeakerCore = core;
        _lines = lines;
        _lineIndex = 0;
        _charsShown = 0;
        _charTimer = 0;
        IsActive = true;
    }

    public string CurrentLine => _lines.Length == 0 ? "" : _lines[_lineIndex];
    public bool LineFullyShown => _charsShown >= CurrentLine.Length;
    public bool OnLastLine => _lineIndex >= _lines.Length - 1;
    public string VisibleText => CurrentLine[.._charsShown];

    public void Update(float dt)
    {
        if (!IsActive || LineFullyShown) return;
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
            IsActive = false;
            return;
        }
        _lineIndex++;
        _charsShown = 0;
        _charTimer = 0;
    }
}
