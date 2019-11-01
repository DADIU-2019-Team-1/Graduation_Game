using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    private Rigidbody playerRB;
    private bool touchStart = false, canMove = false, canJump = false, canAttack;
    private Vector3 initTouchPos;
    private Vector3 currTouchPos;

    private Vector3 joystickPos;
    private Vector3 stickLimitPos;

    [Tooltip("Put the joystick here")]
    public Transform stick;
    [Tooltip("Put the joystick border here")]
    public Transform stickLimit;

    public FloatReference sneakSpeed;
    public FloatReference walkSpeed;
    public FloatReference runSpeed;
    public FloatReference rotationSpeed;
    public FloatReference jumpHeight;
    public IntReference radius;

    public FloatVariable currentSpeed;

    public FloatReference fallMultiplier;

    //public FloatReference floatingWeight;

    private int leftTouch = 99;
    private Vector3 _previousPosition = Vector3.zero;
    
    [Header("Sneak threshold must be smaller than run threshold!")]
    [SerializeField] [Range(0.0f,1.0f)]
    private float sneakThreshold;
    [SerializeField] [Range(0.0f, 1.0f)] 
    private float runThreshold;
    //public List<Touchlocation> touches = new List<Touchlocation>();
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        _previousPosition = transform.position;
        // Making sure touches only run on Android
        #if UNITY_ANDROID
        int i = 0;
        while(i < Input.touchCount){
            Touch t = Input.GetTouch(i);
            if(t.phase == TouchPhase.Began) {
                if(t.position.x < Screen.width / 2) {
                    stick.gameObject.SetActive(true);
                    stickLimit.gameObject.SetActive(true);  
                    leftTouch = t.fingerId;
                    stickLimit.transform.position = t.position;
                    canMove = true;
                                       
                }


                else if (t.position.x > Screen.width /2 && canJump) {
                    playerJump(Vector3.up, jumpHeight.value);
                }

            } else if((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)  && leftTouch == t.fingerId  && canMove){

                Vector3 offset = new Vector3(t.position.x - stickLimit.transform.position.x, 0,  t.position.y-stickLimit.transform.position.y);
                Vector3 direction = Vector3.ClampMagnitude(offset, 1.0f);
                float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
                Vector2 joyDiff = t.position - new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
                // Need new clamping.
                joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);
                //Debug.Log("Mouse pos is: " + Input.mousePosition);
                //Debug.Log("Touch pos is " + t.position);

                if (dragDist <= radius.value * sneakThreshold)
                {
                    movePlayer(direction, sneakSpeed.value);
                }
                else if (dragDist > radius.value * sneakThreshold && dragDist < radius.value * runThreshold)
                {
                    movePlayer(direction, walkSpeed.value);
                }
                else if (dragDist >= radius.value * runThreshold)
                {
                    movePlayer(direction, runSpeed.value);
                }
                stick.transform.position = joyDiff + new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
                // t.deltaPosition; is a Vector2 of the difference between the last frame to its position this frame. 
                if(stickLimit.transform.position.x < Screen.width /2) {
                    stick.gameObject.SetActive(true);
                    stickLimit.gameObject.SetActive(true);
                    
                }
                else if(t.position.x > Screen.width /2) {
                    // Swipe movement? Maybe use t.deltaPosition to check change for swiping. 
                    // Could use TouchPhase.Stationary for just jumping?
                }
                
            } else if(t.phase == TouchPhase.Ended && leftTouch == t.fingerId) {
                leftTouch = 99;
                stick.gameObject.SetActive(false);
                stickLimit.gameObject.SetActive(false);
                if(canMove)  
                    canMove = false;  
            }    
            ++i;
        }
        #endif

        // Making sure Mouse only runs on PC.
        #if UNITY_EDITOR
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

/*             if(Input.mousePosition.x > Screen.width/2 && canJump) {
                playerJump(Vector3.up, jumpHeight.value);
            } */

        }
        if(Input.GetMouseButton(0)) {
            
            touchStart = true;
            currTouchPos = Input.mousePosition;
        }
        else {
            touchStart = false;
        }

        if(touchStart && canMove){

            Vector3 offset = new Vector3(currTouchPos.x-initTouchPos.x, 0, currTouchPos.y - initTouchPos.y);
            Vector3 direction = Vector3.ClampMagnitude(offset, 1.0f);
            float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
            Vector2 joyDiff = Input.mousePosition - stickLimit.transform.position;
            joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);

            if(dragDist <= radius.value * sneakThreshold) {
                movePlayer(direction, sneakSpeed.value);
            }
            else if(dragDist > radius.value * sneakThreshold && dragDist < radius.value * runThreshold) {
                movePlayer(direction, walkSpeed.value);
            }
            else if(dragDist >= radius.value * runThreshold) {
                movePlayer(direction, runSpeed.value);
            }
                
            stick.transform.position = joyDiff + new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
        }
        else {
            stick.gameObject.SetActive(false);
            stickLimit.gameObject.SetActive(false);
            canMove = false;
        }
        #endif
    }

/*     void FixedUpdate() {
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
                movePlayer(direction, walkSpeed.value);
                Debug.Log("Is walking with speed " + playerRB.velocity.magnitude);
            }
                

            if(dragDist > radius.value * 0.6) {
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
    } */

    private void movePlayer(Vector3 direction, float speedMove) {
        Quaternion rotation = direction != Vector3.zero
            ? Quaternion.LookRotation(direction) : Quaternion.identity; // Shorthand if : else
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed.value);
        playerRB.MovePosition(transform.position + (direction * speedMove * Time.deltaTime));
    }

    private void playerJump(Vector3 direction, float jumpHeight) {
        if(playerRB.velocity.y < 0) {
            //playerRB.mass * fallMultiplier.value;
        }
        playerRB.AddForce(direction * jumpHeight, ForceMode.Impulse);
        //Debug.Log("Jumped");
        //canJump = false;
    }

    public float GetSpeed()
    {
        currentSpeed.value = Vector3.Distance(transform.position, _previousPosition) / Time.fixedDeltaTime;
        return currentSpeed.value;
    }
}
