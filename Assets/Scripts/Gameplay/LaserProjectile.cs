using UnityEngine;
using System.Collections.Generic;

namespace SpaceExplore
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class LaserProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damage = 1;
        private Rigidbody2D body;
        private SpriteRenderer glowRenderer;
        private Vector2 travelDirection = Vector2.up;
        private bool piercing;
        private readonly HashSet<AsteroidController> hitAsteroids = new();

        public int DamageValue => damage;
        public bool IsPiercing => piercing;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null)
            {
                GameObject glow = new("Laser Glow");
                glow.transform.SetParent(transform, false);
                glow.transform.localScale = Vector3.one * 1.8f;
                glowRenderer = glow.AddComponent<SpriteRenderer>();
                glowRenderer.sprite = renderer.sprite;
                glowRenderer.color = new Color(.1f, .8f, 1f, .3f);
                glowRenderer.sortingOrder = renderer.sortingOrder - 1;
            }
        }

        private void OnEnable() => Destroy(gameObject, lifetime);

        private void Start() => body.linearVelocity = travelDirection.normalized * speed;

        public void Initialize(Vector2 direction, int damageAmount, bool piercingShot)
        {
            travelDirection = direction.sqrMagnitude > .01f ? direction.normalized : Vector2.up;
            damage = Mathf.Max(1, damageAmount);
            piercing = piercingShot;
            transform.up = travelDirection;
        }

        private void Update()
        {
            if (glowRenderer != null) glowRenderer.color = new Color(.1f, .8f, 1f, .2f + Mathf.Sin(Time.time * 22f) * .08f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            AsteroidController asteroid = other.GetComponent<AsteroidController>() ?? other.GetComponentInParent<AsteroidController>();
            if (asteroid == null) return;
            if (!hitAsteroids.Add(asteroid)) return;
            asteroid.TakeHit(damage);
            if (!piercing) Destroy(gameObject);
        }
    }
}
