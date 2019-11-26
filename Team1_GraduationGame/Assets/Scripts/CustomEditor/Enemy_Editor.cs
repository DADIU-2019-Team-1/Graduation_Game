// Script by Jakob Elkjær Husted
using Team1_GraduationGame.Enemies;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Team1_GraduationGame.Editor
{
    [CustomEditor(typeof(Enemy))]
    public class Enemy_Editor : UnityEditor.Editor
    {
        private GUIStyle _style = new GUIStyle();
        private GameObject _parentWayPoint;
        private bool _runOnce;

        public override void OnInspectorGUI()
        {
            if (!_runOnce)
            {
                _style.fontStyle = FontStyle.Bold;
                _style.alignment = TextAnchor.MiddleCenter;
                _style.fontSize = 14;
                _runOnce = true;
            }

            EditorGUILayout.HelpBox("Please only use the 'Add WayPoint' button to create new waypoints. They can then be found in the 'WayPoints' object in the hierarchy.\nAlso make sure that every enemy is on the 'Enemies' layer", MessageType.Info);

            DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as Enemy;

            if (script.useWaitTime)
            {
                script.useGlobalWaitTime = EditorGUILayout.Toggle("Use Global Wait Time?", script.useGlobalWaitTime);
                EditorGUILayout.HelpBox("If a wait time is specified on the waypoint it will override the global wait time value", MessageType.None);
            }
            if (script.useGlobalWaitTime && script.useWaitTime)
                script.waitTime = EditorGUILayout.FloatField("Global Wait Time", script.waitTime);

            script.activateOnDistance = EditorGUILayout.Toggle("Activate On Distance?", script.activateOnDistance);

            if (script.activateOnDistance)
                script.activationDistance = EditorGUILayout.FloatField("Activation Distance", script.activationDistance);

            DrawUILine(false);

            if (script.wayPoints != null)
            {
                if (script.wayPoints.Count == 0)
                {
                    _style.normal.textColor = Color.red;
                }
                else
                {
                    _style.normal.textColor = Color.green;
                }

                EditorGUILayout.LabelField(script.wayPoints.Count.ToString() + " waypoints active", _style);
            }
            else
            {
                _style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("0 waypoints active", _style);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add WayPoint"))
            {
                script.AddWayPoint();
            }

            if (GUILayout.Button("Remove WayPoint"))
            {
                script.RemoveWayPoint();
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }

        #region DrawUILine function
        public static void DrawUILine(bool start)
        {
            Color color = new Color(1, 1, 1, 0.3f);
            int thickness = 1;
            if (start)
                thickness = 7;
            int padding = 8;

            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        #endregion
    }
}
#endif