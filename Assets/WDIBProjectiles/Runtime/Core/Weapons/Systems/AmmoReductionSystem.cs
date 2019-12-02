using Unity.Entities;
using WDIB.Utilities;

namespace WDIB.Weapons
{
    // TODO: Implmement actioning check
    [UpdateInGroup(typeof(SupplementalSystemGroup))]
    public class AmmoReductionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<ReduceAmmoTag, Weapon>().ForEach((Entity entity, ref AmmoComponent ammo) =>
            {
                ammo.value -= 1;
                World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<ReduceAmmoTag>(entity);
            });
        }
    }
}