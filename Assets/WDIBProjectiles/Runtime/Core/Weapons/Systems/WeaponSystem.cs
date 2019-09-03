using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using WDIB.Components;
using WDIB.Factory;
using WDIB.Parameters;

public class WeaponSystem : JobComponentSystem
{
    private EntityQuery m_Inputs;

    // This creates a new weapon state based on weapon state
    [BurstCompile]
    public struct ProcessWeaponState : IJobForEach<Weapon, WeaponState, Owner>
    {
        [DeallocateOnJobCompletion]
        public NativeArray<InputState> inputs;

        [DeallocateOnJobCompletion]
        public NativeArray<Owner> inputsOwner;

        public void Execute([ReadOnly] ref Weapon weapon, [WriteOnly] ref WeaponState state, [ReadOnly] ref Owner owner)
        {
            state = new WeaponState()
            {
                isReloading = false,
                isShooting = false
            };


            for (int i = 0; i < inputs.Length; i++)
            {
                if(owner.ID == inputsOwner[i].ID)
                {
                    state.isShooting = inputs[i].isPrimaryAction;
                    state.isReloading = inputs[i].isReloading;
                }
            }
        }
    }

    // TODO: Should use PlayerState's Camera Position and Rotation for projectiles
    [BurstCompile]
    public struct ShootFromJob : IJobForEach<WeaponState, Weapon, TimeBetweenShots, Owner, Rotation, ShootFrom>
    {
        public NativeQueue<ProjectileQueueData>.Concurrent queueData;

        public void Execute([ReadOnly] ref WeaponState state, [ReadOnly] ref Weapon weapon, [ReadOnly] ref TimeBetweenShots timeBetween, 
            [ReadOnly] ref Owner owner, [ReadOnly] ref Rotation rotation, [ReadOnly] ref ShootFrom shootFrom)
        {
            if (state.isShooting && timeBetween.value < 0)
            {
                // gather all our shots to be fired
                queueData.Enqueue(new ProjectileQueueData { owner = (int)owner.ID, weaponID = weapon.ID, spawnPos = shootFrom.position, spawnRot = shootFrom.rotation });

                // we shot, gun needs to wait
                timeBetween.value = timeBetween.resetValue;
            }
        }
    }

    [BurstCompile]
    [ExcludeComponent(typeof(ShootFrom))]
    public struct ShootFromMuzzleJob : IJobForEach<WeaponState, Weapon, TimeBetweenShots, Owner, Rotation, MuzzleOffset>
    {
        public NativeQueue<ProjectileQueueData>.Concurrent queueData;

        public void Execute([ReadOnly] ref WeaponState state, [ReadOnly] ref Weapon weapon, [ReadOnly] ref TimeBetweenShots timeBetween, 
            [ReadOnly] ref Owner owner, [ReadOnly] ref Rotation rotation, [ReadOnly] ref MuzzleOffset muzzle)
        {
            if (state.isShooting && timeBetween.value < 0)
            {
                // gather all our shots to be fired
                queueData.Enqueue(new ProjectileQueueData { owner = (int)owner.ID, weaponID = weapon.ID, spawnPos = muzzle.Value, spawnRot = rotation.Value });

                // we shot, gun needs to wait
                timeBetween.value = timeBetween.resetValue;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        NativeQueue<ProjectileQueueData> queueData = new NativeQueue<ProjectileQueueData>(Allocator.TempJob);

        // set weapon states
        JobHandle processWeaponJob = new ProcessWeaponState()
        {
            inputs = m_Inputs.ToComponentDataArray<InputState>(Allocator.TempJob),
            inputsOwner = m_Inputs.ToComponentDataArray<Owner>(Allocator.TempJob)

        }.Schedule(this, inputDeps);

        // create projectile data for weapons
        JobHandle shootFromJob = new ShootFromJob()
        {
            queueData = queueData.ToConcurrent()
        }.Schedule(this, processWeaponJob);

        shootFromJob.Complete();

        // create projectile data for weapons
        JobHandle shootFromMuzzleJob = new ShootFromMuzzleJob()
        {
            queueData = queueData.ToConcurrent()
        }.Schedule(this, processWeaponJob);

        shootFromMuzzleJob.Complete();


        // Spawn all of projectiles from the weapons
        for (int i = 0; i < queueData.Count; i++)
        {
            ProjectileQueueData data = queueData.Dequeue();

            ProjectileFactory.CreateProjectiles(WeaponParameters.Instance.GetWeaponDataByID(data.weaponID).projectileId, data.spawnPos, data.spawnRot, (uint) data.owner);
        }

        queueData.Dispose();

        return JobHandle.CombineDependencies(shootFromJob, shootFromMuzzleJob);
    }

    protected override void OnCreate()
    {
        m_Inputs = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<InputState>(), ComponentType.ReadOnly<Owner>(), ComponentType.ReadOnly<PlayerState>() }
        }) ;

    }
}

public struct ProjectileQueueData
{
    public int weaponID;
    public int owner;

    public float3 spawnPos;
    public quaternion spawnRot;
}