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

        private float _timer;

        void OnEnable()
        {
            _timer = 0f;
        }

        void Update()
        {
            transform.Translate(speed * Time.deltaTime * Vector3.forward);

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                BulletPool.Instance.ReturnBullet(this); // Return to pool
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if ((hitMask.value & (1 << other.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage((int)damage);
                }
                BulletPool.Instance.ReturnBullet(this); // Return bullet to pool
            }
        }

        public void SetDamage(float weaponDamage)
        {
            damage = weaponDamage;
        }
    }
}