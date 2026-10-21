using System.Collections.Generic;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class AsteroidSpawner : MonoBehaviour
    {
        [SerializeField] private AsteroidController asteroidPrefab;
        [SerializeField] private DifficultyManager difficultyManager;
        [SerializeField] private List<AsteroidVariant> asteroidVariants = new();
        [SerializeField] private int initialCount = 8;
        [SerializeField] private float playerSafeRadius = 2.3f;
        [SerializeField] private float edgeSpawnOffset = 1.4f;
        private readonly List<GameObject> spawned = new();
        private float nextSpawnTime;

        public int ActiveAsteroidCount
        {
            get { spawned.RemoveAll(item => item == null); return spawned.Count; }
        }

        private void Start()
        {
            if (difficultyManager == null) difficultyManager = FindAnyObjectByType<DifficultyManager>();
            EnsureVariants();
            for (int i = 0; i < initialCount; i++) SpawnAsteroid(true);
            nextSpawnTime = Time.time + CurrentInterval;
        }

        private void Update()
        {
            if (difficultyManager == null || asteroidPrefab == null) return;
            if (Time.time < nextSpawnTime || ActiveAsteroidCount >= difficultyManager.CurrentSettings.maximumAsteroids) return;
            SpawnAsteroid(false);
            nextSpawnTime = Time.time + CurrentInterval;
        }

        private float CurrentInterval => difficultyManager != null ? difficultyManager.CurrentSettings.spawnInterval : 2.5f;

        private void SpawnAsteroid(bool anywhere)
        {
            if (asteroidPrefab == null || Camera.main == null || difficultyManager == null) return;
            Camera camera = Camera.main;
            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            Vector3 center = camera.transform.position;
            Vector3 position = Vector3.zero;
            Vector3 target = new Vector3(Random.Range(center.x - halfWidth + 1f, center.x + halfWidth - 1f), Random.Range(center.y - halfHeight + 1f, center.y + halfHeight - 1f), 0f);
            PlayerController player = FindAnyObjectByType<PlayerController>();
            for (int attempt = 0; attempt < 24; attempt++)
            {
                if (anywhere)
                {
                    position = new Vector3(Random.Range(center.x - halfWidth + .7f, center.x + halfWidth - .7f), Random.Range(center.y - halfHeight + .7f, center.y + halfHeight - .7f), 0f);
                }
                else
                {
                    int edge = Random.Range(0, 4);
                    position = edge switch
                    {
                        0 => new Vector3(center.x + Random.Range(-halfWidth, halfWidth), center.y + halfHeight + edgeSpawnOffset, 0f),
                        1 => new Vector3(center.x + Random.Range(-halfWidth, halfWidth), center.y - halfHeight - edgeSpawnOffset, 0f),
                        2 => new Vector3(center.x - halfWidth - edgeSpawnOffset, center.y + Random.Range(-halfHeight, halfHeight), 0f),
                        _ => new Vector3(center.x + halfWidth + edgeSpawnOffset, center.y + Random.Range(-halfHeight, halfHeight), 0f)
                    };
                }
                if (!anywhere || player == null || Vector2.Distance(position, player.transform.position) >= playerSafeRadius) break;
            }
            AsteroidVariant selected = SelectVariant();
            AsteroidController instance = Instantiate(asteroidPrefab, position, Quaternion.identity, transform);
            instance.Configure(selected, difficultyManager.GetSpeedMultiplier());
            instance.AimAt(target);
            spawned.Add(instance.gameObject);
        }

        private AsteroidVariant SelectVariant()
        {
            DifficultyStage stage = difficultyManager.CurrentSettings;
            float total = 0f;
            foreach (AsteroidVariant variant in asteroidVariants) total += stage.WeightFor(variant.type);
            float roll = Random.value * Mathf.Max(.01f, total);
            foreach (AsteroidVariant variant in asteroidVariants)
            {
                roll -= stage.WeightFor(variant.type);
                if (roll <= 0f) return variant;
            }
            return asteroidVariants[0];
        }

        private void EnsureVariants()
        {
            if (asteroidVariants != null && asteroidVariants.Count >= 6) return;
            asteroidVariants = new List<AsteroidVariant>
            {
                new() { type = AsteroidType.Small, maxHealth = 1, score = 5, size = .55f, minimumSpeed = 1.5f, maximumSpeed = 2.6f, tint = new Color(.65f, .7f, .82f) },
                new() { type = AsteroidType.Medium, maxHealth = 2, score = 10, size = .8f, minimumSpeed = 1.25f, maximumSpeed = 2.2f, tint = new Color(.55f, .6f, .75f) },
                new() { type = AsteroidType.Large, maxHealth = 5, score = 25, size = 1.2f, minimumSpeed = .8f, maximumSpeed = 1.45f, tint = new Color(.48f, .5f, .66f) },
                new() { type = AsteroidType.Fast, maxHealth = 1, score = 15, size = .6f, minimumSpeed = 3.1f, maximumSpeed = 4.4f, tint = new Color(1f, .5f, .28f) },
                new() { type = AsteroidType.Armored, maxHealth = 8, score = 40, size = 1.05f, minimumSpeed = .7f, maximumSpeed = 1.15f, tint = new Color(.42f, .55f, .72f) },
                new() { type = AsteroidType.Special, maxHealth = 12, score = 75, size = 1.25f, minimumSpeed = 1.2f, maximumSpeed = 2f, tint = new Color(.85f, .25f, 1f), directionChangeInterval = 1.2f }
            };
        }
    }
}
