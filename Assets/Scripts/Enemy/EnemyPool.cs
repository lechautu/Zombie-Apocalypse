using UnityEngine;
using System.Collections.Generic;

namespace Enemy
{public class ZombiePool : MonoBehaviour
    {
        public static ZombiePool Instance;

        [Header("Zombie Variants")]
        public List<ZombieVariant> zombieVariants;

        [Header("Limits")]
        public int maxActiveZombies = 30;

        private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private HashSet<GameObject> activeZombies = new HashSet<GameObject>();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            foreach (var variant in zombieVariants)
            {
                Queue<GameObject> pool = new Queue<GameObject>();

                for (int i = 0; i < variant.poolSize; i++)
                {
                    GameObject zombie = Instantiate(variant.prefab);
                    zombie.name = variant.name; // Remove (Clone) suffix tracking
                    zombie.SetActive(false);
                    pool.Enqueue(zombie);
                }

                pools[variant.name] = pool;
            }
        }

        public GameObject GetRandomZombie(Vector3 position, Quaternion rotation)
        {
            if (activeZombies.Count >= maxActiveZombies)
                return null;

            ZombieVariant selected = zombieVariants[Random.Range(0, zombieVariants.Count)];
            string name = selected.name;

            GameObject zombie;

            if (pools[name].Count > 0)
            {
                zombie = pools[name].Dequeue();
            }
            else
            {
                zombie = Instantiate(selected.prefab);
                zombie.name = name;
            }

            zombie.transform.SetPositionAndRotation(position, rotation);
            zombie.SetActive(true);

            activeZombies.Add(zombie);
            return zombie;
        }

        public void ReturnZombie(GameObject zombie)
        {
            zombie.SetActive(false);
            activeZombies.Remove(zombie);

            string name = zombie.name.Replace("(Clone)", "").Trim();
            if (pools.ContainsKey(name))
            {
                pools[name].Enqueue(zombie);
            }
            else
            {
                Destroy(zombie); // fallback
            }
        }

        public int ActiveZombieCount => activeZombies.Count;
    }
    
    [System.Serializable]
    public class ZombieVariant
    {
        public string name;           // Unique name for this variant
        public GameObject prefab;     // The prefab to spawn
        public int poolSize = 10;     // How many to preload in pool
    }

    
}