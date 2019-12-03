// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/#post-2904879
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Team1_GraduationGame.Editor
{
    [CustomPropertyDrawer(typeof(NamedArrayAttribute))]
    public class NamedArrayDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            try
            {
                int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
                EditorGUI.PropertyField(rect, property, new GUIContent(((NamedArrayAttribute)attribute).names[pos]));
            }
            catch
            {
                EditorGUI.PropertyField(rect, property, label);
            }

        }
    }
}
#endif
