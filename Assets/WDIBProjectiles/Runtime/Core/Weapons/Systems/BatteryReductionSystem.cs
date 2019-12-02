using Unity.Entities;
using WDIB.Utilities;

namespace WDIB.Weapons
{
    [UpdateInGroup(typeof(SupplementalSystemGroup))]
    public class BatteryReductionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<ReduceBatteryTag, Weapon>().ForEach((Entity entity, ref BatteryComponent battery) =>
            {
                battery.value -= 1;
                World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<ReduceBatteryTag>(entity);
            });
        }
    }
}