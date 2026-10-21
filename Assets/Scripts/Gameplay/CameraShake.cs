using UnityEngine;

namespace SpaceExplore
{
    public sealed class CameraShake : MonoBehaviour
    {
        private static CameraShake instance;
        private float remaining;
        private float strength;
        private Vector3 originalPosition;

        private void Awake()
        {
            instance = this;
            originalPosition = transform.localPosition;
        }

        public static void Shake(float duration, float amount)
        {
            if (instance == null) return;
            instance.remaining = Mathf.Max(instance.remaining, duration);
            instance.strength = Mathf.Max(instance.strength, amount);
        }

        private void LateUpdate()
        {
            if (remaining <= 0f)
            {
                transform.localPosition = originalPosition;
                return;
            }
            remaining -= Time.deltaTime;
            transform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * strength * Mathf.Clamp01(remaining * 8f);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
