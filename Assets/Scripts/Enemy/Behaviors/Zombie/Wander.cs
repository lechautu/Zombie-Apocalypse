using AI.BehaviorTree;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Behaviors
{
    public class Wander : BehaviorNode
    {
        private Transform _enemy;
        private NavMeshAgent _agent;
        private float _wanderRadius = 10f;

        public Wander(Transform enemy)
        {
            _enemy = enemy;
            _agent = _enemy.GetComponent<NavMeshAgent>();
        }

        public override NodeState Execute()
        {
            Vector3 randomPos = Random.insideUnitSphere * _wanderRadius + _enemy.position;
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, _wanderRadius, 1))
            {
                _agent.SetDestination(hit.position);
                return NodeState.Running;
            }
            return NodeState.Failure;
        }
    }
}