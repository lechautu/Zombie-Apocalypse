using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    public abstract class BehaviorNode
    {
        public abstract NodeState Execute();
    }

    public enum NodeState
    {
        Running,
        Success,
        Failure
    }
}