using System.Collections;
using UnityEngine;

namespace SpaceExplore
{
    public enum AsteroidType { Small, Medium, Large, Fast, Armored, Special }

    [System.Serializable]
    public sealed class AsteroidVariant
    {
        public AsteroidType type;
        [Min(1)] public int maxHealth = 1;
        [Min(1)] public int score = 5;
        [Min(0.25f)] public float size = 0.65f;
        [Min(0.1f)] public float minimumSpeed = 1.5f;
        [Min(0.1f)] public float maximumSpeed = 2.8f;
        public Color tint = Color.white;
        public float directionChangeInterval = 0f;
        public Sprite sprite;
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class AsteroidController : MonoBehaviour
    {
        [SerializeField] private AsteroidVariant defaultVariant = new();
        [SerializeField] private float maximumLifetime = 24f;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private AsteroidVariant variant;
        private int health;
        private bool collisionHandled;
        private float directionTimer;
        private float speedMultiplier = 1f;
        private Camera mainCamera;

        public AsteroidType Type => variant != null ? variant.type : AsteroidType.Small;
        public int CurrentHealth => health;
        public int MaxHealth => variant != null ? variant.maxHealth : 1;
        public int ScoreValue => variant != null ? variant.score : 5;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            body.gravityScale = 0f;
            body.freezeRotation = false;
            mainCamera = Camera.main;
            Configure(defaultVariant, 1f);
        }

        private void Start()
        {
            // Spawner may have already aimed this instance from an outside edge into the play area.
            // Only fall back to a random vector when no directed velocity was assigned.
            if (body.linearVelocity.sqrMagnitude < 0.01f) ApplyVelocity();
            Destroy(gameObject, maximumLifetime);
        }

        private void Update()
        {
            if (mainCamera != null && (Mathf.Abs(transform.position.x - mainCamera.transform.position.x) > mainCamera.orthographicSize * mainCamera.aspect + 5f || Mathf.Abs(transform.position.y - mainCamera.transform.position.y) > mainCamera.orthographicSize + 5f))
            {
                Destroy(gameObject);
                return;
            }
            if (collisionHandled || variant == null || variant.directionChangeInterval <= 0f) return;
            directionTimer -= Time.deltaTime;
            if (directionTimer <= 0f)
            {
                directionTimer = variant.directionChangeInterval;
                ApplyVelocity();
            }
        }

        public void Configure(AsteroidVariant configuration, float difficultySpeedMultiplier)
        {
            variant = configuration ?? defaultVariant ?? new AsteroidVariant();
            speedMultiplier = Mathf.Max(0.1f, difficultySpeedMultiplier);
            health = Mathf.Max(1, variant.maxHealth);
            transform.localScale = Vector3.one * Mathf.Max(0.25f, variant.size);
            if (spriteRenderer != null) spriteRenderer.color = variant.tint;
            if (spriteRenderer != null && variant.sprite != null) spriteRenderer.sprite = variant.sprite;
            directionTimer = variant.directionChangeInterval;
        }

        public void AimAt(Vector2 worldTarget)
        {
            Vector2 direction = (worldTarget - (Vector2)transform.position).normalized;
            if (direction.sqrMagnitude > .01f) SetTravelDirection(direction);
        }

        public void SetTravelDirection(Vector2 direction)
        {
            if (variant == null) return;
            float speed = Random.Range(variant.minimumSpeed, Mathf.Max(variant.minimumSpeed, variant.maximumSpeed)) * speedMultiplier;
            body.linearVelocity = direction.normalized * speed;
            body.angularVelocity = Random.Range(-110f, 110f);
        }

        public void TakeHit(int damage = 1)
        {
            if (collisionHandled) return;
            health -= Mathf.Max(1, damage);
            StartCoroutine(HitFlashRoutine());
            if (health <= 0) DestroyByGameplay();
        }

        public void DestroyByBomb()
        {
            if (collisionHandled) return;
            collisionHandled = true;
            SpaceExplorerGameManager.Instance?.AwardPoints(Mathf.CeilToInt(ScoreValue * .5f), false);
            SpawnBurst(new Color(1f, .45f, .15f, .8f), 1.8f);
            Destroy(gameObject);
        }

        private IEnumerator HitFlashRoutine()
        {
            if (spriteRenderer == null) yield break;
            Color previous = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            if (!collisionHandled && spriteRenderer != null) spriteRenderer.color = previous;
        }

        private void ApplyVelocity()
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.1f) direction = Vector2.down;
            float min = variant != null ? variant.minimumSpeed : 1.1f;
            float max = variant != null ? variant.maximumSpeed : 2.8f;
            body.linearVelocity = direction * Random.Range(min, Mathf.Max(min, max)) * speedMultiplier;
            body.angularVelocity = Random.Range(-110f, 110f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collisionHandled) return;
            if (collision.collider.GetComponent<PlayerController>() != null || collision.collider.GetComponentInParent<PlayerController>() != null) HandlePlayerCollision();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collisionHandled) return;
            if (other.GetComponent<PlayerController>() != null || other.GetComponentInParent<PlayerController>() != null) HandlePlayerCollision();
        }

        private void HandlePlayerCollision()
        {
            if (collisionHandled) return;
            collisionHandled = true;
            SpaceExplorerGameManager.Instance?.HandleAsteroidCollision(this);
            Destroy(gameObject);
        }

        private void DestroyByGameplay()
        {
            if (collisionHandled) return;
            collisionHandled = true;
            SpaceExplorerGameManager.Instance?.HandleAsteroidDestroyed(this);
            SpawnBurst(new Color(1f, .8f, .35f, .9f), 1.35f);
            Destroy(gameObject);
        }

        private void SpawnBurst(Color color, float scale)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;
            GameObject burst = new("Asteroid Burst FX");
            burst.transform.position = transform.position;
            burst.transform.localScale = transform.lossyScale * scale;
            SpriteRenderer renderer = burst.AddComponent<SpriteRenderer>();
            renderer.sprite = spriteRenderer.sprite;
            renderer.color = color;
            renderer.sortingOrder = spriteRenderer.sortingOrder + 2;
            Destroy(burst, .18f);
        }
    }
}
