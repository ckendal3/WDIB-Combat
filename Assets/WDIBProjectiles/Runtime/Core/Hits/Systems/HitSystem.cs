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
    public class HitSystem : JobComponentSystem
    {
        private EntityQuery DetectHitQuery;

        private WeaponParameters Parameters;
        private LayerMask HitMask;

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
            int count = DetectHitQuery.CalculateEntityCount();
            if (count == 0)
            {
                return inputDeps;
            }

            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            var setupCommandsJob = new SetupCommandsJob
            {
                prevPositions = DetectHitQuery.ToComponentDataArray<PreviousTranslation>(Allocator.TempJob),
                curPositions = DetectHitQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                rotations = DetectHitQuery.ToComponentDataArray<Rotation>(Allocator.TempJob),
                cmds = commands,
                hitMask = Parameters.GetProjectileHitLayer()
            }.Schedule(count, 1, inputDeps);

            setupCommandsJob.Complete();

            NativeArray<Entity> entities = DetectHitQuery.ToEntityArray(Allocator.TempJob, out JobHandle entitiesHandle);
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
                        Entity = entities[i],
                        ProjectileID = EntityManager.GetComponentData<Projectile>(entities[i]).ID,
                        OwnerID = EntityManager.GetComponentData<OwnerID>(entities[i]).Value
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
            Parameters = WeaponParameters.Instance;
            HitMask = Parameters.GetProjectileHitLayer();

            DetectHitQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PreviousTranslation>(), ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(),  ComponentType.ReadWrite<Damage>(), ComponentType.ReadWrite<Distance>(),
                ComponentType.ReadOnly<Projectile>()
            },
                None = new ComponentType[] { ComponentType.ReadOnly<MultiHit>() }
            });
        }
    }
}