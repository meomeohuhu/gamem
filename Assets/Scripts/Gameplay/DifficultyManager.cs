using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceExplore
{
    [Serializable]
    public sealed class DifficultyStage
    {
        public string label = "STAGE 1";
        public float startsAtSeconds;
        [Min(1)] public int maximumAsteroids = 5;
        [Min(0.1f)] public float spawnInterval = 2.5f;
        [Min(0.1f)] public float speedMultiplier = 1f;
        [Range(0f, 1f)] public float smallWeight = 0.65f;
        [Range(0f, 1f)] public float mediumWeight = 0.35f;
        [Range(0f, 1f)] public float largeWeight;
        [Range(0f, 1f)] public float fastWeight;
        [Range(0f, 1f)] public float armoredWeight;
        [Range(0f, 1f)] public float specialWeight;

        public float WeightFor(AsteroidType type)
        {
            return type switch
            {
                AsteroidType.Small => smallWeight,
                AsteroidType.Medium => mediumWeight,
                AsteroidType.Large => largeWeight,
                AsteroidType.Fast => fastWeight,
                AsteroidType.Armored => armoredWeight,
                AsteroidType.Special => specialWeight,
                _ => 0f
            };
        }
    }

    public sealed class DifficultyManager : MonoBehaviour
    {
        [SerializeField] private List<DifficultyStage> stages = new();
        [SerializeField] private float maximumSpeedMultiplier = 2.2f;
        private float elapsedTime;
        private int currentStageIndex;

        public float ElapsedTime => elapsedTime;
        public int CurrentStage => currentStageIndex + 1;
        public DifficultyStage CurrentSettings => stages[Mathf.Clamp(currentStageIndex, 0, stages.Count - 1)];
        public int CurrentMaximumAsteroids => CurrentSettings.maximumAsteroids;
        public float CurrentSpawnInterval => CurrentSettings.spawnInterval;
        public IReadOnlyList<DifficultyStage> Stages => stages;

        private void Awake()
        {
            EnsureDefaults();
            ResetDifficulty();
        }

        public void ResetDifficulty()
        {
            elapsedTime = 0f;
            currentStageIndex = 0;
        }

        public void SetElapsedTime(float time)
        {
            elapsedTime = Mathf.Max(0f, time);
            currentStageIndex = GetStageIndexForTime(elapsedTime);
        }

        public float GetSpeedMultiplier()
        {
            return Mathf.Min(maximumSpeedMultiplier, CurrentSettings.speedMultiplier);
        }

        public static int GetStageIndexForTime(float time)
        {
            if (time >= 300f) return 5;
            if (time >= 180f) return 4;
            if (time >= 120f) return 3;
            if (time >= 60f) return 2;
            if (time >= 30f) return 1;
            return 0;
        }

        public void EnsureDefaults()
        {
            if (stages != null && stages.Count == 6 && stages[0].maximumAsteroids >= 12 && stages[5].maximumAsteroids >= 80 && stages[0].spawnInterval <= .9f) return;
            stages = new List<DifficultyStage>
            {
                new() { label = "STAGE 1", startsAtSeconds = 0f, maximumAsteroids = 12, spawnInterval = .9f, speedMultiplier = 1f, smallWeight = .62f, mediumWeight = .30f, largeWeight = .08f },
                new() { label = "STAGE 2", startsAtSeconds = 30f, maximumAsteroids = 20, spawnInterval = .65f, speedMultiplier = 1.08f, smallWeight = .42f, mediumWeight = .32f, largeWeight = .18f, fastWeight = .08f },
                new() { label = "STAGE 3", startsAtSeconds = 60f, maximumAsteroids = 30, spawnInterval = .45f, speedMultiplier = 1.18f, smallWeight = .28f, mediumWeight = .28f, largeWeight = .18f, fastWeight = .18f, armoredWeight = .08f },
                new() { label = "STAGE 4", startsAtSeconds = 120f, maximumAsteroids = 45, spawnInterval = .3f, speedMultiplier = 1.3f, smallWeight = .20f, mediumWeight = .22f, largeWeight = .18f, fastWeight = .18f, armoredWeight = .16f, specialWeight = .06f },
                new() { label = "STAGE 5", startsAtSeconds = 180f, maximumAsteroids = 60, spawnInterval = .2f, speedMultiplier = 1.48f, smallWeight = .14f, mediumWeight = .18f, largeWeight = .18f, fastWeight = .18f, armoredWeight = .18f, specialWeight = .14f },
                new() { label = "STAGE 6", startsAtSeconds = 300f, maximumAsteroids = 80, spawnInterval = .15f, speedMultiplier = 1.7f, smallWeight = .09f, mediumWeight = .13f, largeWeight = .18f, fastWeight = .19f, armoredWeight = .21f, specialWeight = .20f }
            };
        }
    }
}
