using NUnit.Framework;
using UnityEngine;

namespace SpaceExplore.Tests
{
    public sealed class SpaceExplorerGameplayTests
    {
        [TestCase(0f, 1)]
        [TestCase(29.99f, 1)]
        [TestCase(30f, 2)]
        [TestCase(60f, 3)]
        [TestCase(120f, 4)]
        [TestCase(180f, 5)]
        [TestCase(300f, 6)]
        public void DifficultyStageBoundariesAreCorrect(float seconds, int expectedStage)
        {
            Assert.AreEqual(expectedStage, DifficultyManager.GetStageIndexForTime(seconds) + 1);
        }

        [Test]
        public void DifficultySettingsMatchSurvivalTargets()
        {
            GameObject root = new("Difficulty Test");
            DifficultyManager manager = root.AddComponent<DifficultyManager>();
            manager.EnsureDefaults();
            Assert.AreEqual(6, manager.Stages.Count);
            int[] caps = { 12, 20, 30, 45, 60, 80 };
            float[] intervals = { .9f, .65f, .45f, .3f, .2f, .15f };
            for (int i = 0; i < caps.Length; i++)
            {
                Assert.AreEqual(caps[i], manager.Stages[i].maximumAsteroids, $"Stage {i + 1} cap");
                Assert.AreEqual(intervals[i], manager.Stages[i].spawnInterval, .001f, $"Stage {i + 1} interval");
            }
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SessionHealthAndHighScoreResetBetweenRuns()
        {
            GameSession.BeginRun(3);
            GameSession.AddScore(75);
            Assert.AreEqual(3, GameSession.CurrentHP);
            Assert.IsFalse(GameSession.RemoveHealth());
            Assert.AreEqual(2, GameSession.CurrentHP);
            GameSession.BeginRun(3);
            Assert.AreEqual(0, GameSession.CurrentScore);
            Assert.AreEqual(3, GameSession.CurrentHP);
        }

        [Test]
        public void AsteroidVariantDataContainsSixConfiguredTypes()
        {
            AsteroidVariant[] variants =
            {
                new() { type = AsteroidType.Small, maxHealth = 1, score = 5 },
                new() { type = AsteroidType.Medium, maxHealth = 2, score = 10 },
                new() { type = AsteroidType.Large, maxHealth = 5, score = 25 },
                new() { type = AsteroidType.Fast, maxHealth = 1, score = 15 },
                new() { type = AsteroidType.Armored, maxHealth = 8, score = 40 },
                new() { type = AsteroidType.Special, maxHealth = 12, score = 75 }
            };
            Assert.AreEqual(6, variants.Length);
            Assert.AreEqual(75, variants[5].score);
        }

        [Test]
        public void GiftEnumContainsEightGameplayRewards()
        {
            Assert.AreEqual(8, System.Enum.GetValues(typeof(GiftType)).Length);
        }
    }
}
