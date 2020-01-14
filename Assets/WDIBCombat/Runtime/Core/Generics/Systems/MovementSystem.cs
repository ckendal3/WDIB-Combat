using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    public class MovementSystem : JobComponentSystem
    {
        [BurstCompile]
        public struct PreviousPositionUpdateJob : IJobForEach<Translation, PreviousTranslation>
        {
            public void Execute([ReadOnly] ref Translation currentPos, [WriteOnly] ref PreviousTranslation previousPos)
            {
                previousPos = new PreviousTranslation { Value = currentPos.Value };
            }
        }

        [BurstCompile]
        public struct MovementJob : IJobForEach<Translation, Rotation, Speed>
        {
            public float deltaTime;

            public void Execute(ref Translation position, [ReadOnly] ref Rotation rotation, [ReadOnly] ref Speed speed)
            {
                float3 newPos = position.Value + (deltaTime * speed.Value) * math.forward(rotation.Value);

                position.Value = newPos;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var prevPositionJob = new PreviousPositionUpdateJob
            { }.Schedule(this, inputDeps);

            var movementJob = new MovementJob
            {
                deltaTime = Time.DeltaTime
            }.Schedule(this, prevPositionJob);

            return movementJob;
        }
    }
}