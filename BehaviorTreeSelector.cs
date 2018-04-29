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
    [System.Serializable]
    public abstract class BehaviorTreeSelector : BehaviorTreeNode
    {
        public BehaviorTreeSelector()
            : base()
        {
        }

        public BehaviorTreeSelector(string name, int depth, int id)
            : base(name, depth, id)
        {
        }

        protected void DEBUG_logChildState(int index, BehaviorState returnState)
        {
            Debug.Log("[" + Children[index].ToString() + "] returned with state: " + StateToString(returnState));
        }
    }
}