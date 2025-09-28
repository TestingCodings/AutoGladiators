using NUnit.Framework;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Models;
using System.IO;

namespace AutoGladiators.Tests.Services
{
    [TestFixture]
    public class SaveLoadServiceTests
    {
        [Test]
        public void SaveLoad_RoundTrip_PreservesPlayerProfile()
        {
            var profile = new PlayerProfile { Name = "Test", Gold = 123 };
            var path = Path.GetTempFileName();
            SaveLoadService.SaveProfile(profile, path);
            var loaded = SaveLoadService.LoadProfile(path);
            Assert.That(loaded.Name, Is.EqualTo(profile.Name));
            Assert.That(loaded.Gold, Is.EqualTo(profile.Gold));
            File.Delete(path);
        }

        [Test]
        public void SQLite_SmokeTest_CanOpenAndQuery()
        {
            var db = new SQLite.SQLiteConnection(":memory:");
            db.CreateTable<PlayerProfile>();
            db.Insert(new PlayerProfile { Name = "SqliteTest", Gold = 1 });
            var result = db.Table<PlayerProfile>().FirstOrDefault();
            Assert.That(result, Is.Not.Null);
        }
    }
}
