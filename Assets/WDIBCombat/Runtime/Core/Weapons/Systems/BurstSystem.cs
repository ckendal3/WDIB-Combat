using Unity.Entities;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Projectiles;
using WDIB.Utilities;

namespace WDIB.Weapons
{
    [UpdateInGroup(typeof(SupplementalSystemGroup))]
    public class BurstSystem : ComponentSystem
    {
        private static WeaponParameters Parameters;

        protected override void OnUpdate()
        {
            Entities.WithAll<EquippedTag>().ForEach((Entity entity, ref Weapon weapon, ref BurstFire burst, ref WeaponState state, ref ShootFrom shootFrom, ref OwnerID owner) =>
            {
            // if we are TRYING to and CAN burst
            if (state.IsShooting && burst.Value < 0)
                {
                    ProjectileFactory.CreateProjectiles(Parameters.GetWeaponDataByID(weapon.ID).projectileId, shootFrom.Position, shootFrom.Rotation, owner.Value);

                // Restart timer and increase our burst count
                burst.Value = burst.ResetValue;
                    burst.Count += 1;

                // If we are done bursting now, stop bursting
                if (burst.Count >= burst.MaxCount)
                    {
                        state.IsShooting = false;
                    }

                // TODO: Setup actioning on it
            }

            });
        }

        protected override void OnCreate()
        {
            if (Parameters == null)
            {
                Parameters = WeaponParameters.Instance;
            }
        }
    }
}