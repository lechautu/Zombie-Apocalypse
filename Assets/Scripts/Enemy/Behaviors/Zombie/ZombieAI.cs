using System.Collections;
using System.Collections.Generic;
using AI.BehaviorTree;
using Characters;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Behaviors
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ZombieAI : MonoBehaviour, IDamageable
    {
        [Header("Zombie AI Definition")]
        [Tooltip("Scriptable object containing the character's stats and abilities.")]
        public ScriptableCharacter characterDefinition;

        [Header("Zombie AI settings")]
        [Tooltip("The player transform to follow.")]
        public Transform player;

        [Tooltip("The range within which the zombie can detect the player.")]
        public float detectionRange = 15f;
        public float attackRange = 2f;

        private BehaviorNode _rootNode;
        private NavMeshAgent _agent;
        private Animator _animator;
        private AttackPlayer _attackPlayer;

        private int _animSpeed;
        private int _animDead;

        private bool _hasAnimator;

        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public void TakeDamage(int damage)
        {
            if (IsDead) return; // Prevent taking damage if already dead

            CurrentHealth -= damage;
            if (IsDead)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        private void AssignAnimationParameters()
        {
            _animSpeed = Animator.StringToHash("Speed");
            _animDead = Animator.StringToHash("Die");
        }

        private void OnEnable()
        {
            CurrentHealth = characterDefinition.maxHealth;
            if (_agent)
            {
                _agent.isStopped = false;
            }
        }

        void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            player = GameObject.FindGameObjectWithTag("Player").transform;
            _agent = GetComponent<NavMeshAgent>();

            _agent.speed = characterDefinition.speed;

            _attackPlayer = new AttackPlayer(transform, player);
            _rootNode = new SelectorNode(new List<BehaviorNode>
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

            AssignAnimationParameters();
        }

        void Update()
        {
            if (IsDead) return; // Skip update if dead
            
            _rootNode.Execute();
            if (_hasAnimator)
            {
                _animator.SetFloat(_animSpeed, _agent.velocity.magnitude);
            }
        }

        private void Die()
        {
            if (_hasAnimator)
            {
                _animator.SetTrigger(_animDead);
            }

            if (!_agent.isStopped)
            {
                _agent.isStopped = true;
            }

            Invoke(nameof(DisableZombie), 3f); // Wait for the death animation to finish
            
        }

        void DisableZombie()
        {
            ZombiePool.Instance.ReturnZombie(gameObject); // Return to pool
        }

        public void DealDamage()
        {
            _attackPlayer.DealDamage();
        }
    }
}
