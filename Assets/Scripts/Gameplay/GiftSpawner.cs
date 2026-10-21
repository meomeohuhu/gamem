using System.Collections.Generic;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class GiftSpawner : MonoBehaviour
    {
        [SerializeField] private GiftBox giftPrefab;
        [SerializeField] private GameObject giftPrefabAsset;
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private int maximumActive = 2;
        [SerializeField] private float spawnInterval = 10f;
        [SerializeField] private float playerSafeRadius = 1.8f;
        [SerializeField] private float greenWeight = 34f;
        [SerializeField] private float blueWeight = 24f;
        [SerializeField] private float goldWeight = 7f;
        [SerializeField] private float doubleScoreWeight = 9f;
        [SerializeField] private float rapidFireWeight = 9f;
        [SerializeField] private float shieldWeight = 8f;
        [SerializeField] private float healthWeight = 6f;
        [SerializeField] private float bombWeight = 3f;
        private readonly List<GameObject> spawned = new();
        private float nextSpawnTime;

        private void Start()
        {
            if (giftPrefab == null && giftPrefabAsset != null) giftPrefab = giftPrefabAsset.GetComponent<GiftBox>();
            if (giftPrefab == null) giftPrefab = CreateRuntimeGiftPrefab();
            nextSpawnTime = Time.time + 3f;
        }

        private void Update()
        {
            spawned.RemoveAll(item => item == null);
            if (giftPrefab == null || spawned.Count >= maximumActive || Time.time < nextSpawnTime) return;
            SpawnGift();
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void SpawnGift()
        {
            if (Camera.main == null) return;
            Camera camera = Camera.main;
            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            Vector3 center = camera.transform.position;
            PlayerController player = FindAnyObjectByType<PlayerController>();
            Vector3 position = center;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                position = new Vector3(Random.Range(center.x - halfWidth + 1f, center.x + halfWidth - 1f), Random.Range(center.y - halfHeight + 1f, center.y + halfHeight - 1f), 0f);
                if (player == null || Vector2.Distance(position, player.transform.position) >= playerSafeRadius) break;
            }
            GiftBox instance = Instantiate(giftPrefab, position, Quaternion.identity, transform);
            instance.gameObject.SetActive(true);
            instance.Configure(ChooseGiftType());
            spawned.Add(instance.gameObject);
        }

        private GiftType ChooseGiftType()
        {
            float total = greenWeight + blueWeight + goldWeight + doubleScoreWeight + rapidFireWeight + shieldWeight + healthWeight + bombWeight;
            float roll = Random.value * Mathf.Max(.01f, total);
            GiftType[] types = { GiftType.Green, GiftType.Blue, GiftType.Gold, GiftType.DoubleScore, GiftType.RapidFire, GiftType.Shield, GiftType.Health, GiftType.Bomb };
            float[] weights = { greenWeight, blueWeight, goldWeight, doubleScoreWeight, rapidFireWeight, shieldWeight, healthWeight, bombWeight };
            for (int i = 0; i < types.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return types[i];
            }
            return GiftType.Green;
        }

        private GiftBox CreateRuntimeGiftPrefab()
        {
            GameObject root = new("Gift Box Runtime Template");
            root.SetActive(false);
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = fallbackSprite;
            renderer.sortingOrder = 3;
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            GiftBox gift = root.AddComponent<GiftBox>();
            return gift;
        }
    }
}
