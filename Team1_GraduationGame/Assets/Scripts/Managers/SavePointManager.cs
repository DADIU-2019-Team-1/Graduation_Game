using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
using Team1_GraduationGame.Interaction;
using UnityEditor;
using UnityEngine;

namespace Team1_GraduationGame.SaveLoadSystem
{
    public class SavePointManager : MonoBehaviour
    {
        // References:
        public SaveLoadManager saveLoadManager;

        // Public
        public bool drawGizmos = true;
        [HideInInspector] public List<GameObject> savePoints;


        public void Awake()
        {
            saveLoadManager = new SaveLoadManager();

            if (PlayerPrefs.GetInt("loadGameOnAwake") == 1)
            {
                PlayerPrefs.SetInt("loadGameOnAwake", 0);
                saveLoadManager.LoadGame();
            }
        }

        public void NewGame()
        {
            saveLoadManager?.NewGame();
        }

        public void Continue()
        {
            saveLoadManager?.ContinueGame();
        }

        public void SaveGame()
        {
            saveLoadManager?.SaveGame();
        }

        public void LoadGame()
        {
            saveLoadManager?.LoadGame();
        }

        public void NextLevel()
        {
            saveLoadManager?.NextLevel();
        }

#if UNITY_EDITOR
        public void AddSavePoint()
        {
            if (Application.isEditor)
            {
                GameObject tempSavePoint;

                tempSavePoint = new GameObject("SavePoint" + (savePoints.Count + 1));
                tempSavePoint.AddComponent<SavePoint>();
                tempSavePoint.transform.position = gameObject.transform.position;
                tempSavePoint.transform.parent = transform;
                tempSavePoint.layer = 2;

                savePoints.Add(tempSavePoint);
                tempSavePoint.GetComponent<SavePoint>().thisID = savePoints.Count;
                tempSavePoint.GetComponent<SavePoint>().thisSavePointManager = gameObject.GetComponent<SavePointManager>();
            }
        }

        private void OnDrawGizmos()
        {
            if (drawGizmos && Application.isEditor)
                if (savePoints != null)
                {
                    Gizmos.color = Color.magenta;
                    Handles.color = Color.red;

                    for (int i = 0; i < savePoints.Count; i++)
                    {
                        Gizmos.DrawWireSphere(savePoints[i].transform.position, 1.0f);
                        Handles.Label(savePoints[i].transform.position + (Vector3.up * 1.0f), "SavePoint " + i);

                        if (savePoints[i].GetComponent<Collider>() != null)
                        {
                            Gizmos.color = Color.white;
                            Collider tempCollider = savePoints[i].GetComponent<Collider>();
                            Gizmos.DrawWireCube(tempCollider.bounds.center, savePoints[i].GetComponent<Collider>().bounds.size);
                        }
                    }
                }
        }
#endif

    }

    #region Custom Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(SavePointManager))]
    public class SavePointManager_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = target as SavePointManager;

            EditorGUILayout.Space();

            if (script.savePoints != null)
            {
                EditorGUILayout.LabelField(script.savePoints.Count.ToString() + " SavePoints Active");
            }
            else
            {
                EditorGUILayout.LabelField("0 SavePoints Active");
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Please only create new savepoints by using the 'Add SavePoint' button. IMPORTANT: The first savepoint must be at the player start position of the level!", MessageType.Info);

            if (GUILayout.Button("Add SavePoint"))
            {
                script.AddSavePoint();
            }

            if (GUI.changed)
                EditorUtility.SetDirty(script);
        }
    }
#endif
    #endregion
}
