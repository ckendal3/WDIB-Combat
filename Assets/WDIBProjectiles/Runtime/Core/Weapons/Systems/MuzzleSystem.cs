using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using WDIB.Utilities;

namespace WDIB.Weapons
{
    [UpdateInGroup(typeof(SupplementalSystemGroup))]
    public class MuzzleSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Weapon>().ForEach((Entity entity, ref Muzzle muzzle, ref Translation position, ref Rotation rotation) =>
            {
                muzzle.Rotation = rotation.Value;
                muzzle.Position = position.Value + muzzle.Offset * math.forward(muzzle.Rotation);
            });
        }
    }
}