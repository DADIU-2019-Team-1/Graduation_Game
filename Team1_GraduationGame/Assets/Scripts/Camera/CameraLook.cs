using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Tooltip("If no player is defined, the GameObject with the tag \"Player\" will be used.")]
    public Transform player;
    public Vector3 offset = new Vector3(0.0f,0.0f,0.0f);

    private Movement playerMovement;
    private Vector3 moveDir;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player.GetComponent<Movement>() != null)
            playerMovement = player.GetComponent<Movement>();
        else
            Debug.LogError("The player does not have a movement script attached!", player);
    }

    // Update is called once per frame
    void Update()
    {
        moveDir = player.forward * playerMovement.GetSpeed();
        transform.position = player.position + offset;
    }
}
