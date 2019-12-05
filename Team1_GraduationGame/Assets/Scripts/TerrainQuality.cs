using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainQuality : MonoBehaviour
{

    public GameObject[] terrains;

    void Start()
    {
        updateTerrainQuality();
    }

    public void updateTerrainQuality()
    {
        if(QualitySettings.GetQualityLevel() == 0)
        {
            if(terrains != null)
            {
                foreach(GameObject terrainVar in terrains)
                {
                    if (terrainVar.GetComponent<Terrain>() != null)
                        terrainVar.GetComponent<Terrain>().drawTreesAndFoliage = false;
                }
            }
        }
        // Set terrain to do draw grass if settingLevel higher than 0
        else 
        {
            if(terrains != null)
            {
                foreach(GameObject terrainVar in terrains)
                {
                    if (terrainVar.GetComponent<Terrain>() != null)
                        terrainVar.GetComponent<Terrain>().drawTreesAndFoliage = true;
                }
            }
        }
    }
}