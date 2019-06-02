using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using WDIB.Components;
using WDIB.Utilities;

namespace WDIB.Systems
{
    [UpdateInGroup(typeof(LifeSystemGroup))]
    public class LifeTimeSystem : JobComponentSystem
    {
        [BurstCompile]
        public struct LifeTimeJob : IJobForEach<LifeTime>
        {
            [ReadOnly] public float deltaTime;

            public void Execute(ref LifeTime life)
            {
                life.Value -= deltaTime;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var lifeJob = new LifeTimeJob
            {
                deltaTime = Time.deltaTime
            }.Schedule(this, inputDeps);

            return lifeJob;
        }
    }
}