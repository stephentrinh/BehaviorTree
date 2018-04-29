using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/***************************************************************************************
*    Title: Behavior Tree
*    Author: Stephen Trinh
*    Date: 4/28/18
*    Code version: 1.0
*    Availability: https://github.com/stephentrinh/BehaviorTree
*
***************************************************************************************/

// https://www.codeproject.com/Articles/451326/Type-safe-blackboard-property-bag
namespace BehaviorTree
{
    /// <summary>
    /// Strongly typed property identifier for properties on a blackboard
    /// </summary>
    /// <typeparam name="T">The type of the property value it identifies</typeparam>
    [Serializable]
    public class BlackboardProperty<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// The name of the property.
        /// <remarks>
        /// Properties on the blackboard are stored by name, use caution NOT to have different 
        /// properties using the same name, as they will overwrite each others values if used on
        /// the same blackboard.
        /// </remarks>
        /// </summary>
        public string Name;

        public BlackboardProperty()
        {
            Name = Guid.NewGuid().ToString();
        }

        public BlackboardProperty(string name)
        {
            Name = name;
        }
    }

    [Serializable]
    public class Blackboard : ISerializationCallbackReceiver
    {
        Dictionary<string, UnityEngine.Object> _dictionary = new Dictionary<string, UnityEngine.Object>();

        public Dictionary<string, UnityEngine.Object> Dictionary { get { return _dictionary; } }

        public T Get<T>(BlackboardProperty<T> property) where T : UnityEngine.Object
        {
            if (!_dictionary.ContainsKey(property.Name))
                Debug.LogError("Property [" + property + "] is missing.");

            return (T)_dictionary[property.Name];
        }

        public void Set<T>(BlackboardProperty<T> property, T value) where T : UnityEngine.Object
        {
            _dictionary[property.Name] = value;
        }

        public void Set<T>(string propertyName, T value) where T : UnityEngine.Object
        {
            _dictionary[propertyName] = value;
        }

        public void Remove(string propertyName)
        {
            _dictionary.Remove(propertyName);
        }

        public override string ToString()
        {
            string bbStr = "Blackboard contains:";
            foreach (var item in _dictionary)
                bbStr += "\n\t" + item.ToString();
            return bbStr;
        }

        #region Serialization Methods
        [Serializable]
        public struct SerializableBlackboardData
        {
            [HideInInspector] public string Name;
            public UnityEngine.Object Data;
        }

        public List<SerializableBlackboardData> SerializedPropertyList;
        public void OnBeforeSerialize()
        {
            if (SerializedPropertyList == null) SerializedPropertyList = new List<SerializableBlackboardData>();
            if (_dictionary == null) _dictionary = new Dictionary<string, UnityEngine.Object>();

            SerializedPropertyList.Clear();
            SerializeDictionaryData(_dictionary);
        }

        void SerializeDictionaryData(Dictionary<string, UnityEngine.Object> dictionary)
        {
            foreach (var entry in dictionary)
            {
                var serializedPropertyData = new SerializableBlackboardData()
                {
                    Name = entry.Key,
                    Data = entry.Value
                };

                SerializedPropertyList.Add(serializedPropertyData);
            }
        }

        public void OnAfterDeserialize()
        {
            if (SerializedPropertyList.Count > 0)
                DeserializePropertyData();
            else
                _dictionary = new Dictionary<string, UnityEngine.Object>();
        }

        void DeserializePropertyData()
        {
            foreach (var data in SerializedPropertyList)
            {
                Set(data.Name, data.Data);
            }
        }
        #endregion

        //        #region Editor Methods
        //#if UNITY_EDITOR
        //        [MenuItem("GameObject/Behavior Tree/Blackboard", false, 10)]
        //        static void CreateBlackboardGameObject(MenuCommand menuCommand)
        //        {
        //            GameObject go = new GameObject("Blackboard");
        //            go.AddComponent<Blackboard>();
        //            go.transform.position = Vector3.zero;
        //            GameObjectUtility.SetParentAndAlign(go, (GameObject)menuCommand.context);
        //            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        //            Selection.activeObject = go;
        //        }

        //#endif

        //#endregion
    }


}