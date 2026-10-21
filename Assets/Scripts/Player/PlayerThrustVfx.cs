using UnityEngine;

namespace SpaceExplore
{
    public sealed class PlayerThrustVfx : MonoBehaviour
    {
        [SerializeField] private Sprite flameSprite;
        private readonly SpriteRenderer[] flames = new SpriteRenderer[2];

        private void Awake()
        {
            for (int i = 0; i < flames.Length; i++)
            {
                GameObject flame = new($"Engine Flame {i}");
                flame.transform.SetParent(transform, false);
                flame.transform.localPosition = new Vector3(i == 0 ? -.18f : .18f, -.72f, 0f);
                flame.transform.localScale = new Vector3(.28f, .52f, 1f);
                SpriteRenderer renderer = flame.AddComponent<SpriteRenderer>();
                renderer.sprite = flameSprite;
                renderer.color = new Color(1f, .48f, .12f, .8f);
                renderer.sortingOrder = 4;
                flames[i] = renderer;
            }
        }

        private void Update()
        {
            float pulse = .8f + Mathf.Sin(Time.time * 18f) * .2f;
            for (int i = 0; i < flames.Length; i++)
            {
                if (flames[i] == null) continue;
                flames[i].transform.localScale = new Vector3(.28f, .52f * pulse, 1f);
                Color color = flames[i].color;
                color.a = .55f + pulse * .25f;
                flames[i].color = color;
            }
        }
    }
}
