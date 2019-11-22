// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.SaveLoadSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using Team1_GraduationGame.Enemies;
    using Team1_GraduationGame.Interaction;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.SceneManagement;

    public class SaveLoadManager
    {
        private const string SAVE_SEPERATOR = "#SAVE-VALUE#";
        public bool newGame = true;
        public int firstSceneIndex = 1;

        // References:
        private GameObject _player;
        private GameObject[] _enemies;
        private GameObject[] _interactables;
        private Big[] _bigs;


        public void NewGame()
        {
            PlayerPrefs.SetInt("currentScene", firstSceneIndex);
            PlayerPrefs.SetInt("loadGameOnAwake", 0);

            Scene startScene = SceneManager.GetSceneAt(firstSceneIndex);
            SceneManager.LoadScene(startScene.buildIndex);
        }

        public void ContinueGame()
        {
            if (PlayerPrefs.GetInt("previousGame") == 1)
            {
                PlayerPrefs.SetInt("loadGameOnAwake", 1);
                SceneManager.LoadScene(PlayerPrefs.GetInt("currentScene"));
            }
            else
                Debug.Log("Save/Load Manager: No previous games to load");
        }

        public void OpenLevel(int atBuildIndex)
        {
            if (atBuildIndex < SceneManager.sceneCountInBuildSettings && atBuildIndex >= 0)
            {
                SceneManager.LoadScene(atBuildIndex);
            }
        }

        public void NextLevel()
        {
            if (SceneManager.GetActiveScene().buildIndex != SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                Debug.Log("Error: There is no next scene!");
            }
        }

        public void SaveGame()
        {
            string tempSaveString = "";
            _player = GameObject.FindWithTag("Player");

            //// Player save: ////
            if (_player != null)
            {
                Vector3 tempPlayerPosition = _player.transform.position;
                Quaternion tempPlayerRotation = _player.transform.rotation;
                tempSaveString = JsonUtility.ToJson(tempPlayerPosition) + SAVE_SEPERATOR + JsonUtility.ToJson(tempPlayerRotation);
                PlayerPrefs.SetString("playerSave", tempSaveString);
            }
            else
            {
                Debug.Log("Save/Load Manager: Failed to save the game. Player not found");
                return;
            }

            //// Enemy save: ////
            tempSaveString = "";
            _enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (_enemies != null)
            {
                EnemyContainer tempEnemyContainer = new EnemyContainer();

                for (int i = 0; i < _enemies.Length; i++)
                {
                    Enemy tempEnemyComponent = _enemies[i].GetComponent<Enemy>();
                    tempEnemyContainer.pos = _enemies[i].transform.position;
                    tempEnemyContainer.rot = _enemies[i].transform.rotation;
                    tempEnemyContainer.isAggro = tempEnemyComponent.GetAggro();
                    tempEnemyContainer.currentWayPoint = tempEnemyComponent.GetCurrentWaypoint();
                    tempEnemyContainer.lastSighting = tempEnemyComponent.GetLastSighting();

                    tempSaveString += JsonUtility.ToJson(tempEnemyContainer) + SAVE_SEPERATOR;
                }
                PlayerPrefs.SetString("enemySave", tempSaveString);
            }

            //// Interactable save: ////
            tempSaveString = "";
            _interactables = GameObject.FindGameObjectsWithTag("Interactable");

            if (_interactables != null)
            {
                InteractableContainer tempInteractableContainer = new InteractableContainer();

                for (int i = 0; i < _interactables.Length; i++)
                {
                    tempInteractableContainer.pos = _interactables[i].transform.position;
                    tempInteractableContainer.toggleState =
                        _interactables[i].GetComponent<Interactable>().toggleState;

                    tempSaveString += JsonUtility.ToJson(tempInteractableContainer) + SAVE_SEPERATOR;
                }

                PlayerPrefs.SetString("interactableSave", tempSaveString);
            }

            //// SavePoint State: ////
            SavePoint[] tempSavePoints = GameObject.FindObjectsOfType<SavePoint>();
            tempSaveString = "";

            if (tempSavePoints != null)
            {
                List<SavePointContainer> tempSavePointContainerList = new List<SavePointContainer>();

                for (int i = 0; i < tempSavePoints.Length; i++)
                {
                    SavePointContainer tempSavePointContainer = new SavePointContainer();

                    tempSavePointContainer.savePointUsed = tempSavePoints[i].savePointUsed;
                    tempSavePointContainer.thisID = tempSavePoints[i].thisID;

                    tempSavePointContainerList.Add(tempSavePointContainer);

                    tempSaveString = tempSaveString + SAVE_SEPERATOR + JsonUtility.ToJson(tempSavePointContainerList[i]);
                }

                PlayerPrefs.SetString("savePointStateSave", tempSaveString);
            }

            //// Scene save: ////
            PlayerPrefs.SetInt("currentScene", SceneManager.GetActiveScene().buildIndex);

            PlayerPrefs.SetInt("previousGame", 1);

            Debug.Log("Save/Load Manager: Succesfully saved the game");
        }

        /// <summary>
        /// Load game to savepoint (checkpoint reached in scene). This does not load to scene, use 'Continue' for that.
        /// </summary>
        /// <param name="loadSavePointState">Only set true if loading using 'continue' button</param>
        public void LoadGame(bool loadSavePointState)
        {
            if (loadSavePointState)
            {
                // SavePoint State load:
                SavePoint[] tempSavePoints = GameObject.FindObjectsOfType<SavePoint>();
                string tempLoadString = PlayerPrefs.GetString("savePointStateSave");

                if (tempSavePoints != null)
                {
                    string[] tempDataString1 = tempLoadString.Split(new[] { SAVE_SEPERATOR }, System.StringSplitOptions.None);
                    List<SavePointContainer> tempSavePointContainers = new List<SavePointContainer>();

                    for (int i = 1; i < tempDataString1.Length; i++) // Must start at 1
                    {
                        SavePointContainer tempSavePointContainer =
                            JsonUtility.FromJson<SavePointContainer>(tempDataString1[i]);

                        for (int j = 0; j < tempSavePoints.Length; j++)
                        {
                            if (tempSavePointContainer.thisID == tempSavePoints[j].thisID)
                                tempSavePoints[j].savePointUsed = tempSavePointContainer.savePointUsed;
                        }
                    }
                }

                LoadGame();
            }
            else
            {
                LoadGame();
            }
        }

        /// <summary>
        /// Load game to savepoint (checkpoint reached in scene). This does not load to scene, use 'Continue' for that.
        /// </summary>
        public void LoadGame()
        {
            string tempLoadString = "";
            _player = GameObject.FindWithTag("Player");

            if (PlayerPrefs.GetInt("previousGame") != 1)
                return;

            if (_player != null)
            {

                //// Enemy load: ////
                tempLoadString = PlayerPrefs.GetString("enemySave");
                string[] tempDataString2 = tempLoadString.Split(new[] { SAVE_SEPERATOR }, System.StringSplitOptions.None);

                _enemies = GameObject.FindGameObjectsWithTag("Enemy");
                _bigs = GameObject.FindObjectsOfType<Big>();

                if (_enemies != null)
                {
                    EnemyContainer tempEnemyContainer = new EnemyContainer();

                    for (int i = 0; i < _enemies.Length; i++)
                    {
                        tempEnemyContainer = JsonUtility.FromJson<EnemyContainer>(tempDataString2[i]);
                        Enemy tempEnemyComponent = _enemies[i].GetComponent<Enemy>();

                        NavMeshAgent tempNavMeshAgent = _enemies[i].GetComponent<NavMeshAgent>();
                        tempNavMeshAgent.updatePosition = false;
                        tempNavMeshAgent.updateRotation = false;
                        tempNavMeshAgent.Warp(tempEnemyContainer.pos);
                        _enemies[i].transform.rotation = tempEnemyContainer.rot;

                        tempEnemyComponent.ResetEnemy();
                        tempEnemyComponent.SetAggro(tempEnemyContainer.isAggro);
                        tempEnemyComponent.SetCurrentWaypoint(tempEnemyContainer.currentWayPoint);
                        tempEnemyComponent.SetLastSighting(tempEnemyContainer.lastSighting);

                        tempNavMeshAgent.updatePosition = true;
                        tempNavMeshAgent.updateRotation = true;
                    }

                    for (int i = 0; i < _bigs.Length; i++)
                    {
                        _bigs[i].ResetBig();
                    }
                }

                //// Pushable/interactable load: ////
                _interactables = GameObject.FindGameObjectsWithTag("Interactable");

                if (_interactables != null)
                {
                    tempLoadString = PlayerPrefs.GetString("interactableSave");
                    tempDataString2 = tempLoadString.Split(new[] { SAVE_SEPERATOR }, System.StringSplitOptions.None);

                    InteractableContainer tempInteractableContainer = new InteractableContainer();

                    for (int i = 0; i < _interactables.Length; i++)
                    {
                        tempInteractableContainer = JsonUtility.FromJson<InteractableContainer>(tempDataString2[i]);
                        _interactables[i].transform.position = tempInteractableContainer.pos;
                        _interactables[i].GetComponent<Interactable>().toggleState =
                            tempInteractableContainer.toggleState;
                    }
                }

                //// Player load: ////
                tempLoadString = PlayerPrefs.GetString("playerSave");
                tempDataString2 = tempLoadString.Split(new[] { SAVE_SEPERATOR }, System.StringSplitOptions.None);

                Vector3 tempPlayerPosition = JsonUtility.FromJson<Vector3>(tempDataString2[0]);
                Quaternion tempPlayerRotation = JsonUtility.FromJson<Quaternion>(tempDataString2[1]);

                _player.transform.position = tempPlayerPosition;
                _player.transform.rotation = tempPlayerRotation;
            }
            else
            {
                Debug.Log("Save/Load Manager: Failed to load the game. Player not found");
                return;
            }

            Debug.Log("Save/Load Manager: Succesfully loaded the game");

        }

        public class SavePointContainer
        {
            public bool savePointUsed;
            public int thisID;
        }

        public class EnemyContainer
        {
            public Vector3 pos;
            public Quaternion rot;
            public bool isAggro;
            public int currentWayPoint;
            public Vector3 lastSighting;
        }

        public class InteractableContainer
        {
            public Vector3 pos;
            public bool toggleState;
        }
    }
}