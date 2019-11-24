using Unity.Entities;

namespace WDIB.Weapons
{
    public static class WeaponExtension
    {
        public static EntityArchetype GetBaseAmmoWeaponArchetype()
        {
            return World.Active.EntityManager.CreateArchetype(
                typeof(Weapon), typeof(Reload), typeof(AmmoComponent),
                typeof(MeleeComponent), typeof(TimeBetweenShots));
        }

        public static EntityArchetype GetBaseBatteryWeaponArchetype()
        {
            return World.Active.EntityManager.CreateArchetype(
                typeof(Weapon), typeof(Reload), typeof(BatteryComponent),
                typeof(MeleeComponent), typeof(TimeBetweenShots));
        }


        public static EntityArchetype GetBaseMeleeWeaponArchetype()
        {
            return World.Active.EntityManager.CreateArchetype(typeof(Weapon), typeof(MeleeComponent), typeof(TimeBetweenShots));
        }
    }
}