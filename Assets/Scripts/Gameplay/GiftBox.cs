using UnityEngine;

namespace SpaceExplore
{
    public enum GiftType { Green, Blue, Gold, DoubleScore, RapidFire, Shield, Health, Bomb }

    public sealed class GiftBox : MonoBehaviour
    {
        [SerializeField] private GiftType giftType = GiftType.Green;
        [SerializeField] private float rotationSpeed = 70f;
        [SerializeField] private Sprite[] icons = new Sprite[8];
        private bool collected;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer glowRenderer;

        public GiftType Type => giftType;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            CreateGlow();
            ApplyVisual();
        }

        private void Update() => transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        public void Configure(GiftType type)
        {
            giftType = type;
            ApplyVisual();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;
            if (other.GetComponent<PlayerController>() == null && other.GetComponentInParent<PlayerController>() == null) return;
            collected = true;
            SpaceExplorerGameManager.Instance?.ActivateGift(giftType);
            PickupFx.Spawn(spriteRenderer != null ? spriteRenderer.sprite : null, transform.position, GetColor());
            Destroy(gameObject);
        }

        private void ApplyVisual()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.sprite = icons != null && (int)giftType < icons.Length && icons[(int)giftType] != null ? icons[(int)giftType] : spriteRenderer.sprite;
            spriteRenderer.color = GetColor();
            transform.localScale = giftType is GiftType.Gold or GiftType.DoubleScore or GiftType.Bomb ? Vector3.one * 1.15f : Vector3.one * .8f;
            if (glowRenderer != null)
            {
                glowRenderer.sprite = spriteRenderer.sprite;
                glowRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, .18f);
            }
        }

        private Color GetColor()
        {
            return giftType switch
            {
                GiftType.Green => new Color(.2f, 1f, .35f), GiftType.Blue => new Color(.2f, .55f, 1f), GiftType.Gold => new Color(1f, .8f, .15f),
                GiftType.DoubleScore => new Color(.75f, .2f, 1f), GiftType.RapidFire => new Color(1f, .3f, .2f), GiftType.Shield => new Color(.2f, .9f, 1f),
                GiftType.Health => new Color(1f, .35f, .45f), GiftType.Bomb => new Color(1f, .45f, .1f), _ => Color.white
            };
        }

        private void CreateGlow()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;
            GameObject glow = new("Gift Glow");
            glow.transform.SetParent(transform, false);
            glow.transform.localScale = Vector3.one * 1.18f;
            glowRenderer = glow.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = spriteRenderer.sprite;
            glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
    }
}
