// Code owner: Jannik Neerdal - Optimized
using Cinemachine;
using UnityEngine;
using Unity.Mathematics;

public class CameraLook : MonoBehaviour
{
    // --- Inspector
    [Tooltip("If no player is defined, the GameObject with the tag \"Player\" will be used.")]
    public Transform player;

    [Tooltip("If no Input is defined, the object with the script \"CameraMovement\" attached will be used.")]
    public CameraMovement camMovement;

    [Tooltip("Control the amount that the players direction influences the camera.")]
    [Range(0.0f, 1.0f)] [SerializeField] private float lookDirFactor = 0.5f;
    public CameraOffset[] offsetTrack;

    // --- Hidden
    private Movement playerMovement;
    private CinemachineSmoothPath cmPath;
    private Camera cam;
    [HideInInspector] public Vector3 camTarget, camPosOffset;
    private Vector3 _lookDir,
        _camLookOffset;
    private float _offsetTrackLerpValue, 
        _startingFOV,
        _cameraFOV;

    private void Awake()
    {
        if (GetComponent<CinemachineSmoothPath>() != null)
            cmPath = GetComponent<CinemachineSmoothPath>();
        else
            Debug.LogError("The DollyTrack is missing its track!", gameObject);
        OnArrayChanged();

        // Check if the Offset Track has any indices that return null, then set those indices to zero
        for (int i = 0; i < offsetTrack.Length; i++)
        {
            if (offsetTrack[i] == null)
            {   
                for (int j = i; j < offsetTrack.Length; j++)
                {
                    offsetTrack[j] = new CameraOffset(Vector3.zero, Vector3.zero, 0.0f);
                }
                break;
            }
        }
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player.GetComponent<Movement>() != null)
            playerMovement = player.GetComponent<Movement>();
        else
            Debug.LogError("The player does not have a movement script attached!", player);

        if (camMovement == null)
            camMovement = FindObjectOfType<CameraMovement>();
        cam = camMovement.GetComponent<Camera>();
        _startingFOV = cam.fieldOfView;
    }

    public void OnArrayChanged() // Called in a custom inspector
    {
        if (cmPath == null)
        {
            if (GetComponent<CinemachineSmoothPath>() != null)
                cmPath = GetComponent<CinemachineSmoothPath>();
            else
                Debug.LogError("The DollyTrack is missing its track!", gameObject);
        }

        if (cmPath.m_Waypoints.Length != offsetTrack.Length)
        {
            CameraOffset[] temp = new CameraOffset[offsetTrack.Length];
            for (int i = 0; i < offsetTrack.Length; i++)
            {
                temp[i] = new CameraOffset(offsetTrack[i].GetLook(), offsetTrack[i].GetPos(), offsetTrack[i].GetFOV());
            }
            offsetTrack = new CameraOffset[cmPath.m_Waypoints.Length];
            for (int i = 0; i < cmPath.m_Waypoints.Length && i < temp.Length; i++)
            {
                offsetTrack[i] = temp[i];
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(camTarget, 0.5f);
    }

    void LateUpdate()
    {
        if (playerMovement != null)
        {
            _lookDir = player.forward * playerMovement.GetSpeed() * lookDirFactor;
        }

        // Update the Camera position, look position, and field of view, based on its position on the track, and the values the next and previous indices in the track have
        if (camMovement != null && cmPath.m_Waypoints.Length > 1 && offsetTrack.Length > 1) // Error-handling
        {
            _offsetTrackLerpValue = (camMovement.railCam.position.x - cmPath.m_Waypoints[camMovement.previousTrackIndex].position.x - camMovement.trackX) /
                             (cmPath.m_Waypoints[camMovement.nextTrackIndex].position.x - cmPath.m_Waypoints[camMovement.previousTrackIndex].position.x);
            _camLookOffset = Vector3.Lerp(offsetTrack[camMovement.previousTrackIndex].GetLook(), offsetTrack[camMovement.nextTrackIndex].GetLook(), _offsetTrackLerpValue);
            camPosOffset = Vector3.Lerp(offsetTrack[camMovement.previousTrackIndex].GetPos(), offsetTrack[camMovement.nextTrackIndex].GetPos(), _offsetTrackLerpValue);
            _cameraFOV = math.lerp(_startingFOV + offsetTrack[camMovement.previousTrackIndex].GetFOV(),_startingFOV + offsetTrack[camMovement.nextTrackIndex].GetFOV(), _offsetTrackLerpValue);
            if (cam.fieldOfView != _cameraFOV) // Only update the camera FOV if there was a change
                cam.fieldOfView = _cameraFOV;
        }
        else if (camMovement == null)
        {
            Debug.LogError("Camera movement reference is missing. Is there a camera in the scene with the Camera Movement script attached?");
        }
        else if (cmPath.m_Waypoints.Length < 2)
        {
            Debug.LogError("The rail does not have enough points to support proper camera look. The length of the rail is: " + cmPath.m_Waypoints.Length);
        }
        else if (offsetTrack.Length < 2)
        {
            Debug.LogError("The offset track does not have enough points to support proper camera look.\nThe length of the track is " + offsetTrack.Length + " and has to be at least 2!");
        }
        camTarget = player.position + _camLookOffset + _lookDir;
    }
}
