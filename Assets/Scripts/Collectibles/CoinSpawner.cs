using System.Collections.Generic;
using UnityEngine;
using TempleRunner.Track;

namespace TempleRunner.Collectibles
{
    public class CoinSpawner : MonoBehaviour
    {
        public static CoinSpawner Instance { get; private set; }

        [SerializeField] private GameObject coinPrefab;
        [SerializeField] [Range(0f, 1f)] private float spawnProbability = 0.85f;
        [SerializeField] private int coinsPerRibbon = 5;
        [SerializeField] private float coinSpacing = 1.8f;

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
            if (coinPrefab == null) return;
            if (Random.value > spawnProbability) return;

            Transform anchor = segment.transform;
            Vector3 forward = segment.transform.forward;
            Vector3 right = segment.transform.right;

            // Pick lane: -1 (left), 0 (center), or +1 (right)
            int lane = Random.Range(-1, 2);
            float laneX = lane * 1.6f;

            Vector3 startPos = anchor.position + (right * laneX) + (Vector3.up * 0.6f);

            for (int i = 0; i < coinsPerRibbon; i++)
            {
                Vector3 coinPos = startPos + (forward * (i * coinSpacing));
                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity, segment.transform);
                coin.tag = "Coin";
            }
        }
    }
}
