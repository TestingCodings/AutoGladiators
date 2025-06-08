using NUnit.Framework;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Models;
using System.IO;

namespace AutoGladiators.Tests.Services
{
    public class SaveLoadServiceTests
    {
        private SaveLoadService _service;

        [SetUp]
        public void Setup()
        {
            _service = new SaveLoadService();
        }

        [Test]
        public void SaveAndLoadGame_ShouldPreservePlayerData()
        {
            var player = new Player { Name = "TestHero" };
            _service.SaveGame(player);
            var loadedPlayer = _service.LoadGame();

            Assert.AreEqual(player.Name, loadedPlayer.Name);
        }
    }
}