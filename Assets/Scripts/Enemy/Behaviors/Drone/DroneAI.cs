using AI.BehaviorTree;
using UnityEngine;
using System.Collections.Generic;
using Enemy.Behaviors;
using Weapon;

namespace Characters.AI
{
    // Placeholder for future Drone AI behavior implementation
    public class DroneAI : MonoBehaviour, IHasTarget
    {
        private BehaviorNode _rootNode;
        private BaseZombieAI _currentTarget; 
        private WeaponBase _droneWeapon;       

        void Start()
        {
            if (TryGetComponent<WeaponBase>(out var weapon))
            {
                _droneWeapon = weapon;
                SetupBehaviorTree();
            }
        }

        void Update()
        {
            _rootNode?.Execute();
        }

        void SetupBehaviorTree()
        {
            _rootNode = new SelectorNode(new List<BehaviorNode>
            {
                new SequenceNode(new List<BehaviorNode>
                {
                    new DroneCheckIfEnemyInRange(3f, this, transform),
                    new DroneAttack(transform, _droneWeapon, this)
                }),
                new DroneLookForEnemy(5f, LayerMask.GetMask("Enemy"), transform)
            });
        }

        public void SetTarget(BaseZombieAI target)
        {
            _currentTarget = target;
        }

        public BaseZombieAI GetTarget()
        {
            return _currentTarget;
        }
    }
}
