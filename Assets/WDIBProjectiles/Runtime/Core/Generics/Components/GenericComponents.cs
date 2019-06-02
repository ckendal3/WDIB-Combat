using Unity.Entities;
using Unity.Mathematics;

namespace WDIB.Components
{
    // these are the axis to lock and not do operations on
    public struct LockAxisRotation : IComponentData
    {
        public bool3 Value;
    }

    // These are the axis on movement that should move
    public struct LockAxisTranslation : IComponentData
    {
        public bool3 Value;
    }

    // this is like player controller - like the owner
    public struct Owner : IComponentData
    {
        public uint ID;
    }

    // Where the controlled object should be looking
    // this could be a shared component and save perf
  
    public struct ControlledByPlayer : IComponentData
    {
        public float3 lookPosition;
    }

    // How much health an entity should have
    public struct Health : IComponentData
    {
        public float Value;

        // This allows us to do stuff like: health - damage
        // it *implicitly* returns a float rather than the struct
        public static implicit operator float(Health health)
        {
            return health.Value;
        }
    }

    // The time the entity should be alive
    public struct LifeTime : IComponentData
    {
        public float Value;

        // This allows us to do stuff like: lifeTime - deltaTime
        // it *implicitly* returns a float rather than the struct
        public static implicit operator float(LifeTime lifeTime)
        {
            return lifeTime.Value;
        }

    }

    // how much damage the entity should do
    public struct Damage : IComponentData
    {
        public float Value;

        // This allows us to do stuff like: health - damage
        // it *implicitly* returns a float rather than the struct
        public static implicit operator float(Damage damage)
        {
            return damage.Value;
        }
    }


    // where an entity shoud look
    // probably use an entity/translation
    public struct TargetPosition : IComponentData
    {
        public float3 Value;
    }

    //****************************************************
    //**********************TAGS**************************
    //****************************************************

    public struct DynamicTargetTag : IComponentData {}

    // could set the entity to look at
    public struct TargetDynamicPositionTag : IComponentData { }
    public struct TargetStaticPositionTag : IComponentData { }
}
