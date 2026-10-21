using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class StarSpawner : MonoBehaviour
    {
        [SerializeField] private StarCollectible starPrefab;
        [SerializeField] private int targetCount = 5;
        [SerializeField] private float replenishInterval = 1.5f;

        private readonly List<GameObject> spawned = new();

        private void Start()
        {
            Replenish();
            StartCoroutine(ReplenishLoop());
        }

        private IEnumerator ReplenishLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(replenishInterval);
                Replenish();
            }
        }

        private void Replenish()
        {
            spawned.RemoveAll(item => item == null);
            while (spawned.Count < targetCount) SpawnStar();
        }

        private void SpawnStar()
        {
            if (starPrefab == null || Camera.main == null) return;
            Camera camera = Camera.main;
            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            Vector3 center = camera.transform.position;
            Vector3 position = new(
                Random.Range(center.x - halfWidth + 0.8f, center.x + halfWidth - 0.8f),
                Random.Range(center.y - halfHeight + 0.8f, center.y + halfHeight - 0.8f),
                0f);
            spawned.Add(Instantiate(starPrefab, position, Quaternion.identity, transform).gameObject);
        }
    }
}
