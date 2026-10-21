using UnityEngine;

namespace SpaceExplore
{
    public sealed class PickupFx : MonoBehaviour
    {
        private SpriteRenderer renderer;
        private float elapsed;
        private Color color;

        public static void Spawn(Sprite sprite, Vector3 position, Color tint)
        {
            if (sprite == null) return;
            GameObject root = new("Pickup FX");
            root.transform.position = position;
            root.transform.localScale = Vector3.one * .55f;
            PickupFx effect = root.AddComponent<PickupFx>();
            effect.renderer = root.AddComponent<SpriteRenderer>();
            effect.renderer.sprite = sprite;
            effect.renderer.sortingOrder = 9;
            effect.color = tint;
            effect.renderer.color = tint;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.localScale += Vector3.one * Time.deltaTime * 2.3f;
            if (renderer != null) renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(1f - elapsed * 2.3f));
            if (elapsed >= .45f) Destroy(gameObject);
        }
    }
}
