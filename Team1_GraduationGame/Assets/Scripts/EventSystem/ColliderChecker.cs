using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventManager))]
public class ColliderChecker : MonoBehaviour
{
    bool isActive = false;
    bool isTrigger = false;
    string tagName = "None";
    bool isColliding = false;
    float delayTime = 0f;
    string eventToFire = "CollisionPlaceholderEvent";
    GameObject eventManagerObj;

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

    void Update()
    {
        
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
                    eventManagerObj.GetComponent<EventManager>().Fire(eventToFire);

                }
                else if (col.tag == tagName)
                {
                    isColliding = true;
                    eventManagerObj.GetComponent<EventManager>().Fire(eventToFire);
                }
            }

            if (delayTime == 0f)
            {
                isActive = false;
            }
            else
            {
                StartCoroutine(ColCooldown());
            }
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
                    eventManagerObj.GetComponent<EventManager>().Fire(eventToFire);

                }
                else if (col.gameObject.tag == tagName)
                {
                    isColliding = true;
                    eventManagerObj.GetComponent<EventManager>().Fire(eventToFire);
                }

                if (delayTime == 0f)
                {
                    isActive = false;
                }
                else
                {
                    StartCoroutine(ColCooldown());
                }
            }
        }
    }

    public void SetUpColliderChecker(string eName, float dTime, bool triggerBool, GameObject eManObj, string tagString)
    {
        SetEventManagerObject(eManObj);
        SetEventName(eName);
        SetTag(tagString);
        SetDelayTime(dTime);
        isTrigger = triggerBool;
        isActive = true;
    }

    public void SetUpColliderChecker(string eName, float dTime, bool triggerBool, GameObject eManObj)
    {
        SetEventManagerObject(eManObj);
        SetEventName(eName);
        SetDelayTime(dTime);
        tagName = "None";
        isTrigger = triggerBool;
        isActive = true;
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

    IEnumerator ColCooldown()
    {
        yield return new WaitForSeconds(delayTime);
        isActive = true;
    }
}
