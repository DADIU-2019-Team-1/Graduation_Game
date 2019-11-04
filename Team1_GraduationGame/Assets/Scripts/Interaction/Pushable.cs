using System.Collections;
using System.Collections.Generic;
using Team1_GraduationGame.Enemies;
using UnityEngine;
using UnityEngine.Events;

namespace Team1_GraduationGame.Interaction
{
    public class Pushable : MonoBehaviour
    {
        // References:
        private Movement _movement;
        private Enemy _thisEnemy;
        private GameObject _player;

        // Public:
        public bool useEvents;
        public UnityEvent eventOnInteraction;

        // Private:
        private bool _isEnemy;

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_player != null)
                if (_player.GetComponent<Movement>() != null)
                    _movement = _player.GetComponent<Movement>();

            if (gameObject.GetComponent<Enemy>() != null)
            {
                _isEnemy = true;
                _thisEnemy = GetComponent<Enemy>();
            }
        }

        public void Interact()
        {
            if (useEvents)
                eventOnInteraction.Invoke();

            if (_isEnemy)
                _thisEnemy.PushDown();
        }
    }
}
