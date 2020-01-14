using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using WDIB.Components;
using WDIB.Player;
using WDIB.Utilities;

namespace WDIB.Weapons
{
    [UpdateInGroup(typeof(SupplementalSystemGroup))]
    public class ShootFromSystem : JobComponentSystem
    {
        private EntityQuery StatesQuery;

        [BurstCompile]
        [RequireComponentTag(typeof(ShootFromMuzzleTag))]
        public struct UpdateShootFromMuzzle : IJobForEach<ShootFrom, Muzzle, OwnerID>
        {
            public void Execute([WriteOnly] ref ShootFrom shootfrom, [ReadOnly] ref Muzzle muzzle, [ReadOnly] ref OwnerID owner)
            {
                shootfrom = new ShootFrom
                {
                    Position = muzzle.Position,
                    Rotation = muzzle.Rotation
                };
            }
        }


        [BurstCompile]
        [RequireComponentTag(typeof(ShootFromCameraTag))]
        public struct UpdateShootFromCamera : IJobForEach<ShootFrom, OwnerID>
        {
            [DeallocateOnJobCompletion]
            public NativeArray<PlayerState> states;
            [DeallocateOnJobCompletion]
            public NativeArray<OwnerID> statesID;

            public void Execute([WriteOnly] ref ShootFrom shootFrom, [ReadOnly] ref OwnerID owner)
            {
                for (int i = 0; i < states.Length; i++)
                {
                    if (statesID[i].Value == owner.Value)
                    {
                        shootFrom = new ShootFrom
                        {
                            Position = states[i].CameraPos,
                            Rotation = states[i].CameraRot
                        };
                    }
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle fromMuzzle = new UpdateShootFromMuzzle
            { }.Schedule(this, inputDeps);

            if(StatesQuery.CalculateEntityCount() > 0)
            {
                JobHandle fromCamera = new UpdateShootFromCamera()
                {
                    states = StatesQuery.ToComponentDataArray<PlayerState>(Allocator.TempJob),
                    statesID = StatesQuery.ToComponentDataArray<OwnerID>(Allocator.TempJob)

                }.Schedule(this, fromMuzzle);

                return fromCamera;
            }

            return fromMuzzle;
        }

        protected override void OnCreate()
        {
            StatesQuery = GetEntityQuery(new EntityQueryDesc
                {
                    All = new ComponentType[] { ComponentType.ReadOnly<PlayerState>(), ComponentType.ReadOnly<OwnerID>()
                }
            });
        }
    }
}