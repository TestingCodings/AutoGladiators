using NUnit.Framework;
using AutoGladiators.Core;
using AutoGladiators.Core.Behaviors;
using AutoGladiators.Core.Simulation;
using System.Collections.Generic;

namespace AutoGladiators.Tests
{
    public class GladiatorTests
    {
        GladiatorBot botA;
        GladiatorBot botB;

        [SetUp]
        public void Setup()
        {
            botA = new GladiatorBot("Ares", new AggressiveBehavior())
            {
                Speed = 7, Strength = 9, Agility = 6, Intelligence = 4, Durability = 8
            };

            botB = new GladiatorBot("Hades", new AggressiveBehavior())
            {
                Speed = 6, Strength = 8, Agility = 7, Intelligence = 5, Durability = 7
            };
        }

        [Test]
        public void TestBattleWinner()
        {
            var battle = new BattleSimulator();
            var winner = battle.Simulate(botA, botB);
            Assert.That(winner, Is.Not.Null);
        }

        [Test]
        public void TestRaceWinner()
        {
            var race = new RaceSimulator();
            var winner = race.SimulateRace(new List<GladiatorBot> { botA, botB });
            Assert.That(winner, Is.Not.Null);
        }

        [Test]
        public void TestObstacleCourseWinner()
        {
            var course = new ObstacleCourseSimulator();
            var winner = course.SimulateCourse(new List<GladiatorBot> { botA, botB });
            Assert.That(winner, Is.Not.Null);
        }
    }
}
