using UnityEngine;

namespace SpaceExplore
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject instructionsPanel;

        public void ShowInstructions()
        {
            if (instructionsPanel != null) instructionsPanel.SetActive(true);
        }

        public void HideInstructions()
        {
            if (instructionsPanel != null) instructionsPanel.SetActive(false);
        }
    }
}
