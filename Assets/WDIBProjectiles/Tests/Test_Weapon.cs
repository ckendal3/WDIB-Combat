using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Inputs;
using WDIB.Player;
using WDIB.Weapons;

public class Test_Weapon : MonoBehaviour
{
    private Entity weapEntity;
    private Entity barrelSocket;

    [SerializeField]
    public DebugShootFromType type = DebugShootFromType.Muzzle;

    EntityManager EntityManager;

    public GameObject offsetObject;
    private float offsetDistance = 2f;


    // Start is called before the first frame update
    void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        CreateFakePlayer();
        if(offsetObject != null)
        {
            offsetDistance = math.distance(transform.position, offsetObject.transform.position);
        }

        weapEntity = WeaponFactory.CreateWeapon(0, transform.position, transform.rotation, 0, offsetDistance);

        if(type == DebugShootFromType.Muzzle)
        {
            EntityManager.AddComponentData(weapEntity, new ShootFromMuzzleTag { });
        }
        else if(type == DebugShootFromType.Camera)
        {
            EntityManager.AddComponentData(weapEntity, new ShootFromCameraTag { });
        }

        
        EntityManager.AddComponentData(weapEntity, new EquippedTag { });
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
        EntityManager.AddComponentData<PlayerState>(entity, new PlayerState
        {
            CameraPos = transform.position,
            CameraRot = transform.rotation,
            Position = transform.position,
            Rotation = transform.rotation
        });



    }

    public enum DebugShootFromType
    {
        Camera,
        Muzzle
    }
}