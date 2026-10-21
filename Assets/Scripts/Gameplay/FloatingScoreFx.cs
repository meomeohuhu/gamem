using TMPro;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class FloatingScoreFx : MonoBehaviour
    {
        [SerializeField] private float lifetime = .65f;
        [SerializeField] private float riseSpeed = .9f;
        private TMP_Text text;
        private float elapsed;
        private Color startColor;

        public static void Spawn(int points, Vector3 position)
        {
            if (points <= 0) return;
            GameObject root = new("Floating Score FX");
            root.transform.position = position;
            TextMeshPro label = root.AddComponent<TextMeshPro>();
            label.text = $"+{points}";
            label.fontSize = .65f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(1f, .88f, .3f, 1f);
            root.AddComponent<FloatingScoreFx>();
        }

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            startColor = text != null ? text.color : Color.white;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (text != null) text.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Clamp01(1f - elapsed / lifetime));
            if (elapsed >= lifetime) Destroy(gameObject);
        }
    }
}
