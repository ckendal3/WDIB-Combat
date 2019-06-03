using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(HitSystemGroup))]
    public class MultiHitSystem : JobComponentSystem
    {
        private EntityManager eManager;
        private EntityQuery m_MultiHitGroup;
        private RaycastHit[] hits;

        private WeaponParameters wParameters;
        private LayerMask hitMask;

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
                    maxHits = math.clamp(multiHits[index].maxHits - multiHits[index].hits, 0, multiHits[index].maxHits)
                };
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // if there are no entities - return
            int count = m_MultiHitGroup.CalculateLength();
            if (count == 0)
            {
                return inputDeps;
            }

            // Get the matching entities from the query
            var entities = m_MultiHitGroup.ToEntityArray(Allocator.TempJob, out JobHandle entArrayHandle);
            entArrayHandle.Complete();

            // Setup Ray Commands
            NativeArray<RaycastCommand> rayCommands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            JobHandle setupJob = new SetupCommandsJob
            {
                // these get the data from the m_HitGroup query
                prevPositions = m_MultiHitGroup.ToComponentDataArray<PreviousTranslation>(Allocator.TempJob),
                positions = m_MultiHitGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                rotations = m_MultiHitGroup.ToComponentDataArray<Rotation>(Allocator.TempJob),
                commands = rayCommands,
                hitLayer = hitMask,
                multiHits = m_MultiHitGroup.ToComponentDataArray<MultiHit>(Allocator.TempJob)
            }.Schedule(count, 32, inputDeps);

            setupJob.Complete();

            #region Default raycast
            Ray ray;
            int hitCount;
            uint projectileID;
            HitHandlerData handlerData;
            // for every projectile
            for (int i = 0; i < count; i++)
            {
                ray = new Ray { origin = rayCommands[i].from, direction = rayCommands[i].direction };
                projectileID = eManager.GetComponentData<ProjectileID>(entities[i]).ID;

                handlerData = new HitHandlerData
                {
                    entity = entities[i],
                    projectileID = eManager.GetComponentData<ProjectileID>(entities[i]).ID,
                    ownerID = eManager.GetComponentData<Owner>(entities[i]).ID
                };

                // if we have any hits
                hitCount = Physics.RaycastNonAlloc(ray, hits, rayCommands[i].distance, rayCommands[i].layerMask);
                if (hitCount > 0)
                {
                    NativeArray<RaycastHit> tmpHits = new NativeArray<RaycastHit>(hitCount, Allocator.TempJob);

                    // for every hit
                    for(int j = 0; j < hitCount; j++)
                    {
                        tmpHits[j] = hits[j];
                    }

                    // call our individual multihit handler
                    onMultiHitSystemFinish?.Invoke(handlerData, tmpHits);

                    tmpHits.Dispose();
                }
            }
            #endregion

            entities.Dispose();
            rayCommands.Dispose();

            return JobHandle.CombineDependencies(inputDeps, setupJob);
        }

        protected override void OnCreate()
        {
            eManager = World.Active.EntityManager;

            wParameters = WeaponParameters.Instance;
            hitMask = wParameters.GetProjectileHitLayer();

            m_MultiHitGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                ComponentType.ReadOnly<PreviousTranslation>(), ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(), ComponentType.ReadOnly<Damage>(), ComponentType.ReadOnly<Owner>(),
                ComponentType.ReadWrite<MultiHit>()
                }
            });

            hits = new RaycastHit[40];
        }
    }
}