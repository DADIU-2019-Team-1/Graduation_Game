using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
using Team1_GraduationGame.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Team1_GraduationGame.SaveLoadSystem
{
    [ExecuteInEditMode]
    public class SavePoint : MonoBehaviour
    {
        // References:
        [HideInInspector] public SavePointManager thisSavePointManager;

        // Public:
        public int thisID;
        [HideInInspector] public bool savePointUsed;
        public enum colliderTypes
        {
            None,
            Box,
            Capsule,
            Mesh,
            Sphere,
            Wheel
        }
        [HideInInspector] public colliderTypes attachCollider;

        public void Awake()
        {
            if (Application.isPlaying)
                if (transform.parent.gameObject.GetComponent<SavePointManager>() != null)
                    thisSavePointManager = transform.parent.gameObject.GetComponent<SavePointManager>();
        }

        private void OnTriggerEnter(Collider col)
        {
            if (Application.isPlaying)
            {
                if (col.tag == "Player" && !savePointUsed)
                {
                    thisSavePointManager.saveLoadManager.SaveGame();
                    savePointUsed = true;
                }
            }
        }

#if UNITY_EDITOR
        public void AttachCollider(int enumIndex)
        {
            Debug.Log("Attaching " + attachCollider + " collider to " + gameObject.name);
            switch (enumIndex)
            {
                case 0:
                    break;
                case 1:
                    gameObject.AddComponent<BoxCollider>();
                    break;
                case 2:
                    gameObject.AddComponent<CapsuleCollider>();
                    break;
                case 3:
                    gameObject.AddComponent<MeshCollider>();
                    break;
                case 4:
                    gameObject.AddComponent<SphereCollider>();
                    break;
                case 5:
                    gameObject.AddComponent<WheelCollider>();
                    break;
                default:
                    break;
            }

            GetComponent<Collider>().isTrigger = true;
        }

        void OnDestroy()
        {
            if (thisSavePointManager != null)
            {
                thisSavePointManager.savePoints.RemoveAt(thisID - 1);

                for (int i = 0; i < thisSavePointManager.savePoints.Count; i++)
                {
                    if (thisSavePointManager.savePoints[i].GetComponent<SavePoint>() != null)
                    {
                        thisSavePointManager.savePoints[i].GetComponent<SavePoint>().thisID = i + 1;
                        thisSavePointManager.savePoints[i].name = "SavePoint" + (i + 1);
                    }
                }
            }
        }
#endif
    }

    #region Custom Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(SavePoint))]
    public class SavePoint_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = target as SavePoint;

            if (script.gameObject.GetComponent<Collider>() == null)
            {
                EditorGUILayout.HelpBox("Please attach a collider below for this waypoint to work. Then scale it to your preferences.", MessageType.Warning);

                SerializedProperty colliderSelectProp = serializedObject.FindProperty("attachCollider");
                EditorGUILayout.PropertyField(colliderSelectProp);
                serializedObject.ApplyModifiedProperties();

                if ((int)script.attachCollider != 0)
                {
                    script.AttachCollider((int)script.attachCollider);
                    script.attachCollider = SavePoint.colliderTypes.None;
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif
    #endregion
}
