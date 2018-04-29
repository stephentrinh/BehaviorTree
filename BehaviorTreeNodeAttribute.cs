using System;
using System.Reflection;
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
    public struct AttributeTypePair
    {
        public string attributeName;
        public Type classType;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BehaviorTreeNodeAttribute : Attribute
    {
        public BehaviorTreeNodeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class BehaviorTreeNodeAttributeHandler
    {
        const string BEHAVIOR_TREE_NODE_ASSEMBLY_NAME = "BehaviorTree-ASM";

        public static void LoadAttributes(ref List<AttributeTypePair> names)
        {
            Assembly loadedAssembly = Assembly.Load(BEHAVIOR_TREE_NODE_ASSEMBLY_NAME);

            Type[] types = loadedAssembly.GetTypes();
            for (int i = 0; i < types.Length; ++i)
            {
                object[] attributes = types[i].GetCustomAttributes(
                    typeof(BehaviorTreeNodeAttribute),
                    false);

                for (int j = 0; j < attributes.Length; ++j)
                {
                    BehaviorTreeNodeAttribute nodeAtt = attributes[j] as BehaviorTreeNodeAttribute;
                    AttributeTypePair pair;
                    pair.attributeName = nodeAtt.Name;
                    pair.classType = types[i];
                    names.Add(pair);
                }
            }
        }
    }
}