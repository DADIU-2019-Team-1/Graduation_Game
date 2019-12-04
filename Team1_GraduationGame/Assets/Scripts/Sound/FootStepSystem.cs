// Script by Jakob Elkjær Husted
namespace Team1_GraduationGame.Sound
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Team1_GraduationGame.Events;

    [RequireComponent(typeof(AkGameObj))]
    public class FootStepSystem : MonoBehaviour
    {
        public bool checkMaterial = true;
        public string[] materialTypes;
        public AK.Wwise.Event footStepEvent;
        public AK.Wwise.RTPC materialRTPC;
        private bool _active = false, _footStepCooldown = false;
        //public FloatEvent footStepEvent;

        private void Awake()
        {
            if (materialTypes != null && footStepEvent != null && materialRTPC != null)
                for (int i = 0; i < materialTypes.Length; i++)
                {
                    materialTypes[i] = materialTypes[i].ToLower();
                    _active = true;
                }
        }

        private void OnTriggerEnter(Collider col)
        {
            if (_active)
            {
                if (!_footStepCooldown)
                {
                    _footStepCooldown = true;

                    if (col.tag == "Enemy" || col.tag == "Player")
                        return;

                    if (checkMaterial)
                    {
                        if (col.gameObject.GetComponent<Terrain>() != null)
                        {
                            Terrain thisTerrain = col.gameObject.GetComponent<Terrain>();
                            if (thisTerrain.terrainData.terrainLayers.Length > 0)   // Currently only finds the top layer in the terrain. Not sure how to detect if a second layer is used?
                            {
                                FootStepRaise(thisTerrain.terrainData.terrainLayers[0].diffuseTexture.ToString());
                            }
                        }
                        else if (col.gameObject.GetComponent<Collider>() != null)
                        {
                            FootStepRaise(col.gameObject.name);
                        }
                    }
                    else
                    {
                        footStepEvent.Post(gameObject);
                    }

                    Invoke("FootStepCooldown", 0.3f);
                }
            }
        }

        private void FootStepCooldown()
        {
            _footStepCooldown = false;
        }

        private void FootStepRaise(string matLayerName)
        {
            if (materialTypes == null)
                return;

            matLayerName = matLayerName.ToLower();

            for (int i = 0; i < materialTypes.Length; i++)
            {
                if (matLayerName.Contains(materialTypes[i]))
                {
                    AkSoundEngine.SetRTPCValue(materialRTPC.Id, i);
                    footStepEvent.Post(gameObject);
                    return;
                }
            }
        }

        public void DisableMaterialCheck()
        {
            checkMaterial = false;
        }
    }
}