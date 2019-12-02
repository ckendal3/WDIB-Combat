using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(DestroySystemGroup))]
    public class DestroyDistanceSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem buffer;
        private float MAXDISTANCE;

        [BurstCompile]
        public struct ExceededDistanceJob : IJobForEachWithEntity<Distance>
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;
            [ReadOnly] public float MaxDistance;

            public void Execute(Entity entity, int index, [ReadOnly] ref Distance distance)
            {
                if (distance.Value >= distance.MaxDistance || distance.Value >= MaxDistance) // cache distance and compare squares
                {
                    Buffer.DestroyEntity(index, entity);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJob = new ExceededDistanceJob
            {
                Buffer = buffer.CreateCommandBuffer().ToConcurrent(),
                MaxDistance = MAXDISTANCE
            }.Schedule(this, inputDeps);

            buffer.AddJobHandleForProducer(destroyJob);

            return destroyJob;
        }

        protected override void OnCreate()
        {
            buffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            MAXDISTANCE = WeaponParameters.Instance.GetProjectileMaximumDistance();
        }

    }
}