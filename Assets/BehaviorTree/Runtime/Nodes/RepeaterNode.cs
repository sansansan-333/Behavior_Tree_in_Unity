namespace BehaviorTree
{
    public class RepeaterNode : BaseNode
    {
        public int repeatCount; // Number of times it repeats child node.

        public RepeaterNode()
        {
            repeatCount = 0;
        }

        /// <returns>returns Success if every OnUodate() returns Success</returns>
        public override BehaviourState OnUpdate()
        {
            bool stateValue = true;

            if (State == BehaviourState.Inactive)
            {
                OnStart();
            }

            if (ChildNodes.Count > 0) {
                for (int i = 0; i < repeatCount; i++)
                {
                    stateValue &= ChildNodes[0].OnUpdate() == BehaviourState.Success;
                }
            }

            return stateValue ? BehaviourState.Success : BehaviourState.Failure;
        }
    }
}