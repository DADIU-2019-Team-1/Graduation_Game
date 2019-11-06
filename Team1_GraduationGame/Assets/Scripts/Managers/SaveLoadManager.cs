using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Team1_GraduationGame.Managers
{
    public class SaveLoadManager : MonoBehaviour
    {
        private const string SAVE_SEPERATOR = "#SAVE-VALUE#";
        public bool newGame = true;

        // References:
        private GameObject _player;
        //private GameObject[] _enemies;


        private void Awake()
        {
            _player = GameObject.FindWithTag("Player");
            //_enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (PlayerPrefs.GetInt("sceneLoaded") == 2)
            {
                Scene startScene = SceneManager.GetSceneAt(1);
                SceneManager.LoadScene(startScene.buildIndex);
                PlayerPrefs.SetInt("sceneLoaded", 0);
            }

        }

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
            PlayerPrefs.SetInt("sceneLoaded", 2);
            SaveGame();
        }

        public void ContinueGame()
        {
            if (PlayerPrefs.GetInt("previousGame") == 1)
                LoadGame();
            else
                Debug.Log("Save/Load Manager: No previous games to load");
        }

        public void SaveGame()
        {
            string tempSaveString = "";

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

            //// Scene save: //// TODO: Should not be here
            //tempSaveString = JsonUtility.ToJson(SceneManager.GetActiveScene().buildIndex);
            //PlayerPrefs.SetString("sceneSave", tempSaveString);


            Debug.Log("Save/Load Manager: Succesfully saved the game");
        }

        public void LoadGame()
        {
            string tempLoadString = "";

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

            //// Load scene: //// TODO: Should not be here
            //tempLoadString = PlayerPrefs.GetString("sceneSave");
            //Scene tempScene = JsonUtility.FromJson<Scene>(tempLoadString);
            //SceneManager.LoadScene(tempScene.buildIndex);

        }



    }
}