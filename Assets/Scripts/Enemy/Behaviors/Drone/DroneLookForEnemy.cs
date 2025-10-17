using AI.BehaviorTree;
using Enemy.Behaviors;
using UnityEngine;
using Weapon;

namespace Characters.AI
{
    public sealed class DroneLookForEnemy : BehaviorNode
    {
        private readonly float _detectionRadius;
        private readonly LayerMask _enemyLayer;

        // Non-alloc buffer (kept per-node). Adjust size to your expected max crowd.
        private readonly Collider[] _hits;
        private Transform _owner;

        public DroneLookForEnemy(float detectionRadius, LayerMask enemyLayer, Transform owner)
        {
            _detectionRadius = Mathf.Max(0.01f, detectionRadius);
            _enemyLayer = enemyLayer;
            _hits = new Collider[16];
            _owner = owner;
        }

        public override NodeState Execute()
        {
            Debug.Log("Drone is looking for enemies...");
            Vector3 origin = _owner.position;

            int count = Physics.OverlapSphereNonAlloc(
                origin,
                _detectionRadius,
                _hits,
                _enemyLayer,
                QueryTriggerInteraction.Collide // include trigger enemies if you use trigger hitboxes
            );

            if (count <= 0)
            {
                Debug.Log("Drone found no enemies in range");
                return NodeState.Failure;
            }

            var hasTarget = _owner != null
                ? _owner.GetComponent<IHasTarget>()
                : null;

            if (hasTarget != null)
            {
                ZombieAI nearest = null;
                float nearestSqr = float.MaxValue;

                for (int i = 0; i < count; i++)
                {
                    var c = _hits[i];
                    if (c == null || !c.TryGetComponent<ZombieAI>(out var zombieAI) || zombieAI.IsDead) continue;
                    float sqr = (c.transform.position - origin).sqrMagnitude;
                    if (sqr < nearestSqr)
                    {
                        nearestSqr = sqr;
                        nearest = zombieAI;
                    }
                }
                if (nearest != null)
                {
                    Debug.Log("Drone found nearest zombie: " + nearest.name);
                    hasTarget.SetTarget(nearest);
                }
                else
                {
                    Debug.Log("Drone found no valid targets");
                    hasTarget.SetTarget(null);
                    return NodeState.Failure;
                }
            }
            return NodeState.Success;
        }
    }

    /// <summary>
    /// Optional small contract your agent can implement to receive the nearest target.
    /// Keeps the node independent from blackboards/MBs.
    /// </summary>
    public interface IHasTarget
    {
        void SetTarget(ZombieAI target);
        ZombieAI GetTarget();
    }
}