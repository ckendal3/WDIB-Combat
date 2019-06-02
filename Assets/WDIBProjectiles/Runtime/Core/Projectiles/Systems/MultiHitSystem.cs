using System.Collections.Generic;
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

        public delegate void HitMultiSystemEvent(ECSMultiHitData[] hitData);
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

            [WriteOnly] public NativeArray<RaycastCommand> commands;

            [ReadOnly] public LayerMask hitLayer;

            public void Execute(int index)
            {
                commands[index] = new RaycastCommand
                {
                    from = prevPositions[index].Value,
                    direction = math.forward(rotations[index].Value),
                    distance = math.distance(positions[index].Value, prevPositions[index].Value),
                    layerMask = hitLayer
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

            // Get the entities matching our query
            NativeArray<Entity> entities = m_MultiHitGroup.ToEntityArray(Allocator.TempJob, out JobHandle entArrayHandle);
            var multiHits = m_MultiHitGroup.ToComponentDataArray<MultiHit>(Allocator.TempJob, out JobHandle multiHandle);

            // Setup commands
            NativeArray<RaycastCommand> rayCommands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            JobHandle setupJob = new SetupCommandsJob
            {
                // these get the data from the m_HitGroup query
                prevPositions = m_MultiHitGroup.ToComponentDataArray<PreviousTranslation>(Allocator.TempJob),
                positions = m_MultiHitGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                rotations = m_MultiHitGroup.ToComponentDataArray<Rotation>(Allocator.TempJob),
                commands = rayCommands,
                hitLayer = wParameters.GetProjectileHitLayer()

            }.Schedule(count, 32, inputDeps);

            // We need the main thread to wait for all the entities matching our query to be gathered
            entArrayHandle.Complete();
            setupJob.Complete();
            multiHandle.Complete();

            // --------------------------
            // THIS IS GENERATING GARBAGE
            // --------------------------
            List<ECSMultiHitData> tempHitData = new List<ECSMultiHitData>();
            NativeList<RaycastHit> hitsData;

            #region Default raycast
            Ray ray;
            int hitCount;
            uint projectileID;
            // for every projectile
            for (int i = 0; i < count; i++)
            {
                ray = new Ray { origin = rayCommands[i].from, direction = rayCommands[i].direction };
                projectileID = eManager.GetComponentData<ProjectileID>(entities[i]).ID;

                // if we have a hit
                ECSMultiHitData ecsData;
                hitCount = Physics.RaycastNonAlloc(ray, hits, rayCommands[i].distance); //, rayCommands[i].layerMask
                if (hitCount > 0)
                {
                    hitsData = new NativeList<RaycastHit>(Allocator.TempJob);
                    ecsData = new ECSMultiHitData { entity = entities[i], projectileID = projectileID};

                    // For every hit
                    for (int j = 0; j < hits.Length; j++)
                    {
                        if (hits[j].collider != null)
                        {
                            hitsData.Add(hits[j]);
                        }
                    }

                    // if we had valid hits
                    if (hitsData.Length > 0)
                    {
                        ecsData.hits = hitsData.ToArray();
                        tempHitData.Add(ecsData);
                    }
                    hitsData.Dispose();
                }
            }
            #endregion

            // if we had multihit data
            if (tempHitData.Count > 0)
            {
                onMultiHitSystemFinish?.Invoke(tempHitData.ToArray());
            }
            
            entities.Dispose();
            rayCommands.Dispose();
            multiHits.Dispose();

            return setupJob;
        }

        protected override void OnCreate()
        {
            eManager = World.Active.EntityManager;

            wParameters = WeaponParameters.Instance;

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