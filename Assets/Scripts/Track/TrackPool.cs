using System.Collections.Generic;
using UnityEngine;

namespace TempleRunner.Track
{
    /// <summary>
    /// Zero-GC object pooler for modular 3D track segments.
    /// </summary>
    public class TrackPool : MonoBehaviour
    {
        public static TrackPool Instance { get; private set; }

        [System.Serializable]
        public struct PoolItem
        {
            public SegmentType type;
            public GameObject prefab;
            public int initialPoolSize;
        }

        [SerializeField] private List<PoolItem> poolItems = new List<PoolItem>();
        private Dictionary<SegmentType, Queue<TrackSegment>> pools = new Dictionary<SegmentType, Queue<TrackSegment>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var item in poolItems)
            {
                Queue<TrackSegment> queue = new Queue<TrackSegment>();
                for (int i = 0; i < item.initialPoolSize; i++)
                {
                    GameObject obj = Instantiate(item.prefab, transform);
                    obj.SetActive(false);
                    TrackSegment segment = obj.GetComponent<TrackSegment>();
                    queue.Enqueue(segment);
                }
                pools[item.type] = queue;
            }
        }

        public TrackSegment GetSegment(SegmentType type)
        {
            if (pools.ContainsKey(type) && pools[type].Count > 0)
            {
                TrackSegment segment = pools[type].Dequeue();
                segment.gameObject.SetActive(true);
                return segment;
            }

            // Fallback instantiate if pool exhausted
            var item = poolItems.Find(p => p.type == type);
            if (item.prefab != null)
            {
                GameObject obj = Instantiate(item.prefab, transform);
                return obj.GetComponent<TrackSegment>();
            }

            return null;
        }

        public void ReturnSegment(TrackSegment segment)
        {
            if (segment == null) return;
            segment.CleanForReuse();
            segment.gameObject.SetActive(false);
            segment.transform.SetParent(transform);

            if (pools.ContainsKey(segment.Type))
            {
                pools[segment.Type].Enqueue(segment);
            }
        }
    }
}
