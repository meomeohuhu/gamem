using TMPro;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class EndGameUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text highScoreText;

        private void Start()
        {
            if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE: {GameSession.FinalScore}";
            if (highScoreText != null) highScoreText.text = $"HIGH SCORE: {GameSession.HighScore}";
        }
    }
}
