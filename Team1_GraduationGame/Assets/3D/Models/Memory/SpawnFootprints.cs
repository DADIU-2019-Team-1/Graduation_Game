using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnFootprints : MonoBehaviour
{
    public GameObject Rclone;
    public GameObject Lclone;
    public Transform LfootPos;
    public Transform RfootPos;
    public Transform Mother;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void FootprintSpawnL()
    {
        GameObject gameObject = Instantiate(Lclone, LfootPos.position,Quaternion.identity);
        gameObject.transform.Rotate(new Vector3(0, Mother.localEulerAngles.y,0));
        gameObject.SetActive(true);
    }
        public void FootprintSpawnR()
    {
        GameObject gameObject = Instantiate(Rclone, RfootPos.position, Quaternion.identity /*new Quaternion(Rclone.transform.rotation[0], Rclone.transform.rotation[0], Rclone.transform.rotation[2], Rclone.transform.rotation[3])*/);
        gameObject.transform.Rotate(new Vector3(0, Mother.localEulerAngles.y,0));
        gameObject.SetActive(true);
    }
}
