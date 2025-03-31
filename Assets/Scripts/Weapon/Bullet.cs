using Characters;
using UnityEngine;

namespace Weapon
{
    public class Bullet : MonoBehaviour
    {
        public float speed = 50f;
        private float damage = 10f;
        public float lifetime = 3f;
        public LayerMask hitMask;
        public ParticleSystem hitEffect;

        private float _speed;

        private float _timer;

        void OnEnable()
        {
            _timer = 0f;
            _speed = speed;
        }

        void Update()
        {
            transform.Translate(_speed * Time.deltaTime * Vector3.forward);

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                ReturnToPool(); // Return to pool
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if ((hitMask.value & (1 << other.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent<IDamageable>(out var damageable))
                {
                    _speed = 0f; // Stop the bullet
                    damageable.TakeDamage((int)damage);
                    hitEffect.Play();
                    Invoke(nameof(ReturnToPool), hitEffect.main.duration); // Wait for the effect to finish before returning to pool
                }
                else
                {
                    ReturnToPool();
                }
            }
        }

        public void SetDamage(float weaponDamage)
        {
            damage = weaponDamage;
        }

        private void ReturnToPool()
        {
            BulletPool.Instance.ReturnBullet(this);
        }
    }
}