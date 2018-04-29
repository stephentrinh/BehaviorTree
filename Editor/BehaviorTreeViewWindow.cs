using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections;
using System.Collections.Generic;

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
    public class BehaviorTreeViewWindow : EditorWindow
    {
        [NonSerialized] bool _initialized;
        [SerializeField] TreeViewState _treeViewState;
        [SerializeField] MultiColumnHeaderState _multiColumnHeaderState;

        public BehaviorTreeView<BehaviorTreeNode> TreeView { get; private set; }
        public BehaviorTree BehaviorTreeAsset { get; private set; }
        SearchField _searchField;

        const string WINDOW_NAME = "Behavior Tree";
        const string ICON_PATH = "BehaviorTree/Icons/ICON_BehaviorTreeWindow.png";
        const string WINDOW_TOOLTIP = "Used for viewing and editing a Behavior Tree Asset.";

        [MenuItem("Window/Behavior Tree/Behavior Tree Editor")]
        public static BehaviorTreeViewWindow GetWindow()
        {
            var window = GetWindow<BehaviorTreeViewWindow>();
            window.titleContent = new GUIContent(WINDOW_NAME, (Texture2D)EditorGUIUtility.Load(ICON_PATH), WINDOW_TOOLTIP);
            window.Focus();
            window.Repaint();
            return window;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var behaviorTreeAsset = EditorUtility.InstanceIDToObject(instanceID) as BehaviorTree;
            if (behaviorTreeAsset != null)
            {
                var window = GetWindow();
                window.SetTreeAsset(behaviorTreeAsset);
                return true;
            }

            return false;
        }

        void SetTreeAsset(BehaviorTree behaviorTreeAsset)
        {
            BehaviorTreeAsset = behaviorTreeAsset;
            _initialized = false;
        }

        Rect behaviorTreeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        }
        Rect toolbarRect
        {
            get { return new Rect(20f, 10f, position.width - 40f, 20f); }
        }
        Rect bottomToolbarRect
        {
            get { return new Rect(20f, position.height - 18f, position.width - 40f, 16f); }
        }

        void InitIfNeeded()
        {
            if (!_initialized)
            {
                if (_treeViewState == null)
                    _treeViewState = new TreeViewState();

                bool firstInit = _multiColumnHeaderState == null;
                var headerState = BehaviorTreeView<BehaviorTreeNode>.CreateDefaultMultiColumnHeaderState(behaviorTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);
                _multiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new BehaviorTreeModel<BehaviorTreeNode>(GetData(), BehaviorTreeAsset);

                if (TreeView != null)
                {
                    TreeView.UnregisterCallbacks();
                    TreeView.TreeModel.e_ModelChanged -= SaveAsset;
                }

                TreeView = new BehaviorTreeView<BehaviorTreeNode>(_treeViewState, multiColumnHeader, treeModel);
                TreeView.RegisterCallbacks();
                TreeView.TreeModel.e_ModelChanged += SaveAsset;

                TreeView.multiColumnHeader.canSort = false;
                TreeView.multiColumnHeader.height = MultiColumnHeader.DefaultGUI.minimumHeight;

                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;

                _initialized = true;
            }
        }

        IList<BehaviorTreeNode> GetData()
        {
            if (BehaviorTreeAsset != null)
            {
                //if (_behaviorTreeAsset.Root.Children.Count == 0)
                //{
                //    Debug.Log("Creating Children.");
                //    var treeModel = TreeView.TreeModel;
                //    treeModel.AddElement(new MoveToDestination("Move to Point", 0, treeModel.GenerateUniqueID()));
                //    treeModel.AddElement(new MoveToDestination("Move to Point2", 0, treeModel.GenerateUniqueID()));
                //}

                List<BehaviorTreeNode> nodes = new List<BehaviorTreeNode>();
                BehaviorTreeNodeUtility.TreeToList(BehaviorTreeAsset.Root, nodes);
                return nodes;
            }

            var empty = new List<BehaviorTreeNode>();
            empty.Add(new BehaviorTreeRoot("Root", -1, 0));
            return empty;
        }

        private void OnSelectionChange()
        {
            if (!_initialized)
                return;

            var treeAsset = Selection.activeObject as BehaviorTree;
            if (treeAsset != null && treeAsset != BehaviorTreeAsset)
            {
                BehaviorTreeAsset = treeAsset;
                TreeView.TreeModel.SetData(GetData());
                TreeView.Reload();
            }
        }

        private void OnGUI()
        {
            InitIfNeeded();

            SearchBar(toolbarRect);
            DoTreeView(behaviorTreeViewRect);
            BottomToolbar(bottomToolbarRect);
        }

        void SearchBar(Rect rect)
        {
            TreeView.searchString = _searchField.OnGUI(rect, TreeView.searchString);
        }

        void DoTreeView(Rect rect)
        {
            TreeView.OnGUI(rect);
        }

        void BottomToolbar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("Expand All", style))
                {
                    TreeView.ExpandAll();
                }

                if (GUILayout.Button("Collapse All", style))
                {
                    TreeView.CollapseAll();
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label(BehaviorTreeAsset != null ? AssetDatabase.GetAssetPath(BehaviorTreeAsset) : string.Empty);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Open Node Window", style))
                {
                    BehaviorTreeNodeWindow.GetWindow();
                }
            }

            GUILayout.EndArea();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoCallback;
        }

        private void OnDestroy()
        {
            if (TreeView != null)
                TreeView.UnregisterCallbacks();

            Undo.undoRedoPerformed -= OnUndoCallback;
        }

        void OnUndoCallback()
        {
            if (TreeView == null)
                return;

            TreeView.TreeModel.SetData(GetData());
            TreeView.Reload();
            SaveAsset();
        }

        void SaveAsset()
        {
            EditorUtility.SetDirty(BehaviorTreeAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}