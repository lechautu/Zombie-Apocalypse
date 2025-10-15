using Characters;
using UnityEngine;

namespace Weapon
{
    public struct CustomRayCastHit
    {
        public Collider collider;
        public Vector3 point;
        public Vector3 normal;

        public CustomRayCastHit(Collider collider, Vector3 point, Vector3 normal)
        {
            this.collider = collider;
            this.point = point;
            this.normal = normal;
        }
    }

    public class Bullet : MonoBehaviour
    {
        [Header("Motion")]
        public float speed = 50f;
        public float lifetime = 3f;

        [Tooltip("Keeps bullet flight perfectly flat for top-down.")]
        public bool lockYPlane = true;

        [Header("Hit")]
        public LayerMask hitMask;
        public float damage = 10f;
        public float sweepRadius = 1f; // spherecast radius; small forgiveness vs thin colliders
        public ParticleSystem hitEffect;
        public TrailRenderer trail;

        // runtime
        private float _speed;
        private float _timer;
        private float _lockedY;
        private bool _hit;

        void OnEnable()
        {
            _timer = 0f;
            _speed = speed;
            _hit = false;

            if (lockYPlane) _lockedY = transform.position.y;

            CancelInvoke(nameof(ReturnToPool));
        }

        void Update()
        {
            if (_hit) return;

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                ReturnToPool();
                return;
            }

            // Compute forward/next position
            Vector3 fwd = transform.forward;
            if (lockYPlane)
            {
                // keep the direction flat in top-down
                fwd.y = 0f;
                fwd = fwd.sqrMagnitude > 0f ? fwd.normalized : transform.forward;
            }

            Vector3 nextPos = transform.position + fwd * (_speed * Time.deltaTime);
            if (lockYPlane) nextPos.y = _lockedY;

            // Sweep from current -> next with a small sphere
            Vector3 delta = nextPos - transform.position;
            float dist = delta.magnitude;

            if (dist > 0f)
            {
                if (Physics.SphereCast(transform.position, sweepRadius, delta.normalized,
                                       out RaycastHit hit, dist, hitMask, QueryTriggerInteraction.Collide))
                {
                    Debug.Log($"Bullet hit: {hit.collider.name}");
                    CustomRayCastHit customHit = new(hit.collider, hit.point, hit.normal);
                    HandleHit(customHit);
                    return;
                }
            }

            // No swept hit: move normally
            transform.position = nextPos;
        }

        private void HandleHit(CustomRayCastHit hit)
        {
            _hit = true;
            _speed = 0f;

            // Snap for consistent VFX
            // transform.position = hit.point;

            // Try damage on collider, else its parents (common enemy setup)
            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage((int)damage);
            }
            else
            {
                var parentD = hit.collider.GetComponentInParent<IDamageable>();
                if (parentD != null)
                    parentD.TakeDamage((int)damage);
            }

            // Play VFX (at hit point, oriented to surface normal if available)
            if (hitEffect != null)
            {
                hitEffect.transform.SetPositionAndRotation(
                    hit.point,
                    Quaternion.LookRotation(hit.normal != Vector3.zero ? hit.normal : transform.forward)
                );
                hitEffect.Play();
                Invoke(nameof(ReturnToPool), hitEffect.main.duration);
            }
            else
            {
                ReturnToPool();
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

        void OnDisable()
        {
            CancelInvoke(nameof(ReturnToPool));
            if (trail != null) trail.Clear();
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sweepRadius);
        }
#endif
    }
}
