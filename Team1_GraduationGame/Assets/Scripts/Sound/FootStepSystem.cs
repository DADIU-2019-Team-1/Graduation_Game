namespace Team1_GraduationGame.Sound
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FootStepSystem : MonoBehaviour
    {

        private void Awake()
        {

        }

        private void OnCollisionEnter(Collision col)
        {
            // Debug.Log(col.gameObject.name);

            if (col.gameObject.GetComponent<Terrain>() != null)
            {
                Terrain thisTerrain = col.gameObject.GetComponent<Terrain>();
                Debug.Log(thisTerrain.terrainData.GetAlphamapTexture(0).name);
            }

        }

    }
}

