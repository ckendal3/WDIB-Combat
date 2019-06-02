using Unity.Entities;
using Unity.Mathematics;

namespace WDIB.Components
{
    // previous position of an entity
    public struct PreviousTranslation : IComponentData
    {
        public float3 Value;

        // This allows us to do stuff like: previousTranslation - translation.value
        // it *implicitly* returns a float rather than the struct
        public static implicit operator float3(PreviousTranslation translation)
        {
            return translation.Value;
        }
    }

    // how fast an entity should move
    public struct Speed : IComponentData
    {
        public float Value;

        // This allows us to do stuff like: previousTranslation - translation.value
        // it *implicitly* returns a float rather than the struct
        public static implicit operator float(Speed speed)
        {
            return speed.Value;
        }
    }

    // how far an entity can and has traveled
    public struct Distance : IComponentData
    {
        public float Value;
        public float MaxDistance;
    }

}
