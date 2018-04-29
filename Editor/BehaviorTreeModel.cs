using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    /// Data representation for the behavior tree.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BehaviorTreeModel<T> where T : BehaviorTreeNode
    {
        public T Root;

        IList<T> _data;
        int _maxID;
        BehaviorTree _behaviorTreeAsset;

        public IList<T> TreeAsList { get { return _data; } }
        public event Action e_ModelChanged;

        public BehaviorTreeModel(IList<T> data, BehaviorTree behaviorTreeAsset)
        {
            SetData(data);
            _behaviorTreeAsset = behaviorTreeAsset;
        }

        public T Find(int id)
        {
            return _data.FirstOrDefault(element => element.ID == id);
        }

        public void SetData(IList<T> data)
        {
            Init(data);
        }

        void Init(IList<T> data)
        {
            if(data == null)
                throw new ArgumentNullException("data", "Input data is null. Ensure input is a non-null list.");

            _data = data;
            if (_data.Count > 0)
                Root = BehaviorTreeNodeUtility.ListToTree(data);

            _maxID = _data.Max(e => e.ID);
        }

        public int GenerateUniqueID()
        {
            return ++_maxID;
        }

        public IList<int> GetAncestors(int id)
        {
            var parents = new List<int>();
            BehaviorTreeNode T = Find(id);
            if (T != null)
            {
                while (T.Parent != null)
                {
                    parents.Add(T.Parent.ID);
                    T = T.Parent;
                }
            }
            return parents;
        }

        public IList<int> GetDescendantsThatHaveChildren(int id)
        {
            T searchFromThis = Find(id);
            if (searchFromThis != null)
            {
                return GetParentsBelowStackBased(searchFromThis);
            }
            return new List<int>();
        }

        IList<int> GetParentsBelowStackBased(BehaviorTreeNode searchFromThis)
        {
            Stack<BehaviorTreeNode> stack = new Stack<BehaviorTreeNode>();
            stack.Push(searchFromThis);

            var parentsBelow = new List<int>();
            while (stack.Count > 0)
            {
                BehaviorTreeNode current = stack.Pop();
                if (current.HasChildren)
                {
                    parentsBelow.Add(current.ID);
                    foreach (var T in current.Children)
                    {
                        stack.Push(T);
                    }
                }
            }

            return parentsBelow;
        }

        public void RemoveElements(IList<int> elementIDs)
        {
            IList<T> elements = _data.Where(element => elementIDs.Contains(element.ID)).ToArray();
            RemoveElements(elements);
        }

        public void RemoveElements(IList<T> elements)
        {
            foreach (var element in elements)
            {
                if (element == Root)
                    throw new ArgumentException("It is not allowed to remove the root element");
            }

            RecordUndo("Remove nodes");

            var commonAncestors = BehaviorTreeNodeUtility.FindCommonAncestorsWithinList(elements);

            foreach (var element in commonAncestors)
            {
                element.Parent.Children.Remove(element);
                element.Parent = null;
            }

            BehaviorTreeNodeUtility.TreeToList(Root, _data);

            Changed();
        }

        public void AddRoot(T root)
        {
            if (root == null)
                throw new ArgumentNullException("root", "root is null");

            if (_data == null)
                throw new InvalidOperationException("Internal Error: data list is null");

            if (_data.Count != 0)
                throw new InvalidOperationException("AddRoot is only allowed on empty data list");

            root.ID = GenerateUniqueID();
            root.Depth = -1;
            _data.Add(root);
        }

        /// <summary>
        /// Adds an element directly underneath the root.
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(T element)
        {
            AddElement(element, Root);
        }

        public void AddElement(T element, BehaviorTreeNode parent)
        {
            if (element == null)
                throw new ArgumentNullException("element", "element is null");
            if (parent == null)
                throw new ArgumentNullException("parent", "parent is null");

            RecordUndo("Add node");

            if (parent.Children == null)
                parent.Children = new List<BehaviorTreeNode>();

            parent.Children.Add(element);
            element.Parent = parent;

            BehaviorTreeNodeUtility.UpdateDepthValues(parent);
            BehaviorTreeNodeUtility.TreeToList(Root, _data);

            Changed();
        }

        public void AddElements(IList<T> elements)
        {
            AddElements(elements, Root);
        }

        public void AddElements(IList<T> elements, BehaviorTreeNode parent)
        {
            if (elements == null)
                throw new ArgumentNullException("elements", "elements is null");
            if (elements.Count == 0)
                throw new ArgumentNullException("elements", "elements Count is 0: nothing to add");
            if (parent == null)
                throw new ArgumentNullException("parent", "parent is null");

            RecordUndo("Add nodes");

            if (parent.Children == null)
                parent.Children = new List<BehaviorTreeNode>();

            parent.Children.AddRange(elements.Cast<BehaviorTreeNode>());
            foreach (var element in elements)
            {
                element.Parent = parent;
                element.Depth = parent.Depth + 1;
                BehaviorTreeNodeUtility.UpdateDepthValues(element);
            }

            BehaviorTreeNodeUtility.TreeToList(Root, _data);

            Changed();
        }

        public void MoveElements(BehaviorTreeNode parentElement, int insertionIndex, List<BehaviorTreeNode> elements)
        {
            if (insertionIndex < 0)
                throw new ArgumentException("Invalid input: insertionIndex is -1, client needs to decide what index elements should be reparented at");

            // Invalid reparenting input
            if (parentElement == null)
                return;

            RecordUndo("Move nodes");

            // We are moving items so we adjust the insertion index to accomodate that any items above the insertion index is removed before inserting
            if (insertionIndex > 0)
                insertionIndex -= parentElement.Children.GetRange(0, insertionIndex).Count(elements.Contains);

            // Remove draggedItems from their parents
            foreach (var draggedItem in elements)
            {
                draggedItem.Parent.Children.Remove(draggedItem);    // remove from old parent
                draggedItem.Parent = parentElement;                 // set new parent
            }

            if (parentElement.Children == null)
                parentElement.Children = new List<BehaviorTreeNode>();

            // Insert dragged items under new parent
            parentElement.Children.InsertRange(insertionIndex, elements);

            BehaviorTreeNodeUtility.UpdateDepthValues(Root);
            BehaviorTreeNodeUtility.TreeToList(Root, _data);

            Changed();
        }

        void RecordUndo(string change)
        {
            Undo.RecordObject(_behaviorTreeAsset, change);
        }

        void Changed()
        {
            if (e_ModelChanged != null)
                e_ModelChanged();
        }
    }
}