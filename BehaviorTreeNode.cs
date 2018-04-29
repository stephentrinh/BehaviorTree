using UnityEngine;
using System.Collections.Generic;
using System;

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
    public abstract class BehaviorTreeNode
    {
        protected BehaviorState _currentState;

        public string Name;
        public List<BehaviorTreeNode> Children;
        public BehaviorTreeNode Parent;
        public bool DEBUG_on = false;

        // Specifically used for displaying the behavior tree
        public int ID;
        public int Depth;
        public bool HasChildren { get { return Children != null && Children.Count > 0; } }

        public enum BehaviorState
        {
            kNone = -1,
            kRunning = 0,
            kSuccess = 1,
            kFailure = 2
        }

        protected const BehaviorState
            STATE_NONE = BehaviorState.kNone,
            STATE_RUNNING = BehaviorState.kRunning,
            STATE_SUCCESS = BehaviorState.kSuccess,
            STATE_FAILURE = BehaviorState.kFailure;

        #region Constructors
        public BehaviorTreeNode()
        {
            Children = new List<BehaviorTreeNode>();
        }

        public BehaviorTreeNode(string name, int depth, int id)
            : this()
        {
            Name = name;
            ID = id;
            Depth = depth;
        }
        #endregion

        public abstract BehaviorState NodeUpdate(Blackboard blackboard);

        public void AddNode(BehaviorTreeNode node)
        {
            Children.Add(node);
        }

        public string StateToString(BehaviorState state)
        {
            switch (state)
            {
                case STATE_RUNNING:
                    return "RUNNING";
                case STATE_SUCCESS:
                    return "SUCCESS";
                case STATE_FAILURE:
                    return "FAILURE";
                default:
                    return "NONE";
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #region EDITOR ONLY
#if UNITY_EDITOR
        public virtual string GetIconPath()
        {
            return "BehaviorTree/Icons/ICON_BehaviorTreeNode.png";
        }
#endif
        #endregion
    }
}