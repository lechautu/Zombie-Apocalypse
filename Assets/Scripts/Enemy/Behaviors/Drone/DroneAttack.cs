using AI.BehaviorTree;
using UnityEngine;
using Weapon;

namespace Characters.AI
{
    public sealed class DroneAttack : BehaviorNode
    {
        private Transform _owner;
        private Drone _droneWeapon;

        public DroneAttack(Transform owner, Drone droneWeapon)
        {
            _droneWeapon = droneWeapon;
            _owner = owner;
        }

        public override NodeState Execute()
        {
            var hasTarget = _owner.gameObject != null
                ? _owner.gameObject.GetComponent<IHasTarget>()
                : null;

            if (hasTarget == null || hasTarget.GetTarget() == null)
                return NodeState.Failure;

            Transform target = hasTarget.GetTarget().transform;
            _owner.LookAt(target);

            // In range: perform attack logic here (e.g., reduce health of target if it implements IDamageable)
            if (_droneWeapon.CanShoot())
            {
                _droneWeapon.Shoot();
                return NodeState.Success; // Attack succeeded
            }

            return NodeState.Failure; // Target not damageable
        }
    }
}