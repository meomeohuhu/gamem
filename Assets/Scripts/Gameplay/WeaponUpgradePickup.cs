using UnityEngine;

namespace SpaceExplore
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class WeaponUpgradePickup : MonoBehaviour
    {
        [SerializeField] private WeaponUpgradeType upgradeType = WeaponUpgradeType.RapidFire;
        [SerializeField] private float duration = 8f;
        [SerializeField] private Sprite[] icons;
        [SerializeField] private float rotationSpeed = 80f;

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer glowRenderer;
        private bool collected;

        public WeaponUpgradeType Type => upgradeType;
        public float Duration => duration;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            Collider2D pickupCollider = GetComponent<Collider2D>();
            if (pickupCollider != null) pickupCollider.isTrigger = true;
            CreateGlow();
            ApplyVisual();
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            if (glowRenderer != null)
                glowRenderer.color = new Color(GetColor().r, GetColor().g, GetColor().b, .14f + Mathf.Sin(Time.time * 5f) * .08f);
        }

        public void Configure(WeaponUpgradeType type, float upgradeDuration = -1f)
        {
            upgradeType = type;
            if (upgradeDuration > 0f) duration = upgradeDuration;
            collected = false;
            ApplyVisual();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;
            PlayerController player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
            if (player == null) return;
            PlayerShooter shooter = player.GetComponent<PlayerShooter>();
            if (shooter == null) return;

            collected = true;
            shooter.ActivateWeaponUpgrade(upgradeType, duration);
            PickupFx.Spawn(spriteRenderer != null ? spriteRenderer.sprite : null, transform.position, GetColor());
            Destroy(gameObject);
        }

        private void ApplyVisual()
        {
            if (spriteRenderer == null) return;
            if (icons != null && (int)upgradeType < icons.Length && icons[(int)upgradeType] != null)
                spriteRenderer.sprite = icons[(int)upgradeType];
            spriteRenderer.color = GetColor();
            transform.localScale = Vector3.one * 0.78f;
            if (glowRenderer != null)
            {
                glowRenderer.sprite = spriteRenderer.sprite;
                glowRenderer.color = new Color(GetColor().r, GetColor().g, GetColor().b, .2f);
            }
        }

        private Color GetColor()
        {
            return upgradeType switch
            {
                WeaponUpgradeType.RapidFire => new Color(1f, .3f, .18f),
                WeaponUpgradeType.DoubleShot => new Color(.35f, .8f, 1f),
                WeaponUpgradeType.SpreadShot => new Color(.75f, .35f, 1f),
                WeaponUpgradeType.PiercingShot => new Color(1f, .75f, .2f),
                WeaponUpgradeType.DamageUp => new Color(1f, .2f, .6f),
                _ => Color.white
            };
        }

        private void CreateGlow()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;
            GameObject glow = new("Weapon Upgrade Glow");
            glow.transform.SetParent(transform, false);
            glow.transform.localScale = Vector3.one * 1.22f;
            glowRenderer = glow.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = spriteRenderer.sprite;
            glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
    }
}
