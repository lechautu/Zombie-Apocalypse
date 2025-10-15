using AI.BehaviorTree;
using UnityEngine;
using System.Collections.Generic;

namespace Characters.AI
{
    [RequireComponent(typeof(Weapon.Drone))]
    // Placeholder for future Drone AI behavior implementation
    public class DroneAI : MonoBehaviour
    {
        private BehaviorNode _rootNode;

        void Start()
        {
            SetupBehaviorTree();
        }

        void Update()
        {
            if (_rootNode != null)
            {
                _rootNode.Execute();
            }
        }

        void SetupBehaviorTree()
        {
            _rootNode = new SequenceNode(new List<BehaviorNode>
            {
                new DroneLookForEnemy(5f, LayerMask.GetMask("Enemy"), transform, GetComponent<Weapon.Drone>()),
                new DroneAttack(transform, GetComponent<Weapon.Drone>())
            });
        }
    }
}
