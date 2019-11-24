using Unity.Entities;
using Unity.Mathematics;

namespace WDIB.Player
{
    public struct PlayerState : IComponentData
    {
        public float3 CameraPos;
        public float3 CameraRot;

        public float3 Position;
        public quaternion Rotation;
    }
}