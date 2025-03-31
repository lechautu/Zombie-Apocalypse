using UnityEngine;
using AI.BehaviorTree;

namespace Enemy.Behaviors
{
    public class CheckPlayerInRange : BehaviorNode
    {
        private Transform _enemy, _player;
        private float _range;

        public CheckPlayerInRange(Transform enemy, Transform player, float range)
        {
            _enemy = enemy;
            _player = player;
            _range = range;
        }

        public override NodeState Execute()
        {
            float distance = Vector3.Distance(_enemy.position, _player.position);
            return distance <= _range ? NodeState.Success : NodeState.Failure;
        }
    }
}