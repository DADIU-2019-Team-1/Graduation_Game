﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
 #if UNITY_EDITOR
[CustomPropertyDrawer(typeof(IntReference))]
public class IntReferenceDrawer : PropertyDrawer
{
public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {   
        bool useUnique = property.FindPropertyRelative("UseUnique").boolValue;

        // Choosing the settings icon for the drawer. 
        GUIContent popupIcon = EditorGUIUtility.IconContent("_Popup");
        //propertyUI = EditorGUILayout.BeginFoldoutHeaderGroup(propertyUI, "Property UI");
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        // Position is the position of the InputField, here i displace x by 15, so the button is shown to the left of it. 
        var rect = new Rect(new Vector2(position.x -15, position.y), Vector2.one * 15);
        
        // Code for the actual button, and the menu inside containing Unique and Variable. 
        if (
            EditorGUI.DropdownButton(rect,
                popupIcon, 
                FocusType.Keyboard , 
                new GUIStyle() { fixedWidth = 50f, border = new RectOffset(1,1,1,1)}))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Unique"), 
            useUnique,
            () => SetProperty(property, true));


            menu.AddItem(new GUIContent("Variable"),
            !useUnique,
            () => SetProperty(property,false));

            menu.ShowAsContext();

            
        }

        //position.position = Vector2.right;
        int value = property.FindPropertyRelative("UniqueValue").intValue;
        // If Unique chosen in menu
        if(useUnique) {
            string newValue = EditorGUI.TextField(position, value.ToString());
            int.TryParse(newValue, out value);
            property.FindPropertyRelative("UniqueValue").intValue = value;
        }
        // If Variable is chosen
        else {
            EditorGUI.ObjectField(position, property.FindPropertyRelative("Variable"), GUIContent.none);
        }
        EditorGUI.EndProperty();
        //EditorGUILayout.EndFoldoutHeaderGroup(); 
/*         showMenu = EditorGUILayout.Toggle("Use Unique", showMenu);
        if(showMenu) {
            useUnique = false;
        }
        else
            useUnique = true; */
    }
    // Function to get the boolean from the IntReference script, and sets it from this script. 
    private void SetProperty(SerializedProperty property, bool value) {
        var propRelative = property.FindPropertyRelative("UseUnique");
        propRelative.boolValue = value;
        property.serializedObject.ApplyModifiedProperties();
    }
}
#endif