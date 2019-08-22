using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace WDIB.Utilities.Weapons
{
    public static class WeaponExtension
    {
        public static EntityArchetype GetBaseAmmoWeaponArchetype()
        {
            return World.Active.EntityManager.CreateArchetype(
                typeof(Weapon), typeof(Reload), typeof(AmmoComponent),
                typeof(MeleeComponent), typeof(TimeBetweenShot));
        }

        public static EntityArchetype GetBaseBatteryWeaponArchetype()
        {
            return World.Active.EntityManager.CreateArchetype(
                typeof(Weapon), typeof(Reload), typeof(BatteryComponent),
                typeof(MeleeComponent), typeof(TimeBetweenShot));
        }


        public static EntityArchetype GetBaseMeleeWeaponArchetype()
        {
            return World.Active.EntityManager.CreateArchetype(typeof(Weapon), typeof(MeleeComponent), typeof(TimeBetweenShot));
        }
    }
}