namespace BehaviorTree
{
    public class SelectorNode : BaseNode
    {
        public override BehaviourState OnUpdate()
        {
            if (State == BehaviourState.Inactive)
            {
                OnStart();
            }

            if (ChildNodes.Count == 0) return BehaviourState.Success;

            foreach (var child in ChildNodes)
            {
                if (child.OnUpdate() == BehaviourState.Success)
                {
                    return BehaviourState.Success;
                }
            }

            return BehaviourState.Failure;
        }
    }
}
