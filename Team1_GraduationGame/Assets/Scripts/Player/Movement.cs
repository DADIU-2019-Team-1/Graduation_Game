using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody playerRB;
    private bool touchStart = false;
    private Vector3 initTouchPos;
    private Vector3 currTouchPos;

    public Transform stick;
    public Transform stickLimit;
    public FloatReference movementSpeed;
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Needs checker to only move while on left side of screen. 
        if(Input.GetMouseButtonDown(0)) {
            initTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));

            stickLimit.transform.position = initTouchPos * -1;
            stick.transform.position = initTouchPos * -1;

            stick.GetComponent<SpriteRenderer>().enabled = true;
            stickLimit.GetComponent<SpriteRenderer>().enabled = true;
        }
        if(Input.GetMouseButton(0)) {
            touchStart = true;
            currTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        }
        else {
            touchStart = false;
        }
    }

    void FixedUpdate() {
        if(touchStart) {
            
            Vector3 offset = new Vector3(currTouchPos.x-initTouchPos.x, 0, currTouchPos.y - initTouchPos.y);
            Vector3 direction = Vector3.ClampMagnitude(offset, 1.0f);
            movePlayer(direction * -1);

            stick.transform.position = new Vector3(initTouchPos.x + direction.x, initTouchPos.z + direction.z) * -1;
        }
        else {
            stick.GetComponent<SpriteRenderer>().enabled = false;
            stickLimit.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private void movePlayer(Vector3 direction) {
        Quaternion rotation = direction != Vector3.zero
            ? Quaternion.LookRotation(direction) : Quaternion.identity; // Shorthand if : else
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * movementSpeed.value);
        playerRB.AddForce(direction * movementSpeed.value);
        Debug.Log("Direction vector: " + direction);
        //playerRB.MovePosition((Vector2) transform.position + (direction * movementSpeed.value * Time.deltaTime));
    }
}
