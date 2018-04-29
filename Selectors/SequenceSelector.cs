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
    /// <summary>
    /// Selects its nodes in sequence. If a node fails, the selector resets.
    /// </summary>
    [System.Serializable]
    [BehaviorTreeNode("Selectors/Sequence")]
    public class SequenceSelector : BehaviorTreeSelector
    {
        int _currentBehaviorIndex = 0;

        public SequenceSelector()
            : base()
        {
        }

        public SequenceSelector(string name, int depth, int id)
            : base(name, depth, id)
        {
        }

        public override BehaviorState NodeUpdate(Blackboard blackboard)
        {
            if (DEBUG_on)
                Debug.Log(ToString());

            _currentState = Children[_currentBehaviorIndex].NodeUpdate(blackboard);

            if (DEBUG_on)
                DEBUG_logChildState(_currentBehaviorIndex, _currentState);

            switch (_currentState)
            {
                case STATE_SUCCESS:
                    _currentBehaviorIndex++;

                    if (_currentBehaviorIndex >= Children.Count)
                        ResetSelector();
                    break;
                case STATE_FAILURE:
                    ResetSelector();
                    break;
            }

            return _currentState;
        }

        void ResetSelector()
        {
            _currentBehaviorIndex = 0;
        }

        #region EDITOR ONLY
#if UNITY_EDITOR
        public override string GetIconPath()
        {
            return "BehaviorTree/Icons/ICON_BehaviorTreeSequence.png";
        }
#endif
        #endregion
    }
}