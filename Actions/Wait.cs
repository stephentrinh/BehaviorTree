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
    [BehaviorTreeNode("Actions/Wait")]
    public class Wait : BehaviorTreeAction
    {
        float _currentWaitTime = 0;
        const float WAIT_TIME = 3.0f;

        public Wait(string name, int depth, int id)
            : base(name, depth, id)
        { }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            if (_currentWaitTime < WAIT_TIME)
            {
                if (DEBUG_on)
                    Debug.Log("Waited for: " + _currentWaitTime + " seconds");
                _currentState = STATE_RUNNING;

                _currentWaitTime += Time.deltaTime;
            }
            else
            {
                _currentState = STATE_SUCCESS;
                _currentWaitTime = 0;
            }

            return _currentState;
        }
    }
}