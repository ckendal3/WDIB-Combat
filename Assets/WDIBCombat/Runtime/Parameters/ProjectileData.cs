using Unity.Entities;
using UnityEditor;
using UnityEngine;
using WDIB.Components;
using WDIB.Explosives;
using WDIB.Projectiles;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "Projectile Data", menuName = "WDIB/Data/Projectile")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Main Data")]
        [Tooltip("Name of the Projectiles")]
        public string projectileName = "Default";

        // How many projectile to spawn per a shot
        [Range(1, 25)]
        public int projectilesPerAShot = 1;

        // How far before the projectile should destroy
        [Range(1f, 1000f)]
        public float maxDistance = 1000f;

        [Range(0f, 500f)]
        public float damage = 10f;

        // how many units should the projectile move a second
        [Range(1f, 1000f)]
        public float speed = 10f;

        public PelletSpread spread = default;

        // Visual settings
        [Header("Visual Components")]
        public Mesh mesh;
        public Material material;

        [Range(.01f, 10)]
        public float length = 1.0f;

        [Header("System Components")]

        [NaughtyAttributes.ReorderableList]
        public ComponentDataStruct[] componentData;
    }
}

[System.Serializable]
public struct PelletSpread
{
    [Range(0, -.35f)]
    public float minimumSpread;

    [Range(0, .35f)]
    public float maximumSpread;
}

//TODO: Write a property drawer for pelletspread

[System.Serializable]
public struct ComponentDataStruct : IGetComponent
{
    public EComponentType componentType;

    public float floatValue;
    public int intValue;

    public EComponentType GetComponentType()
    {
        return componentType;
    }

    public IComponentData GetComponentData()
    {
        switch (componentType)
        {
            case EComponentType.HeadShot:
                return new HeadShotMultiplier { Value = floatValue };
            case EComponentType.MultiHit:
                return new MultiHit { Hits = 0, MaxHits = intValue };
            case EComponentType.EMP:
                Debug.LogWarning("EMP is not implemented.");
                return null;
            case EComponentType.SuperCombine:
                return new SuperCombine { Hits = 0, HitsToCombine = intValue };
            case EComponentType.Tracking:
                return new TrackPlayer { ID = (uint) intValue };
            case EComponentType.Explosive:
                return new Explosive { ID = intValue };
            case EComponentType.NotImplemented:
                return null;
            default:
                return null;
        }
    }
}

public interface IGetComponent
{
    EComponentType GetComponentType();

    IComponentData GetComponentData();
}

public enum EComponentType
{
    HeadShot,
    MultiHit,
    EMP,
    SuperCombine,
    Tracking,
    Explosive,
    NotImplemented
}