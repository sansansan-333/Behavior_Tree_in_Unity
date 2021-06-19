namespace BehaviorTree
{
    public class RootNode : BaseNode
    {
        public override BehaviourState OnUpdate()
        {
            if(State == BehaviourState.Inactive)
            {
                OnStart();
            }

            return ChildNodes[0].OnUpdate();
        }
    }
}