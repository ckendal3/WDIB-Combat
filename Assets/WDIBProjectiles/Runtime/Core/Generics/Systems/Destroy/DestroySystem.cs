using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Projectiles;
using WDIB.Utilities;

//TODO: Remove when no longer needed (after performance tests)
namespace WDIB.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(DestroySystemGroup))]
    public class DestroySystem : JobComponentSystem
    {
        private float maxDistance;

        [BurstCompile]
        public struct CollectExceededLifetimeJob : IJobForEachWithEntity<LifeTime>
        {
            [WriteOnly] public NativeQueue<Entity>.ParallelWriter EntQueue;

            public void Execute(Entity entity, int index, [ReadOnly] ref LifeTime life)
            {
                if (life.Value <= 0)
                {
                    EntQueue.Enqueue(entity);
                }
            }
        }

        [BurstCompile]
        public struct CollectExceededDistanceJob : IJobForEachWithEntity<Distance>
        {
            [WriteOnly] public NativeQueue<Entity>.ParallelWriter EntQueue;
            [ReadOnly] public float MaxDistance;

            public void Execute(Entity entity, int index, [ReadOnly] ref Distance distance)
            {
                if (distance.Value >= distance.MaxDistance || distance.Value >= MaxDistance) // cache distance and compare squares
                {
                    EntQueue.Enqueue(entity);
                }
            }
        }

        [BurstCompile]
        public struct CollectExceededHitJob : IJobForEachWithEntity<MultiHit>
        {
            [WriteOnly] public NativeQueue<Entity>.ParallelWriter EntQueue;

            public void Execute(Entity entity, int index, [ReadOnly] ref MultiHit hitComponent)
            {
                if (hitComponent.Hits >= hitComponent.MaxHits)
                {
                    EntQueue.Enqueue(entity);
                }
            }
        }

        [BurstCompile]
        public struct FillDestroyArrayJob : IJob
        {
            [WriteOnly] public NativeArray<Entity> Entities;

            [DeallocateOnJobCompletion]
            public NativeQueue<Entity> EntQueue;

            public void Execute()
            {
                for (int i = 0; i < Entities.Length; i++)
                {
                    Entities[i] = EntQueue.Dequeue();
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Get the things we need to destroy
            NativeQueue<Entity> destroyQueue = new NativeQueue<Entity>(Allocator.TempJob);

            var collectLifeTimeJob = new CollectExceededLifetimeJob
            {
                EntQueue = destroyQueue.AsParallelWriter()
            }.Schedule(this, inputDeps);

            //collectLifeTimeJob.Complete();

            var collectDistanceJob = new CollectExceededDistanceJob
            {
                EntQueue = destroyQueue.AsParallelWriter(),
                MaxDistance = maxDistance
            }.Schedule(this, inputDeps);

            //collectDistanceJob.Complete();

            var multiHitCheckJob = new CollectExceededHitJob
            {
                EntQueue = destroyQueue.AsParallelWriter()
            }.Schedule(this, inputDeps);

            //multiHitCheckJob.Complete();

            var collectors = JobHandle.CombineDependencies(collectLifeTimeJob, collectDistanceJob, multiHitCheckJob);

            NativeArray<Entity> entitiesToDestroy = new NativeArray<Entity>(destroyQueue.Count, Allocator.TempJob);
            var fillJob = new FillDestroyArrayJob
            {
                Entities = entitiesToDestroy,
                EntQueue = destroyQueue
            }.Schedule(JobHandle.CombineDependencies(collectors, inputDeps));

            // Make sure our array fill job is done
            //fillJob.Complete();

            if (fillJob.IsCompleted)
            {
                // Destroy all entities in the array
                if (entitiesToDestroy.Length > 0)
                {
                    EntityManager.DestroyEntity(entitiesToDestroy);
                }

                // Dispose of the NativeContainers
                //destroyQueue.Dispose();
                entitiesToDestroy.Dispose();

                return inputDeps;
            }

            return fillJob;
        }

        protected override void OnCreate()
        {
            maxDistance = WeaponParameters.Instance.GetProjectileMaximumDistance();
        }
    }
}