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

namespace STBehaviorTree
{
    [CreateAssetMenu(fileName = "BehaviorTreeAsset", menuName = "AI/Behavior Tree", order =0)]
    public class BehaviorTree : ScriptableObject, ISerializationCallbackReceiver
    {
        public BehaviorTreeNode Root = new BehaviorTreeRoot("Root", -1, 0);

        public BehaviorTreeNode CreateRoot()
        {
            return new BehaviorTreeRoot("Root", -1, 0);
        }

        public void TreeUpdate(Blackboard blackboard)
        {
            Debug.Assert(Root != null, "This behavior tree has no Root.", this);

            Root.NodeUpdate(blackboard);
        }

        #region Serialization Methods
        [Serializable]
        public struct SerializableNodeData
        {
            [HideInInspector] public string TypeAsString;
            public string Name;
            [HideInInspector] public int ID;
            [HideInInspector] public int Depth;
            [HideInInspector] public int ChildCount;
        }

        public List<SerializableNodeData> SerializedNodeList;
        public void OnBeforeSerialize()
        {
            //Debug.Log("Serializing Behavior Tree.");
            if (SerializedNodeList == null) SerializedNodeList = new List<SerializableNodeData>();
            if (Root == null) Root = CreateRoot();

            SerializedNodeList.Clear();
            SerializeNodeData(Root);
        }

        void SerializeNodeData(BehaviorTreeNode node)
        {
            //Debug.Log("Serializing: " + node.ToString());
            if (node.Children == null)
                node.Children = new List<BehaviorTreeNode>();
            var serializedNodeData = new SerializableNodeData()
            {
                TypeAsString = node.GetType().ToString(),
                Name = node.Name,
                ID = node.ID,
                Depth = node.Depth,
                ChildCount = node.Children.Count
            };

            SerializedNodeList.Add(serializedNodeData);
            for (int i = 0; i < node.Children.Count; ++i)
                SerializeNodeData(node.Children[i]);
        }

        public void OnAfterDeserialize()
        {
            //Debug.Log("Deserializing Behavior Tree.");

            if (SerializedNodeList.Count > 0)
                DeserializeNodeData(0, out Root);
            else
                Root = CreateRoot();
        }

        int DeserializeNodeData(int index, out BehaviorTreeNode node)
        {
            var serializedNode = SerializedNodeList[index];

            //Debug.Log("Deserializing: " + serializedNode.ToString());

            Type nodeType = Type.GetType(serializedNode.TypeAsString);
            BehaviorTreeNode newNode = (BehaviorTreeNode)Activator.CreateInstance(nodeType,
                serializedNode.Name, serializedNode.Depth, serializedNode.ID);

            for (int i = 0; i < serializedNode.ChildCount; ++i)
            {
                BehaviorTreeNode childNode;
                index = DeserializeNodeData(++index, out childNode);
                newNode.Children.Add(childNode);
            }

            node = newNode;
            return index;
        }
        #endregion
    }
}