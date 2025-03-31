using System.Collections.Generic;

namespace AI.BehaviorTree
{
    public class SequenceNode : BehaviorNode
    {
        private List<BehaviorNode> _children;
        private int _currentIndex = 0;

        public SequenceNode(List<BehaviorNode> children)
        {
            _children = children;
        }

        public override NodeState Execute()
        {
            while (_currentIndex < _children.Count)
            {
                NodeState state = _children[_currentIndex].Execute();
                if (state == NodeState.Running) return NodeState.Running;
                if (state == NodeState.Failure) return NodeState.Failure;
                _currentIndex++;
            }
            _currentIndex = 0;
            return NodeState.Success;
        }
    }
}
