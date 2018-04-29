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
    /// Goes through its list of actions to find an action it can perform on each update.
    /// Will start from the first action in the list of actions or from its previous running node.
    /// </summary>
    [System.Serializable]
    [BehaviorTreeNode("Selectors/Priority")]
    public class PrioritySelector : BehaviorTreeSelector
    {
        public PrioritySelector()
            : base()
        {
        }

        public PrioritySelector(string name, int depth, int id)
            : base(name, depth, id)
        {
        }

        /// <summary>
        /// Can only return RUNNING, FAILURE, or SUCCESS.
        /// </summary>
        /// <returns></returns>
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

        #region EDITOR ONLY
#if UNITY_EDITOR
        public override string GetIconPath()
        {
            return "BehaviorTree/Icons/ICON_BehaviorTreePriority.png";
        }
#endif
        #endregion
    }
}