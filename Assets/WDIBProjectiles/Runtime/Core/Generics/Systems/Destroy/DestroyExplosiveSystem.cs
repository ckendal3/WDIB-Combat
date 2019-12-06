using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using WDIB.Explosives;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(DestroySystemGroup))]
    public class DestroyExplosiveSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem buffer;

        // TODO: Implement destruction system in here
        [BurstCompile]
        public struct ExplodedJob : IJobForEachWithEntity<Explosive>
        {
            [WriteOnly] public EntityCommandBuffer.Concurrent Buffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref Explosive explosive)
            {
                //Buffer.DestroyEntity(index, entity);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var destroyJob = new ExplodedJob
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