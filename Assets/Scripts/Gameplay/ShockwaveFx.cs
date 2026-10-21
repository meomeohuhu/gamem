using UnityEngine;

namespace SpaceExplore
{
    public sealed class ShockwaveFx : MonoBehaviour
    {
        private SpriteRenderer renderer;
        private float elapsed;
        private float duration = .42f;
        private Color startColor;

        public static void Create(Sprite sprite, Vector3 position, Color color, float scale)
        {
            if (sprite == null) return;
            GameObject root = new("Shockwave FX");
            root.transform.position = position;
            root.transform.localScale = Vector3.one * scale;
            ShockwaveFx effect = root.AddComponent<ShockwaveFx>();
            effect.renderer = root.AddComponent<SpriteRenderer>();
            effect.renderer.sprite = sprite;
            effect.renderer.sortingOrder = 8;
            effect.startColor = color;
            effect.renderer.color = color;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            transform.localScale *= 1f + Time.deltaTime * 2.5f;
            if (renderer != null) renderer.color = new Color(startColor.r, startColor.g, startColor.b, (1f - normalized) * startColor.a);
            if (normalized >= 1f) Destroy(gameObject);
        }
    }
}
