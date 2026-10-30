using System;
using UnityEngine;
using TempleRunner.Core;
using TempleRunner.Player;

namespace TempleRunner.Scoring
{
    /// <summary>
    /// Handles continuous distance calculation, coin banking, score multiplier progression,
    /// 100-coin bonus meter, and persistent PlayerPrefs high scores.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string PREF_HIGH_SCORE = "TempleRunner_HighScore";
        private const string PREF_TOTAL_COINS = "TempleRunner_TotalCoins";

        [Header("Live Metrics")]
        private float distanceRunMeters = 0f;
        private int sessionCoins = 0;
        private int currentScore = 0;
        private int scoreMultiplier = 1;
        private int coinMeterProgress = 0; // Fills up to 100

        public float DistanceRun => distanceRunMeters;
        public int SessionCoins => sessionCoins;
        public int CurrentScore => currentScore;
        public int Multiplier => scoreMultiplier;
        public float CoinMeterNormalized => coinMeterProgress / 100f;

        [Header("Persistence")]
        public int HighScore { get; private set; }
        public int TotalBankedCoins { get; private set; }

        public event Action<int> OnScoreUpdated;
        public event Action<int> OnCoinsUpdated;
        public event Action<float> OnDistanceUpdated;
        public event Action<int> OnBonusMeterFilled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadPersistentData();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted += ResetSession;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnStumble += HandleStumble;
            }

            ResetSession();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted -= ResetSession;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnStumble -= HandleStumble;
            }
        }

        private void LoadPersistentData()
        {
            HighScore = PlayerPrefs.GetInt(PREF_HIGH_SCORE, 0);
            TotalBankedCoins = PlayerPrefs.GetInt(PREF_TOTAL_COINS, 0);
        }

        private void SavePersistentData()
        {
            if (currentScore > HighScore)
            {
                HighScore = currentScore;
                PlayerPrefs.SetInt(PREF_HIGH_SCORE, HighScore);
            }

            TotalBankedCoins += sessionCoins;
            PlayerPrefs.SetInt(PREF_TOTAL_COINS, TotalBankedCoins);
            PlayerPrefs.Save();
        }

        public void ResetSession()
        {
            distanceRunMeters = 0f;
            sessionCoins = 0;
            currentScore = 0;
            coinMeterProgress = 0;
            scoreMultiplier = 1;

            OnDistanceUpdated?.Invoke(distanceRunMeters);
            OnCoinsUpdated?.Invoke(sessionCoins);
            OnScoreUpdated?.Invoke(currentScore);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (PlayerController.Instance != null)
            {
                float speed = PlayerController.Instance.CurrentSpeed;
                distanceRunMeters += speed * Time.deltaTime;
                OnDistanceUpdated?.Invoke(distanceRunMeters);

                // Score formula: (Distance + Coins * 5) * Multiplier
                currentScore = Mathf.FloorToInt((distanceRunMeters + (sessionCoins * 5f)) * scoreMultiplier);
                OnScoreUpdated?.Invoke(currentScore);
            }
        }

        public void AddCoins(int amount)
        {
            sessionCoins += amount;
            coinMeterProgress += amount;

            if (coinMeterProgress >= 100)
            {
                coinMeterProgress -= 100;
                int bonusPoints = 250 * scoreMultiplier;
                currentScore += bonusPoints;
                OnBonusMeterFilled?.Invoke(bonusPoints);
            }

            OnCoinsUpdated?.Invoke(sessionCoins);
            OnScoreUpdated?.Invoke(currentScore);
        }

        public void IncrementMultiplier()
        {
            scoreMultiplier++;
        }

        private void HandleStumble()
        {
            // Tripping resets the coin bonus meter
            coinMeterProgress = 0;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                SavePersistentData();
            }
        }
    }
}
