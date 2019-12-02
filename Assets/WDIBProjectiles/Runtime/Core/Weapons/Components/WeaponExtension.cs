using Unity.Entities;

namespace WDIB.Weapons
{
    public static class WeaponExtension
    {
        public static EntityArchetype GetBaseAmmoWeaponArchetype()
        {
            return World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
                typeof(Weapon), typeof(AmmoComponent),
                typeof(MeleeComponent), typeof(TimeBetweenShots));
        }

        public static EntityArchetype GetBaseBatteryWeaponArchetype()
        {
            return World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
                typeof(Weapon), typeof(BatteryComponent),
                typeof(MeleeComponent), typeof(TimeBetweenShots));
        }


        public static EntityArchetype GetBaseMeleeWeaponArchetype()
        {
            return World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(typeof(Weapon), typeof(MeleeComponent), typeof(TimeBetweenShots));
        }
    }
}