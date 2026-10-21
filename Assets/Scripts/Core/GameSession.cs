using System;
using UnityEngine;

namespace SpaceExplore
{
    public static class GameSession
    {
        private const string HighScoreKey = "SpaceExplore.HighScore";
        public static int CurrentScore { get; private set; }
        public static int FinalScore { get; private set; }
        public static int HighScore { get; private set; }
        public static int CurrentHP { get; private set; }
        public static int MaxHP { get; private set; }
        public static event Action<int> ScoreChanged;
        public static event Action<int, int> HealthChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            CurrentScore = 0; FinalScore = 0; HighScore = 0; CurrentHP = 0; MaxHP = 0;
            ScoreChanged = null; HealthChanged = null;
        }

        public static void BeginRun(int maxHP = 3)
        {
            CurrentScore = 0; FinalScore = 0; MaxHP = Mathf.Max(1, maxHP); CurrentHP = MaxHP;
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            ScoreChanged?.Invoke(CurrentScore); HealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        public static void AddScore(int amount)
        {
            CurrentScore = Mathf.Max(0, CurrentScore + amount);
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
            ScoreChanged?.Invoke(CurrentScore);
        }

        public static bool RemoveHealth(int amount = 1)
        {
            if (CurrentHP <= 0) return true;
            CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(1, amount));
            HealthChanged?.Invoke(CurrentHP, MaxHP);
            return CurrentHP == 0;
        }

        public static void Heal(int amount = 1)
        {
            CurrentHP = Mathf.Clamp(CurrentHP + Mathf.Max(1, amount), 0, MaxHP);
            HealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        public static void FinishRun() => FinalScore = CurrentScore;
    }
}
