using Unity.Entities;
using UnityEngine;
using WDIB.Components;

namespace WDIB.Inputs
{
    public class InputSystem : ComponentSystem
    {
        private EntityQuery m_InputGroup;

        protected override void OnUpdate()
        {
            int count = m_InputGroup.CalculateEntityCount();

            if (count == 0)
            {
                return;
            }
            // TODO: Start setting inputstates 
            // TODO: Implement Local only input
            // TODO: Implement Network only input
            Entities.ForEach((ref InputState input) =>
            {
                // if our input is great than 0 
                input.IsPrimaryAction = Input.GetKey(KeyCode.Mouse0);
                // if our input is great than 0 
                input.IsSecondaryAction = Input.GetKey(KeyCode.Mouse1);

                // TODO: implement axis buttons for these specific buttons
                input.IsCrouching = Input.GetKey(KeyCode.LeftControl);
                input.IsJumping = Input.GetKey(KeyCode.Space);
                input.IsReloading = Input.GetKey(KeyCode.R);

                #region Movement Input
                input.MovementInput.x = Input.GetAxis("Horizontal");
                input.MovementInput.y = Input.GetAxis("Vertical");
                #endregion

                #region Rotation Input
                input.RotationInput.x = Input.GetAxis("Mouse X");
                input.RotationInput.y = Input.GetAxis("Mouse Y");
                #endregion
            });
        }

        protected override void OnCreate()
        {
            m_InputGroup = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite<InputState>(), ComponentType.ReadOnly<OwnerID>() }
            });
        }
    }
}