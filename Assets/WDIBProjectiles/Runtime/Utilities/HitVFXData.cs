using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "VFX Data", menuName = "WDIB/Data/HitVFX")]
    public class HitVFXData : ScriptableObject
    {
        public MaterialParticle[] materialVFX;

        [Tooltip("Should a mesh spawn too?")]
        public bool bSpawnMesh = false;

        public Mesh mesh;
        public Material material;
    }

    [System.Serializable]
    public struct MaterialParticle
    {
        [Tooltip("This is used to determine the VFX to Spawn")]
        public MaterialType materialType;

        [Tooltip("The different particles for the different materials")]
        public ParticleSystem particleSystem;
    }

    public enum MaterialType
    {
        Metal,
        Rock,
        Water,
        Ice,
        Snow,
        Dirt
    }
}
