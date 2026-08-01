using AutoGladiators.Client.Game;
using SkiaSharp;

namespace AutoGladiators.Tests.ClientGame;

[TestFixture]
public class DialogueBoxTests
{
    [Test]
    public void Update_RevealsCharactersUntilAdvanceCompletesLine()
    {
        var box = new DialogueBox();
        box.Open("Guide", new[] { "HELLO", "WORLD" }, robot: false, SKColors.CornflowerBlue);

        box.Update(0.05f);
        Assert.That(box.VisibleText.Length, Is.InRange(1, 4));

        box.Advance();
        Assert.That(box.VisibleText, Is.EqualTo("HELLO"));

        box.Advance();
        Assert.Multiple(() =>
        {
            Assert.That(box.CurrentLine, Is.EqualTo("WORLD"));
            Assert.That(box.VisibleText, Is.Empty);
            Assert.That(box.IsActive, Is.True);
        });
    }

    [Test]
    public void Advance_OnLastLine_ClosesAndClearsState()
    {
        var box = new DialogueBox();
        box.Open("Guide", new[] { "DONE" }, robot: true, SKColors.OrangeRed);

        box.Advance();
        box.Advance();

        Assert.Multiple(() =>
        {
            Assert.That(box.IsActive, Is.False);
            Assert.That(box.SpeakerName, Is.Empty);
            Assert.That(box.VisibleText, Is.Empty);
            Assert.That(box.CurrentLine, Is.Empty);
        });
    }

    [Test]
    public void Open_IgnoresBlankDialogueAndClosesAnyExistingConversation()
    {
        var box = new DialogueBox();
        box.Open("Guide", new[] { "Ready" }, robot: false, SKColors.White);

        box.Open("Guide", new[] { "", "   " }, robot: false, SKColors.White);

        Assert.That(box.IsActive, Is.False);
    }
}
