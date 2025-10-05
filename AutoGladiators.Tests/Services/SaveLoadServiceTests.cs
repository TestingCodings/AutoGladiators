using NUnit.Framework;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using System.IO;

namespace AutoGladiators.Tests.Services
{
    [TestFixture]
    public class SaveLoadServiceTests
    {
        [Test]
        public void SaveLoad_RoundTrip_PreservesPlayerProfile()
        {
            var profile = new PlayerProfile { PlayerName = "Test" };
            profile.AddItem("Gold", 123);
            var path = Path.GetTempFileName();
            SaveLoadService.SaveProfile(profile, path);
            var loaded = SaveLoadService.LoadProfile(path);
            Assert.That(loaded.PlayerName, Is.EqualTo(profile.PlayerName));
            Assert.That(loaded.Gold, Is.EqualTo(profile.Gold));
            File.Delete(path);
        }

        [Test]
        public void SQLite_SmokeTest_CanOpenAndQuery()
        {
            var db = new SQLite.SQLiteConnection(":memory:");
            db.CreateTable<PlayerProfile>();
            var testProfile = new PlayerProfile { PlayerName = "SqliteTest" };
            testProfile.AddItem("Gold", 1);
            db.Insert(testProfile);
            var result = db.Table<PlayerProfile>().FirstOrDefault();
            Assert.That(result, Is.Not.Null);
        }
    }
}
