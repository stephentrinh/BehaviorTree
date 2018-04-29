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
    /// Runs all child nodes in order and stops when one fails or they all succeed.
    /// </summary>
    [System.Serializable]
    [BehaviorTreeNode("Selectors/Concurrent")]
    public class ConcurrentSelector : BehaviorTreeSelector
    {
        public ConcurrentSelector()
            : base()
        {
        }

        public ConcurrentSelector(string name, int depth, int id)
            : base(name, depth, id)
        {
        }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            if (DEBUG_on)
                Debug.Log(ToString());

            for (int i = 0; i < Children.Count; ++i)
            {
                _currentState = Children[i].NodeUpdate(blackboard);

                if (DEBUG_on)
                    DEBUG_logChildState(i, _currentState);

                if (_currentState == STATE_FAILURE)
                    return _currentState;
            }

            return _currentState;
        }

        #region EDITOR ONLY
#if UNITY_EDITOR
        public override string GetIconPath()
        {
            return "BehaviorTree/Icons/ICON_BehaviorTreeConcurrent.png";
        }
#endif
        #endregion
    }
}