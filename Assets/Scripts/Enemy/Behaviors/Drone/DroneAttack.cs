using AI.BehaviorTree;
using UnityEngine;
using Weapon;

namespace Characters.AI
{
    public sealed class DroneAttack : BehaviorNode
    {
        private Transform _owner;
        private WeaponBase _droneWeapon;

        private readonly IHasTarget _hasTarget;

        public DroneAttack(Transform owner, WeaponBase droneWeapon, IHasTarget hasTarget)
        {
            _droneWeapon = droneWeapon;
            _owner = owner;
            _hasTarget = hasTarget;
        }

        public override NodeState Execute()
        {
            if (_hasTarget == null || _hasTarget.GetTarget() == null)
                return NodeState.Failure;

            Transform target = _hasTarget.GetTarget().transform;
            Vector3 lookAtPosition = new(target.position.x, target.position.y + 0.5f, target.position.z);
            _owner.LookAt(lookAtPosition);

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