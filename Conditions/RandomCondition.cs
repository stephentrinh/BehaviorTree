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

namespace STBehaviorTree
{
    [BehaviorTreeNode("Conditions/Random")]
    public class RandomCondition : BehaviorTreeCondition
    {
        public RandomCondition(string name, int depth, int id)
            : base(name, depth, id)
        { }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            _currentState = (BehaviorState)UnityEngine.Random.Range(-1, 3);

            if (DEBUG_on)
                Debug.Log("Randomed state: " + StateToString(_currentState));

            return _currentState;
        }
    }
}