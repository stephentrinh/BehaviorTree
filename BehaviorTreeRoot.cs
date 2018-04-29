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
    /// <summary>
    /// Acts as a priority selector
    /// </summary>
    public class BehaviorTreeRoot : BehaviorTreeSelector
    {
        public BehaviorTreeRoot(string name, int depth, int id)
            : base(name, depth, id)
        { }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            if (DEBUG_on)
                Debug.Log(ToString());

            for (int i = 0; i < Children.Count; ++i)
            {
                _currentState = Children[i].NodeUpdate(blackboard);

                if (DEBUG_on)
                    DEBUG_logChildState(i, _currentState);

                switch (_currentState)
                {
                    case STATE_RUNNING:
                    case STATE_SUCCESS:
                        return _currentState;
                    default:
                        break;
                }
            }

            return _currentState;
        }
    }
}