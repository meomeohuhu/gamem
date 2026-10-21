using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceExplore
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public void LoadMainMenu() => LoadScene("MainMenu");
        public void LoadGameplay() => LoadScene("Gameplay");
        public void LoadEndGame() => LoadScene("EndGame");

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;
            if (SceneManager.GetActiveScene().name == sceneName) return;
            SceneManager.LoadScene(sceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
