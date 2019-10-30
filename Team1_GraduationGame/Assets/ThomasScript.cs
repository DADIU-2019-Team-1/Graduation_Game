using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThomasScript : MonoBehaviour
{

    GameObject player;
    public Transform resetPos;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            player.transform.position = resetPos.position;
        }
    }
}
