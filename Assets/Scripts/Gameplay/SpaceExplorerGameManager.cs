using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceExplore
{
    public sealed class SpaceExplorerGameManager : MonoBehaviour
    {
        public static SpaceExplorerGameManager Instance { get; private set; }
        [SerializeField] private int maxHP = 3;
        [SerializeField] private float damageInvulnerability = .8f;
        [SerializeField] private string endGameScene = "EndGame";
        [SerializeField] private Sprite shockwaveSprite;
        private PlayerController player;
        private PlayerShooter shooter;
        private DifficultyManager difficultyManager;
        private float runTime;
        private float invulnerableUntil;
        private float doubleScoreUntil;
        private float rapidFireUntil;
        private float shieldUntil;
        private bool runEnded;

        public float SurvivalTime => runTime;
        public int DifficultyStage => difficultyManager != null ? difficultyManager.CurrentStage : 1;
        public bool DoubleScoreActive => Time.time < doubleScoreUntil;
        public bool RapidFireActive => Time.time < rapidFireUntil;
        public bool ShieldActive => Time.time < shieldUntil;
        public float ActivePowerUpRemaining
        {
            get
            {
                float remaining = 0f;
                if (DoubleScoreActive) remaining = Mathf.Max(remaining, doubleScoreUntil - Time.time);
                if (RapidFireActive) remaining = Mathf.Max(remaining, rapidFireUntil - Time.time);
                if (ShieldActive) remaining = Mathf.Max(remaining, shieldUntil - Time.time);
                if (shooter != null) remaining = Mathf.Max(remaining, shooter.ActiveUpgradeRemaining);
                return remaining;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            shooter = FindAnyObjectByType<PlayerShooter>();
            difficultyManager = FindAnyObjectByType<DifficultyManager>();
            GameSession.BeginRun(maxHP);
            runTime = 0f; runEnded = false; invulnerableUntil = 0f;
            doubleScoreUntil = 0f; rapidFireUntil = 0f; shieldUntil = 0f;
            difficultyManager?.ResetDifficulty();
            player?.SetShielded(false);
            shooter?.ResetWeaponUpgrades();
            shooter?.SetRapidFire(false);
        }

        private void Update()
        {
            if (runEnded) return;
            runTime += Time.deltaTime;
            difficultyManager?.SetElapsedTime(runTime);
            if (!RapidFireActive) shooter?.SetRapidFire(false);
            if (!ShieldActive) player?.SetShielded(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void CollectStar() => AwardPoints(10);

        public void HandleAsteroidDestroyed(AsteroidController asteroid)
        {
            if (asteroid != null) AwardPoints(asteroid.ScoreValue);
        }

        public void HandleAsteroidCollision(AsteroidController asteroid)
        {
            if (runEnded) return;
            if (ShieldActive || (player != null && player.IsShielded)) return;
            if (Time.time < invulnerableUntil) return;
            invulnerableUntil = Time.time + damageInvulnerability;
            player?.FlashDamage();
            CameraShake.Shake(.12f, .08f);
            if (GameSession.RemoveHealth()) EndRun();
        }

        // Kept for the Lab 1 editor smoke hook; gameplay collisions use the typed overload above.
        public void HandleAsteroidCollision() => EndRun();

        public void AwardPoints(int basePoints, bool applyMultiplier = true)
        {
            if (runEnded) return;
            int points = Mathf.Max(0, basePoints);
            if (applyMultiplier && DoubleScoreActive) points *= 2;
            GameSession.AddScore(points);
            FloatingScoreFx.Spawn(points, player != null ? player.transform.position : Vector3.zero);
        }

        public void ActivateGift(GiftType giftType)
        {
            if (runEnded) return;
            switch (giftType)
            {
                case GiftType.Green: AwardPoints(10); break;
                case GiftType.Blue: AwardPoints(25); break;
                case GiftType.Gold: AwardPoints(50); break;
                case GiftType.DoubleScore: doubleScoreUntil = Mathf.Max(doubleScoreUntil, Time.time) + 10f; break;
                case GiftType.RapidFire:
                    rapidFireUntil = Mathf.Max(rapidFireUntil, Time.time) + 8f;
                    shooter?.SetRapidFire(true);
                    break;
                case GiftType.Shield:
                    shieldUntil = Mathf.Max(shieldUntil, Time.time) + 6f;
                    player?.SetShielded(true);
                    break;
                case GiftType.Health: GameSession.Heal(1); break;
                case GiftType.Bomb: DetonateBomb(); break;
            }
        }

        public void ActivateWeaponUpgrade(WeaponUpgradeType type, float duration)
        {
            if (runEnded) return;
            shooter?.ActivateWeaponUpgrade(type, duration);
        }

        private void DetonateBomb()
        {
            if (player != null) ShockwaveFx.Create(shockwaveSprite, player.transform.position, new Color(.3f, .8f, 1f, .9f), 1.5f);
            CameraShake.Shake(.22f, .16f);
            AsteroidController[] asteroids = FindObjectsByType<AsteroidController>();
            foreach (AsteroidController asteroid in asteroids)
            {
                asteroid.DestroyByBomb();
            }
        }

        private void EndRun()
        {
            if (runEnded) return;
            runEnded = true;
            GameSession.FinishRun();
            SceneManager.LoadScene(endGameScene);
        }
    }
}
