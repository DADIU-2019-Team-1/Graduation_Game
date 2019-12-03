// Script by Jakob Elkjær Husted
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team1_GraduationGame.Events
{
    public class ColliderChecker : MonoBehaviour
    {
        private bool isActive = false;
        private bool isTrigger = false;
        private string tagName = "None";
        private bool isColliding = false, checkMotionState = false;
        private float delayTime = 0f;
        private string eventToFire = "CollisionPlaceholderEvent";
        private int ignoreState = 1;
        private GameObject eventManagerObj, player;
        private EventManager parentEventManager;
        private Movement playerMovement;

        void Start()
        {
            if (gameObject.GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<Collider>();
            }

            if (isTrigger)
            {
                gameObject.GetComponent<Collider>().isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider col)
        {

            if (isActive)
            {
                if (isTrigger)
                {
                    if (tagName == "None")
                    {
                        isColliding = true;
                        parentEventManager?.Fire(eventToFire);
                        DelayHandler();

                    }
                    else if (col.tag == tagName)
                    {
                        if (!checkMotionState)
                        {
                            isColliding = true;
                            parentEventManager?.Fire(eventToFire);
                            DelayHandler();
                        }
                        else
                        {
                            if (playerMovement != null)
                            {
                                if (playerMovement.moveState.value <= ignoreState)
                                {
                                    // Don't do anything
                                }
                                else
                                {
                                    isColliding = true;
                                    parentEventManager?.Fire(eventToFire);
                                    DelayHandler();
                                }
                            }
                        }
                    }
                }
            }

        }

        private void DelayHandler()
        {
            if (delayTime >= 0.15f)
            {
                isActive = false;
            }
            else
            {
                Invoke("ColCooldown", delayTime);
            }
        }

        private void OnCollisionEnter(Collision col)
        {
            if (isActive)
            {
                if (isTrigger == false)
                {
                    if (tagName == "None")
                    {
                        isColliding = true;
                        parentEventManager?.Fire(eventToFire);
                        DelayHandler();

                    }
                    else if (col.gameObject.tag == tagName)
                    {
                        isColliding = true;
                        parentEventManager?.Fire(eventToFire);
                        DelayHandler();
                    }
                }
            }
        }

        public void SetUpColliderChecker(string eName, float dTime, bool triggerBool, GameObject eManObj, string tagString, bool checkMotion)
        {
            SetEventManagerObject(eManObj);
            SetEventName(eName);
            SetTag(tagString);
            SetDelayTime(dTime);
            isTrigger = triggerBool;
            isActive = true;
            checkMotionState = checkMotion;

            player = GameObject.FindGameObjectWithTag("Player");
            playerMovement = player?.GetComponent<Movement>();
            parentEventManager = eventManagerObj.GetComponent<EventManager>();
        }

        public void SetUpColliderChecker(string eName, float dTime, bool triggerBool, GameObject eManObj)
        {
            SetEventManagerObject(eManObj);
            SetEventName(eName);
            SetDelayTime(dTime);
            tagName = "None";
            isTrigger = triggerBool;
            isActive = true;

            player = GameObject.FindGameObjectWithTag("Player");
            playerMovement = player?.GetComponent<Movement>();
            parentEventManager = eventManagerObj.GetComponent<EventManager>();
        }

        public bool IsColliding()
        {
            return isColliding;
        }

        public void SetTag(string tagString)
        {
            tagName = tagString;
        }

        public void SetEventName(string eName)
        {
            eventToFire = eName;
        }

        public void SetDelayTime(float delay)
        {
            delayTime = delay;
        }

        private void SetEventManagerObject(GameObject eManObj)
        {
            eventManagerObj = eManObj;
        }

        private void ColCooldown()
        {
            isActive = true;
        }
    }

}