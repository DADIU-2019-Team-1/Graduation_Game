using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Tooltip("If no player is defined, the GameObject with the tag \"Player\" will be used.")]
    public Transform player;
    public Vector3 offset = new Vector3(0.0f,0.0f,0.0f);

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + offset;
    }
}
