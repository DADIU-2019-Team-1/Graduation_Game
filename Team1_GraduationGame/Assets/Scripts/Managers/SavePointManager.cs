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
        }

        public void AddSavePoint()
        {
            GameObject tempSavePoint;

            tempSavePoint = new GameObject("SafePoint" + (savePoints.Count + 1));
            tempSavePoint.AddComponent<SavePoint>();
            tempSavePoint.transform.position = gameObject.transform.position;
            tempSavePoint.transform.parent = transform;
            tempSavePoint.layer = 2;

            savePoints.Add(tempSavePoint);
            tempSavePoint.GetComponent<SavePoint>().thisID = savePoints.Count;
            tempSavePoint.GetComponent<SavePoint>().thisSavePointManager = gameObject.GetComponent<SavePointManager>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawGizmos)
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

            if (GUILayout.Button("Add SafePoint"))
            {
                script.AddSavePoint();
            }
        }
    }
#endif
    #endregion
}
