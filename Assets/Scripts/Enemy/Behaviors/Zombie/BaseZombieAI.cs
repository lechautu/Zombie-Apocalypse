using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AI.BehaviorTree;
using ARPG.Core;
using Characters;
using Cysharp.Threading.Tasks;
using GameFx.Core;
using GameFx.Core.PoolSystem;
using UnityEngine;
using UnityEngine.AI;
using Weapon;

namespace Enemy.Behaviors
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseZombieAI : MonoBehaviour
    {
        [Header("Zombie AI Definition")]
        [Tooltip("Scriptable object containing the character's stats and abilities.")]
        public ScriptableCharacter characterDefinition;

        [Header("Zombie AI settings")]
        [Tooltip("The player transform to follow.")]
        protected Transform player;
        [SerializeField] Damageable damageable;

        [Tooltip("The range within which the zombie can detect the player.")]
        public float detectionRange = 15f;
        public float attackRange = 2f;

        [Header("VFX")]
        public ParticleSystem hitEffect;
        [Range(0f, 1f)]
        public float dissolveValue = 0f;

        static readonly int DissolveID = Shader.PropertyToID("_Dissolve");
        List<Material> _materials;


        protected BehaviorNode _rootNode;
        protected NavMeshAgent _agent;
        protected Animator _animator;
        protected AttackPlayer _attackPlayer;

        protected int _animSpeed;

        protected bool _hasAnimator;
        public bool IsDead { get; protected set; }

        private PoolManager poolManager => ServiceLocator.Get<PoolManager>();

        protected void AssignAnimationParameters()
        {
            _animSpeed = Animator.StringToHash("Speed");
        }

        protected void OnEnable()
        {
            damageable.health = damageable.maxHealth;
            IsDead = false;
            if (_agent)
            {
                _agent.isStopped = false;
            }
            SetDissolve(0f);
        }

        void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            player = GameObject.FindGameObjectWithTag("Player").transform;
            _agent = GetComponent<NavMeshAgent>();

            _agent.speed = characterDefinition.speed;

            _rootNode = GetBehaviour();

            AssignAnimationParameters();

            _materials = new List<Material>();
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                _materials.AddRange(rend.materials);
            }
        }

        void Update()
        {
            if (IsDead) return; // Skip update if dead

            if (!IsDead && damageable.health <= 0)
            {
                Die();
            }
            
            _rootNode.Execute();
            if (_hasAnimator)
            {
                _animator.SetFloat(_animSpeed, _agent.velocity.magnitude);
            }
        }

        protected void Die()
        {
            IsDead = true;

            if (!_agent.isStopped)
            {
                _agent.isStopped = true;
            }

            GameManager.Instance.AddScore(characterDefinition.score);
            ServiceLocator.Get<EventDispatcher>().Dispatch(EventConstants.OnEnemyKilled);
            DisableZombie().Forget();
        }

        protected async UniTask DisableZombie()
        {
            await UniTask.WaitForSeconds(2f);
            float dissolve = 0;
            while (dissolve < 1f)
            {
                dissolve += Time.deltaTime;
                SetDissolve(dissolve);
                await UniTask.WaitForEndOfFrame();
            }
            poolManager.ReturnToPool(gameObject);
        }

        public void DealDamage()
        {
            _attackPlayer.DealDamage();
        }

        public void SetDissolve(float value)
        {
            if (_materials == null) return;
            dissolveValue = value;

            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetFloat(DissolveID, dissolveValue);
            }
        }

        protected abstract SelectorNode GetBehaviour();
    }
}
