using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team1_GraduationGame.Managers
{
    public class SaveLoadManager : MonoBehaviour
    {

        private const string SAVE_SEPERATOR = "#SAVE-VALUE#";
        public bool newGame = true;

        private void Awake()
        {

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
            SaveGame();
        }

        public void ContinueGame()
        {
            if (PlayerPrefs.GetInt("previousGame") == 1)
                LoadGame();
            else
                Debug.Log("LoadGame: No previous games to load");
        }

        public void SaveGame()
        {

        }

        public void LoadGame()
        {
            if (PlayerPrefs.GetInt("previousGame") != 1)
                return;

        }



    }
}