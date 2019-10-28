using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    private Rigidbody playerRB;
    private bool touchStart = false, canMove = false;
    private Vector3 initTouchPos;
    private Vector3 currTouchPos;

    private Vector3 joystickPos;

    public Transform stick;
    public Transform stickLimit;
    public FloatReference movementSpeed;

    public FloatReference sneakSpeed;

    public FloatReference runSpeed;

    public IntReference radius;
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

            // Needs to be screenspace, not world space? Not true, needs to be in worldspace, in relation to the camera. 
            // Use a canvas to spawn Joystick UI in Rect space? Keeps relative to camera, enables 2 panels, one for action, one for movement too.

            // Joystick anchor where you initially press. Circle around is the sneak/Walk indicator, if you drag outside it starts running.
            // Movement function which changes between the 3 based on thresholds. Make script, then fit it with UI, so we can disable it. 
            // Movement keeps working, as long as initial position is on the left side of the screen. 

            stickLimit.transform.position = Input.mousePosition; /* initTouchPos * -1; */
            stick.transform.position = Input.mousePosition;  /* initTouchPos * -1; */
            // If the anchor point for Joystick is on the left side of the screen, allow movement.
            if(stickLimit.transform.position.x < Screen.width/2) {
                canMove = true;
                stick.gameObject.SetActive(true);
                stickLimit.gameObject.SetActive(true);                
            }

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
        if(touchStart && canMove){
/*             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
                //Debug.Log("Hit " + hit.transform.gameObject);
                joystickPos = new Vector3(hit.point.x, hit.point.y, 0);
            } */
            Vector3 offset = new Vector3(currTouchPos.x-initTouchPos.x, 0, currTouchPos.y - initTouchPos.y);
            Vector3 direction = Vector3.ClampMagnitude(offset, 1.0f);
            float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
            Vector2 joyDiff = Input.mousePosition - stickLimit.transform.position;
            joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);
            //Debug.Log("Drag Distance is: " + dragDist + "Radius is: " + radius.value);
            if(dragDist < radius.value * 0.25) {
                movePlayer(direction * -1, sneakSpeed.value);
                //Debug.Log("Is sneaking");
            }
            if(dragDist > radius.value * 0.25 && dragDist < radius.value * 0.8f) {
                movePlayer(direction * -1, movementSpeed.value);
                //Debug.Log("Is walking");
            }
                

            if(dragDist > radius.value * 0.8) {
                movePlayer(direction * -1, runSpeed.value);
                //Debug.Log("Is running");
            }
                
            //movePlayer(direction * -1, movementSpeed.value);
            //if(Vector2.Distance(Input.mousePosition, stickLimit.transform.position) < dragDist)
            stick.transform.position = joyDiff + new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);/* Input.mousePosition */ /* Vector2.ClampMagnitude(Input.mousePosition, 1100) */;/* new Vector3(initTouchPos.x + direction.x, initTouchPos.z + direction.z) * -1; */
        }
        else {
            stick.gameObject.SetActive(false);
            stickLimit.gameObject.SetActive(false);
            canMove = false;
        }
    }

    private void movePlayer(Vector3 direction, float speedMove) {
        Quaternion rotation = direction != Vector3.zero
            ? Quaternion.LookRotation(direction) : Quaternion.identity; // Shorthand if : else
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speedMove);
        playerRB.AddForce(direction * speedMove);
        //Debug.Log("Direction vector: " + direction);
        //playerRB.MovePosition((Vector2) transform.position + (direction * movementSpeed.value * Time.deltaTime));
    }
}
