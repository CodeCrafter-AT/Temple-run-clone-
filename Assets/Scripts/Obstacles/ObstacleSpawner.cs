using System.Collections.Generic;
using UnityEngine;
using TempleRunner.Track;

namespace TempleRunner.Obstacles
{
    public class ObstacleSpawner : MonoBehaviour
    {
        public static ObstacleSpawner Instance { get; private set; }

        [Header("Obstacle Prefabs")]
        [SerializeField] private List<GameObject> lowJumpPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> highSlidePrefabs = new List<GameObject>();

        [Header("Spawn Chance")]
        [SerializeField] [Range(0f, 1f)] private float spawnProbability = 0.65f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PopulateSegment(TrackSegment segment)
        {
            if (segment.ObstacleSpawnPoints.Count == 0) return;
            if (Random.value > spawnProbability) return;

            Transform anchor = segment.ObstacleSpawnPoints[Random.Range(0, segment.ObstacleSpawnPoints.Count)];

            bool isJump = Random.value > 0.5f;
            List<GameObject> pool = (isJump && lowJumpPrefabs.Count > 0) ? lowJumpPrefabs : highSlidePrefabs;

            if (pool != null && pool.Count > 0)
            {
                GameObject prefab = pool[Random.Range(0, pool.Count)];
                GameObject obstacle = Instantiate(prefab, anchor.position, anchor.rotation, segment.transform);
                obstacle.tag = "Obstacle";
            }
        }
    }
}
