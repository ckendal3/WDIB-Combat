using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;

public class ShootFromSystem : JobComponentSystem
{
    [BurstCompile]
    public struct UpdateShootFrom: IJobForEach<ShootFromOffset, Owner, Translation, Rotation>
    {
        public void Execute(ref ShootFromOffset shootFrom, [ReadOnly] ref Owner owner, [ReadOnly] ref Translation position, [ReadOnly] ref Rotation rotation)
        {
            //shootFrom.value = new float3(position.Value + (shootFrom.offset * math.forward(rotation.Value)));

            shootFrom.Value = position.Value + (shootFrom.Offset * (math.forward(rotation.Value) * shootFrom.Heading)); // math.normalize(shootFrom.value) + math.forward(rotation.Value)
        }
    }

    // TODO: Implement this in i02
    //[BurstCompile]
    //public struct UpdateShootFromCamera : IJobForEach<ShootFromCamera, Owner>
    //{
    //    NativeArray<PlayerState> states;

    //    public void Execute([WriteOnly] ref ShootFromCamera shootFrom, [ReadOnly] ref Owner owner)
    //    {
    //        for (int i = 0; i < states.Length; i++)
    //        {
    //            if(states[i].ID == owner.ID)
    //            {
    //                shootFrom.Position = states[i].Position;
    //                shootFrom.Rotation = states[i].Rotation;
    //                return;
    //            }
    //        }
    //    }
    //}


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new UpdateShootFrom() { }.Schedule(this, inputDeps);
    }
}
