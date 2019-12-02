using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using WDIB.Components;
using WDIB.Inputs;
using WDIB.Parameters;
using WDIB.Player;
using WDIB.Projectiles;
using WDIB.Utilities;

namespace WDIB.Weapons
{
    // TODO: Get rid of projectilequeuedata stuff
    [UpdateInGroup(typeof(SupplementalSystemGroup))]
    public class WeaponSystem : JobComponentSystem
    {
        private EntityQuery InputQuery;

        [BurstCompile]
        [RequireComponentTag(typeof(EquippedTag))]
        public struct ProcessWeaponState : IJobForEach<Weapon, WeaponState, OwnerID>
        {
            [DeallocateOnJobCompletion]
            public NativeArray<InputState> inputs;

            [DeallocateOnJobCompletion]
            public NativeArray<OwnerID> inputsOwner;

            public void Execute([ReadOnly] ref Weapon weapon, [WriteOnly] ref WeaponState state, [ReadOnly] ref OwnerID owner)
            {
                state = new WeaponState()
                {
                    IsReloading = false,
                    IsShooting = false
                };


                for (int i = 0; i < inputs.Length; i++)
                {
                    if (owner.Value == inputsOwner[i].Value)
                    {
                        state.IsShooting = inputs[i].IsPrimaryAction;
                        state.IsReloading = inputs[i].IsReloading;
                    }
                }
            }
        }

        [BurstCompile]
        public struct ShootWeaponJob : IJobForEach<WeaponState, Weapon, TimeBetweenShots, OwnerID, Rotation, ShootFrom>
        {
            public NativeQueue<ProjectileQueueData>.ParallelWriter queueData;

            public void Execute([ReadOnly] ref WeaponState state, [ReadOnly] ref Weapon weapon, ref TimeBetweenShots timeBetween,
                [ReadOnly] ref OwnerID owner, [ReadOnly] ref Rotation rotation, [ReadOnly] ref ShootFrom shootFrom)
            {
                if (state.IsShooting && timeBetween.Value < 0)
                {
                    // gather all our shots to be fired
                    queueData.Enqueue(new ProjectileQueueData { Owner = owner.Value, WeaponID = weapon.ID, SpawnPos = shootFrom.Position, SpawnRot = shootFrom.Rotation });

                    // we shot, gun needs to wait
                    timeBetween.Value = timeBetween.ResetValue;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeQueue<ProjectileQueueData> queueData = new NativeQueue<ProjectileQueueData>(Allocator.TempJob);

            // set weapon states
            JobHandle processWeaponJob = new ProcessWeaponState()
            {
                inputs = InputQuery.ToComponentDataArray<InputState>(Allocator.TempJob),
                inputsOwner = InputQuery.ToComponentDataArray<OwnerID>(Allocator.TempJob)

            }.Schedule(this, inputDeps);

            // create projectile data for weapons
            JobHandle shootJob = new ShootWeaponJob()
            {
                queueData = queueData.AsParallelWriter()
            }.Schedule(this, processWeaponJob);

            shootJob.Complete();

            // Spawn all of projectiles from the weapons
            for (int i = 0; i < queueData.Count; i++)
            {
                ProjectileQueueData data = queueData.Dequeue();

                ProjectileFactory.CreateProjectiles(WeaponParameters.Instance.GetWeaponDataByID(data.WeaponID).projectileId, data.SpawnPos, data.SpawnRot, data.Owner);
            }

            queueData.Dispose();

            return shootJob;
        }

        protected override void OnCreate()
        {
            InputQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<InputState>(), ComponentType.ReadOnly<OwnerID>(), ComponentType.ReadOnly<PlayerState>() }
            });

        }
    }

    public struct ProjectileQueueData
    {
        public int WeaponID;
        public uint Owner;

        public float3 SpawnPos;
        public quaternion SpawnRot;
    }
}