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

    public struct ShootFromOffset : IComponentData
    {
        public float3 Value;
        public float3 Offset;
        public float3 Heading;
    }

    public struct ShootFromCamera : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    public struct ProjectileCount : IComponentData
    {
        public int Value;
    }

    public struct TimeBetweenBurstShots : IComponentData
    {
        public float Value;
        public float ResetValue;
    }

    // This should just be an offset point?
    public struct ProjectileSpawnPoint : IComponentData
    {
        public float distance; // how far away to spawns
        public float3 heading;
        public float3 direction; // the direction
    }

    public struct Reload : IComponentData
    {
        public float Value; // this is how long is left
        public float ResetValue; // this is where the countdown timer starts
    }

    public struct ReloadTag : IComponentData { }

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

    /// <summary>
    /// A tag to determine if the weapon is equipped
    /// </summary>
    public struct EquippedTag : IComponentData { }

    public struct Actioning : IComponentData
    {
        public float Value;
    }
}