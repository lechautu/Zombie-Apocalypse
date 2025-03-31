using System.Collections.Generic;

namespace AI.BehaviorTree
{
    public class SelectorNode : BehaviorNode
    {
        private List<BehaviorNode> _children;

        public SelectorNode(List<BehaviorNode> children)
        {
            _children = children;
        }

        public override NodeState Execute()
        {
            foreach (var child in _children)
            {
                NodeState state = child.Execute();
                if (state == NodeState.Success) return NodeState.Success;
                if (state == NodeState.Running) return NodeState.Running;
            }
            return NodeState.Failure;
        }
    }
}