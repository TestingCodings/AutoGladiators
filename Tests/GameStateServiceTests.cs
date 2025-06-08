using NUnit.Framework;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Tests.Services
{
    public class GameStateServiceTests
    {
        private GameStateService _service;

        [SetUp]
        public void Setup()
        {
            _service = new GameStateService();
        }

        [Test]
        public void SetPlayer_ShouldStoreAndReturnCorrectPlayer()
        {
            var player = new Player { Name = "TestPlayer" };
            _service.SetPlayer(player);

            var result = _service.GetPlayer();
            Assert.AreEqual("TestPlayer", result.Name);
        }
    }
}