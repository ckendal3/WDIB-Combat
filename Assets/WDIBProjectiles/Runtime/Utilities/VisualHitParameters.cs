using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WDIB.Utilities;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "VisualHit Parameters", menuName = "WDIB/Parameters/VisualHitParameters")]
    public class VisualHitParameters : SingletonScriptableObject<VisualHitParameters>
    {
        [SerializeField]
        private HitVFXData[] hitVFXSet = null;

        [SerializeField]
        private HitVFXData defaultVFX = null;

        public HitVFXData GetHitVFXDataByType(int ID, MaterialType matType)
        {
            // if our id is less than the length
            if(ID < hitVFXSet.Length)
            {
                HitVFXData data = hitVFXSet[ID];
                for (int i = 0; i < data.materialVFX.Length; i++)
                {
                    if (data.materialVFX[i].materialType == matType)
                    {
                        return hitVFXSet[i];
                    }
                }

                #if UNITY_EDITOR
                Debug.LogWarning("Could not find matching MaterialType - returning default VFX.");
                #endif
                return defaultVFX;
            }

            #if UNITY_EDITOR
            Debug.LogWarning("Material ID is out of range - returning default VFX.");
            #endif
            return defaultVFX;
        }   
    }
}