using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team1_GraduationGame.Managers
{


    public class SaveLoadManager : MonoBehaviour
    {
        private void Awake()
        {

        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown("l"))
            {
                SaveGame();
                Debug.Log("Save/Load Manager: Saved Game");
            }
            else if (Input.GetKeyDown("k"))
            {

            }
        }


        public void NewGame()
        {

        }

        public void SaveGame()
        {

        }

        public void LoadGame()
        {

        }



    }
}