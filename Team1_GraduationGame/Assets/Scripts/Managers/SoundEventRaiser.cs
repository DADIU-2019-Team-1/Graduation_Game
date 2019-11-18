// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Events
{
    using System.Collections;
    using System.Collections.Generic;
    using Team1_GraduationGame.Events;
    using UnityEngine;

    public class SoundEventRaiser : MonoBehaviour
    {
        public VoidEvent newGameEvent;
        public VoidEvent inMenuEvent;
        public VoidEvent inHubEvent;
        public VoidEvent inMemoryEvent;
        public FloatEvent musicSliderEvent;
        public FloatEvent sfxSliderEvent;

        private void Start()
        {
            if (FindObjectOfType<HubMenu>() != null)
            {
                HubMenu tempHubMenu = FindObjectOfType<HubMenu>();
                tempHubMenu.startGameEvent += NewGameEvent;
                tempHubMenu.musicSliderEvent += MusicSliderEvent;
                tempHubMenu.sfxSliderEvent += SFXSliderEvent;
            }

            if (FindObjectOfType<InGameUI>() != null)
            {
                FindObjectOfType<InGameUI>().gamePauseState += InMenuEvent;
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
            musicSliderEvent?.Raise(value);
        }
        private void SFXSliderEvent(float value)
        {
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
    }

}
