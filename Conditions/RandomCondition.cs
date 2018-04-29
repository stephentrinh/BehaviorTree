using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***************************************************************************************
*    Title: Behavior Tree
*    Author: Stephen Trinh
*    Date: 4/28/18
*    Code version: 1.0
*    Availability: https://github.com/stephentrinh/BehaviorTree
*
***************************************************************************************/

namespace BehaviorTree
{
    [BehaviorTreeNode("Conditions/Random")]
    public class SuccessCondition : BehaviorTreeCondition
    {
        public SuccessCondition(string name, int depth, int id)
            : base(name, depth, id)
        { }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            if (DEBUG_on)
                Debug.Log("I'm debugging.");
            _currentState = STATE_SUCCESS;
            return _currentState;
        }
    }
}