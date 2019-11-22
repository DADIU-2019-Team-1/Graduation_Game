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
        public string[] materialTypes;
        public AK.Wwise.Event footStepEvent;
        public AK.Wwise.RTPC materialRTPC;
        private bool _active = false;
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
                if (col.gameObject.GetComponent<Terrain>() != null)
                {
                    Terrain thisTerrain = col.gameObject.GetComponent<Terrain>();
                    if (thisTerrain.terrainData.terrainLayers.Length > 0)
                    {
                        FootStepRaise(thisTerrain.terrainData.terrainLayers[0].diffuseTexture.ToString());
                    }
                }
                else if (col.gameObject.GetComponent<Collider>() != null)
                {
                    FootStepRaise(col.gameObject.name);
                }
            }
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
                    footStepEvent.Post(gameObject);
                    AkSoundEngine.SetRTPCValue(materialRTPC.Id, i);
                    return;
                }
            }
        }
    }
}