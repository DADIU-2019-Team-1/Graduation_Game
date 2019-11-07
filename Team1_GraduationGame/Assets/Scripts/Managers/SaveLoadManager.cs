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

        // References:
        private GameObject _player;
        private GameObject[] _enemies;


#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown("l"))
            {
                LoadGame();
                Debug.Log("Save/Load Manager: Loaded Game");
            }
            else if (Input.GetKeyDown("k"))
            {
                SaveGame();
                Debug.Log("Save/Load Manager: Saved Game");
            }
        }
#endif

        public void NewGame()
        {
            PlayerPrefs.SetInt("previousGame", 1);
            PlayerPrefs.SetInt("currentScene", 1);

            Scene startScene = SceneManager.GetSceneAt(1);
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

        public void SaveGame()
        {
            string tempSaveString = "";
            _player = GameObject.FindWithTag("Player");

            //// Player save: ////
            if (_player != null)
            {
                tempSaveString = JsonUtility.ToJson(_player.transform);
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

            //// Scene save: ////
            tempSaveString = JsonUtility.ToJson(SceneManager.GetActiveScene().buildIndex);
            PlayerPrefs.SetInt("currentScene", int.Parse(tempSaveString));


            Debug.Log("Save/Load Manager: Succesfully saved the game");
        }

        public void LoadGame()
        {
            string tempLoadString = "";
            _player = GameObject.FindWithTag("Player");

            if (PlayerPrefs.GetInt("previousGame") != 1)
                return;

            //// Player load: ////
            if (_player != null)
            {
                tempLoadString = PlayerPrefs.GetString("playerSave");
                Transform tempPlayerTransform = JsonUtility.FromJson<Transform>(tempLoadString);
                _player.transform.position = tempPlayerTransform.position;
                _player.transform.rotation = tempPlayerTransform.rotation;
            }
            else
            {
                Debug.Log("Save/Load Manager: Failed to load the game. Player not found");
                return;
            }
        }
    }
}