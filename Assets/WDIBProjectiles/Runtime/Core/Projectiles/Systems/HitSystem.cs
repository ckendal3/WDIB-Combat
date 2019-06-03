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
    public class HitSystem : JobComponentSystem
    {
        private EntityQuery m_DetectHitGroup, m_hittableGroup;
        private EntityManager eManager;

        private WeaponParameters wParameters;
        private LayerMask hitMask;

        public delegate void HitSystemEvent(HitHandlerData handlerData, RaycastHit hit);
        public static HitSystemEvent onHitSystemFinish;

        [BurstCompile]
        public struct SetupCommandsJob : IJobParallelFor
        {
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<PreviousTranslation> prevPositions;
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Translation> curPositions;
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Rotation> rotations;

            [WriteOnly] public NativeArray<RaycastCommand> cmds;
            [ReadOnly] public LayerMask hitMask;

            public void Execute(int index)
            {
                cmds[index] = new RaycastCommand
                {
                    from = prevPositions[index].Value,
                    direction = math.forward(rotations[index].Value),
                    distance = math.distance(prevPositions[index].Value, curPositions[index].Value),
                    layerMask = hitMask,
                    maxHits = 1
                };
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // make sure we have entities
            int count = m_DetectHitGroup.CalculateLength();
            if (count == 0)
            {
                return inputDeps;
            }

            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            var setupCommandsJob = new SetupCommandsJob
            {
                prevPositions = m_DetectHitGroup.ToComponentDataArray<PreviousTranslation>(Allocator.TempJob),
                curPositions = m_DetectHitGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                rotations = m_DetectHitGroup.ToComponentDataArray<Rotation>(Allocator.TempJob),
                cmds = commands,
                hitMask = wParameters.GetProjectileHitLayer()
            }.Schedule(count, 1, inputDeps);

            setupCommandsJob.Complete();

            NativeArray<Entity> entities = m_DetectHitGroup.ToEntityArray(Allocator.TempJob, out JobHandle entitiesHandle);
            entitiesHandle.Complete();

            #region Default Physics
            // Try using batch raycasting
            UnityEngine.Ray ray;
            UnityEngine.RaycastHit hit;

            // for every hit
            HitHandlerData handlerData;
            for (int i = 0; i < commands.Length; i++)
            {
                ray = new Ray(commands[i].from, commands[i].direction);

                if (Physics.Raycast(ray, out hit, commands[i].distance, commands[i].layerMask))
                {
                    // don't add null hits
                    if (hit.collider == null)
                    {
                        continue;
                    }

                    handlerData = new HitHandlerData
                    {
                        entity = entities[i],
                        projectileID = eManager.GetComponentData<ProjectileID>(entities[i]).ID,
                        ownerID = eManager.GetComponentData<Owner>(entities[i]).ID
                    };

                    onHitSystemFinish?.Invoke(handlerData, hit);
                }
            }

            #endregion
            entities.Dispose();
            commands.Dispose();

            return inputDeps;
        }

        protected override void OnCreate()
        {
            eManager = World.Active.EntityManager;

            wParameters = WeaponParameters.Instance;
            hitMask = wParameters.GetProjectileHitLayer();

            m_DetectHitGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PreviousTranslation>(), ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(),  ComponentType.ReadWrite<Damage>(), ComponentType.ReadWrite<Distance>(),
                ComponentType.ReadOnly<ProjectileID>()
            },
                None = new ComponentType[] { ComponentType.ReadOnly<MultiHit>() }
            });
        }
    }
}