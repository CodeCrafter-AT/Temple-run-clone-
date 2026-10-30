using UnityEngine;
using UnityEngine.UI;
using TempleRunner.Scoring;
using TempleRunner.Collectibles;

namespace TempleRunner.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Text Displays")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text coinText;
        [SerializeField] private Text multiplierText;

        [Header("Bonus Meter & Wings")]
        [SerializeField] private Image coinMeterFill;
        [SerializeField] private GameObject wingActiveIcon;

        private void Start()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreUpdated += UpdateScore;
                ScoreManager.Instance.OnCoinsUpdated += UpdateCoins;
                ScoreManager.Instance.OnDistanceUpdated += UpdateDistance;
            }

            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.OnWingPrimed += ShowWingIcon;
                PowerUpManager.Instance.OnWingConsumed += HideWingIcon;
            }

            HideWingIcon();
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreUpdated -= UpdateScore;
                ScoreManager.Instance.OnCoinsUpdated -= UpdateCoins;
                ScoreManager.Instance.OnDistanceUpdated -= UpdateDistance;
            }
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.OnWingPrimed -= ShowWingIcon;
                PowerUpManager.Instance.OnWingConsumed -= HideWingIcon;
            }
        }

        private void Update()
        {
            if (ScoreManager.Instance != null && coinMeterFill != null)
            {
                coinMeterFill.fillAmount = ScoreManager.Instance.CoinMeterNormalized;
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
        }

        private void UpdateCoins(int coins)
        {
            if (coinText != null) coinText.text = coins.ToString();
        }

        private void UpdateDistance(float meters)
        {
            if (distanceText != null) distanceText.text = Mathf.FloorToInt(meters) + "m";
            if (multiplierText != null && ScoreManager.Instance != null)
            {
                multiplierText.text = ScoreManager.Instance.Multiplier + "x";
            }
        }

        private void ShowWingIcon()
        {
            if (wingActiveIcon != null) wingActiveIcon.SetActive(true);
        }

        private void HideWingIcon()
        {
            if (wingActiveIcon != null) wingActiveIcon.SetActive(false);
        }
    }
}
