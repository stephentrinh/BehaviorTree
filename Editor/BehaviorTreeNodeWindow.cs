using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    public class BehaviorTreeNodeWindow : EditorWindow
    {
        [NonSerialized] bool _initialized = false;
        [SerializeField] TreeViewState _treeViewState;

        BehaviorTreeNodeModelView _treeView;
        SearchField _searchField;

        const string WINDOW_NAME = "Nodes";
        const string ICON_PATH = "BehaviorTree/Icons/ICON_BehaviorTreeWindow";
        const string WINDOW_TOOLTIP = "Used for adding nodes to a behavior tree.";


        public static BehaviorTreeNodeWindow GetWindow()
        {
            var window = GetWindow<BehaviorTreeNodeWindow>();
            window.titleContent = new GUIContent(WINDOW_NAME, (Texture2D)EditorGUIUtility.Load(ICON_PATH), WINDOW_TOOLTIP);
            window.Focus();
            window.Repaint();
            return window;
        }

        private void OnEnable()
        {
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();

            _treeView = new BehaviorTreeNodeModelView(_treeViewState);
            _searchField = new SearchField();
            _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;
        }

        Rect nodeTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }
        Rect toolbarRect
        {
            get { return new Rect(20, 10, position.width - 40, 20); }
        }
        Rect bottomToolbarRect
        {
            get { return new Rect(20, position.height - 18, position.width - 40, 16); }
        }

        void InitIfNeeded()
        {
            if (!_initialized)
            {
                if (_treeViewState == null)
                    _treeViewState = new TreeViewState();

                _treeView = new BehaviorTreeNodeModelView(_treeViewState);

                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;

                _initialized = true;
            }
        }

        private void OnGUI()
        {
            InitIfNeeded();

            SearchBar(toolbarRect);
            DoTreeView(nodeTreeViewRect);
            DoBottomToolbar(bottomToolbarRect);
        }

        private void SearchBar(Rect rect)
        {
            _treeView.searchString = _searchField.OnGUI(rect, _treeView.searchString);
        }

        void DoTreeView(Rect rect)
        {
            _treeView.OnGUI(rect);
        }

        void DoBottomToolbar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                GUILayout.FlexibleSpace();
                var possibleSelection = _treeView.GetSelectionFromIndices();
                EditorGUI.BeginDisabledGroup(!(possibleSelection != null && possibleSelection.Data != null));
                if (GUILayout.Button("Add Node", style))
                {
                    AddNode(possibleSelection);
                }
                EditorGUI.EndDisabledGroup();
            }

            GUILayout.EndArea();
        }

        public void AddNode(BehaviorTreeNodeItem selectedItem)
        {
            BehaviorTreeViewWindow btWindow = BehaviorTreeViewWindow.GetWindow();

            if (btWindow.BehaviorTreeAsset == null)
            {
                Debug.LogError("Open a behavior tree asset before trying to add a node.");
                return;
            }

            BehaviorTreeModel<BehaviorTreeNode> btModel = btWindow.TreeView.TreeModel;

            var selectedIDsInBT = btWindow.TreeView.GetSelection();
            // Case: No node is selected
            if (selectedIDsInBT.Count == 0)
            {
                BehaviorTreeNode nodeToAdd = (BehaviorTreeNode)Activator.CreateInstance(
                    selectedItem.Data, selectedItem.displayName, 0, btModel.GenerateUniqueID());

                //Debug.Log("Adding node: " + nodeToAdd.ToString() + " to Root");
                btModel.AddElement(nodeToAdd);
                return;
            }

            for (int i = 0; i < selectedIDsInBT.Count; ++i)
            {
                //Debug.Log(selectedIDsInBT[i]);
                BehaviorTreeNode possibleParent = btModel.Find(selectedIDsInBT[i]);
                if (possibleParent != null)
                {
                    BehaviorTreeNode nodeToAdd = (BehaviorTreeNode)Activator.CreateInstance(
                         selectedItem.Data, selectedItem.displayName, 0, btModel.GenerateUniqueID());
                    //Debug.Log("Adding node: " + nodeToAdd.ToString() + " to: " + possibleParent.ToString());
                    btModel.AddElement(nodeToAdd, possibleParent);
                }
                // Just in case the ID wasn't found
                else
                {
                    BehaviorTreeNode nodeToAdd = (BehaviorTreeNode)Activator.CreateInstance(
                        selectedItem.Data, selectedItem.displayName, 0, btModel.GenerateUniqueID());

                    //Debug.Log("Adding node: " + nodeToAdd.ToString() + " to Root");
                    btModel.AddElement(nodeToAdd);
                    return;
                }
            }
        }
    }
}