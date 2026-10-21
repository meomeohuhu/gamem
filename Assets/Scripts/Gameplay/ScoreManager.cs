using TMPro;
using UnityEngine;

namespace SpaceExplore
{
    public sealed class ScoreManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text survivalTimeText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text powerUpText;
        private SpaceExplorerGameManager manager;
        private PlayerShooter shooter;

        private void OnEnable()
        {
            GameSession.ScoreChanged += Refresh;
            Refresh(GameSession.CurrentScore);
        }

        private void OnDisable() => GameSession.ScoreChanged -= Refresh;

        private void Update()
        {
            if (manager == null) manager = SpaceExplorerGameManager.Instance;
            if (shooter == null) shooter = FindAnyObjectByType<PlayerShooter>();
            if (manager == null) return;
            if (survivalTimeText != null) survivalTimeText.text = $"SURVIVAL  {FormatTime(manager.SurvivalTime)}";
            if (hpText != null) hpText.text = $"HP  {GameSession.CurrentHP}/{GameSession.MaxHP}";
            if (stageText != null) stageText.text = $"{manager.DifficultyStage} / 6";
            if (highScoreText != null) highScoreText.text = $"HIGH  {GameSession.HighScore}";
            if (powerUpText != null)
            {
                float remaining = manager.ActivePowerUpRemaining;
                string weaponLabel = shooter != null ? shooter.GetActiveUpgradeHudText() : string.Empty;
                powerUpText.text = remaining > 0f
                    ? $"POWER  {GetPowerLabel(manager)}{(weaponLabel.Length > 0 ? $"  |  WEAPON {weaponLabel}" : string.Empty)}  {remaining:0.0}s"
                    : weaponLabel.Length > 0 ? $"WEAPON  {weaponLabel}" : "POWER  —";
            }
        }

        public void Refresh(int score)
        {
            if (scoreText != null) scoreText.text = $"SCORE  {score}";
        }

        private static string GetPowerLabel(SpaceExplorerGameManager gameManager)
        {
            if (gameManager.ShieldActive) return "SHIELD";
            if (gameManager.RapidFireActive) return "RAPID FIRE";
            if (gameManager.DoubleScoreActive) return "x2 SCORE";
            return "POWER";
        }

        private static string FormatTime(float seconds)
        {
            int whole = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{whole / 60:00}:{whole % 60:00}";
        }
    }
}
