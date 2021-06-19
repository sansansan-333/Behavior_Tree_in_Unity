using System;
using System.Collections;

namespace BehaviorTree
{
    /// <summary>
    /// Act that agent does in one Action node.
    /// </summary>
    public enum Act
    {
        // default value start, please don't delete this value
        None,
        // default value end

        OutputLog,
        Wait,
        MoveRight,
        MoveLeft,
    }

    public delegate IEnumerator ActFunction();

    public class ActFuncPair
    {
        public Act act;
        public ActFunction action;

        public ActFuncPair(Act act, ActFunction action)
        {
            this.act = act;
            this.action = action;
        }
    }
}
