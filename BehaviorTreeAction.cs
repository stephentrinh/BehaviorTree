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
    [System.Serializable]
    public abstract class BehaviorTreeAction : BehaviorTreeNode
    {
        public BehaviorTreeAction()
            : base()
        { }

        public BehaviorTreeAction(string name, int depth, int id)
            : base(name, depth, id)
        { }

        #region EDITOR ONLY
#if UNITY_EDITOR
        public override string GetIconPath()
        {
            return "BehaviorTree/Icons/ICON_BehaviorTreeAction.png";
        }
#endif
        #endregion
    }
}