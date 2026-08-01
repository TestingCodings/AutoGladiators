using AutoGladiators.Client.Game;

namespace AutoGladiators.Tests.ClientGame;

[TestFixture]
public class GameActorTests
{
    [Test]
    public void StartMoveTo_SameTile_StopsMovementAndResetsAnimation()
    {
        var actor = new GameActor(null, 4, 7, 16);
        actor.StartMoveTo(5, 7, Facing.Right);
        actor.Update(GameActor.StepDuration / 2f);

        actor.StartMoveTo(actor.TileX, actor.TileY, Facing.Right);

        Assert.Multiple(() =>
        {
            Assert.That(actor.IsMoving, Is.False);
            Assert.That(actor.AnimTime, Is.EqualTo(0f));
            Assert.That(actor.FrameIndex(), Is.EqualTo(10));
        });
    }

    [Test]
    public void Update_AdvancesTowardDestinationAndSnapsAtEnd()
    {
        var actor = new GameActor(null, 1, 2, 16);
        actor.StartMoveTo(2, 2, Facing.Right);

        actor.Update(GameActor.StepDuration / 2f);

        Assert.Multiple(() =>
        {
            Assert.That(actor.IsMoving, Is.True);
            Assert.That(actor.PixelX, Is.GreaterThan(16f).And.LessThan(32f));
            Assert.That(actor.PixelY, Is.EqualTo(32f));
            Assert.That(actor.FrameIndex(), Is.EqualTo(11));
        });

        actor.Update(GameActor.StepDuration);

        Assert.Multiple(() =>
        {
            Assert.That(actor.IsMoving, Is.False);
            Assert.That(actor.PixelX, Is.EqualTo(32f));
            Assert.That(actor.PixelY, Is.EqualTo(32f));
            Assert.That(actor.FrameIndex(), Is.EqualTo(10));
        });
    }

    [Test]
    public void Constructor_RejectsNonPositiveTileSize()
    {
        Assert.That(() => new GameActor(null, 0, 0, 0),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
