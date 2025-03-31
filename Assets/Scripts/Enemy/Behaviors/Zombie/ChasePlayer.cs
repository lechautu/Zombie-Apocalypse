using AI.BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Behaviors
{
    public class ChasePlayer : BehaviorNode
    {
        private Transform _enemy, _player;
        private NavMeshAgent _agent;

        public ChasePlayer(Transform enemy, Transform player)
        {
            _enemy = enemy;
            _player = player;
            _agent = _enemy.GetComponent<NavMeshAgent>();
        }

        public override NodeState Execute()
        {
            if (_agent == null) return NodeState.Failure;
            _agent.SetDestination(_player.position);
            return NodeState.Running;
        }
    }
}