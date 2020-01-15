using Unity.Entities;
using Unity.Mathematics;

// TODO: Add overridable systems

namespace WDIB.Components
{
    /// <summary>
    /// Can be used for spawn delays, time between shots, time between burst rounds, etc
    /// </summary>
    public struct TimeBetween : IComponentData
    {
        public float resetValue; // resets to this value when timeLeft <= 0
        public float timeLeft; // this is the timer before it can do whatever action again
    }

    /// <summary>
    /// The axes to lock and to prevent operations on (freeze)
    /// </summary>
    public struct LockAxisRotation : IComponentData
    {
        public bool3 Value;
    }

    /// <summary>
    /// The axes to prevent operations on (freeze)
    /// </summary>
    public struct LockAxisTranslation : IComponentData
    {
        public bool3 Value;
    }

    /// <summary>
    /// The player owner with occupanying ID
    /// </summary>
    public struct OwnerID : IComponentData
    {
        public uint Value;
    }

    /// <summary>
    /// Health component of an entity
    /// </summary>
    public struct Health : IComponentData
    {
        public float Value;
        public float MaxValue;
    }

    /// <summary>
    /// The lifetime of an entity
    /// </summary>
    public struct LifeTime : IComponentData
    {
        public float Value;
    }

    /// <summary>
    /// Amount of damage to apply on hits and the damage type
    /// </summary>
    public struct Damage : IComponentData
    {
        public float Value;
    }

    public struct SpecialDamage : IComponentData
    {
        public float Value;
        public DamageType DamageType;
    }

    /// <summary>
    /// Track target position
    /// </summary>
    public struct TrackPosition : IComponentData
    {
        public float3 Value;
    }

    /// <summary>
    /// Track target Entity
    /// </summary>
    public struct TrackEntity : IComponentData
    {
        public Entity Value;
    }

    public struct TrackPlayer : IComponentData
    {
        public uint ID;

        public float3 Position;
    }
}