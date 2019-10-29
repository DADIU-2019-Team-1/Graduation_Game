using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    private Rigidbody playerRB;
    private bool touchStart = false, canMove = false, isGrounded;
    private Vector3 initTouchPos;
    private Vector3 currTouchPos;

    private Vector3 joystickPos;

    public Camera cam;
    public Transform stick;
    public Transform stickLimit;
    public FloatReference movementSpeed;


    public FloatReference sneakSpeed;

    public FloatReference runSpeed;

    public FloatReference jumpHeight;

    public FloatReference hillAssist;

    public FloatReference rotationSpeed;
    public IntReference radius;

    public List<Touchlocation> touches = new List<Touchlocation>();
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        int i = 0;
        while(i < Input.touchCount){
            Touch t = Input.GetTouch(i);
            if(t.phase == TouchPhase.Began) {
                touches.Add(new Touchlocation(t.fingerId, initTouchPos));
            } else if(t.phase == TouchPhase.Ended) {
                Touchlocation thisTouch = touches.Find(Touchlocation => Touchlocation.touchID == t.fingerId);
                touches.RemoveAt(touches.IndexOf(thisTouch));
            } else if(t.phase == TouchPhase.Moved){
                Touchlocation thisTouch = touches.Find(Touchlocation => Touchlocation.touchID == t.fingerId);
                //thisTouch.stick.transform.position = t.position;
            }
        }
        if(Input.GetMouseButtonDown(0)) {
            initTouchPos = Input.mousePosition;

            // Joystick anchor where you initially press. Circle around is the sneak/Walk indicator, if you drag outside it starts running.
            // Movement function which changes between the 3 based on thresholds. Make script, then fit it with UI, so we can disable it. 
            // Movement keeps working, as long as initial position is on the left side of the screen. 

            stickLimit.transform.position = Input.mousePosition; 
            stick.transform.position = Input.mousePosition;  
            // If the anchor point for Joystick is on the left side of the screen, allow movement.
            if(stickLimit.transform.position.x < Screen.width/2) {
                canMove = true;
                stick.gameObject.SetActive(true);
                stickLimit.gameObject.SetActive(true);                
            }

            if(/* isGrounded &&  */Input.mousePosition.x > Screen.width/2) {
                playerJump(Vector3.up, jumpHeight.value);
            }

        }
        if(Input.GetMouseButton(0)) {
            
            touchStart = true;
            currTouchPos = Input.mousePosition;
        }
        else {
            touchStart = false;
        }
    }

    void FixedUpdate() {
        if(touchStart && canMove){

            Vector3 offset = new Vector3(currTouchPos.x-initTouchPos.x, 0, currTouchPos.y - initTouchPos.y);
            Vector3 direction = Vector3.ClampMagnitude(offset, 1.0f);
            float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
            Vector2 joyDiff = Input.mousePosition - stickLimit.transform.position;
            joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);
            if(dragDist < radius.value * 0.25) {
                movePlayer(direction, sneakSpeed.value);
                Debug.Log("Is sneaking with speed " + playerRB.velocity.magnitude);
                
            }
            if(dragDist > radius.value * 0.25 && dragDist < radius.value * 0.8f) {
                movePlayer(direction, movementSpeed.value);
                Debug.Log("Is walking with speed " + playerRB.velocity.magnitude);
            }
                

            if(dragDist > radius.value * 0.8) {
                movePlayer(direction, runSpeed.value);
                Debug.Log("Is running with speed " + playerRB.velocity.magnitude);
            }
                
            stick.transform.position = joyDiff + new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
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
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed.value);
        playerRB.MovePosition(transform.position + (direction * speedMove * Time.deltaTime));

    }

    private void playerJump(Vector3 direction, float jumpHeight) {
        playerRB.AddForce(direction * jumpHeight);
        Debug.Log("Jumped");
        isGrounded = false;
    }

}
