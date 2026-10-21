using System.Collections.Generic;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class WeaponUpgradeSpawner : MonoBehaviour
    {
        [SerializeField] private WeaponUpgradePickup upgradePrefab;
        [SerializeField] private GameObject upgradePrefabAsset;
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private int maximumActive = 2;
        [SerializeField] private float spawnInterval = 15f;
        [SerializeField] private float firstSpawnDelay = 6f;
        [SerializeField] private float playerSafeRadius = 2.4f;
        [SerializeField] private float rapidFireWeight = 24f;
        [SerializeField] private float doubleShotWeight = 20f;
        [SerializeField] private float spreadShotWeight = 20f;
        [SerializeField] private float piercingShotWeight = 18f;
        [SerializeField] private float damageUpWeight = 18f;

        private readonly List<GameObject> spawned = new();
        private float nextSpawnTime;

        private void Start()
        {
            if (upgradePrefab == null && upgradePrefabAsset != null)
                upgradePrefab = upgradePrefabAsset.GetComponent<WeaponUpgradePickup>();
            if (upgradePrefab == null) upgradePrefab = CreateRuntimeUpgradePrefab();
            nextSpawnTime = Time.time + Mathf.Max(1f, firstSpawnDelay);
        }

        private void Update()
        {
            spawned.RemoveAll(item => item == null);
            if (upgradePrefab == null || spawned.Count >= maximumActive || Time.time < nextSpawnTime) return;
            SpawnUpgrade();
            nextSpawnTime = Time.time + Mathf.Max(4f, spawnInterval);
        }

        private void SpawnUpgrade()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            Vector3 center = camera.transform.position;
            PlayerController player = FindAnyObjectByType<PlayerController>();
            Vector3 position = center;
            for (int attempt = 0; attempt < 24; attempt++)
            {
                position = new Vector3(
                    Random.Range(center.x - halfWidth + .8f, center.x + halfWidth - .8f),
                    Random.Range(center.y - halfHeight + .8f, center.y + halfHeight - .8f), 0f);
                if (player == null || Vector2.Distance(position, player.transform.position) >= playerSafeRadius) break;
            }

            WeaponUpgradePickup instance = Instantiate(upgradePrefab, position, Quaternion.identity, transform);
            instance.gameObject.SetActive(true);
            WeaponUpgradeType type = ChooseUpgradeType();
            instance.Configure(type, GetDuration(type));
            spawned.Add(instance.gameObject);
        }

        private WeaponUpgradeType ChooseUpgradeType()
        {
            float total = rapidFireWeight + doubleShotWeight + spreadShotWeight + piercingShotWeight + damageUpWeight;
            float roll = Random.value * Mathf.Max(.01f, total);
            WeaponUpgradeType[] types = {
                WeaponUpgradeType.RapidFire, WeaponUpgradeType.DoubleShot, WeaponUpgradeType.SpreadShot,
                WeaponUpgradeType.PiercingShot, WeaponUpgradeType.DamageUp
            };
            float[] weights = { rapidFireWeight, doubleShotWeight, spreadShotWeight, piercingShotWeight, damageUpWeight };
            for (int i = 0; i < types.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return types[i];
            }
            return WeaponUpgradeType.RapidFire;
        }

        private static float GetDuration(WeaponUpgradeType type)
        {
            return type switch
            {
                WeaponUpgradeType.RapidFire => 8f,
                WeaponUpgradeType.DoubleShot => 10f,
                WeaponUpgradeType.SpreadShot => 10f,
                WeaponUpgradeType.PiercingShot => 8f,
                WeaponUpgradeType.DamageUp => 12f,
                _ => 8f
            };
        }

        private WeaponUpgradePickup CreateRuntimeUpgradePrefab()
        {
            GameObject root = new("Weapon Upgrade Runtime Template");
            root.SetActive(false);
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = fallbackSprite;
            renderer.sortingOrder = 4;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            return root.AddComponent<WeaponUpgradePickup>();
        }
    }
}
