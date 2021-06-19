namespace BehaviorTree
{
    /// <summary>
    /// Condition that used in one Conditional node.
    /// </summary>
    public enum Condition
    {
        // default value start, please don't delete this values
        None,
        True,
        False,
        // default value end

        FiftyFifty,
        IsInRange,
    }

    public delegate bool JudgeFunc();

    public class CondFuncPair
    {
        public Condition condition;
        public JudgeFunc judgeFunc;

        public CondFuncPair(Condition condition, JudgeFunc judgeFunc)
        {
            this.condition = condition;
            this.judgeFunc = judgeFunc;
        }
    }
}
