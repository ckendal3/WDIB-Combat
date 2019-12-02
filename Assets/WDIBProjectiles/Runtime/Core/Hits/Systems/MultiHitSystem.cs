using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Projectiles;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(HitSystemGroup))]
    public class MultiHitSystem : JobComponentSystem
    {
        private EntityQuery MultiHitQuery;
        private RaycastHit[] Hits;

        private WeaponParameters Parameters;
        private LayerMask HitMask;

        public delegate void HitMultiSystemEvent(HitHandlerData ecsData, NativeArray<RaycastHit> hitsData);
        public static HitMultiSystemEvent onMultiHitSystemFinish;

        [BurstCompile]
        public struct SetupCommandsJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<PreviousTranslation> prevPositions;

            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Translation> positions;

            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Rotation> rotations;

            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<MultiHit> multiHits;

            [WriteOnly] public NativeArray<RaycastCommand> commands;
            [ReadOnly] public LayerMask hitLayer;

            public void Execute(int index)
            {
                commands[index] = new RaycastCommand
                {
                    from = prevPositions[index].Value,
                    direction = math.forward(rotations[index].Value),
                    distance = math.distance(positions[index].Value, prevPositions[index].Value),
                    layerMask = hitLayer,
                    maxHits = math.clamp(multiHits[index].MaxHits - multiHits[index].Hits, 0, multiHits[index].MaxHits)
                };
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // if there are no entities - return
            int count = MultiHitQuery.CalculateEntityCount();
            if (count == 0)
            {
                return inputDeps;
            }

            // Get the matching entities from the query
            var entities = MultiHitQuery.ToEntityArray(Allocator.TempJob, out JobHandle entArrayHandle);
            entArrayHandle.Complete();

            // Setup Ray Commands
            NativeArray<RaycastCommand> rayCommands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            JobHandle setupJob = new SetupCommandsJob
            {
                // these get the data from the m_HitGroup query
                prevPositions = MultiHitQuery.ToComponentDataArray<PreviousTranslation>(Allocator.TempJob),
                positions = MultiHitQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                rotations = MultiHitQuery.ToComponentDataArray<Rotation>(Allocator.TempJob),
                commands = rayCommands,
                hitLayer = HitMask,
                multiHits = MultiHitQuery.ToComponentDataArray<MultiHit>(Allocator.TempJob)
            }.Schedule(count, 32, inputDeps);

            setupJob.Complete();

            #region Default raycast
            Ray ray;
            int hitCount;
            int projectileID;
            HitHandlerData handlerData;
            // for every projectile
            for (int i = 0; i < count; i++)
            {
                ray = new Ray { origin = rayCommands[i].from, direction = rayCommands[i].direction };
                projectileID = EntityManager.GetComponentData<Projectile>(entities[i]).ID;

                handlerData = new HitHandlerData
                {
                    Entity = entities[i],
                    ProjectileID = EntityManager.GetComponentData<Projectile>(entities[i]).ID,
                    OwnerID = EntityManager.GetComponentData<OwnerID>(entities[i]).Value
                };

                // if we have any hits
                hitCount = Physics.RaycastNonAlloc(ray, Hits, rayCommands[i].distance, rayCommands[i].layerMask);
                if (hitCount > 0)
                {
                    NativeArray<RaycastHit> tmpHits = new NativeArray<RaycastHit>(hitCount, Allocator.TempJob);

                    // for every hit
                    for(int j = 0; j < hitCount; j++)
                    {
                        tmpHits[j] = Hits[j];
                    }

                    // call our individual multihit handler
                    onMultiHitSystemFinish?.Invoke(handlerData, tmpHits);

                    tmpHits.Dispose();
                }
            }
            #endregion

            entities.Dispose();
            rayCommands.Dispose();

            return inputDeps;
        }

        protected override void OnCreate()
        {
            Parameters = WeaponParameters.Instance;
            HitMask = Parameters.GetProjectileHitLayer();

            MultiHitQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                ComponentType.ReadOnly<PreviousTranslation>(), ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(), ComponentType.ReadOnly<Damage>(), ComponentType.ReadOnly<OwnerID>(),
                ComponentType.ReadWrite<MultiHit>()
                }
            });

            Hits = new RaycastHit[40];
        }
    }
}