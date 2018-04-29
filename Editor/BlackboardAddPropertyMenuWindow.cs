using System;
using System.Collections;
using System.Collections.Generic;
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

namespace STBehaviorTree
{
    public class BlackboardAddPropertyMenuWindow : EditorWindow
    {
        GenericMenu _menu;

        static void Init()
        {
            EditorWindow window = GetWindow<BlackboardAddPropertyMenuWindow>();
            window.position = new Rect(50f, 50f, 200f, 24f);
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Add Blackboard Property");
        }

        void AddMenuItemForBlackboardProperty<T>(GenericMenu menu, string menuPath, BlackboardProperty<T> property) where T : Component
        {
            menu.AddItem(new GUIContent(menuPath), true, OnBlackboardPropertySelected, property);
        }

        void OnBlackboardPropertySelected(object name)
        {
        }

        private void OnGUI()
        {
            Type type = typeof(BlackboardProperties);
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            {
                AddMenuItemForBlackboardProperty(_menu, field.Name, (BlackboardProperty<Component>)field.GetValue(null));
            }
        }

        void InitIfNeeded()
        {
            if (_menu == null) _menu = new GenericMenu();
        }
    }
}