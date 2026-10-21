using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceExplore
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float screenPadding = 0.35f;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private float damageFlashDuration = 0.16f;

        private Rigidbody2D body;
        private Vector2 input;
        private Collider2D playerCollider;
        private SpriteRenderer spriteRenderer;
        private GameObject shieldVisual;
        private Color baseColor = Color.white;
        public bool IsShielded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) baseColor = spriteRenderer.color;
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            CreateShieldVisual();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) { input = Vector2.zero; return; }
            float horizontal = (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed ? 1f : 0f);
            float vertical = (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed ? 1f : 0f);
            input = new Vector2(horizontal, vertical);
            if (input.sqrMagnitude > 1f) input.Normalize();
        }

        private void FixedUpdate()
        {
            Vector2 next = body.position + input * moveSpeed * Time.fixedDeltaTime;
            body.MovePosition(ClampToCamera(next));
        }

        public void SetShielded(bool enabled)
        {
            IsShielded = enabled;
            if (shieldVisual != null) shieldVisual.SetActive(enabled);
        }

        public void FlashDamage()
        {
            StopAllCoroutines();
            StartCoroutine(DamageFlashRoutine());
        }

        private IEnumerator DamageFlashRoutine()
        {
            if (spriteRenderer == null) yield break;
            float elapsed = 0f;
            while (elapsed < damageFlashDuration)
            {
                elapsed += Time.deltaTime;
                spriteRenderer.color = Mathf.FloorToInt(elapsed * 24f) % 2 == 0 ? Color.white : baseColor;
                yield return null;
            }
            spriteRenderer.color = baseColor;
        }

        private void CreateShieldVisual()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;
            shieldVisual = new GameObject("Shield Visual");
            shieldVisual.transform.SetParent(transform, false);
            shieldVisual.transform.localScale = Vector3.one * 1.45f;
            SpriteRenderer ring = shieldVisual.AddComponent<SpriteRenderer>();
            ring.sprite = spriteRenderer.sprite;
            ring.color = new Color(0.2f, 0.8f, 1f, 0.28f);
            ring.sortingOrder = spriteRenderer.sortingOrder + 1;
            shieldVisual.SetActive(false);
        }

        private Vector2 ClampToCamera(Vector2 position)
        {
            if (gameplayCamera == null || !gameplayCamera.orthographic) return position;
            float halfHeight = gameplayCamera.orthographicSize;
            float halfWidth = halfHeight * gameplayCamera.aspect;
            float radius = playerCollider != null ? Mathf.Max(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y) : 0.35f;
            Vector3 cameraPosition = gameplayCamera.transform.position;
            return new Vector2(
                Mathf.Clamp(position.x, cameraPosition.x - halfWidth + radius + screenPadding, cameraPosition.x + halfWidth - radius - screenPadding),
                Mathf.Clamp(position.y, cameraPosition.y - halfHeight + radius + screenPadding, cameraPosition.y + halfHeight - radius - screenPadding));
        }
    }
}
