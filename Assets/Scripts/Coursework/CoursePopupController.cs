using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceExplore.Coursework
{
    public sealed class CoursePopupController : MonoBehaviour
    {
        [SerializeField] private GameObject popup;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private string nextScene = "Gameplay";

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(NextScene);
            if (restartButton != null) restartButton.onClick.AddListener(RestartScene);
            if (popup != null) popup.SetActive(false);
        }

        public void ShowPopup()
        {
            if (popup != null) popup.SetActive(true);
        }

        public void NextScene()
        {
            if (Application.CanStreamedLevelBeLoaded(nextScene))
                SceneManager.LoadScene(nextScene);
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
