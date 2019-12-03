// Code Owner: Jannik Neerdal
#if UNITY_EDITOR
using UnityEditor;

namespace Team1_GraduationGame.Editor
{
    [CustomEditor(typeof(CameraLook))]
    public class CameraLook_Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var temp = target as CameraLook;
            temp.OnArrayChanged();
        }
    }
}
#endif