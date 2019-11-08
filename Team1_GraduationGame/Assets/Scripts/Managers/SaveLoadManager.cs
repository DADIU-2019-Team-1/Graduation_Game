using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Team1_GraduationGame.SaveLoadSystem
{
    public class SaveLoadManager
    {
        private const string SAVE_SEPERATOR = "#SAVE-VALUE#";
        public bool newGame = true;
        public int firstSceneIndex = 0; // TODO: change to be first scene (should be 1 and main menu 0?) - YYY Change back after testing

        // References:
        private GameObject _player;
        private GameObject[] _enemies;


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
            //tempSaveString = "";
            //for (int i = 0; i < _enemies.Length; i++)
            //{
            //    tempSaveString += JsonUtility.ToJson(_enemies[i].transform) + SAVE_SEPERATOR;
            //}
            //PlayerPrefs.SetString("enemySave", tempSaveString);

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

        public void LoadGame()
        {
            string tempLoadString = "";
            _player = GameObject.FindWithTag("Player");

            if (PlayerPrefs.GetInt("previousGame") != 1)
                return;

            if (_player != null)
            {
                //// SavePoint State load: ////
                SavePoint[] tempSavePoints = GameObject.FindObjectsOfType<SavePoint>();
                tempLoadString = PlayerPrefs.GetString("savePointStateSave");

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

                //// Player load: ////
                tempLoadString = PlayerPrefs.GetString("playerSave");
                string[] tempDataString2 = tempLoadString.Split(new[] { SAVE_SEPERATOR }, System.StringSplitOptions.None);

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
    }
}