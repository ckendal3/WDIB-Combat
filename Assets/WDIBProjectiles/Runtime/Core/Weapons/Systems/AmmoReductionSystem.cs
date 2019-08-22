using Unity.Entities;

public class AmmoReductionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<ReduceAmmoTag, Weapon>().ForEach((Entity entity, ref AmmoComponent ammo) =>
        {
            ammo.value -= 1;
            World.Active.EntityManager.RemoveComponent<ReduceAmmoTag>(entity);
        });
    }
}
