using UnityEngine;
using System.Collections.Generic;

namespace Weapon
{
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance; // Singleton for easy access
        public GameObject bulletPrefab; // Assign bullet prefab in Inspector
        public int poolSize = 20; // Number of bullets to pre-instantiate

        private readonly Queue<Bullet> bulletQueue = new();

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            // Pre-instantiate bullets and store them in the queue
            for (int i = 0; i < poolSize; i++)
            {
                Bullet bullet = Instantiate(bulletPrefab, transform).GetComponent<Bullet>();
                bullet.gameObject.SetActive(false);
                bulletQueue.Enqueue(bullet);
            }
        }

        public Bullet GetBullet()
        {
            if (bulletQueue.Count > 0)
            {
                Bullet bullet = bulletQueue.Dequeue();
                bullet.gameObject.SetActive(true);
                return bullet;
            }
            else
            {
                // If the pool runs out, create a new bullet (optional)
                Bullet bullet = Instantiate(bulletPrefab, transform).GetComponent<Bullet>();
                return bullet;
            }
        }

        public void ReturnBullet(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
            bulletQueue.Enqueue(bullet);
        }
    }
}