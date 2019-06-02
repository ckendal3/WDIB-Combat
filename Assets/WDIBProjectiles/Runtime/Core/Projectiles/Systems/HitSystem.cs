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

        public delegate void HitSystemEvent(ECSHitData[] hitData);
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
                    layerMask = hitMask
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

            // Used for the delegate
            NativeList<ECSHitData> tempHitData = new NativeList<ECSHitData>(0, Allocator.TempJob);

            #region Default Physics
            // Try using batch raycasting
            UnityEngine.Ray ray;
            UnityEngine.RaycastHit hit;

            // for every hit
            ECSHitData tmpData;
            uint projectileID;
            for (int i = 0; i < commands.Length; i++)
            {
                ray = new UnityEngine.Ray(commands[i].from, commands[i].direction);

                if (Physics.Raycast(ray, out hit, commands[i].distance, commands[i].layerMask))
                {
                    // don't add null hits
                    if (hit.collider == null)
                    {
                        continue;
                    }

                    projectileID = eManager.GetComponentData<ProjectileID>(entities[i]).ID;

                    tmpData = new ECSHitData
                    {
                        hit = hit,
                        entity = entities[i],
                        projectileID = projectileID
                    };

                    tempHitData.Add(tmpData);
                }
            }
            #endregion

            // if we have hits - hit delegate
            if (tempHitData.Length > 0)
            {
                onHitSystemFinish?.Invoke(tempHitData.ToArray());
            }

            tempHitData.Dispose();
            entities.Dispose();
            commands.Dispose();

            return inputDeps;
        }

        protected override void OnCreate()
        {
            eManager = World.Active.EntityManager;

            wParameters = WeaponParameters.Instance;

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