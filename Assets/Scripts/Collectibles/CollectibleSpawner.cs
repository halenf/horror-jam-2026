using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Train.Collectibles
{
    public class CollectibleSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Collectible[] m_collectiblePrefabs;

        [Tooltip("Size of Collectible object pool for this CollectibleSpawner;\nNumber of Collectibles that can exist at any given time.")]
        [SerializeField, Min(0)] private int m_poolSize;
        [SerializeField, Min(0f)] private float m_respawnTimerLower = 1f;
        [SerializeField, Min(0f)] private float m_respawnTimerUpper = 3f;
        [Tooltip("When the game starts, the initial respawn timer values for each Collectible will be separated by this many seconds.")]
        [SerializeField, Min(0f)] private float m_initialSpawnTimerGap = 1.8f;
        [Header("Spawn Location")]
        [Tooltip("Given a position (x,y) and a size (z,w), an area for collectables to spawn in is made.")]
        [SerializeField] private Vector4[] m_spawnLocations;
        [Tooltip("How far off the ground all the spawn locations should be. Moreso for ensuring spawned collectables spawn at the correct levels")]
        [SerializeField] private float m_spawnHeight;
        //[Tooltip("When a Collectible is spawned, a random offset will be applied with a +- range from this Vector on the X and Z axes.")]
        //[SerializeField] private Vector2 m_randomSpawnOffset;

        private Collectible[] m_pooledCollectibles;
        private float[] m_respawnTimers;
        private List<int> m_freeIndices = new();

        private void Start()
        {
            CreatePool();
        }

        private void Update()
        {
            for (int i = 0; i < m_poolSize; i++)
            {
                // if Collectible is not active
                if (!m_pooledCollectibles[i].gameObject.activeSelf)
                {
                    // if timer is counting down
                    if (m_respawnTimers[i] > 0)
                    {
                        m_respawnTimers[i] -= Time.deltaTime;
                    }
                    // if timer is over
                    else
                    {
                        Spawn(i);
                    }
                }
            }
        }

        private void OnDisable()
        {
            // good memory management
            foreach (Collectible c in m_pooledCollectibles)
            {
                c.onCollected -= Despawn;
            }
        }

        private void OnValidate()
        {
            if (m_respawnTimerUpper < m_respawnTimerLower || m_respawnTimerLower > m_respawnTimerUpper)
            {
                m_respawnTimerUpper = m_respawnTimerLower;
            }
        }

        public void CreatePool()
        {
            // init arrays
            m_pooledCollectibles = new Collectible[m_poolSize];
            m_respawnTimers = new float[m_poolSize];
            for (int i = 0; i < m_poolSize; i++)
            {
                // create object
                Collectible spawnedCollectible = Instantiate(GetRandomCollectiblePrefab(), transform);
                // add despawn event
                spawnedCollectible.onCollected += Despawn;
                // set name to index
                spawnedCollectible.name = "Collectible " + i;
                // disable object
                spawnedCollectible.gameObject.SetActive(false);
                // add to pool
                m_pooledCollectibles[i] = spawnedCollectible;
                // set initial respawn timer
                m_respawnTimers[i] = GetRandomRespawnTimer() + m_initialSpawnTimerGap * i;
                // add free index as it starts despawned
                m_freeIndices.Add(i);
            }
        }

        public void Spawn(int indexToSpawn = -1)
        {
            if (m_freeIndices.Count == 0)
            {
                Debug.LogWarning("No free Collectibles to spawn, increase pool size.", this);
                return;
            }

            // if no index provided, get the oldest free Collectible from the Pool
            if (indexToSpawn == -1)
            {
                indexToSpawn = m_freeIndices[0];
                m_freeIndices.RemoveAt(0);
            }
            // remove it from free indices as it is being spawned
            else
            {
                m_freeIndices.Remove(indexToSpawn);
            }

            // get Collectible
            Collectible collectibleToSpawn = m_pooledCollectibles[indexToSpawn];

            // get where to spawn it
            collectibleToSpawn.transform.SetPositionAndRotation(GetRandomSpawnLocation(), GetRandomSpawnRotation());

            // reset values
            collectibleToSpawn.OnRespawn();

            // activate it
            collectibleToSpawn.gameObject.SetActive(true);
        }

        public void Despawn(Collectible collectible)
        {
            // turn it off
            collectible.gameObject.SetActive(false);

            // get its index in the pool
            int collectiblePoolIndex = Array.IndexOf(m_pooledCollectibles, collectible);

            // set respawn timer
            m_respawnTimers[collectiblePoolIndex] = GetRandomRespawnTimer();

            // add to free indices
            m_freeIndices.Add(collectiblePoolIndex);
        }

        private Vector3 GetRandomSpawnLocation()
        {
            if (m_spawnLocations.Length == 0)
                return transform.position;
            else
            {
                Vector4 selectedSpawn = m_spawnLocations[Random.Range(0, m_spawnLocations.Length)];
                // (Random.value * 2f - 1f) is random value between -1 and 1
                Vector3 randomOffset = new(Random.Range(-1f, 1f) * selectedSpawn.z, 0, Random.Range(-1f, 1f) * selectedSpawn.w);
                return new Vector3(selectedSpawn.x, m_spawnHeight, selectedSpawn.y) + randomOffset;
            }
        }

        private Collectible GetRandomCollectiblePrefab()
        {
            return m_collectiblePrefabs[Random.Range(0, m_collectiblePrefabs.Length)];
        }
        private float GetRandomRespawnTimer()
        {
            return Random.Range(m_respawnTimerLower, m_respawnTimerUpper);
        }
        private Quaternion GetRandomSpawnRotation()
        {
            Vector2 randomDir = Random.onUnitCircle;
            return Quaternion.LookRotation(new(randomDir.x, 0, randomDir.y));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            /*
            // scale gizmo size with scene camera distance to keep them "constant"
            Vector3 averagePos = Vector3.zero;
            foreach (Vector3 pos in m_spawnLocations)
            {
                averagePos += pos;
            }
            averagePos /= m_spawnLocations.Length;
            
            foreach (Vector3 pos in m_spawnLocations)
            {
                Camera sceneCamera = SceneView.lastActiveSceneView.camera;
                float size = Mathf.Max(0.3f, Vector3.Distance(sceneCamera.transform.position, averagePos) / 50f);
                Gizmos.DrawCube(pos, Vector3.one * size);
            }
            */

            Gizmos.color = Color.cornflowerBlue;
            foreach (Vector4 pos in m_spawnLocations)
            {
                Gizmos.DrawCube(new(pos.x, m_spawnHeight, pos.y), new(pos.z, 1, pos.w));
            }
        }

        [ContextMenu("Spawn Collectible")]
        public void Spawn_EditorOnly()
        {
            Spawn();
        }

        [Header("Editor Fields")]
        [SerializeField] private string m_collectibleLoadPath = "Assets\\Prefabs\\Collectibles\\";
        [ContextMenu("Load Collectible Prefabs")]
        public void LoadCollectiblePrefabs_EditorOnly()
        {
            string[] guids = AssetDatabase.FindAssets(string.Empty, new string[] { m_collectibleLoadPath });
            int prefabCount = guids.Length;
            m_collectiblePrefabs = new Collectible[prefabCount];
            for (int i = 0; i < prefabCount; i++)
            {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);

                try
                {
                    Collectible c = AssetDatabase.LoadAssetAtPath<Collectible>(path);
                    m_collectiblePrefabs[i] = c;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error importing Collectible [{path}]: {e.Message}", this);
                }
            }

            Debug.Log($"Finished importing Collectibles [{m_collectibleLoadPath}]", this);
        }
#endif
    }
}
