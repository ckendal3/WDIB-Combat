using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using WDIB.Components;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(LifeSystemGroup))]
    [UpdateAfter(typeof(LifeTimeSystem))]
    public class DestroySystem : JobComponentSystem
    {
        float globalMaxDistance;

        [BurstCompile]
        public struct CollectExceededLifetimeJob : IJobForEachWithEntity<LifeTime>
        {
            [WriteOnly] public NativeQueue<Entity>.Concurrent entQueue;

            public void Execute(Entity entity, int index, [ReadOnly] ref LifeTime life)
            {
                if (life.Value <= 0)
                {
                    entQueue.Enqueue(entity);
                }
            }
        }

        [BurstCompile]
        public struct CollectExceededDistanceJob : IJobForEachWithEntity<Distance>
        {
            [WriteOnly] public NativeQueue<Entity>.Concurrent entQueue;
            public float jobMaxDistance;

            public void Execute(Entity entity, int index, [ReadOnly] ref Distance distance)
            {
                if (distance.Value >= distance.MaxDistance || distance.Value >= jobMaxDistance) // cache distance and compare squares
                {
                    entQueue.Enqueue(entity);
                }
            }
        }

        [BurstCompile]
        public struct CollectExceededHitJob : IJobForEachWithEntity<MultiHit>
        {
            [WriteOnly] public NativeQueue<Entity>.Concurrent entQueue;

            public void Execute(Entity entity, int index, ref MultiHit hitComponent)
            {
                if (hitComponent.hits >= hitComponent.maxHits)
                {
                    entQueue.Enqueue(entity);
                }
            }
        }

        [BurstCompile]
        public struct FillDestroyArrayJob : IJob
        {
            [WriteOnly] public NativeArray<Entity> entities;
            public NativeQueue<Entity> entityQueue;

            public void Execute()
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    entities[i] = entityQueue.Dequeue();
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Get the things we need to destroy
            NativeQueue<Entity> destroyQueue = new NativeQueue<Entity>(Allocator.TempJob);

            var collectLifeTimeJob = new CollectExceededLifetimeJob
            {
                entQueue = destroyQueue.ToConcurrent()
            }.Schedule(this, inputDeps);

            collectLifeTimeJob.Complete();

            var collectDistanceJob = new CollectExceededDistanceJob
            {
                entQueue = destroyQueue.ToConcurrent(),
                jobMaxDistance = globalMaxDistance
            }.Schedule(this, inputDeps);

            collectDistanceJob.Complete();

            var multiHitCheckJob = new CollectExceededHitJob
            {
                entQueue = destroyQueue.ToConcurrent()
            }.Schedule(this, inputDeps);

            multiHitCheckJob.Complete();

            NativeArray<Entity> entitiesToDestroy = new NativeArray<Entity>(destroyQueue.Count, Allocator.TempJob);
            var fillJob = new FillDestroyArrayJob
            {
                entities = entitiesToDestroy,
                entityQueue = destroyQueue
            }.Schedule(inputDeps);

            // Make sure our array fill job is done
            fillJob.Complete();

            // Destroy all entities in the array
            if (entitiesToDestroy.Length > 0)
            {
                EntityManager.DestroyEntity(entitiesToDestroy);
            }

            // Dispose of the NativeContainers
            destroyQueue.Dispose();
            entitiesToDestroy.Dispose();

            return JobHandle.CombineDependencies(fillJob, collectLifeTimeJob, collectDistanceJob);
        }

        protected override void OnCreate()
        {
            globalMaxDistance = 10000f;
        }
    }
}