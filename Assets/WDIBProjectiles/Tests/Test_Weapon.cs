using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Factory;

public class Test_Weapon : MonoBehaviour
{
    private Entity weapEntity;
    private Entity barrelSocket;

    EntityManager eManager;

    public GameObject offsetObject;
    private float3 offsetDistance = new float3(0,0,0);


    // Start is called before the first frame update
    void Start()
    {
        eManager = World.Active.EntityManager;

        CreateFakePlayer();
        if(offsetObject != null)
        {
            offsetDistance = transform.position - offsetObject.transform.position;
        }



        weapEntity = WeaponFactory.CreateWeapon(0, transform.position, transform.rotation, 0, offsetDistance);

        barrelSocket = eManager.CreateEntity();
        eManager.SetName(barrelSocket, "barrelSocket");
        eManager.AddComponentData(barrelSocket, new Parent { Value = weapEntity });
        eManager.AddComponentData(barrelSocket, new LocalToWorld { });
        eManager.AddComponentData(barrelSocket, new LocalToParent { });
        eManager.AddComponentData(barrelSocket, new Rotation { Value = transform.rotation });
        eManager.AddComponentData(barrelSocket, new Translation { Value = offsetObject.transform.position });
        eManager.AddComponentData(barrelSocket, new PreviousParent { });

    }

    void CreateFakePlayer()
    {
        Entity entity = eManager.CreateEntity();

        eManager.AddComponentData<InputState>(entity, new InputState()
        {
            isCrouching = false,
            isJumping = false,
            isPrimaryAction = false,
            isPulling = false,
            isSecondaryAction = false,
            isPushing = false,
            isReloading = false,
            movementInput = new Unity.Mathematics.float2(),
            rotationInput = new Unity.Mathematics.float2()
        });

        eManager.AddComponentData<Owner>(entity, new Owner() { ID = 0 });
    }
}