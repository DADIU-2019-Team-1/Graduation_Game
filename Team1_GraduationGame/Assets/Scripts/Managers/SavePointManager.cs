using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
using Team1_GraduationGame.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Team1_GraduationGame.SaveLoadSystem
{
    public class SavePointManager : MonoBehaviour
    {
        // References:
        public SaveLoadManager saveLoadManager;

        // Public
        public bool drawGizmos = true;
        public List<GameObject> savePoints;


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
                    Gizmos.color = Color.green;
                    Handles.color = Color.red;

                    for (int i = 0; i < savePoints.Count; i++)
                    {
                        Gizmos.DrawWireSphere(savePoints[i].transform.position, 1.0f);
                        Handles.Label(savePoints[i].transform.position + (Vector3.up * 2.0f), "SavePoint " + i);
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

            EditorGUILayout.HelpBox("Please only create new savepoints by using the 'Add SavePoint' button", MessageType.Info);

            if (GUILayout.Button("Add SavePoint"))
            {
                script.AddSavePoint();
            }
        }
    }
#endif
    #endregion
}
