using System.Collections.Generic;
using AI.BehaviorTree;
using Enemy.Behaviors;

namespace Enemy
{
    public class RegularZombie : BaseZombieAI
    {
        protected override SelectorNode GetBehaviour()
        {
            _attackPlayer = new AttackPlayer(transform, player);
            var node = new SelectorNode(new List<BehaviorNode>
            {
                new SequenceNode(new List<BehaviorNode>  // If player is in attack range, attack
                {
                    new CheckPlayerInRange(transform, player, attackRange),
                    _attackPlayer
                }),
                new SequenceNode(new List<BehaviorNode>  // If player is in detection range, chase
                {
                    new CheckPlayerInRange(transform, player, detectionRange),
                    new ChasePlayer(transform, player)
                }),
                new Wander(transform)  // Else, wander
            });
            return node;
        }
    }
}