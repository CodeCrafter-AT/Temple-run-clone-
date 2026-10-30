using UnityEngine;
using UnityEngine.UI;
using TempleRunner.Player;
using TempleRunner.Scoring;

namespace TempleRunner.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Text deathReasonText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalDistanceText;
        [SerializeField] private Text finalCoinsText;
        [SerializeField] private Text highScoreText;

        private void Start()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnPlayerDied += SetDeathReason;
            }
        }

        private void OnDestroy()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnPlayerDied -= SetDeathReason;
            }
        }

        private void OnEnable()
        {
            if (ScoreManager.Instance != null)
            {
                if (finalScoreText != null) finalScoreText.text = "Score: " + ScoreManager.Instance.CurrentScore.ToString("N0");
                if (finalDistanceText != null) finalDistanceText.text = "Distance: " + Mathf.FloorToInt(ScoreManager.Instance.DistanceRun) + "m";
                if (finalCoinsText != null) finalCoinsText.text = "Coins: " + ScoreManager.Instance.SessionCoins;
                if (highScoreText != null) highScoreText.text = "High Score: " + ScoreManager.Instance.HighScore.ToString("N0");
            }
        }

        private void SetDeathReason(string reason)
        {
            if (deathReasonText != null)
            {
                deathReasonText.text = reason;
            }
        }
    }
}
