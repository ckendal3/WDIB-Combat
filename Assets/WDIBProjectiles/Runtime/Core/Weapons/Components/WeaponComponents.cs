using Unity.Entities;
using Unity.Mathematics;

namespace WDIB.Weapons
{
    public struct Weapon : IComponentData
    {
        public int ID;
    }

    public struct TimeBetweenShots : IComponentData
    {
        public float Value;
        public float ResetValue;
    }

    public struct WeaponState : IComponentData
    {
        public bool IsShooting;
        public bool IsReloading;
    }

    public struct ShootFrom : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    public struct Muzzle : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;

        public float Offset;
    }

    public struct BurstFire : IComponentData
    {
        public float Value;
        public float ResetValue;

        public int Count;
        public int MaxCount;
    }

    public struct MeleeComponent : IComponentData
    {
        public float Damage; // how much damage to do

        // TODO: Read the comments below
        // these should be pairable with other actions - it could just use an action tag
        public float Duration; // how long till its done
        public float ResetDuration; // this is where the countdown timer starts
    }

    public struct LungeComponent : IComponentData
    {
        public float Distance; // max lunge distance
        public float Rate; // how many m/s to move
    }

    //TODO: It should be a buffer to determine how many levels
    // each element is the amount to zoom in
    public struct Zoom : IComponentData
    {

    }

    public struct Actioning : IComponentData
    {
        public float Value;
    }

    /// <summary>
    /// ***************** TAGS **********************
    /// </summary>
    
    public struct ShootFromCameraTag : IComponentData { }

    public struct ShootFromMuzzleTag : IComponentData { }

    public struct ReloadTag : IComponentData { }

    /// <summary>
    /// A tag to determine if the weapon is equipped
    /// </summary>
    public struct EquippedTag : IComponentData { }
}