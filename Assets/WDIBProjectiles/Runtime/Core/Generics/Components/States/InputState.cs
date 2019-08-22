using Unity.Entities;
using Unity.Mathematics;

public struct InputState : IComponentData
{
    public bool isPrimaryAction;
    public bool isSecondaryAction;

    public bool isReloading; // think reloading

    public bool isPulling;
    public bool isPushing;
    public bool isJumping;
    public bool isCrouching;

    public float2 movementInput;
    public float2 rotationInput;
}
