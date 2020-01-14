using Unity.Entities;
using WDIB.Utilities;

namespace WDIB.Weapons
{
    // TODO: Implement timed reload
    // TODO: Implement ammo reduction on reload
    // TODO: Implement Battery Cooldown
    [UpdateInGroup(typeof(SupplementalSystemGroup))]
    public class ReloadSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<ReloadTag, Weapon>().ForEach((Entity entity, ref AmmoComponent ammo) =>
            {
                ammo.value = ammo.maxValue;

                World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<ReloadTag>(entity);
            });
        }
    }
}