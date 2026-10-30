using System.Collections.Generic;
using UnityEngine;
using TempleRunner.Core;
using TempleRunner.Player;

namespace TempleRunner.Track
{
    /// <summary>
    /// Procedural endless track generator. Seamlessly connects modular tiles along
    /// dynamic world-space forward vectors, handles T-junction branching, and recycles tiles.
    /// </summary>
    public class TrackGenerator : MonoBehaviour
    {
        public static TrackGenerator Instance { get; private set; }

        [Header("Generation Settings")]
        [SerializeField] private int initialSegments = 12;
        [SerializeField] private int maxActiveSegments = 18;
        [SerializeField] private float despawnDistanceBehind = 25f;

        [Header("Tile Selection Weights")]
        [SerializeField] [Range(0, 100)] private int straightWeight = 75;
        [SerializeField] [Range(0, 100)] private int cornerWeight = 20;
        [SerializeField] [Range(0, 100)] private int junctionWeight = 5;
        [SerializeField] private int minStraightsBetweenTurns = 4;

        private int straightsSinceLastTurn = 0;
        private List<TrackSegment> activeSegments = new List<TrackSegment>();

        private Transform nextSocket;
        private Vector3 currentDirection = Vector3.forward;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted += ResetTrack;
            }

            ResetTrack();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestarted -= ResetTrack;
            }
        }

        public void ResetTrack()
        {
            // Return all active segments to pool
            foreach (var seg in activeSegments)
            {
                TrackPool.Instance?.ReturnSegment(seg);
            }
            activeSegments.Clear();

            nextSocket = transform;
            currentDirection = Vector3.forward;
            straightsSinceLastTurn = 0;

            // Generate safe initial runway (pure straights without obstacles)
            for (int i = 0; i < initialSegments; i++)
            {
                bool isSafeZone = (i < 5);
                SpawnSegment(SegmentType.Straight, isSafeZone);
            }
        }

        private void Update()
        {
            if (PlayerController.Instance == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            Vector3 playerPos = PlayerController.Instance.transform.position;

            // Check if leading segment is close enough to trigger next spawn
            if (activeSegments.Count > 0)
            {
                TrackSegment lead = activeSegments[activeSegments.Count - 1];
                float distToLead = Vector3.Distance(playerPos, lead.ExitSocket.position);

                if (distToLead < 80f && activeSegments.Count < maxActiveSegments)
                {
                    SpawnNextProceduralSegment();
                }
            }

            // Recycle trailing segments
            if (activeSegments.Count > 0)
            {
                TrackSegment oldest = activeSegments[0];
                Vector3 toPlayer = playerPos - oldest.ExitSocket.position;
                // If player has passed the segment by despawnDistanceBehind
                if (Vector3.Dot(toPlayer, oldest.transform.forward) > despawnDistanceBehind)
                {
                    activeSegments.RemoveAt(0);
                    TrackPool.Instance?.ReturnSegment(oldest);
                }
            }
        }

        private void SpawnNextProceduralSegment()
        {
            SegmentType chosenType = SegmentType.Straight;

            if (straightsSinceLastTurn >= minStraightsBetweenTurns)
            {
                int roll = Random.Range(0, straightWeight + cornerWeight + junctionWeight);

                if (roll < straightWeight)
                {
                    chosenType = SegmentType.Straight;
                    straightsSinceLastTurn++;
                }
                else if (roll < straightWeight + cornerWeight)
                {
                    chosenType = Random.value > 0.5f ? SegmentType.CornerLeft : SegmentType.CornerRight;
                    straightsSinceLastTurn = 0;
                }
                else
                {
                    chosenType = SegmentType.TJunction;
                    straightsSinceLastTurn = 0;
                }
            }
            else
            {
                chosenType = SegmentType.Straight;
                straightsSinceLastTurn++;
            }

            SpawnSegment(chosenType, false);
        }

        private void SpawnSegment(SegmentType type, bool isSafe)
        {
            TrackSegment segment = TrackPool.Instance != null ? TrackPool.Instance.GetSegment(type) : null;
            if (segment == null) return;

            // Align segment entry socket with previous exit socket
            if (activeSegments.Count == 0)
            {
                segment.transform.position = Vector3.zero;
                segment.transform.rotation = Quaternion.identity;
            }
            else
            {
                TrackSegment prev = activeSegments[activeSegments.Count - 1];
                Transform attachSocket = prev.ExitSocket;

                segment.transform.rotation = attachSocket.rotation;
                Vector3 offset = segment.transform.position - segment.EntrySocket.position;
                segment.transform.position = attachSocket.position + offset;
            }

            activeSegments.Add(segment);

            // Populate straightaways with obstacles & coins
            if (!isSafe && type == SegmentType.Straight)
            {
                Obstacles.ObstacleSpawner.Instance?.PopulateSegment(segment);
                Collectibles.CoinSpawner.Instance?.PopulateSegment(segment);
            }
        }
    }
}
