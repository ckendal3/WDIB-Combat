using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace WDIB.Weapons
{
    public class TimeBetweenShotsSystem : JobComponentSystem
    {
        public struct DecreaseTimeJob : IJobForEach<Weapon, TimeBetweenShots>
        {
            public float deltaTime;

            public void Execute([ReadOnly]ref Weapon weapon, ref TimeBetweenShots timeBetween)
            {

                // stop reducing at -1 just to avoid a possible overflow
                if (timeBetween.Value < -1)
                {
                    return;
                }

                timeBetween.Value -= deltaTime;
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
}