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
    // TODO: Make SetupCommands for both Hit and MultiHit system (move to own class)
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
            int count = DetectHitQuery.CalculateEntityCount();
            if (count == 0)
            {
                return inputDeps;
            }

            var resultsArray = new NativeArray<RaycastHit>(count, Allocator.TempJob);

            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            var setupCommandsJob = new SetupCommandsJob
            {
                prevPositions = DetectHitQuery.ToComponentDataArray<PreviousTranslation>(Allocator.TempJob),
                curPositions = DetectHitQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                rotations = DetectHitQuery.ToComponentDataArray<Rotation>(Allocator.TempJob),
                cmds = commands,
                hitMask = Parameters.GetProjectileHitLayer()
            }.Schedule(count, 1, inputDeps);

            RaycastCommand.ScheduleBatch(commands, resultsArray, 32, setupCommandsJob).Complete();

            var entities = DetectHitQuery.ToEntityArray(Allocator.TempJob);
            var ownerIDs = DetectHitQuery.ToComponentDataArray<OwnerID>(Allocator.TempJob);
            var projectiles = DetectHitQuery.ToComponentDataArray<Projectile>(Allocator.TempJob);

            HitHandlerData handlerData;
            for (int i = 0; i < resultsArray.Length; i++)
            {
                if (resultsArray[i].collider != null)
                {
                    handlerData = new HitHandlerData
                    {
                        Entity = entities[i],
                        ProjectileID = projectiles[i].ID,
                        OwnerID = ownerIDs[i].Value
                    };

                    onHitSystemFinish?.Invoke(handlerData, resultsArray[i]);
                }
            }

            entities.Dispose();
            ownerIDs.Dispose();
            projectiles.Dispose();

            commands.Dispose();
            resultsArray.Dispose();

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
                ComponentType.ReadOnly<Projectile>(), ComponentType.ReadOnly<OwnerID>()
            },
                None = new ComponentType[] { ComponentType.ReadOnly<MultiHit>() }
            });
        }
    }
}