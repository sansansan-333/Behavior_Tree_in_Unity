namespace BehaviorTree
{
    public class InverterNode : BaseNode
    {
        public override BehaviourState OnUpdate()
        {
            if (State == BehaviourState.Inactive)
            {
                OnStart();
            }

            BehaviourState state = default;

            if(ChildNodes.Count > 0) state = ChildNodes[0].OnUpdate();

            if (state == BehaviourState.Success) return BehaviourState.Failure;
            if (state == BehaviourState.Failure) return BehaviourState.Success;

            return BehaviourState.Success;
        }
    }
}
