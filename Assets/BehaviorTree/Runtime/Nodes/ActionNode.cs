using System;

namespace BehaviorTree
{
    public class ActionNode : BaseNode
    {
        public Act act;
        public ActFunction action;
        public BehaviorTreeExecutor targetExecutor;

        public ActionNode()
        {
            act = Act.None;
        }

        public override BehaviourState OnUpdate()
        {
            if(State == BehaviourState.Inactive)
            {
                OnStart();
            }

            if(act != Act.None) targetExecutor.SetActFunction(action);

            return BehaviourState.Success;
        }
    }
}