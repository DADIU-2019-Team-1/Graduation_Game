// Code owner: Jannik Neerdal
using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

[ExecuteInEditMode]
public class CameraLook : MonoBehaviour
{
    // --- Inspector
    [Tooltip("If no player is defined, the GameObject with the tag \"Player\" will be used.")]
    public Transform player;

    [Tooltip("If no Input is defined, the object with the script \"CameraMovement\" attached will be used.")]
    public CameraMovement camMovement;

    [Tooltip("The default offset that will always be added")]
    public Vector3 defaultOffset = new Vector3(0.0f,0.0f,0.0f);

    [Tooltip("Control the amount that the players direction influences the camera.")]
    [Range(0.0f, 1.0f)] [SerializeField] private float lookDirFactor = 0.5f;

    [Tooltip("Determines how long it should take for the offset to update. 1 is instant.")]
    [Range(0.01f, 1.0f)] [SerializeField] private float offsetLerpTime = 0.05f;

    public CameraOffset[] offsetTrack;

    // --- Hidden
    [HideInInspector] public Vector3 camTarget;
    private Movement playerMovement;
    private Vector3 moveDir;
    private Vector3 offset;
    private CinemachineSmoothPath cmPath;

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

        if (GetComponent<CinemachineSmoothPath>() != null)
            cmPath = GetComponent<CinemachineSmoothPath>();
        else
            Debug.LogError("The DollyTrack is missing its track!", gameObject);

        if (cmPath.m_Waypoints.Length != offsetTrack.Length)
        {
            CameraOffset[] temp = new CameraOffset[offsetTrack.Length];
            for (int i = 0; i < offsetTrack.Length; i++)
            {
                temp[i] = new CameraOffset(offsetTrack[i].GetPos(), offsetTrack[i].GetFOV());
            }
            offsetTrack = new CameraOffset[cmPath.m_Waypoints.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                offsetTrack[i] = temp[i]; // TODO: Move to OnValidate or similar
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(camTarget, 1.0f);
    }

    void Update()
    {
        if (playerMovement != null)
        {
            moveDir = player.forward * playerMovement.GetSpeed() * lookDirFactor;
        }
        offset = Vector3.Lerp(offset, (defaultOffset + offsetTrack[camMovement.previousTrackIndex].GetPos() + offsetTrack[camMovement.nextTrackIndex].GetPos()) / 2, offsetLerpTime);
        camTarget = player.position + offset + moveDir;
    }
}
