using Unity.Entities;

public class BatteryReductionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<ReduceBatteryTag, Weapon>().ForEach((Entity entity, ref BatteryComponent battery) =>
        {
            battery.value -= 1;
            World.Active.EntityManager.RemoveComponent<ReduceBatteryTag>(entity);
        });
    }
}