using UnityEngine;
using System.Collections.Generic;
using GameFx.Core.PoolSystem;
using GameFx.Core;
using UnityEngine.AI;
using System;
using System.Collections;

namespace Enemy
{
    public class ZombieSpawner : MonoBehaviour
    {
        [Serializable]
        public struct ZombiePrefabEntry
        {
            public EnemyType type;
            public GameObject prefab;
        }

        [Header("Setup")]
        [SerializeField] private List<ZombiePrefabEntry> zombiePrefabs;
        [SerializeField] private float spawnInterval = 0.2f;

        [Header("Spawn Area")]
        [SerializeField] private float spawnRadius = 15f;
        [SerializeField] private float navMeshSampleDistance = 3f;

        private Dictionary<EnemyType, GameObject> _prefabMap;
        private Transform playerTransform;

        private void Awake()
        {
            _prefabMap = new Dictionary<EnemyType, GameObject>();
            foreach (var e in zombiePrefabs)
            {
                _prefabMap[e.type] = e.prefab;
            }
        }

        /// <summary>
        /// Called by DifficultyController to spawn a wave of zombies.
        /// </summary>
        public void SpawnWave(WaveConfig wave)
        {
            StartCoroutine(SpawnWaveRoutine(wave));
        }

        private IEnumerator SpawnWaveRoutine(WaveConfig wave)
        {
            foreach (var entry in wave.entries)
            {
                if (!_prefabMap.TryGetValue(entry.enemyType, out var prefab))
                {
                    Debug.LogWarning($"[ZombieSpawner] No prefab mapped for {entry.enemyType}");
                    continue;
                }

                for (int i = 0; i < entry.amount; i++)
                {
                    SpawnZombie(prefab);
                    yield return new WaitForSeconds(spawnInterval);
                }
            }
        }

        private void SpawnZombie(GameObject prefab)
        {

            if (TryGetRandomPosition(out var spawnPos))
            {
                var go = ServiceLocator.Get<PoolManager>().GetPool(prefab).GetObject();
                go.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
            }
        }

        /// <summary>
        /// Tries to find a random NavMesh position around the player within spawnRadius.
        /// </summary>
        private bool TryGetRandomPosition(out Vector3 spawnPosition)
        {
            spawnPosition = default;

            // Cache player transform
            if (playerTransform == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    playerTransform = playerObj.transform;
            }

            if (playerTransform == null)
                return false;

            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 candidatePosition = playerTransform.position + direction * spawnRadius;

            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                return true;
            }

            return false;
        }
    }
}