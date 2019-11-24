using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Inputs;
using WDIB.Weapons;

public class Test_Weapon : MonoBehaviour
{
    private Entity weapEntity;
    private Entity barrelSocket;

    EntityManager EntityManager;

    public GameObject offsetObject;
    private float3 offsetDistance = new float3(0,0,0);


    // Start is called before the first frame update
    void Start()
    {
        EntityManager = World.Active.EntityManager;

        CreateFakePlayer();
        if(offsetObject != null)
        {
            offsetDistance = transform.position - offsetObject.transform.position;
        }



        weapEntity = WeaponFactory.CreateWeapon(0, transform.position, transform.rotation, 0, offsetDistance);

        barrelSocket = EntityManager.CreateEntity();
        EntityManager.SetName(barrelSocket, "barrelSocket");
        EntityManager.AddComponentData(barrelSocket, new Parent { Value = weapEntity });
        EntityManager.AddComponentData(barrelSocket, new LocalToWorld { });
        EntityManager.AddComponentData(barrelSocket, new LocalToParent { });
        EntityManager.AddComponentData(barrelSocket, new Rotation { Value = transform.rotation });
        EntityManager.AddComponentData(barrelSocket, new Translation { Value = offsetObject.transform.position });
        EntityManager.AddComponentData(barrelSocket, new PreviousParent { });

    }

    void CreateFakePlayer()
    {
        Entity entity = EntityManager.CreateEntity();

        EntityManager.AddComponentData(entity, new InputState()
        {
            IsPrimaryAction = false,
            IsSecondaryAction = false,
            IsCrouching = false,
            IsJumping = false,
            IsReloading = false,
            MovementInput = new Unity.Mathematics.float2(),
            RotationInput = new Unity.Mathematics.float2()
        });

        EntityManager.AddComponentData<OwnerID>(entity, new OwnerID() { Value = 0 });
    }
}