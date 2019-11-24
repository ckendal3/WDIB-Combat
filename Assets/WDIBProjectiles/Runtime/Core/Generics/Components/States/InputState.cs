using Unity.Entities;
using Unity.Mathematics;

namespace WDIB.Inputs
{
    public struct InputState : IComponentData
    {
        public bool IsPrimaryAction;
        public bool IsSecondaryAction;

        public bool IsReloading;

        public bool IsJumping;
        public bool IsCrouching;

        public float2 MovementInput;
        public float2 RotationInput;
    }
}