using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using WDIB.Components;

namespace WDIB.Weapons
{
    public class ShootFromSystem : JobComponentSystem
    {
        [BurstCompile]
        public struct UpdateShootFrom : IJobForEach<ShootFromOffset, OwnerID, Translation, Rotation>
        {
            public void Execute(ref ShootFromOffset shootFrom, [ReadOnly] ref OwnerID owner, [ReadOnly] ref Translation position, [ReadOnly] ref Rotation rotation)
            {
                //shootFrom.value = new float3(position.Value + (shootFrom.offset * math.forward(rotation.Value)));

                shootFrom.Value = position.Value + (shootFrom.Offset * (math.forward(rotation.Value) * shootFrom.Heading)); // math.normalize(shootFrom.value) + math.forward(rotation.Value)
            }
        }

        // TODO: Implement shoot from camera
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
}