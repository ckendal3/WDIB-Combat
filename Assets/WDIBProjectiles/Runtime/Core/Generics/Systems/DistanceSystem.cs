using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using WDIB.Components;

namespace WDIB.Systems
{
    public class DistanceSystem : JobComponentSystem
    {
        [BurstCompile]
        public struct DistanceIncrementerJob : IJobForEach<Distance, PreviousTranslation, Translation>
        {
            public void Execute(ref Distance distance, [ReadOnly] ref PreviousTranslation prevPosition, [ReadOnly] ref Translation curPosition)
            {
                distance.Value += math.distance(prevPosition.Value, curPosition.Value);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle distanceJob = new DistanceIncrementerJob
            { }.Schedule(this, inputDeps);

            return distanceJob;
        }
    }
}