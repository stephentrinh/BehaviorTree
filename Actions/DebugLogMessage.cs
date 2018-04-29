using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace STBehaviorTree
{
    [BehaviorTreeNode("Actions/DebugLogMessage")]
    public class DebugLogMessage : BehaviorTreeAction
    {
        public DebugLogMessage(string name, int depth, int id)
            : base(name, depth, id)
        { }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            BlackboardString message = blackboard.Get(BlackboardProperties.Message);
            Debug.Log(message.Message);
            _currentState = STATE_SUCCESS;
            return _currentState;
        }
    }
}