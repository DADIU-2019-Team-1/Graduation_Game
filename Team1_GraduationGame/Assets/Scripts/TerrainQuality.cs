// Code adapted from Jakob Baldwin by Jannik Neerdal
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TerrainQuality : MonoBehaviour
{
    public GameObject[] terrains;
    private List<Terrain> _terrainComponents = new List<Terrain>();
    void Start()
    {
        for (int i = 0; i < terrains.Length; i++)
        {
            if (terrains[i] != null)
            {
                _terrainComponents.Add(terrains[i].GetComponent<Terrain>());
            }
        }
        updateTerrainQuality();
    }

    public void updateTerrainQuality()
    {
        if(QualitySettings.GetQualityLevel() == 0)
        {
            for (int i = 0; i < _terrainComponents.Count; i++)
            {
                _terrainComponents[i].drawTreesAndFoliage = false;
            }
        }
        // Set terrain to do draw grass if settingLevel higher than 0
        else
        {
            for (int i = 0; i < _terrainComponents.Count; i++)
            {
                _terrainComponents[i].drawTreesAndFoliage = true;
            }
        }
    }
}