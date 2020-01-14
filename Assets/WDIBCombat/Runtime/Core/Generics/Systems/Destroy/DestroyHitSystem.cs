using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using WDIB.Projectiles;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(DestroySystemGroup))]
    public class DestroyHitSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem buffer;

        [BurstCompile]
        public struct ExceededHitJob : IJobForEachWithEntity<MultiHit>
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref MultiHit hitComponent)
            {
                if (hitComponent.Hits >= hitComponent.MaxHits)
                {
                    Buffer.DestroyEntity(index, entity);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJob = new ExceededHitJob
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