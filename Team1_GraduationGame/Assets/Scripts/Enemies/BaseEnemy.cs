namespace Team1_GraduationGame.Enemies
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/New Enemy")]
    public class BaseEnemy : ScriptableObject
    {
        public bool canRun = true;
        public float walkSpeed;
        public float walkTurnSpeed;

        public float runSpeed;
        public float runTurnSpeed;
        public float AccelerationTime;
        public float DeAccelerationTime;

        public float fieldOfView;
        public float viewDistance;
        public float hearingDistance;
        [Tooltip("In seconds")] public float aggroTime;

        [Tooltip("0 means they cannot be pushed down")]
        public float pushedDownDuration; // If 0, they cannot be pushed down
        public float embraceDistance;
        public float embraceDelay;

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BaseEnemy))]
    public class BaseEnemy_Inspector : UnityEditor.Editor
    {
        private GUIStyle _style = new GUIStyle();
        private GameObject _parentWayPoint;
        private bool _runOnce;

        public override void OnInspectorGUI()
        {
            if (!_runOnce)
            {
                _style.fontStyle = FontStyle.Bold;
                _style.normal.textColor = Color.white;
                _style.alignment = TextAnchor.MiddleCenter;
                _style.fontSize = 12;
                _runOnce = true;
            }

            // DrawDefaultInspector(); // for other non-HideInInspector fields

            var script = target as BaseEnemy;

            script.canRun = EditorGUILayout.Toggle("Can Run?", script.canRun);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Walking", _style);
            EditorGUILayout.Space();

            script.walkSpeed = EditorGUILayout.FloatField("Walk Speed", script.walkSpeed);
            script.walkTurnSpeed = EditorGUILayout.FloatField("Walk Turn Speed", script.walkTurnSpeed);

            DrawUILine(true);

            if (script.canRun)
            {
                EditorGUILayout.LabelField("Running", _style);
                EditorGUILayout.Space();

                script.runSpeed = EditorGUILayout.FloatField("Run Speed", script.runSpeed);
                script.runTurnSpeed = EditorGUILayout.FloatField("Run Turn Speed", script.runTurnSpeed);
                script.AccelerationTime = EditorGUILayout.FloatField("Acceleration", script.AccelerationTime);
                script.DeAccelerationTime = EditorGUILayout.FloatField("DeAcceleration", script.DeAccelerationTime);

                DrawUILine(true);
            }

            EditorGUILayout.LabelField("General", _style);
            EditorGUILayout.Space();

        script.fieldOfView = EditorGUILayout.FloatField("Field Of View", script.fieldOfView);
        script.viewDistance = EditorGUILayout.FloatField("View Distance", script.viewDistance);
        script.hearingDistance = EditorGUILayout.FloatField("Hearing Distance", script.hearingDistance);
            script.aggroTime = EditorGUILayout.FloatField("Aggro Time (sec)", script.aggroTime);
        script.pushedDownDuration = EditorGUILayout.FloatField("Pushed Down Duration", script.pushedDownDuration);
        script.embraceDistance = EditorGUILayout.FloatField("Embrace Distance", script.embraceDistance);
        script.embraceDelay = EditorGUILayout.FloatField("Embrace Delay", script.embraceDelay);

            DrawUILine(true);

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
#endif

}
