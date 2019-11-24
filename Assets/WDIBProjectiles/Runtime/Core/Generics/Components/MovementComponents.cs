using Unity.Entities;
using Unity.Mathematics;

namespace WDIB.Components
{
    /// <summary>
    /// Last frames translation of this entity
    /// </summary>
    public struct PreviousTranslation : IComponentData
    {
        public float3 Value;
    }

    /// <summary>
    /// How fast an entity travels
    /// </summary>
    public struct Speed : IComponentData
    {
        public float Value;
    }

    /// <summary>
    /// How far entity has and can travel.
    /// </summary>
    public struct Distance : IComponentData
    {
        public float Value;
        public float MaxDistance;
    }

}
