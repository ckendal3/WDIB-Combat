using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using WDIB.Components;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(DestroySystemGroup))]
    public class DestroyLifeSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem buffer;

        [BurstCompile]
        public struct ExceededLifeJob : IJobForEachWithEntity<LifeTime>
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref LifeTime life)
            {
                if (life.Value <= 0)
                {
                    Buffer.DestroyEntity(index, entity);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJob = new ExceededLifeJob
            {
                Buffer = buffer.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);

            buffer.AddJobHandleForProducer(destroyJob);

            return destroyJob;
        }

        protected override void OnCreate()
        {
            buffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

    }
}