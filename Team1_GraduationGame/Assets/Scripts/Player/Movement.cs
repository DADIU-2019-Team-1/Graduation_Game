// Codeowners: Nicolai Hansen, Sebastian Andresen.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team1_GraduationGame.Interaction;
using Team1_GraduationGame.Events;
using Team1_GraduationGame.MotionMatching;
using Team1_GraduationGame.Sound;
using Unity.Mathematics;
using UnityEditor;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]

public class Movement : MonoBehaviour
{
    private Rigidbody playerRB;
    private bool touchStart = false, canMove = false, canJump = true, canPush = true, isPushing = false, moveFrozen = false, isAttacked = false, touchMoved = false, touchEndedJump = false;
    private float rotationSpeedCurrent, rotationSpeedMax = 5.0f, rotationSpeedGoal, rotationAccelerationFactor = 0.1f, pushRotationAccelerationFactor = 0.7f, rotationAngleReactionFactor = 0.1f, pushRotationAngleReactionFactor = 0.7f;
    
    [HideInInspector]
    public float targetSpeed;
    public bool isJumping = false, inSneakZone = false;

    // Tried a particle system for the ground
    public ParticleSystem Groundparticles;

    public PlayerSoundManager playerSoundManager;

    [SerializeField] private float accelerationFactor = 0.1f;

    private Vector3 initTouchPos, currTouchPos, joystickPos, stickLimitPos, velocity;
    private Quaternion lookRotation, pushRotation;
    
    [HideInInspector]
    public Vector3 direction = Vector3.zero, pushDirection = Vector3.zero;

    public BoolVariable atOrbTrigger;

    private Vector2 swipeStartPos, swipeEndPos, swipeDirection;

    [Tooltip("Put the joystick here")]
    public Transform stick;
    [Tooltip("Put the joystick border here")]
    public Transform stickLimit;

    public FloatReference sneakSpeed, walkSpeed, runSpeed, rotationSpeed, jumpHeight, jumpSpeed, fallMultiplier, attackRange, swipeTimeThreshold, defaultPushForce, pushForce;
    public IntReference radius, attackDegree, swipePixelDistance, attackCoolDown;

    private PhysicMaterial _jumpMaterial;
    [Tooltip("Height in meters for checking jump")]
    public FloatReference ghostJumpHeight;

    public FloatReference jumpLength;

    public FloatVariable currentSpeed;
    public IntVariable moveState;
    private RaycastHit[] hit;

    public GameObject leftHeelPos, rightHeelPos, rightToePos, leftToePos;

    private CapsuleCollider _collider;
    
    //Comment out this when motionmatching works!
    private Animator animator;

    //private SphereCollider playerTrigger;

    //public FloatReference floatingWeight;

    private int leftTouch = 99;
    private int rightTouch = 98;
    private int attackCooldown;
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

    [Header("This is for SneakZones")]
    [SerializeField]
    [Range(0.0f, 0.99f)]
    private float zoneSneakThreshold;

    [Header("Motion Matching variables")]
    private MotionMatching mm;
    private TrajectoryPoint[] trajPoints;

    [TagSelector] [SerializeField] private string[] tagsToInteractWith;
    [SerializeField] private List<GameObject> interactableObjects;

    [TagSelector] [SerializeField] private string[] jumpPlatformTag;
    [SerializeField] private List<GameObject> jumpPlatforms;

    public event Action attack;

    [SerializeField]
    private LayerMask _layerMask;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        moveState.value = 0;
        if (GetComponent<MotionMatching>() != null)
        {
            mm = GetComponent<MotionMatching>();
            trajPoints = new TrajectoryPoint[mm.pointsPerTrajectory];
        }

        //playerTrigger = GetComponent<SphereCollider>();
        //playerTrigger.radius = attackRange.value;
        //playerTrigger.isTrigger = true;
        _jumpMaterial = setJumpMaterial();
        _collider = GetComponent<CapsuleCollider>();

        //_layerMask = ~LayerMask.GetMask("Character");
    }
    void Start()
    {
        interactableObjects = new List<GameObject>();
        for (int i = 0; i < tagsToInteractWith.Length; i++)
            interactableObjects.AddRange(GameObject.FindGameObjectsWithTag(tagsToInteractWith[i]));
        for (int j = 0; j < jumpPlatformTag.Length; j++)
        {
            jumpPlatforms.AddRange(GameObject.FindGameObjectsWithTag(jumpPlatformTag[j]));
        }
    }

    void Update()
    {
        currentSpeed.value = Vector3.Distance(transform.position, _previousPosition) / Time.fixedDeltaTime;

        
        //Tried a particle system for the ground
        // if (currentSpeed.value > 4 && isJumping == false)
        // {
        //     Groundparticles.Play();
        // } else {
        //     Groundparticles.Stop();
        // }
        //I set a temp Speed animator if we arent using motion matching

        if (animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("Speed", currentSpeed.value);
        }

        lookRotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : Quaternion.identity;
        velocity = direction.normalized * currentSpeed.value;

#if UNITY_ANDROID
        int i = 0;
        while (i < Input.touchCount)
        {
            Touch t = Input.GetTouch(i);
            // If you have started the first touch, and the player is not Frozen
            // If your tap position is on left side of the screen
            // If your current fingerId is less than 50 (assigned), set the joystick, set leftTouch to the current finger ID.
            // Since we have now spawned the joystick, you can move. 
            if (t.phase == TouchPhase.Began && !moveFrozen)
            {
                if (t.position.x < (Screen.width / 2) - 10)
                {
                    if (t.fingerId < 50)
                    {
                        stick.gameObject.SetActive(true);
                        stickLimit.gameObject.SetActive(true);
                        stickLimit.transform.position = t.position;
                        leftTouch = t.fingerId;

                        canMove = true;
                    }
                }
                // If you tap on the right side of the screen
                // If your current fingerID is less than 50 (assigned), set rightTouch to the current finger ID.
                // Set the start point for swipe.
                else if (t.position.x > Screen.width / 2)
                {
                    //Debug.Log("Entered right tap");
                    if (t.fingerId < 50)
                    {

                        rightTouch = t.fingerId;
                        //Debug.Log("Setting rightTouch: " + rightTouch);
                        swipeStartPos = t.position;

                    }

                }

            }
            // If you moved your finger, or keeping it still, and it's the left finger, the player can move, and not frozen.
            // Calculate/constrict the joystick UI to the touched position, calculate the dragged distance, and move with that. 

            else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && leftTouch == t.fingerId && canMove && !moveFrozen)
            {
                //touchPos = t.position;
                //touchMoved = true;
                Vector3 offset = new Vector3(t.position.x - stickLimit.transform.position.x, 0, t.position.y - stickLimit.transform.position.y);
                direction = Vector3.ClampMagnitude(offset, 1.0f);
                //float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
                float dragDist = math.distancesq(stick.transform.position, stickLimit.transform.position);
                Vector2 joyDiff = t.position - new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
                joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);

                //requestMovement = true;
                PlayerMoveRequest(dragDist);
                touchMoved = true;

                stick.transform.position = joyDiff + new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);

            }
            // It never enters this.
            // If the touch is no longer supported somehow, and it's on the right screen, and not at an orb.
            // "Unassign" the right touch, and jump in the direction of your last movement.



            // If touch ended, and it's the left finger.
            // Unassign left touch, remove the joystick, can no longer move, joystick direction reset. 
            // If touch ended, and it's the right finger.
            // Unassign right touch, set swipe end position, find the offset and direction
            // If that swipe is long enough, you can push, and it's not on cooldown
            // Set the push direction, rotation and add cooldown. Activate push, play animation if available.
            // If not pushing, and not frozen, jump. 

            if (t.phase == TouchPhase.Ended && leftTouch == t.fingerId)
            {
                //if(leftTouch == t.fingerId) {

                leftTouch = 99;
                stick.gameObject.SetActive(false);
                stickLimit.gameObject.SetActive(false);

                if (canMove)
                    canMove = false;
                //}

                direction = new Vector3(0, 0, 0);
                touchMoved = false;
                
            }
            if (t.phase == TouchPhase.Ended && rightTouch == t.fingerId && !atOrbTrigger.value)
            {
                //Debug.Log("Entered end tap");
                rightTouch = 98;
                swipeEndPos = t.position;
                Vector2 swipeOffSet = new Vector2(swipeEndPos.x - swipeStartPos.x, swipeEndPos.y - swipeStartPos.y);
                swipeDirection = swipeOffSet.normalized;

                if (swipeOffSet.sqrMagnitude > Mathf.Pow(swipePixelDistance.value, 2) && canPush)
                {
                    //Debug.Log("Entered first push if");
                    if (attackCooldown <= 0)
                    {
                        //Debug.Log("Entered second push if");
                        pushDirection = new Vector3(swipeDirection.x, 0, swipeDirection.y);
                        pushRotation = pushDirection != Vector3.zero ? Quaternion.LookRotation(pushDirection) : Quaternion.identity;


                        attackCooldown = attackCoolDown.value;

                        isPushing = true;
                        //playerAttack(pushDirection);
                        // I set a temp push animator if we arent using motion matching
                        if (animator.runtimeAnimatorController != null)
                        {
                            animator.SetTrigger("Attack");
                        }
                    }

                }
                else if (!moveFrozen && canJump)
                {
                    touchEndedJump = true;
                    //Debug.Log("Entered Jump");
                    // Jump feels sluggish inside else if, but when only if, triggers every time and you never swipe/do both always.
                    //playerJump(direction, jumpHeight.value);
                }


            }


            //else if (t.phase == TouchPhase.Canceled /*|| t.phase == TouchPhase.Stationary) */ && rightTouch == t.fingerId && !atOrbTrigger.value)
            //{
            //    rightTouch = 98;
            //    Debug.Log("Entered canceled");
            //    playerJump(direction, jumpHeight.value);
            //}

            //else if (t.phase == TouchPhase.Stationary)
            //{
            //    rightTouch = 98;
            //    Debug.Log("Entered Stationary");
            //}

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
            //Debug.Log("Mouse down");
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
                //Debug.Log("Entered jump statement");
                swipeStartPos = Input.mousePosition;
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
            //Debug.Log("Mouse up");
            swipeEndPos = Input.mousePosition;
            Vector2 swipeOffSet = new Vector2(swipeEndPos.x - swipeStartPos.x, swipeEndPos.y - swipeStartPos.y);
            swipeDirection = swipeOffSet.normalized;

            if (swipeOffSet.sqrMagnitude > Mathf.Pow(swipePixelDistance.value, 2) && Input.mousePosition.x > Screen.width / 2 && !canMove && !atOrbTrigger.value)
            {
                //Debug.Log("Entered swipe");
                if (attackCooldown <= 0 && canPush)
                {
                    pushDirection = new Vector3(swipeDirection.x, 0, swipeDirection.y);
                    pushRotation = pushDirection != Vector3.zero ? Quaternion.LookRotation(pushDirection) : Quaternion.identity;
                    attackCooldown = attackCoolDown.value;

                    isPushing = true;
                    //playerAttack(pushDirection);

                    // I set a temp push animator if we arent using motion matching
                    if (animator.runtimeAnimatorController != null)
                    {
                        //Debug.Log("Entered mouse up animator");
                        animator.SetTrigger("Attack");
                    }
                }

            }

            else if (!moveFrozen && Input.mousePosition.x > Screen.width / 2 && canJump)
            {
                
                touchEndedJump = true;
                //Debug.Log("Entered jump statement mouse up: " + touchEndedJump);
                //playerJump(direction, jumpHeight.value);
            }
        }

        if (touchStart && canMove && !moveFrozen)
        {

            Vector3 offset = new Vector3(currTouchPos.x - initTouchPos.x, 0, currTouchPos.y - initTouchPos.y);
            direction = Vector3.ClampMagnitude(offset, 1.0f);
            // float dragDist = Vector2.Distance(stick.transform.position, stickLimit.transform.position);
            float dragDist = math.distancesq(stick.transform.position, stickLimit.transform.position);
            Vector2 joyDiff = Input.mousePosition - stickLimit.transform.position;
            joyDiff = Vector2.ClampMagnitude(joyDiff, radius.value);
            PlayerMoveRequest(dragDist);

            touchMoved = true;

            stick.transform.position = joyDiff + new Vector2(stickLimit.transform.position.x, stickLimit.transform.position.y);
        }
        else
        {
            stick.gameObject.SetActive(false);
            stickLimit.gameObject.SetActive(false);
            canMove = false;
            direction = Vector3.zero;
            rotationSpeedCurrent = 0.0f;
            if (atOrbTrigger != null && !atOrbTrigger.value)
            {
                targetSpeed = 0.0f;
            }

            touchMoved = false;

        }

        if (Input.GetKey(KeyCode.Space) && !isJumping && canJump)
        {
            
            //playerJump(direction, jumpHeight.value);
            touchEndedJump = true;
            //Debug.Log("Entered Space jump: " + touchEndedJump);
        }
#endif
    }

    void FixedUpdate()
    {
        _previousPosition = transform.position;
        if (playerRB.velocity.y <= 0.05f && isJumping)
        {
            playerRB.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier.value - 1) * Time.deltaTime;

            if (Physics.Raycast(leftToePos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask) || Physics.Raycast(rightToePos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask) || Physics.Raycast(leftHeelPos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask) || Physics.Raycast(rightHeelPos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask) || Physics.Raycast(transform.position + Vector3.up, Vector3.down, ghostJumpHeight.value + 1.0f, _layerMask))
            {
                isJumping = false;
                touchEndedJump = false;

                if (animator.runtimeAnimatorController != null)
                {
                    animator.SetTrigger("Land");
                }

                _collider.material = null;
                for (int j = 0; j < jumpPlatforms.Count; j++)
                {
                    jumpPlatforms[j].GetComponent<Collider>().material = null;
                    
                }

            }




        }
        // Making sure touches only run on Android
            if (touchMoved)
            {
                movePlayer(direction);
            }

            if (touchEndedJump)
            {
                playerJump(direction, jumpHeight.value);
                //touchEndedJump = false;
            }
        SetState();

        if (isPushing)
        {
            if (attackCooldown > 0)
            {
                // Was pushdirection before
                targetSpeed = runSpeed.value;
                movePlayer(pushDirection);
            }

            else
            {
                isPushing = false;
                
            }

        }



        if (attackCooldown > 0)
        {
            attackCooldown--; 
            
        }
    }

    private void OnTriggerEnter(Collider sneakZone)
    {
        if (sneakZone.tag == "Sneakzone")
        {
            inSneakZone = true;
        }
    }

    private void OnTriggerExit(Collider sneakZone)
    {
        if (sneakZone.tag == "Sneakzone")
        {
            inSneakZone = false;
        }
    }
    private void PlayerMoveRequest(float dragDist)
    {
        if (!isPushing)
        {
            if (inSneakZone)
            {
                if (dragDist < Mathf.Pow(radius.value * idleThreshold, 2))
                {
                    //movePlayer(direction, 0);
                    //moveState.value = 0;
                }
                else if (dragDist < Mathf.Pow(radius.value * zoneSneakThreshold,2))
                {
                    targetSpeed = sneakSpeed.value;
                    //movePlayer(direction);
                    //moveState.value = 1;
                }
                else
                {
                    targetSpeed = walkSpeed.value;
                    //movePlayer(direction);
                    //moveState.value = 2;
                }

            }
            else
            {
                if (dragDist < Mathf.Pow(radius.value * idleThreshold, 2)) // Idle
                {
                    // Empty
                }
                else if (dragDist < Mathf.Pow(radius.value * sneakThreshold, 2)) // Sneak
                {
                    targetSpeed = sneakSpeed.value;
                    //movePlayer(direction);
                }
                else if (dragDist < Mathf.Pow(radius.value * runThreshold, 2)) // Walk
                {
                    targetSpeed = walkSpeed.value;
                    //movePlayer(direction);
                }
                else // Run
                {
                    targetSpeed = runSpeed.value;
                    //movePlayer(direction);
                }
            }
        }

    }

    private Boolean CrossProductPositive(Vector3 a, Vector3 b)
    {
        return a.x * b.z - a.z * b.x >= 0;
    }

    public void movePlayer(Vector3 _direction)
    {
        if (atOrbTrigger != null && atOrbTrigger.value)
        {
            targetSpeed = walkSpeed.value;
        }

        int wayToRotate = CrossProductPositive(transform.forward, _direction) ? 1 : -1;
        rotationSpeedGoal = Mathf.Min(rotationSpeed.value, Vector3.Angle(transform.forward, _direction) * (isPushing? pushRotationAngleReactionFactor: rotationAngleReactionFactor)) * wayToRotate;
        rotationSpeedCurrent += (rotationSpeedGoal - rotationSpeedCurrent) * (isPushing? pushRotationAccelerationFactor: rotationAccelerationFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation,isPushing? pushRotation: lookRotation, Time.deltaTime * Mathf.Abs(rotationSpeedCurrent));
        if (!isJumping)
        {
            playerRB.MovePosition(transform.position + (_direction + transform.forward).normalized * ((targetSpeed - currentSpeed.value) * accelerationFactor + currentSpeed.value) * Time.fixedDeltaTime);
        }
        else
        {
//#if UNITY_EDITOR
            playerRB.MovePosition(transform.position + (_direction * targetSpeed * Time.deltaTime));
//#endif
//#if UNITY_ANDROID
//            playerRB.AddForce(direction * targetSpeed * Time.deltaTime, ForceMode.Impulse);
//#endif
        }

        if (!isJumping && !(Physics.Raycast(leftToePos.transform.position, Vector3.down, 0.3f, _layerMask) ||
                           Physics.Raycast(rightToePos.transform.position, Vector3.down, 0.3f, _layerMask) ||
                           Physics.Raycast(leftHeelPos.transform.position, Vector3.down, 0.3f, _layerMask) ||
                           Physics.Raycast(rightHeelPos.transform.position, Vector3.down, 0.3f, _layerMask) ||
                           Physics.Raycast(transform.position + Vector3.up, Vector3.down, 0.3f + 1.0f, _layerMask)))
        {
            isJumping = true;
            //playerSoundManager?.MiniJumpEvent();

            // also setting jump on temp Animator
            if (animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    private void playerJump(Vector3 direction, float jumpHeight)
    {
        if (!isJumping && (Physics.Raycast(leftToePos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask) || Physics.Raycast(rightToePos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask) || Physics.Raycast(leftHeelPos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask) || Physics.Raycast(rightHeelPos.transform.position, Vector3.down, ghostJumpHeight.value, _layerMask)))
        {
            _collider.material = _jumpMaterial;
            for (int j = 0; j < jumpPlatforms.Count; j++)
            {
                jumpPlatforms[j].GetComponent<Collider>().material = _jumpMaterial;
                
            }
            playerRB.AddForce(((Vector3.up * jumpHeight) + (direction * jumpLength.value)), ForceMode.Impulse);
            /*         if(playerRB.velocity.y <= 0) {
                        //playerRB.mass * fallMultiplier.value;
                        playerRB.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier.value -1) * Time.deltaTime;
                    } */
            // If the feet are atleast 10 cm away from the ground. 
            isJumping = true;

            // also setting jump on temp Animator
            if (animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Jump");
            }

            playerSoundManager?.JumpEvent();
            //_collider.material = _jumpMaterial;


        }


        //canJump = false;
    }

    public void playerAttack(Vector3 direction)
    {
        //float angle = ((float)attackDegree.value * Mathf.Deg2Rad) / 2;
        //for (float j = -angle; j <= angle; j += 0.02f)
        //{
        //    float x = direction.x * Mathf.Cos(j) - direction.z * Mathf.Sin(j);
        //    float z = direction.z * Mathf.Cos(j) + direction.x * Mathf.Sin(j);
        //    Vector3 newPoint = new Vector3(x, 0, z);
        //    Debug.DrawLine(playerRB.transform.position, playerRB.transform.position + newPoint * attackRange.value, Color.magenta, 0.5f);
        //}

        //attackCooldown = attackCoolDown.value;
        
        //isPushing = true;
        
        // check all objects
        for (int i = 0; i < interactableObjects.Count; i++)
        {
            Collider pushCollider = interactableObjects[i].GetComponent<Collider>();
            // if in range
            Vector3 closestPoint = pushCollider.ClosestPointOnBounds(transform.position);
            //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), closestPoint, Quaternion.identity);
            //float refDistance = Vector3.Distance(closestPoint, transform.position);
            float refDistance = math.distancesq(closestPoint, transform.position);
            //transform.rotation = Quaternion.LookRotation(direction);
            //playerRB.AddForce(direction.normalized * pushForce.value, ForceMode.Impulse);


            
            if (refDistance <= Mathf.Pow(attackRange.value,2))
            {

                // Do math stuff. Need to find relative angle downwards to pivot. 
                Vector3 hipClosestPoint = pushCollider.ClosestPointOnBounds(transform.position + Vector3.up);

                Vector3 hipVectorToObject = hipClosestPoint - (transform.position + Vector3.up);

                //Debug.DrawLine(transform.position, hipVectorToObject, Color.black, 5);

                //Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), transform.position, Quaternion.identity);
                if (Math.Abs(hipVectorToObject.y) < 0.3f)
                {
                    Vector3 temp = closestPoint - transform.position;
                    temp.y = 0;

                    float angleToObject = Vector3.Angle(temp, direction);
                    if (angleToObject <= attackDegree.value / 2)
                    {
                        // interact
                        // Sometimes returns nullreference errors.
                        interactableObjects[i].GetComponent<Interactable>().Interact();
                        
                        playerSoundManager?.AttackEvent();
                        //if (attackEvent != null)
                        //    attackEvent.Raise();
                        // interactableObjects[i].interact();
                    }
                }
                // and in attack degree

            }
        }
        attack?.Invoke();
    }

    public float GetSpeed()
    {
        //currentSpeed.value = Vector3.Distance(transform.position, _previousPosition) / Time.fixedDeltaTime;
        return currentSpeed.value;
    }

    public void Frozen(bool move)
    {
        moveFrozen = move;

        if (atOrbTrigger == null)
            return;

        if (moveFrozen && !atOrbTrigger.value)
        {
            playerRB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        }
            
        else
        {
            playerRB.constraints = RigidbodyConstraints.None;
            playerRB.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;

            
            
        }

    }

    public void SetActive(bool isActive)
    {
        if (!isActive)
        {
            playerRB.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            moveFrozen = true;
            canJump = false;
            canPush = false;
        }
        else
        {
            playerRB.constraints = RigidbodyConstraints.None;
            playerRB.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
            moveFrozen = false;
            canJump = true;
            canPush = true;
        }
    }

    public bool GetIsAttacked()
    {
        return isAttacked;
    }

    public void SetIsAttacked(bool attacked)
    {
        isAttacked = attacked;
        if (isAttacked)
        {
            canPush = false;
        }
        else
        {
            canPush = true;
        }
    }


    public void memorySettings(bool memoryactive) {
        canJump = memoryactive;
        canPush = memoryactive;
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

        playerSoundManager?.MotionStateUpdate(moveState.value);
    }

    public Trajectory GetMovementTrajectory()
    {
        float newRotationSpeedCurrent = rotationSpeedCurrent;
        Vector3 playerPos = transform.position, previousPlayerPos;
        Vector3 playerForward = transform.forward;
        Quaternion playerRot = transform.rotation;
        float simulatedSpeed = currentSpeed.value / math.clamp(math.abs(Vector3.Angle(playerForward, direction) / 20.0f), 1.0f, 4.0f);
        int wayToRotate = CrossProductPositive(playerForward, direction) ? 1 : -1;
        int j = 0;
        for (int i = 0; i < trajPoints.Length * mm.framesBetweenTrajectoryPoints; i++)
        {
            previousPlayerPos = playerPos;
            float newRotationSpeedGoal = Mathf.Min(rotationSpeed.value, Vector3.Angle(playerForward, direction) * 0.001f) * wayToRotate;
            newRotationSpeedCurrent += (newRotationSpeedGoal - newRotationSpeedCurrent) * 0.001f;

            playerRot = Quaternion.Slerp(playerRot, lookRotation, Time.deltaTime * Mathf.Abs(newRotationSpeedCurrent));
            playerForward = playerRot * Vector3.forward;
            playerPos += (direction + playerForward).normalized * ((targetSpeed - simulatedSpeed) * 0.01f + simulatedSpeed) * Time.fixedDeltaTime;

            if (i % mm.framesBetweenTrajectoryPoints == 0)
            {
                trajPoints[j] = new TrajectoryPoint(playerPos, playerForward);
                j++;
            }

            simulatedSpeed = Vector3.Distance(playerPos, previousPlayerPos) / Time.fixedDeltaTime;
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

    public void AttackTriggerAnimation()
    {
        if (!atOrbTrigger.value)
        {
            playerAttack(pushDirection);
        }

    }
}
