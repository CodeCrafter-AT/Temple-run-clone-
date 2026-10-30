using UnityEngine;

namespace TempleRunner.Collectibles
{
    public enum PowerUpType
    {
        MegaCoin,
        CoinMagnet,
        Boost,
        Invisibility,
        ResurrectionWing
    }

    public class PowerUp : MonoBehaviour
    {
        [SerializeField] private PowerUpType type = PowerUpType.CoinMagnet;
        [SerializeField] private float durationSeconds = 12f;
        [SerializeField] private int megaCoinAmount = 50;

        private void Update()
        {
            transform.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PowerUpManager.Instance?.ActivatePowerUp(type, durationSeconds, megaCoinAmount);
                gameObject.SetActive(false);
            }
        }
    }
}
