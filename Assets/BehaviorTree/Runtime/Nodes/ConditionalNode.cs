namespace BehaviorTree
{
    /// <summary>
    /// Enum just to display on the window.
    /// </summary>
    public enum TruthValue
    {
        True,
        False
    }

    public class ConditionalNode : BaseNode
    {
        public Condition condition;
        public bool value;

        public JudgeFunc judgeFunc;

        public ConditionalNode()
        {
            condition = Condition.None;
            value = true;
        }

        public override BehaviourState OnUpdate()
        {
            if(State == BehaviourState.Inactive)
            {
                OnStart();
            }

            if (condition == Condition.None) return BehaviourState.Failure;
            else if (condition == Condition.True) return value == true ? BehaviourState.Success : BehaviourState.Failure;
            else if (condition == Condition.False) return value == false ? BehaviourState.Success : BehaviourState.Failure;

            return judgeFunc() == value ? BehaviourState.Success : BehaviourState.Failure;
        }
    }
}