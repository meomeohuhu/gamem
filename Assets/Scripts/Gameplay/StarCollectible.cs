using UnityEngine;

namespace SpaceExplore
{
    public sealed class StarCollectible : MonoBehaviour
    {
        [SerializeField] private int rotationSpeed = 90;
        private bool collected;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer glowRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                GameObject glow = new("Star Glow");
                glow.transform.SetParent(transform, false);
                glow.transform.localScale = Vector3.one * 1.3f;
                glowRenderer = glow.AddComponent<SpriteRenderer>();
                glowRenderer.sprite = spriteRenderer.sprite;
                glowRenderer.color = new Color(1f, .8f, .2f, .22f);
                glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            }
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            if (glowRenderer != null) glowRenderer.color = new Color(1f, .8f, .2f, .14f + Mathf.Sin(Time.time * 5f) * .08f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;
            if (other.GetComponent<PlayerController>() == null && other.GetComponentInParent<PlayerController>() == null) return;
            collected = true;
            SpaceExplorerGameManager.Instance?.CollectStar();
            PickupFx.Spawn(spriteRenderer != null ? spriteRenderer.sprite : null, transform.position, new Color(1f, .85f, .2f));
            Destroy(gameObject);
        }
    }
}
