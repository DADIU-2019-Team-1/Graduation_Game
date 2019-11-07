using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThomasGoToScene : MonoBehaviour
{
    public bool forceSwitch;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GoToSceneWithName(string name)
    {
        Debug.Log("going to scene '" +name+"'");
        SceneManager.LoadScene(name);
    } 
    
    private void OnTriggerEnter(Collider other)
    {
        if(forceSwitch)
            GoToSceneWithName("Mem01");
    }
}


