using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

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
    [CustomPropertyDrawer(typeof(Blackboard))]
    public class BlackboardDrawer : PropertyDrawer
    {
        const string SERIALIZED_BLACKBOARD_LIST_NAME = "SerializedPropertyList";    // Var name found at Blackboard.cs : Line 94
        const string SERIALIZED_BLACKBOARD_PROPERTY_NAME = "Name";                  // Var name found at Blackboard.cs : Line 90
        const string SERIALIZED_BLACKBOARD_PROPERTY_DATA = "Data";                  // Var name found at Blackboard.cs : Line 91

        const float DROPDOWN_BUTTON_HEIGHT = 20f;

        /// <summary>
        /// Used to adjust the property height of the Blackboard itself
        /// </summary>
        float _listHeight;
        float _lastArrayLength;

        Blackboard _blackboard;
        SerializedProperty _blackboardProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _blackboardProperty = property;
            _blackboard = GetBlackboardObject(_blackboardProperty.serializedObject.targetObject);

            EditorGUI.BeginProperty(position, label, property);

            var originalIndent = EditorGUI.indentLevel;

            var propertyNameList = property.FindPropertyRelative(SERIALIZED_BLACKBOARD_LIST_NAME).Copy();

            // Just in case it's not actually an array
            if (!propertyNameList.isArray)
            {
                Debug.LogError("The property list is not actually a list!\n"
                    + "Check the corresponding property in Blackboard.cs"
                    + "(Should be a list under serialization methods region).");
                return;
            }

            propertyNameList.Next(true);    // Skips generic field
            propertyNameList.Next(true);    // Advance to array size field

            int arrayLength = propertyNameList.intValue;    // We should be at array size field
            //Debug.Log("array length: " + arrayLength);

            propertyNameList.Next(true);    // Advance to first array index

            {
                Rect rect = new Rect(position.x, position.y, position.width, 20f);
                property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);
            }

            if (property.isExpanded)
            {
                if (!EditorApplication.isPlaying)
                {
                    position.y += DROPDOWN_BUTTON_HEIGHT;
                    Rect rect = new Rect(position.x, position.y, position.width, 20f);
                    if (EditorGUI.DropdownButton(rect, new GUIContent("Add Blackboard Property"), FocusType.Keyboard))
                    {
                        DoAddPropertyMenu();
                    }
                }

                EditorGUI.indentLevel++;

                _listHeight = 0;

                if (_lastArrayLength != arrayLength)
                {
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    _lastArrayLength = arrayLength;
                }

                // Display all properties in the dictionary
                for (int i = 0; i < arrayLength; ++i)
                {
                    var propertyName = propertyNameList.FindPropertyRelative(SERIALIZED_BLACKBOARD_PROPERTY_NAME);
                    var propertyData = propertyNameList.FindPropertyRelative(SERIALIZED_BLACKBOARD_PROPERTY_DATA);

                    if (i < arrayLength - 1)
                        propertyNameList.Next(false);

                    FieldInfo componentField = typeof(BlackboardProperties).GetField(propertyName.stringValue);
                    // CASE: The blackboard property got removed.
                    if (componentField == null)
                    {
                        _blackboard.Remove(propertyName.stringValue);
                        continue;
                    }

                    //Debug.Log("Name: " + propertyName.stringValue + "\tData: " + propertyData.type);
                    Type componentType = componentField.FieldType.GetGenericArguments()[0];
                    GUIContent name = new GUIContent(propertyName.stringValue);
                    var propertyHeight = EditorGUI.GetPropertyHeight(propertyData, name);
                    position.y += propertyHeight;
                    _listHeight += propertyHeight;

                    //Debug.Log(propertyData.objectReferenceValue);

                    Rect buttonRect = new Rect(position.x + 10f, position.y, 20f, 15f);
                    if (GUI.Button(buttonRect, "x"))
                    {
                        //Debug.Log("Pressed button for " + propertyName.stringValue);
                        _blackboard.Remove(propertyName.stringValue);
                    }

                    Rect rect = new Rect(buttonRect.x + buttonRect.width, position.y, position.width - buttonRect.width - 10f, 15f);
                    EditorGUI.BeginProperty(rect, name, propertyData);
                    propertyData.objectReferenceValue = EditorGUI.ObjectField(rect, name, propertyData.objectReferenceValue, componentType, true);
                    EditorGUI.EndProperty();
                }
            }

            EditorGUI.indentLevel = originalIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label) + DROPDOWN_BUTTON_HEIGHT;
            if (property.isExpanded) height += _listHeight;
            return height;
        }

        #region Add Property Menu Methods
        void DoAddPropertyMenu()
        {
            //Debug.Log("Doing add property menu.");
            GenericMenu addPropertyMenu = new GenericMenu();
            Type type = typeof(BlackboardProperties);
            foreach (FieldInfo field in type.GetFields())
            {
                //Debug.Log("Field name: " + field.Name + " Type: " + field.FieldType);
                AddMenuItemForBlackboardProperty(addPropertyMenu, field.Name, field);
            }

            addPropertyMenu.ShowAsContext();
        }

        void AddMenuItemForBlackboardProperty(GenericMenu menu, string menuPath, FieldInfo field)
        {
            menu.AddItem(new GUIContent(menuPath), false, OnBlackboardPropertySelected, field);
        }

        void OnBlackboardPropertySelected(object name)
        {
            Undo.RecordObject(_blackboardProperty.serializedObject.targetObject, "Add Blackboard Property");

            FieldInfo field = (FieldInfo)name;
            //Debug.Log("\tBlackboard field: " + blackboardField.Name);


            _blackboard.Set(field.Name, (UnityEngine.Object)null);
        }

        Blackboard GetBlackboardObject(UnityEngine.Object targetObj)
        {
            //Type blackboardPropertyType = field.FieldType.GetGenericArguments()[0];
            //Debug.Log("Field name: " + field.Name + " Type: " + blackboardPropertyType.Name);

            var blackboardField = targetObj.GetType().GetField(_blackboardProperty.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic);
            //Debug.Log("\tTarget obj: " + targetObj);

            //Debug.Log("\tBlackboard field: " + blackboardField.Name);
            if (blackboardField == null)
                Debug.LogError("Missing blackboard field for some reason.");

            return (Blackboard)blackboardField.GetValue(targetObj);
        }
        #endregion
    }
}