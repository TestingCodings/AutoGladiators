using AutoGladiators.Core.Utilities;

namespace AutoGladiators.Tests.Utilities;

[TestFixture]
public class SpriteSheetLayoutTests
{
    [Test]
    public void Create_RejectsUnevenFrameGrid()
    {
        Assert.That(
            () => SpriteSheetLayout.Create(80, 117, 16, 16, "sprites/npc_cheer_01.png"),
            Throws.InvalidOperationException.With.Message.Contains("not evenly divisible"));
    }

    [Test]
    public void GetFrameBounds_RejectsInvalidFrameIndexWithContext()
    {
        var layout = SpriteSheetLayout.Create(48, 64, 16, 16, "sprites/npc_01.png");

        Assert.That(
            () => layout.GetFrameBounds(12, "sprites/npc_01.png"),
            Throws.TypeOf<ArgumentOutOfRangeException>().With.Message.Contains("sprites/npc_01.png"));
    }

    [Test]
    public void GetFrameBounds_ReturnsExpectedRowMajorFrame()
    {
        var layout = SpriteSheetLayout.Create(48, 64, 16, 16, "sprites/npc_01.png");

        var frame = layout.GetFrameBounds(7, "sprites/npc_01.png");

        Assert.Multiple(() =>
        {
            Assert.That(layout.Columns, Is.EqualTo(3));
            Assert.That(layout.Rows, Is.EqualTo(4));
            Assert.That(frame.Left, Is.EqualTo(16));
            Assert.That(frame.Top, Is.EqualTo(32));
            Assert.That(frame.Right, Is.EqualTo(32));
            Assert.That(frame.Bottom, Is.EqualTo(48));
        });
    }
}
