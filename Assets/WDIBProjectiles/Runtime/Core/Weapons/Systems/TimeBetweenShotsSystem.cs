using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class TimeBetweenShotsSystem : JobComponentSystem
{
    public struct DecreaseTimeJob : IJobForEach<Weapon, TimeBetweenShots>
    {
        public float deltaTime;

        public void Execute([ReadOnly]ref Weapon weapon, ref TimeBetweenShots tBS)
        {

            // stop reducing at -1 just to avoid a possible overflow
            if(tBS.value < -1)
            {
                return;
            }

            tBS.value -= deltaTime;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new DecreaseTimeJob()
        {
            deltaTime = Time.deltaTime
        }.Schedule(this, inputDeps);
    }

}
