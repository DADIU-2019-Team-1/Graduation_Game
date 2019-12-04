// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Events
{
    using System.Collections;
    using System.Collections.Generic;
    using Team1_GraduationGame.Events;
    using UnityEngine.SceneManagement;
    using UnityEngine;

    public class SoundEventRaiser : MonoBehaviour
    {
        public VoidEvent newGameEvent;
        public VoidEvent inMenuEvent;
        public VoidEvent inHubEvent;
        public VoidEvent inMemoryEvent;
        public VoidEvent menuButtonPressEvent;
        public FloatEvent musicSliderEvent;
        public FloatEvent sfxSliderEvent;

        private void Start()
        {
            Invoke("DelayedStart", 0.7f);
        }

        private void DelayedStart()
        {
            if (PlayerPrefs.GetInt("GameRanBool") == 0)
            {
                PlayerPrefs.SetFloat("SFXSliderSave", 10);
                PlayerPrefs.SetFloat("MusicSliderSave", 10);
                PlayerPrefs.SetInt("GameRanBool", 1);
            }

            SceneManager.sceneLoaded += OnSceneLoad;
            OnSceneLoad();

            musicSliderEvent?.Raise(PlayerPrefs.GetFloat("MusicSliderSave"));
            sfxSliderEvent?.Raise(PlayerPrefs.GetFloat("SFXSliderSave"));
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            OnSceneLoad();
        }

        private void OnSceneLoad()
        {
            UIMenu[] menuObjects = Resources.FindObjectsOfTypeAll<UIMenu>();
            if (menuObjects != null)
            {
                for (int i = 0; i < menuObjects.Length; i++)
                {
                    menuObjects[i].startGameEvent += NewGameEvent;
                    menuObjects[i].musicSliderEvent += MusicSliderEvent;
                    menuObjects[i].sfxSliderEvent += SFXSliderEvent;
                    menuObjects[i].menuButtonPressEvent += MenuButtonPressEvent;
                    menuObjects[i].gamePauseState += InMenuEvent;
                }
            }
        }

        private void InMenuEvent(bool isMenu)
        {
            if (isMenu)
                inMenuEvent?.Raise();
        }
        private void NewGameEvent()
        {
            newGameEvent?.Raise();
        }
        private void MusicSliderEvent(float value)
        {
            PlayerPrefs.SetFloat("MusicSliderSave", value);
            musicSliderEvent?.Raise(value);
        }
        private void SFXSliderEvent(float value)
        {
            PlayerPrefs.SetFloat("SFXSliderSave", value);
            sfxSliderEvent?.Raise(value);
        }
        public void InHubEvent()
        {
            inHubEvent?.Raise();
        }
        public void InMemoryEvent()
        {
            inMemoryEvent?.Raise();
        }
        public void MenuButtonPressEvent()
        {
            menuButtonPressEvent?.Raise();
        }
    }

}
