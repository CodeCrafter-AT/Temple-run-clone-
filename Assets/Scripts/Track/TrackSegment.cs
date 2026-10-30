using System.Collections.Generic;
using UnityEngine;
using TempleRunner.Player;

namespace TempleRunner.Track
{
    public enum SegmentType
    {
        Straight,
        CornerLeft,
        CornerRight,
        TJunction
    }

    /// <summary>
    /// Represents one modular 3D track tile. Contains entry/exit connection sockets,
    /// obstacle/coin spawn anchors, and intersection trigger volumes.
    /// </summary>
    public class TrackSegment : MonoBehaviour
    {
        [Header("Tile Metadata")]
        [SerializeField] private SegmentType segmentType = SegmentType.Straight;
        public SegmentType Type => segmentType;

        [Header("Connection Sockets")]
        [SerializeField] private Transform entrySocket;
        [SerializeField] private Transform exitSocket;
        [SerializeField] private Transform secondaryExitSocket; // For T-Junctions

        public Transform EntrySocket => entrySocket != null ? entrySocket : transform;
        public Transform ExitSocket => exitSocket != null ? exitSocket : transform;
        public Transform SecondaryExitSocket => secondaryExitSocket;

        [Header("Spawn Anchors")]
        [SerializeField] private List<Transform> obstacleSpawnPoints = new List<Transform>();
        [SerializeField] private List<Transform> coinSpawnPoints = new List<Transform>();

        public List<Transform> ObstacleSpawnPoints => obstacleSpawnPoints;
        public List<Transform> CoinSpawnPoints => coinSpawnPoints;

        [Header("Turn Trigger")]
        [SerializeField] private BoxCollider turnTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && turnTrigger != null)
            {
                bool canLeft = segmentType == SegmentType.CornerLeft || segmentType == SegmentType.TJunction;
                bool canRight = segmentType == SegmentType.CornerRight || segmentType == SegmentType.TJunction;
                Vector3 anchor = turnTrigger.bounds.center;

                PlayerController.Instance?.SetTurnZone(canLeft, canRight, anchor);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController.Instance?.ClearTurnZone();
            }
        }

        public void CleanForReuse()
        {
            // Deactivate or return spawned obstacles/coins
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.CompareTag("Obstacle") || child.CompareTag("Coin") || child.CompareTag("PowerUp"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}
