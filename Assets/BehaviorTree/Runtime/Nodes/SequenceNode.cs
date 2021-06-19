namespace BehaviorTree
{
    public class SequenceNode : BaseNode
    {
        public override BehaviourState OnUpdate()
        {
            if (State == BehaviourState.Inactive)
            {
                OnStart();
            }

            foreach (var child in ChildNodes)
            {
                if (child.OnUpdate() == BehaviourState.Failure)
                {
                    return BehaviourState.Failure;
                }
            }

            return BehaviourState.Success;
        }
    }
}