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
    [BehaviorTreeNode("Decorators/Inverter")]
    public class InverterDecorator : BehaviorTreeDecorator
    {
        public InverterDecorator(string name, int depth, int id)
            : base(name, depth, id)
        { }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            Debug.Assert(Children != null && Children.Count == 1, "Inverter decorator does not have ONLY one child.");
            _currentState = Children[0].NodeUpdate(blackboard);

            return _currentState;
        }
    }
}