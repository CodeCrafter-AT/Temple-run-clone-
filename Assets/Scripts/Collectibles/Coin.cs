using UnityEngine;
using TempleRunner.Scoring;
using TempleRunner.Player;

namespace TempleRunner.Collectibles
{
    public enum CoinTier
    {
        Yellow = 1,
        Red = 2,
        Blue = 3
    }

    /// <summary>
    /// Collectible coin supporting continuous rotation, magnetic attraction toward
    /// the player when Coin Magnet is active, and multi-tier coin values.
    /// </summary>
    public class Coin : MonoBehaviour
    {
        [SerializeField] private CoinTier tier = CoinTier.Yellow;
        [SerializeField] private float rotationSpeed = 160f;
        [SerializeField] private float magnetAttractionSpeed = 18f;

        private bool isCollected = false;

        private void OnEnable()
        {
            isCollected = false;
        }

        private void Update()
        {
            // Continuous spin
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Magnetic attraction check
            if (!isCollected && PowerUpManager.Instance != null && PowerUpManager.Instance.IsMagnetActive)
            {
                if (PlayerController.Instance != null)
                {
                    Vector3 targetPos = PlayerController.Instance.transform.position + Vector3.up * 0.8f;
                    float dist = Vector3.Distance(transform.position, targetPos);
                    if (dist < 12.0f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, targetPos, magnetAttractionSpeed * Time.deltaTime);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            if (other.CompareTag("Player"))
            {
                isCollected = true;
                ScoreManager.Instance?.AddCoins((int)tier);
                gameObject.SetActive(false);
            }
        }

        public void SetTier(CoinTier newTier)
        {
            tier = newTier;
            // Visual color could be updated via MaterialPropertyBlock
        }
    }
}
