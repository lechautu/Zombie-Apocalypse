using UnityEngine;
using System.Collections.Generic;

namespace Enemy
{
    public class ZombieSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        public List<Transform> spawnPoints;

        [Tooltip("Time in seconds between spawns")]
        public float spawnInterval = 5f;

        [Tooltip("How many zombies to spawn per wave")]
        public int zombiesPerWave = 3;

        private float timer;

        void Update()
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnWave();
                timer = 0f;
            }
        }

        void SpawnWave()
        {
            if (ZombiePool.Instance == null) return;

            int spawnCount = Mathf.Min(
                zombiesPerWave,
                ZombiePool.Instance.maxActiveZombies - ZombiePool.Instance.ActiveZombieCount
            );

            for (int i = 0; i < spawnCount; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                ZombiePool.Instance.GetRandomZombie(spawnPoint.position, spawnPoint.rotation);
            }

            // Optional scaling: increase difficulty over time
            zombiesPerWave++;
            spawnInterval = Mathf.Max(1f, spawnInterval - 0.1f);
        }
    }
}