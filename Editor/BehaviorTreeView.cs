using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System;
using UnityEditor;
using System.Linq;
using UnityEngine.Assertions;

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
    public class TreeViewItem<T> : TreeViewItem where T : BehaviorTreeNode
    {
        public T data { get; set; }

        public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
        {
            this.data = data;
        }
    }

    public class BehaviorTreeView<T> : TreeView where T : BehaviorTreeNode
    {
        const float ROW_HEIGHTS = 20f;
        const float TOGGLE_WIDTH = 18f;

        public BehaviorTreeModel<T> TreeModel { get; private set; }
        readonly List<TreeViewItem> _rows = new List<TreeViewItem>(100);
        public event Action e_TreeChanged;

        public IList<int> SelectedIndices { get; private set; }
        public event Action<IList<TreeViewItem>> e_BeforeDroppingDraggedItems;

        enum ViewColumns
        {
            kIcon,
            kType,
            kName
        }

        public BehaviorTreeView(TreeViewState state, BehaviorTreeModel<T> model) : base(state)
        {
            Init(model);
        }

        public BehaviorTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, BehaviorTreeModel<T> model)
            : base(state, multiColumnHeader)
        {
            Init(model);

            rowHeight = ROW_HEIGHTS;
            columnIndexForTreeFoldouts = 1;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (ROW_HEIGHTS - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = TOGGLE_WIDTH;

            Reload();
        }

        void Init(BehaviorTreeModel<T> model)
        {
            TreeModel = model;
        }

        public void RegisterCallbacks()
        {
            TreeModel.e_ModelChanged += ModelChanged;
        }

        public void UnregisterCallbacks()
        {
            TreeModel.e_ModelChanged -= ModelChanged;
        }

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; --i)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                    for (int i = current.children.Count - 1; i >= 0; --i)
                        stack.Push(current.children[i]);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            int depthForHiddenRoot = -1;
            return new TreeViewItem<T>(TreeModel.Root.ID, depthForHiddenRoot, TreeModel.Root.Name, TreeModel.Root);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (TreeModel.Root == null)
                Debug.LogError("Tree model root is null. Did you call SetData()?");

            _rows.Clear();
            if (!string.IsNullOrEmpty(searchString))
                Search(TreeModel.Root, searchString, _rows);
            else
                if (TreeModel.Root.HasChildren)
                    AddChildrenRecursive(TreeModel.Root, 0, _rows);

            SetupParentsAndChildrenFromDepths(root, _rows);

            return _rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<BehaviorTreeNode>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (ViewColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<BehaviorTreeNode> item, ViewColumns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            BehaviorTreeNode node = item.data;
            switch (column)
            {
                case ViewColumns.kIcon:
                    {
                        GUI.DrawTexture(cellRect, (Texture2D)EditorGUIUtility.Load(node.GetIconPath()), ScaleMode.ScaleToFit);
                    }
                    break;
                case ViewColumns.kName:
                    {
                        Rect toggleRect = cellRect;
                        toggleRect.x += GetContentIndent(item);
                        toggleRect.width = TOGGLE_WIDTH;
                        if (toggleRect.xMax < cellRect.xMax)
                            item.data.DEBUG_on = EditorGUI.Toggle(toggleRect, item.data.DEBUG_on);

                        args.rowRect = cellRect;
                        args.label = item.data.Name;
                        base.RowGUI(args);
                    }
                    break;
                case ViewColumns.kType:
                    {
                        cellRect.x += GetContentIndent(item);
                        DefaultGUI.Label(cellRect, node.GetType().ToString().Split('.')[1], args.selected, args.focused);
                    }
                    break;
            }
        }

        void ModelChanged()
        {
            if (e_TreeChanged != null)
                e_TreeChanged();

            Reload();
        }

        void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem> newRows)
        {
            for (int i = 0; i < parent.Children.Count; ++i)
            {
                T child = (T)parent.Children[i];
                var item = new TreeViewItem<T>(child.ID, depth, child.Name, child);
                newRows.Add(item);

                if (child.HasChildren)
                {
                    if (IsExpanded(child.ID))
                        AddChildrenRecursive(child, depth + 1, newRows);
                    else
                        item.children = CreateChildListForCollapsedParent();
                }
            }
        }

        #region Key Events
        protected override void KeyEvent()
        {
            base.KeyEvent();
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
            {
                DeleteSelectedIDs();
            }
        }
        #endregion

        #region Selection
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        void DeleteSelectedIDs()
        {
            TreeModel.RemoveElements(GetSelection());
            SetSelection(new List<int>());
        }
        #endregion

        #region Searching
        void Search(T searchFromThis, string search, List<TreeViewItem> result)
        {
            if (string.IsNullOrEmpty(search))
                throw new ArgumentException("Invalid search: cannot be null or empty.", "search");

            const int ITEM_DEPTH = 0;

            Stack<T> stack = new Stack<T>();
            for (int i = 0; i < searchFromThis.Children.Count; ++i)
                stack.Push((T)searchFromThis.Children[i]);

            while (stack.Count > 0)
            {
                T current = stack.Pop();

                if (current.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    result.Add(new TreeViewItem<T>(current.ID, ITEM_DEPTH, current.Name, current));

                if (current.Children != null && current.Children.Count > 0)
                    for (int i = 0; i < current.Children.Count; ++i)
                        stack.Push((T)current.Children[i]);
            }
            SortSearchResult(result);
        }

        protected virtual void SortSearchResult(List<TreeViewItem> rows)
        {
            rows.Sort((x, y) => UnityEditor.EditorUtility.NaturalCompare(x.displayName, y.displayName));
        }

        protected override IList<int> GetAncestors(int id)
        {
            return TreeModel.GetAncestors(id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return TreeModel.GetDescendantsThatHaveChildren(id);
        }
        #endregion

        #region Dragging
        const string GENERIC_DRAG_ID = "GenericDragColumnDragging";

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (hasSearch)
                return;

            DragAndDrop.PrepareStartDrag();
            var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            DragAndDrop.SetGenericData(GENERIC_DRAG_ID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { };
            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var draggedRows = DragAndDrop.GetGenericData(GENERIC_DRAG_ID) as List<TreeViewItem>;
            if (draggedRows == null)
                return DragAndDropVisualMode.None;

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                    {
                        bool validDrag = ValidDrag(args.parentItem, draggedRows);
                        if (args.performDrop && validDrag)
                        {
                            T parentData = ((TreeViewItem<T>)args.parentItem).data;
                            OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                        }
                        return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                    }
                case DragAndDropPosition.OutsideItems:
                    {
                        if (args.performDrop)
                            OnDropDraggedElementsAtIndex(draggedRows, TreeModel.Root, TreeModel.Root.Children.Count);

                        return DragAndDropVisualMode.Move;
                    }
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        public virtual void OnDropDraggedElementsAtIndex(List<TreeViewItem> draggedRows, T parent, int insertIndex)
        {
            if (e_BeforeDroppingDraggedItems != null)
                e_BeforeDroppingDraggedItems(draggedRows);

            var draggedElements = new List<BehaviorTreeNode>();
            for (int i = 0; i < draggedRows.Count; ++i)
                draggedElements.Add(((TreeViewItem<T>) draggedRows[i]).data);

            var selectedIDs = draggedElements.Select(x => x.ID).ToArray();
            TreeModel.MoveElements(parent, insertIndex, draggedElements);
            SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
        }

        bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            TreeViewItem currentParent = parent;
            while (currentParent != null)
            {
                if (draggedItems.Contains(currentParent))
                    return false;
                currentParent = currentParent.parent;
            }
            return true;
        }
        #endregion

        #region Renaming
        protected override bool CanRename(TreeViewItem item)
        {
            Rect renameRect = GetRenameRect(treeViewRect, 0, item);
            return renameRect.width > 30;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (args.acceptedRename)
            {
                var element = TreeModel.Find(args.itemID);
                element.Name = args.newName;
                ModelChanged();
            }
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            cellRect.x += cellRect.width;
            return base.GetRenameRect(cellRect, row, item);
        }
        #endregion

        #region Misc
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent((Texture2D)EditorGUIUtility.Load("BehaviorTree/Icons/ICON_BehaviorTreeTypeIcon.png"), "Icon"),
                    contextMenuText = "Asset",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Type"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(ViewColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
        #endregion
    }
}