using Unity.Entities;
using Unity.Mathematics;


// TODO: Implement systems
// TODO: Make PickupAble component which is for ammo and the weapon itself. Could check if player is close enough or a trigger event with a raycast for walls


// The buffers should act as if they are inventory holders

// only allocate for 2 weapons
// we use an entity because we need to actually keep track of the weapon and its data
[InternalBufferCapacity(2)]
public struct WeaponsBuffer : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator Entity(WeaponsBuffer e) { return e.Value; }
    public static implicit operator WeaponsBuffer(Entity e) { return new WeaponsBuffer { Value = e }; }

    // Actual value each buffer element will store.
    public Entity Value;
}

// only allocate for 4 grenades
// we use an int because we don't actually need the object, its just an ID to spawn the proper grenade 
[InternalBufferCapacity(4)]
public struct GrenadesBuffer : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator int(GrenadesBuffer e) { return e.Value; }
    public static implicit operator GrenadesBuffer(int e) { return new GrenadesBuffer { Value = e }; }

    // Actual value each buffer element will store.
    public int Value;
}

// TODO: Create Entity Archetype for Inventory: Entity with InventoryTag, WeaponsBuffer, GrenadesBuffer
public struct InventoryTag : IComponentData { }

public struct Weapon : IComponentData
{
    public int ID;
}

public struct TimeBetweenShots : IComponentData
{
    public float value;
    public float resetValue;
}

public struct WeaponState : IComponentData
{
    public bool isShooting;
    public bool isReloading;
}

public struct ShootFrom : IComponentData
{
    public Entity entity;
    public float3 position;
    public quaternion rotation;
}

public struct MuzzleOffset : IComponentData
{
    public float3 Value;
}
