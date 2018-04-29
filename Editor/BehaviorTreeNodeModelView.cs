using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
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
    public class BehaviorTreeNodeItem : TreeViewItem
    {
        /// <summary>
        /// Type of the behavior class
        /// </summary>
        public Type Data;

        public BehaviorTreeNodeItem(int id, int depth, string displayName, Type data)
            : base(id, depth, displayName)
        {
            Data = data;
        }
    }

    public class BehaviorTreeNodeModelView : TreeView
    {
        IList<BehaviorTreeNodeItem> _nodes;

        public BehaviorTreeNodeModelView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();

            showBorder = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root"};

            var attributeTypePairs = new List<AttributeTypePair>();
            BehaviorTreeNodeAttributeHandler.LoadAttributes(ref attributeTypePairs);

            _nodes = new List<BehaviorTreeNodeItem>();
            ParseAttributes(attributeTypePairs, ref _nodes);

            SetupParentsAndChildrenFromDepths(root, _nodes.Cast<TreeViewItem>().ToList());
            return root;
        }

        IList<BehaviorTreeNodeItem> GetNodesAsIList()
        {
            return _nodes;
        }

        void ParseAttributes(List<AttributeTypePair> attributeTypePairs, ref IList<BehaviorTreeNodeItem> result)
        {
            attributeTypePairs.Sort((s1, s2) => s1.attributeName.CompareTo(s2.attributeName));

            List<string> category = new List<string>();
            int ID = 1;
            for (int i = 0; i < attributeTypePairs.Count; ++i)
            {
                var split = attributeTypePairs[i].attributeName.Split('/');

                for (int j = 0; j < split.Length; ++j)
                {
                    if (j == split.Length - 1)
                    {
                        result.Add(new BehaviorTreeNodeItem(ID++, j, split[j], attributeTypePairs[i].classType));
                        break;
                    }
                    else if (category.Count <= j)
                    {
                        category.Add(split[j]);
                        result.Add(new BehaviorTreeNodeItem(ID++, j, split[j], null));
                    }
                    else if (category[j] != split[j])
                    {
                        category.RemoveRange(j, category.Count - j);
                        category.Add(split[j]);
                        result.Add(new BehaviorTreeNodeItem(ID++, j, split[j], null));
                    }
                }
            }
        }

        #region Selection
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        /// <summary>
        /// Returns the first index
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public BehaviorTreeNodeItem GetSelectionFromIndices()
        {
            if (GetSelection() == null || !GetSelection().Any())
                return null;

            return _nodes[(GetSelection()[0]) - 1];
        }
        #endregion
    }
}