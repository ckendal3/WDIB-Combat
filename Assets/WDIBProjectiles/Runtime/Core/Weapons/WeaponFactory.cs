using WDIB.Parameters;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using WDIB.Components;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Unity.Burst;
using Unity.Jobs;

namespace WDIB.Factory
{
    public static class WeaponFactory
    {
        static EntityManager eManager;
        static EntityArchetype weaponArch;

        static WeaponParameters wParameters;

        // use for debugging
#if UNITY_EDITOR
        static int debugGroupID = 0;
        static int debugIndividualID = 0;
#endif

        static WeaponFactory()
        {
            eManager = World.Active.EntityManager;

            // create our archetype
            weaponArch = eManager.CreateArchetype(                                                               
                                        ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Rotation>(),
                                        ComponentType.ReadWrite<Weapon>(), ComponentType.ReadWrite<Owner>(),
                                        ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadWrite<TimeBetweenShots>(),
                                        ComponentType.ReadWrite<WeaponState>(), ComponentType.ReadWrite<ShootFromOffset>()
                                        );

            wParameters = WeaponParameters.Instance;
        }

        /// <summary>
        /// Spawn a weapon based on the approprate weapon ID
        /// </summary>
        /// <param name="projectileID"></param>
        /// <param name="spawnAmount"></param>
        public static Entity CreateWeapon(int weaponID, float3 spawnPos, quaternion spawnRot, uint ownerID, float3 shootFromOffset)
        {
            // -------------------
            // create visuals here
            // Make new method ---
            // -------------------


#if UNITY_EDITOR
            if (debugGroupID > 65000)
            {
                debugGroupID = 0;
            }
            debugGroupID += 1;
            debugIndividualID = 0;
#endif

            Entity weaponEntity;

            // create the entity to clone from
            weaponEntity = eManager.CreateEntity(weaponArch);

            SetComponents(ref weaponEntity, weaponID, spawnPos, spawnRot, wParameters.GetWeaponDataByID(weaponID), ownerID, shootFromOffset);

            return weaponEntity;
        }

        /// <summary>
        /// Set the components for the projectile based on the inputted projectile data
        /// </summary>
        /// <param name="spawnTransform"></param>
        /// <param name="data"></param>
        private static void SetComponents(ref Entity entity, int weaponID, float3 spawnPos, quaternion spawnRot, WeaponData data, uint ownerID, float3 offsetPos)
        {
            #region Template Weapon Entity
            


#if UNITY_EDITOR
            debugIndividualID += 1;
            eManager.SetName(entity, data.weaponName + " Weapon " + debugIndividualID + " - Group " + debugGroupID);
#endif

            //Generic Components
            eManager.SetComponentData(entity, new Translation { Value = spawnPos });
            eManager.SetComponentData(entity, new Rotation { Value = spawnRot });

            // specific components
            eManager.SetComponentData(entity, new Owner { ID = ownerID });
            eManager.SetComponentData(entity, new Weapon { ID = weaponID });
            eManager.SetComponentData(entity, new TimeBetweenShots { value = 0, resetValue = data.timeBetweenShots });
            eManager.SetComponentData(entity, new WeaponState { isReloading = false, isShooting = false });
            eManager.SetComponentData(entity, new ShootFromOffset { Value = 0, Offset = offsetPos, Heading = math.normalize(spawnPos - offsetPos) });
            #endregion
        }
    }
}

public enum ShootFromType
{
    barrel,
    player
}