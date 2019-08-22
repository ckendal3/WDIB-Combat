using Unity.Entities;
using UnityEngine;
using WDIB.Components;

public class InputSystem : ComponentSystem
{
    private EntityQuery m_InputGroup;

    protected override void OnUpdate()
    {
        int count = m_InputGroup.CalculateLength();

        if(count < 1)
        {
            return;
        }

        // TODO: Start setting inputstates 
        // TODO: Implement Local only input
        // TODO: Implement Network only input
        Entities.ForEach((ref InputState input) =>
        {
            // if our input is great than 0 
            input.isPrimaryAction = Input.GetKey(KeyCode.Mouse0);
            // if our input is great than 0 
            input.isSecondaryAction = Input.GetKey(KeyCode.Mouse1);

            // TODO: implement axis buttons for these specific buttons
            input.isCrouching = Input.GetKey(KeyCode.LeftControl);
            input.isJumping = Input.GetKey(KeyCode.Space);
            input.isPulling = Input.GetKey(KeyCode.E);
            input.isPushing = Input.GetKey(KeyCode.Q);
            input.isReloading = Input.GetKey(KeyCode.R);

            #region Movement Input
            input.movementInput.x = Input.GetAxis("Horizontal");
            input.movementInput.y = Input.GetAxis("Vertical");
            #endregion

            #region Rotation Input
            input.rotationInput.x = Input.GetAxis("Mouse X");
            input.rotationInput.y = Input.GetAxis("Mouse Y");
            #endregion
        });
    }

    protected override void OnCreate()
    {
        m_InputGroup = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { ComponentType.ReadWrite<InputState>(), ComponentType.ReadOnly<Owner>() }
        });
    }
}