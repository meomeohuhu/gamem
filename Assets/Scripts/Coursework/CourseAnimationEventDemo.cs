using UnityEngine;

namespace SpaceExplore.Coursework
{
    public sealed class CourseAnimationEventDemo : MonoBehaviour
    {
        [SerializeField] private bool logOnStart = true;
        [SerializeField] private bool showOverlay = true;

        private void Start()
        {
            if (logOnStart) AnimationEventFunction("Hello World!");
        }

        private void OnGUI()
        {
            if (!showOverlay) return;

            GUI.Box(new Rect(20f, 20f, 360f, 82f), string.Empty);
            GUI.Label(new Rect(38f, 34f, 320f, 24f), "WEEK 7 - ANIMATION PRACTICE");
            GUI.Label(new Rect(38f, 60f, 320f, 24f), "Animation Event: Hello World!");
        }

        public void AnimationEventFunction(string message)
        {
            Debug.Log(string.IsNullOrWhiteSpace(message) ? "Hello World!" : message, this);
        }
    }
}
