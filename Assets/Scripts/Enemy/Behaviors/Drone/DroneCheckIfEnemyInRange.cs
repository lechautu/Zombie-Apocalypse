using AI.BehaviorTree;
using UnityEngine;

namespace Characters.AI
{
    public sealed class DroneCheckIfEnemyInRange : BehaviorNode
    {
        private readonly float _attackRange;
        private readonly IHasTarget _hasTarget;
        private readonly Transform _ownerTransform;

        public DroneCheckIfEnemyInRange(float attackRange, IHasTarget hasTarget, Transform ownerTransform)
        {
            _attackRange = attackRange;
            _hasTarget = hasTarget;
            _ownerTransform = ownerTransform;
        }

        public override NodeState Execute()
        {
            var target = _hasTarget.GetTarget();
            if (target == null)
            {
                return NodeState.Failure;
            }

            float distanceToTarget = Vector3.Distance(_ownerTransform.position, target.transform.position);
            if (distanceToTarget <= _attackRange)
            {
                return NodeState.Success;
            }

            _ownerTransform.localRotation = Quaternion.identity; // Reset rotation when no target in range
            return NodeState.Failure;
        }
    }
}