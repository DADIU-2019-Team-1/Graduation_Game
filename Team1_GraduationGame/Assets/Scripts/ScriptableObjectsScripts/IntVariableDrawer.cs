using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Team1_GraduationGame.Editor
{
    [CustomPropertyDrawer(typeof(IntVariable))]
    public class IntVariableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            // Position is the position of the InputField, here i displace x by 15, so the button is shown to the left of it. 
            var rect = new Rect(new Vector2(position.x - 15, position.y), Vector2.one * 15);
            EditorGUI.ObjectField(position, property, GUIContent.none);
            EditorGUI.EndProperty();
        }
    }
}
#endif
