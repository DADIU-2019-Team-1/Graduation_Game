using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFlyGlow : MonoBehaviour
{

    Light light;
    bool decrease = false;

    // Start is called before the first frame update
    void Start()
    {
        light = gameObject.GetComponentInChildren<Light>();
    }



    // Update is called once per frame
    void Update()
    {
        if(decrease)
        {
            if (light.intensity > 0)
            {
                light.intensity -= Time.deltaTime;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void DecreaseLigtAndDisable()
    {
        decrease = true;
    }
}
