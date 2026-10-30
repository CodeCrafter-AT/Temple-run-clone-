using UnityEngine;
using TempleRunner.Player;
using TempleRunner.Collectibles;

namespace TempleRunner.Obstacles
{
    public enum ObstacleType
    {
        JumpLow,    // Tree root, log, broken floor gap
        SlideHigh,  // Overhead stone arch, fire ring
        FatalWall   // Solid pillar or barricade
    }

    /// <summary>
    /// Handles collision detection for low jumpable hazards, overhead slide hazards,
    /// and solid obstacles, verifying active player state and power-up invulnerability.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private ObstacleType type = ObstacleType.JumpLow;
        [SerializeField] private bool canCauseStumbleOnEdge = true;
        [SerializeField] private string deathMessage = "Killed by obstacle!";

        private bool hasCollided = false;

        private void OnEnable()
        {
            hasCollided = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasCollided) return;
            if (!other.CompareTag("Player")) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            // Check Power-Up Invulnerability (Invisibility or Boost)
            if (PowerUpManager.Instance != null)
            {
                if (PowerUpManager.Instance.IsBoostActive || PowerUpManager.Instance.IsInvisibilityActive)
                {
                    return; // Invulnerable!
                }
            }

            switch (type)
            {
                case ObstacleType.JumpLow:
                    // If character controller is elevated/jumping, clearance is safe!
                    if (player.transform.position.y > 0.8f)
                    {
                        return; // Successfully jumped!
                    }
                    else if (canCauseStumbleOnEdge && Random.value > 0.6f)
                    {
                        hasCollided = true;
                        player.TriggerStumble();
                    }
                    else
                    {
                        hasCollided = true;
                        player.Die(deathMessage);
                    }
                    break;

                case ObstacleType.SlideHigh:
                    // If player is currently in knee-slide, clearance is safe!
                    if (player.IsSliding)
                    {
                        return; // Successfully slid underneath!
                    }
                    else
                    {
                        hasCollided = true;
                        player.Die(deathMessage);
                    }
                    break;

                case ObstacleType.FatalWall:
                    hasCollided = true;
                    player.Die(deathMessage);
                    break;
            }
        }
    }
}
