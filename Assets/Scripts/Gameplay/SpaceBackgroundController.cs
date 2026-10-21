using System.Collections.Generic;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class SpaceBackgroundController : MonoBehaviour
    {
        [SerializeField] private Sprite dustSprite;
        [SerializeField] private Sprite starSprite;
        [SerializeField] private int dustCount = 28;
        [SerializeField] private int starCount = 14;
        [SerializeField] private float driftSpeed = .12f;
        private readonly List<Transform> stars = new();
        private readonly List<SpriteRenderer> renderers = new();
        private Camera camera;

        private void Start()
        {
            camera = Camera.main;
            CreateStars(dustCount, dustSprite, .35f, .75f, new Color(.35f, .55f, 1f, .45f));
            CreateStars(starCount, starSprite, .08f, .2f, new Color(.7f, .85f, 1f, .75f));
        }

        private void Update()
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) return;
            for (int i = 0; i < stars.Count; i++)
            {
                Transform star = stars[i];
                star.position += Vector3.down * driftSpeed * (i % 3 + 1) * Time.deltaTime;
                if (star.position.y < camera.transform.position.y - camera.orthographicSize - .8f)
                    star.position = new Vector3(star.position.x, camera.transform.position.y + camera.orthographicSize + .8f, 4f);
                SpriteRenderer renderer = renderers[i];
                Color color = renderer.color;
                color.a = Mathf.Clamp01(color.a * (.82f + Mathf.Sin(Time.time * (1.5f + i * .11f) + i) * .18f));
                renderer.color = color;
            }
        }

        private void CreateStars(int count, Sprite sprite, float minScale, float maxScale, Color color)
        {
            if (sprite == null || camera == null) return;
            float width = camera.orthographicSize * camera.aspect;
            for (int i = 0; i < count; i++)
            {
                GameObject star = new("Parallax Star");
                star.transform.position = new Vector3(Random.Range(-width, width), Random.Range(-camera.orthographicSize, camera.orthographicSize), 4f);
                star.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
                SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = color;
                renderer.sortingOrder = -80;
                stars.Add(star.transform);
                renderers.Add(renderer);
            }
        }
    }
}
