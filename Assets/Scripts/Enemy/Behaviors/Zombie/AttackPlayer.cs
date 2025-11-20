using AI.BehaviorTree;
using ARPG.Core;
using Characters;
using UnityEngine;

namespace Enemy.Behaviors
{
    public class AttackPlayer : BehaviorNode
    {
        private Transform _enemy, _player;
        private float _attackCooldown = 1.5f;
        private float _nextAttackTime = 0f;
        private int _damage = 10;

        private bool _hasAnimator;
        private Animator _animator;
        private int _animAttack;

        public AttackPlayer(Transform enemy, Transform player)
        {
            _enemy = enemy;
            _player = player;
            _hasAnimator = _enemy.TryGetComponent(out _animator);
            _animAttack = Animator.StringToHash("Attack");
        }

        public override NodeState Execute()
        {
            if (Time.time < _nextAttackTime) return NodeState.Running;

            if (_hasAnimator)
            {
                _animator.SetTrigger(_animAttack);
            }
            _nextAttackTime = Time.time + _attackCooldown;
            return NodeState.Success;
        }

        public void DealDamage()
        {
            if (Vector3.Distance(_enemy.position, _player.position) <= 2f)
            {
                _player.GetComponent<Damageable>()?.ApplyHit(new ()
                {
                    damage = _damage
                });
            }
        }
    }
}