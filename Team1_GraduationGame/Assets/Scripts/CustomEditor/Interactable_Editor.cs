// Script by Jakob Elkjær Husted
#if UNITY_EDITOR
using Team1_GraduationGame.Enemies;
using Team1_GraduationGame.Interaction;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

namespace Team1_GraduationGame.Editor
{
    [CustomEditor(typeof(Interactable))]
    public class Interactable_Editor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {

            // DrawDefaultInspector();

            var script = target as Interactable;

            if (script.gameObject.GetComponent<Enemy>() != null)
            {
                if (script.thisEnemy == null)
                {
                    script.thisEnemy = script.gameObject.GetComponent<Enemy>();
                }
            }

            if (script.gameObject.GetComponent<Enemy>() == null && script.GetComponent<NavMeshObstacle>() == null)
            {
                if (script.gameObject.tag != "Interactable")
                    script.gameObject.tag = "Interactable";
                NavMeshObstacle tempNavMeshObs = script.gameObject.AddComponent<NavMeshObstacle>();
                tempNavMeshObs.carving = true;
                tempNavMeshObs.carveOnlyStationary = false;
            }

            // Bools:
            script.interactableOnce = EditorGUILayout.Toggle("Run Once?", script.interactableOnce);
            if (!script.interactableOnce)
            {
                script.useCooldown = EditorGUILayout.Toggle("Use Cooldown?", script.useCooldown);
                if (script.useCooldown)
                    script.interactCooldown = EditorGUILayout.FloatField("Time (sec)", script.interactCooldown);
            }

            // Sound emit:
            DrawUILine(false);
            if (script.thisEnemy == null)
            {
                EditorGUILayout.HelpBox("Use the 'sound manager' script if you want to play actual sounds", MessageType.None);
                script.emitSound = EditorGUILayout.Toggle("Emit Sound on Use?", script.emitSound);

                if (script.emitSound)
                    script.soundEmitDistance = EditorGUILayout.FloatField("Sound Distance", script.soundEmitDistance);
            }

            // Conditions:
            DrawUILine(false);
            script.interactConditions = EditorGUILayout.Toggle("Interaction Conditions?", script.interactConditions);

            if (script.interactConditions)
            {
                script.checkForObstructions =
                    EditorGUILayout.Toggle("Check for obstructions?", script.checkForObstructions);

                if (script.interactionDistance == null)
                {
                    script.minDistance = EditorGUILayout.FloatField("Distance", script.minDistance);
                }

                SerializedProperty distanceProp = serializedObject.FindProperty("interactionDistance");
                EditorGUILayout.PropertyField(distanceProp);

                if (script.thisEnemy == null && script.pushable)
                {
                    if (script.interactionAngle == null)
                    {
                        script.angle = EditorGUILayout.FloatField("Angle", script.angle);
                    }

                    SerializedProperty angleProp = serializedObject.FindProperty("interactionAngle");
                    EditorGUILayout.PropertyField(angleProp);
                }
            }

            // Pushable:
            DrawUILine(false);
            script.pushable = EditorGUILayout.Toggle("Pushable?", script.pushable);

            if (script.pushable)
            {
                if (script.gameObject.GetComponent<ObjectPush>() == null && script.GetComponent<Enemy>() == null)
                {
                    script.gameObject.AddComponent<ObjectPush>();
                }
            }

            // Events:
            DrawUILine(false);
            script.useEvents = EditorGUILayout.Toggle("Use Events?", script.useEvents);

            if (script.useEvents)
            {
                SerializedProperty eventProp = serializedObject.FindProperty("eventOnInteraction");
                EditorGUILayout.PropertyField(eventProp);
                DrawUILine(false);
            }

            // Animation
            DrawUILine(false);
            script.useAnimation = EditorGUILayout.Toggle("Use Animation?", script.useAnimation);

            if (script.useAnimation)
            {
                script.switchBetweenAnimations = EditorGUILayout.Toggle("Switch between animations?", script.switchBetweenAnimations);
                if (script.switchBetweenAnimations)
                    script.animationDefault = EditorGUILayout.TextField("Animation Default", script.animationDefault);

                script.animationAction = EditorGUILayout.TextField("Animation Action", script.animationAction);
                DrawUILine(false);
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
                thickness = 4;
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