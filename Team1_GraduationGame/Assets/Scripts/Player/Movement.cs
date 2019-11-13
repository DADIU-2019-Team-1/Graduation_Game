using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.Interaction;
using Team1_GraduationGame.Events;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]

public class Movement : MonoBehaviour
{
    private Rigidbody playerRB;
    private bool touchStart = false, canMove = false, canJump = true, canPush, isJumping = false, moveFrozen = false;
    private float currentRotSpeed, maxRotSpeed = 5.0f, calculatedRotSpeed;

    [SerializeField] private float accelerationFactor = 0.1f;
    private float swipeTimeTimer, currentRotationSpeed;
    private Vector3 initTouchPos, currTouchPos, joystickPos, stickLimitPos, direction, velocity;
    private Quaternion lookRotation, _prevousRotation = Quaternion.identity;
    public VoidEvent jumpEvent, attackEvent;
    public IntEvent stateChangeEvent;

    private Vector2 swipeStartPos, swipeEndPos, swipeDirection;

    [Tooltip("Put the joystick here")]
    public Transform stick;
    [Tooltip("Put the joystick border here")]
    public Transform stickLimit;

    public FloatReference sneakSpeed, walkSpeed, runSpeed, rotationSpeed, jumpHeight, fallMultiplier, attackRange, swipeTimeThreshold;
    public IntReference radius, attackDegree, swipePixelDistance;

    private PhysicMaterial _jumpMaterial;
    [Tooltip("Height in meters for checking jump")]
    public FloatReference ghostJumpHeight;

    public FloatVariable currentSpeed;
    public IntVariable moveState;
    private RaycastHit[] hit;

    public GameObject leftHeelPos, rightHeelPos, rightToePos, leftToePos;

    private CapsuleCollider _collider;

    //private SphereCollider playerTrigger;

    //public FloatReference floatingWeight;

    private int leftTouch = 99;
    private int rightTouch = 98;
    private Vector3 _previousPosition = Vector3.zero;
    [Header("Idle until this has been reached must be smaller than sneak threshold!")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float idleThreshold = 0.1f;

    [Header("Sneak threshold must be smaller than run threshold!")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float sneakThreshold;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float runThreshold;

    [Header("Motion Matching variables")]
    private MotionMatching mm;
    private TrajectoryPoint[] trajPoints;
    
    [TagSelector] [SerializeField] private string[] tagsToInteractWith;
    [SerializeField] private List<GameObject> interactableObjects;
    
    [TagSelector] [SerializeField] private string[] jumpPlatformTag;
    [SerializeField] private List<GameObject> jumpPlatforms;

    private void Awake()
    {
        mm = GetComponent<MotionMatching>();
        trajPoints = new TrajectoryPoint[mm.pointsPerTrajectory];

        //playerTrigger = GetComponent<SphereCollider>();
        //playerTrigger.radius = attackRange.value;
        //playerTrigger.isTrigger = true;
        _jumpMaterial = setJumpMaterial();
        _collider = GetComponent<CapsuleCollider>();
    }
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();

        interactableObjects = new List<GameObject>();
        for (int i = 0; i < tagsToInteractWith.Length; i++)
            interactableObjects.AddRange(GameObject.FindGameObjectsWithTag(tagsToInteractWith[i]));
        moveState.value = 0;
        
        for (int j = 0; j < jumpPlatformTag.Length; j++)
        {
            jumpPlatforms.AddRange(GameObject.FindGameObjectsWithTag(jumpPlatformTag[j]));
        }
    }

    void Update() {
        currentSpeed.value = Vector3.Distance(transform.position, _previousPosition) / Time.fixedDeltaTime;
        currentRotationSpeed = Quaternion.Angle(transform.rotation, _prevousRotation) / Time.fixedDeltaTime;
        lookRotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : Quaternion.identity;
        velocity = direction.normalized * currentSpeed.value;
    }

    void FixedUpdate()
    {
        _previousPosition = transform.position;
        _prevousRotation = transform.rotation;
        if (playerRB.velocity.y <= 0 && isJumping)
        {
            //playerRB.mass * fallMultiplier.value;

            playerRB.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier.value - 1) * Time.deltaTime;

            if (Physics.Raycast(leftToePos.transform.position, Vector3.down, ghostJumpHeight.value) || Physics.Raycast(rightToePos.transform.position, Vector3.down, ghostJumpHeight.value) || Physics.Raycast(leftHeelPos.transform.position, Vector3.down, ghostJumpHeight.value) || Physics.Raycast(rightHeelPos.transform.position, Vector3.down, ghostJumpHeight.value))
            {
                Debug.Log(true);
                isJumping = false;
                for (int j = 0; j < jumpPlatforms.Count; j++)
                {
                    jumpPlatforms[j].GetComponent<Collider>().material = null;
                }
                
            }
        }
        // Making sure touches only run on Android
#if UNITY_ANDROID
        int i = 0;
        while (i < Input.touchCount)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began && !moveFrozen)
            {
                if (t.position.x < (Screen.width / 2)-10)
                {   
                    if(t.fingerId < 50) 
                    {
                        stick.gameObject.SetActive(true);
                        stickLimit.gameObject.SetActive(true);
                        stickLimit.transform.position = t.position;
                        leftTouch = t.fingerId;
                        
                        canMove = true;
                    }
                }

                else if (t.position.x > Screen.width / 2)
                {
                    if(t.fingerId < 50) {
                        rightTouch = t.fingerId;
                        swipeStartPos = t.position;

                        swipeTimeTimer = Time.time;                        
                    }

                    //Debug.Log("Began phase: " + swipeTimeTimer);

                    /*                     if(canJump && rightTouch == t.fingerId) {
                                            playerJump(Vector3.up, jumpHeight.value);
                                        } */

                    // Start timer on finger down.

                }

            }
            else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && leftTouch == t.fingerId && canMove && !moveFrozen)
            {

                Vector3 offset = new Vector3(t.position.x - stickLimit.transform.position.x, 0,  t.position.y - stickLimit.transform.position.y);
                direction = Vector3.ClampMagnitude(offset, 1.0f);
                float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
                Vector2 joyDiff = t.position - new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
                // Need new clamping.
                joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);

                if (dragDist <= radius.value * idleThreshold)
                {
                    //movePlayer(direction,0);
                    //moveState.value = 0;
                }
                else if (dragDist > radius.value * idleThreshold && dragDist <= radius.value * sneakThreshold)
                {
                    movePlayer(direction, sneakSpeed.value);
                    //moveState.value = 1;
                }
                else if (dragDist > radius.value * sneakThreshold && dragDist < radius.value * runThreshold)
                {
                    movePlayer(direction, walkSpeed.value);
                    //moveState.value = 2;
                }
                else if (dragDist >= radius.value * runThreshold)
                {
                    movePlayer(direction, runSpeed.value);
                    //moveState.value = 3;
                }
                stick.transform.position = joyDiff + new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
                // t.deltaPosition; is a Vector2 of the difference between the last frame to its position this frame. 
/*                 if (stickLimit.transform.position.x < Screen.width / 2)
                {
                    stick.gameObject.SetActive(true);
                    stickLimit.gameObject.SetActive(true);
                }
                else if (t.position.x > Screen.width / 2)
                {
                    // Swipe movement? Maybe use t.deltaPosition to check change for swiping. 
                    // Could use TouchPhase.Stationary for just jumping?
                }
 */
            }
            else {
                if (t.phase == TouchPhase.Ended && leftTouch == t.fingerId)
                {
                    //if(leftTouch == t.fingerId) {
                    
                    leftTouch = 99;
                    stick.gameObject.SetActive(false);
                    stickLimit.gameObject.SetActive(false);
                    
                    if (canMove)
                        canMove = false;
                    //}

                    direction = new Vector3(0,0,0);

                }
                if (t.phase == TouchPhase.Ended && rightTouch == t.fingerId)
                {
                    rightTouch = 98;
                    swipeEndPos = t.position;
                    Vector2 swipeOffSet = new Vector2(swipeEndPos.x - swipeStartPos.x, swipeEndPos.y - swipeStartPos.y);
                    swipeDirection = swipeOffSet.normalized;
                    Vector3 worldDirection = new Vector3(swipeDirection.x, 0, swipeDirection.y);
                    //Debug.Log("End phase: " + Time.time);
                    if (swipeOffSet.magnitude > swipePixelDistance.value)
                    {
                        playerAttack(worldDirection);
                    }
                    else if (/*swipeTimeTimer + swipeTimeThreshold.value >= Time.time &&*/ !moveFrozen)
                    {
                        // Jump feels sluggish inside else if, but when only if, triggers every time and you never swipe/do both always.
                        playerJump(Vector3.up + direction, jumpHeight.value);
                        //Debug.Log("Jump");
                    }

                }
            }
            ++i;

            // if((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && !isJumping && leftTouch != t.fingerId) {
            //     stickLimit.gameObject.SetActive(false);
            //     stick.gameObject.SetActive(false);
            // }
        }
#endif

        // Making sure Mouse only runs on PC.
        // If this says && !UNITY_ANDROID, delete it. This is used to test on Unity Remote
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            initTouchPos = Input.mousePosition;

            // Joystick anchor where you initially press. Circle around is the sneak/Walk indicator, if you drag outside it starts running.
            // Movement function which changes between the 3 based on thresholds. Make script, then fit it with UI, so we can disable it. 
            // Movement keeps working, as long as initial position is on the left side of the screen. 

            stickLimit.transform.position = Input.mousePosition;
            stick.transform.position = Input.mousePosition;
            // If the anchor point for Joystick is on the left side of the screen, allow movement.
            if (stickLimit.transform.position.x < Screen.width / 2 && !moveFrozen)
            {
                canMove = true;
                stick.gameObject.SetActive(true);
                stickLimit.gameObject.SetActive(true);
            }

            if (Input.mousePosition.x > Screen.width / 2 && canJump)
            {
                swipeStartPos = Input.mousePosition;
                swipeTimeTimer = Time.time;
                //playerJump(Vector3.up, jumpHeight.value);
            }

        }
        if (Input.GetMouseButton(0))
        {
            touchStart = true;
            currTouchPos = Input.mousePosition;
        }
        else
        {
            touchStart = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            swipeEndPos = Input.mousePosition;
            Vector2 swipeOffSet = new Vector2(swipeEndPos.x - swipeStartPos.x, swipeEndPos.y - swipeStartPos.y);
            swipeDirection = swipeOffSet.normalized;
            Vector3 worldDirection = new Vector3(swipeDirection.x, 0, swipeDirection.y);
            //Debug.Log("End phase: " + Time.time);
            if (swipeOffSet.magnitude > swipePixelDistance.value && Input.mousePosition.x > Screen.width /2 && !canMove)
            {
                
                playerAttack(worldDirection);
                //Debug.Log("Swipe");
                //Debug.DrawLine(swipeStartPos, swipeStartPos + swipeDirection * 300, Color.red, 5);
                //Debug.DrawLine(playerRB.transform.position, playerRB.transform.position + worldDirection * 5, Color.green, 5);


            }
            else if (swipeTimeTimer + swipeTimeThreshold.value >= Time.time && !moveFrozen)
            {
                playerJump(Vector3.up, jumpHeight.value);
                //Debug.Log("Jump");
            }
        }

        if (touchStart && canMove && !moveFrozen)
        {

            Vector3 offset = new Vector3(currTouchPos.x - initTouchPos.x, 0, currTouchPos.y - initTouchPos.y);
            direction = Vector3.ClampMagnitude(offset, 1.0f);
            float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
            Vector2 joyDiff = Input.mousePosition - stickLimit.transform.position;
            joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);

            if (dragDist <= radius.value * idleThreshold)
            {
                // Empty
            }
            else if (dragDist <= radius.value * sneakThreshold)
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
        }
        else
        {
            stick.gameObject.SetActive(false);
            stickLimit.gameObject.SetActive(false);
            canMove = false;

            direction = Vector3.zero;
        }
#endif
        SetState();
    }

    private void movePlayer(Vector3 direction, float targetSpeed)
    {
        Vector3 currentVelocity = direction.normalized * currentSpeed.value;
        float accelerationSpeed = Mathf.Lerp(currentSpeed.value, targetSpeed, accelerationFactor);
        Quaternion targetRotation = direction != Vector3.zero
            ? Quaternion.LookRotation(direction) : Quaternion.identity; // Shorthand if : else 

        // TODO: Seperate rotation acceleration from speed acceleration
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed.value);
        if (!isJumping)
        {
            playerRB.MovePosition(transform.position + (direction + transform.forward /* * rotation factor can be inserted  here*/).normalized * ((targetSpeed - currentSpeed.value)*accelerationFactor + currentSpeed.value) * Time.deltaTime);
        }
        else
        {
            playerRB.AddForce(direction * jumpSpeed.value * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    private void playerJump(Vector3 direction, float jumpHeight)
    {
        if (!isJumping && (Physics.Raycast(leftToePos.transform.position, Vector3.down, ghostJumpHeight.value) || Physics.Raycast(rightToePos.transform.position, Vector3.down, ghostJumpHeight.value) || Physics.Raycast(leftHeelPos.transform.position, Vector3.down, ghostJumpHeight.value) || Physics.Raycast(rightHeelPos.transform.position, Vector3.down, ghostJumpHeight.value)))
        {
            for (int j = 0; j < jumpPlatforms.Count; j++)
            {
                jumpPlatforms[j].GetComponent<Collider>().material = _jumpMaterial;
            }
            playerRB.AddForce(direction * jumpHeight, ForceMode.Impulse);
            /*         if(playerRB.velocity.y <= 0) {
                        //playerRB.mass * fallMultiplier.value;
                        playerRB.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier.value -1) * Time.deltaTime;
                    } */
            // If the feet are atleast 10 cm away from the ground. 
            isJumping = true;
            if (jumpEvent != null) 
                jumpEvent.Raise();
            
            Debug.Log("Is Jumping: " + isJumping);
            //_collider.material = _jumpMaterial;


        }


        //canJump = false;
    }

    private void playerAttack(Vector3 direction)
    {

        float angle = ((float)attackDegree.value * Mathf.Deg2Rad) / 2;
        for (float j = -angle; j <= angle; j += 0.02f)
        {
            float x = direction.x * Mathf.Cos(j) - direction.z * Mathf.Sin(j);
            float z = direction.z * Mathf.Cos(j) + direction.x * Mathf.Sin(j);
            Vector3 newPoint = new Vector3(x, 0, z);
            Debug.DrawLine(playerRB.transform.position, playerRB.transform.position + newPoint * attackRange.value, Color.magenta, 0.5f);
        }
        
        // check all objects
        for (int i = 0; i < interactableObjects.Count; i++)
        {
            // if in range
            Vector3 closestPoint = interactableObjects[i].GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            //Debug.Log("Closest point is: " + closestPoint);
            //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), closestPoint, Quaternion.identity);
            float refDistance = Vector3.Distance(closestPoint, transform.position);
            
            if (refDistance <= attackRange.value)
            {
                
                // and in attack degree
                Vector3 temp = closestPoint - transform.position;
                temp.y = 0;
                
                float angleToObject = Vector3.Angle(temp, direction);
                if (angleToObject <= attackDegree.value / 2)
                {
                    // interact
                    interactableObjects[i].GetComponent<Interactable>().Interact();
                    if (attackEvent != null)
                        attackEvent.Raise();
                    // Debug.Log("INTERACT!!!!!");
                    // interactableObjects[i].interact();
                }
            }
        }
    }

    public float GetSpeed()
    {
        //currentSpeed.value = Vector3.Distance(transform.position, _previousPosition) / Time.fixedDeltaTime;
        return currentSpeed.value;
    }

    public void Frozen(bool move) 
    {
        moveFrozen = move;
    }

    public void SetState()
    {
        if (currentSpeed.value <= 0.01f)
        {
            moveState.value = 0;

        }
        else if (currentSpeed.value <= sneakSpeed.value + 0.05f)
        {
            moveState.value = 1;
        }
        else if (currentSpeed.value <= walkSpeed.value + 0.05f)
        {
            moveState.value = 2;

        }
        else
        {
            moveState.value = 3;
        }
        if(stateChangeEvent != null)
            stateChangeEvent.Raise(moveState.value);
    }

    public Trajectory GetMovementTrajectory()
    {
        for (int i = 0; i < trajPoints.Length; i++)
        {
            if (i > 0)
            {
                //Vector3 tempPos = trajPoints[i - 1].GetPoint() + Quaternion.Slerp(transform.rotation, lookRotation, (float) (i + 1) / trajPoints.Length) * Vector3.forward * Mathf.Clamp(speed, 0.1f, 1.0f);
                Vector3 tempPos = trajPoints[i - 1].GetPoint() + direction * currentSpeed.value;

                Vector3 tempForward = tempPos + (Quaternion.Slerp(transform.rotation, lookRotation,
                                          (float)i / trajPoints.Length) * Vector3.forward * rotationSpeed.value).normalized;

                trajPoints[i] = new TrajectoryPoint(tempPos, tempForward);
            }
            else
                trajPoints[i] = new TrajectoryPoint(transform.position, transform.position + transform.forward);
        }
        return new Trajectory(trajPoints);
    }
    
    private PhysicMaterial setJumpMaterial() 
    {
        PhysicMaterial newPhysMaterial;
        newPhysMaterial = new PhysicMaterial
        {
            bounciness = 0,
            frictionCombine = 0,
            dynamicFriction = 0,
            staticFriction = 0,
            bounceCombine = 0,
            name = "Jump_Material"
        };
        return newPhysMaterial;
    }
}
