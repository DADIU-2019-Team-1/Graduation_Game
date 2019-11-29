// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.SaveLoadSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Team1_GraduationGame.Enemies;
    using Team1_GraduationGame.Events;
    using Team1_GraduationGame.Interaction;
    using Team1_GraduationGame.UI;
    using UnityEngine.SceneManagement;
    using UnityEngine.Playables;
    using UnityEngine;
    using TMPro;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class SavePointManager : MonoBehaviour
    {
        // References:
        public SaveLoadManager saveLoadManager;
        public PlayableDirector _playableDirector;
        private WhiteFadeController _whiteFadeCtrl;

        // Public
        public int firstSceneBuildIndex = 0;
        public bool drawGizmos = true;
        [HideInInspector] public List<GameObject> savePoints;
        [HideInInspector] public int previousCheckPoint = 1;


        public void Awake()
        {
            saveLoadManager = new SaveLoadManager();
            saveLoadManager.firstSceneIndex = firstSceneBuildIndex;
            _whiteFadeCtrl = FindObjectOfType<WhiteFadeController>();
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt("previousGame") == 1)
            {
                GameObject continueTextObj = GameObject.FindGameObjectWithTag("ContinueBtn");
                if (continueTextObj != null)
                {
                    TextMeshProUGUI continueText = continueTextObj.GetComponent<TextMeshProUGUI>();
                    continueText.color = Color.white;
                }
            }

            if (PlayerPrefs.GetInt("loadGameOnAwake") == 1)
            {
                if (_playableDirector != null)
                {
                    _playableDirector.time = _playableDirector.duration;
                }

                PlayerPrefs.SetInt("loadGameOnAwake", 0);

                _whiteFadeCtrl?.RaiseFadeEvent();

                saveLoadManager.LoadGame(true);
            }

            UIMenu[] menuObjects = Resources.FindObjectsOfTypeAll<UIMenu>();
            if (menuObjects != null)
            {
                for (int i = 0; i < menuObjects.Length; i++)
                {
                    menuObjects[i].continueGameEvent += Continue;
                }
            }
        }

        public void DisableSavingOnSavePoints()
        {
            if (savePoints != null && Application.isPlaying)    // Should be called when playing (for debugging)
                for (int i = 0; i < savePoints.Count; i++)
                {
                    if (savePoints[i].GetComponent<SavePoint>() != null)
                    {
                        savePoints[i].GetComponent<SavePoint>().savingDisabled = true;
                    }
                }
        }

        public void TeleportToSavePoint(int savePointNumber)
        {
            if (savePoints.ElementAtOrDefault(savePointNumber - 1))
            {
                if (GameObject.FindGameObjectWithTag("Player") != null)
                {
                    GameObject tempPlayer = GameObject.FindGameObjectWithTag("Player");

                    if (tempPlayer != null)
                    {
                        tempPlayer.GetComponent<Movement>().Frozen(false);
                    }

                    tempPlayer.transform.position =
                        savePoints[savePointNumber - 1].transform.position + transform.up;
                }
            }
        }

        public void LoadToPreviousCheckpoint()
        {
            if (savePoints.ElementAtOrDefault(previousCheckPoint - 1))
            {
                if (GameObject.FindGameObjectWithTag("Player") != null)
                {
                    GameObject tempPlayer = GameObject.FindGameObjectWithTag("Player");

                    LoadGame();

                    tempPlayer.GetComponent<Movement>().Frozen(false);
                }
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

        public void ResetLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

#if UNITY_EDITOR
        public void AddSavePoint()
        {
            if (Application.isEditor)
            {
                GameObject tempSavePoint;

                if (savePoints == null)
                    savePoints = new List<GameObject>();

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
                        Handles.Label(savePoints[i].transform.position + (Vector3.up * 1.0f), "SavePoint " + (i + 1));

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

            EditorGUILayout.HelpBox("IF PREFAB: Deleting SavePoints will only work if prefab is unpacked!", MessageType.Warning);

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
