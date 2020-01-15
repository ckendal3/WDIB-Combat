using WDIB.Parameters;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using WDIB.Components;

namespace WDIB.Weapons
{
    // TODO: ALLOW SHOOTFROMCAMERA and SHOOTFROMMUZZLE
    // TODO: Add Dynamic component adding
    // TODO: Add Single, Burst, Automatic for gun firing modes
    public static class WeaponFactory
    {
        static EntityManager EntityManager;
        static EntityArchetype Archetype;

        static WeaponParameters Parameters;

        // use for debugging
        #if UNITY_EDITOR
        static int debugGroupID = 0;
        static int debugIndividualID = 0;
        #endif

        static WeaponFactory()
        {
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // create our archetype
            Archetype = EntityManager.CreateArchetype(                                                               
                                        ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Rotation>(),
                                        ComponentType.ReadWrite<Weapon>(), ComponentType.ReadWrite<OwnerID>(),
                                        ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadWrite<TimeBetweenShots>(),
                                        ComponentType.ReadWrite<WeaponState>(), ComponentType.ReadWrite<ShootFrom>(),
                                        ComponentType.ReadWrite<Muzzle>()
                                        );

            Parameters = WeaponParameters.Instance;
        }

        /// <summary>
        /// Spawn a weapon based on the approprate weapon ID
        /// </summary>
        /// <param name="projectileID"></param>
        /// <param name="spawnAmount"></param>
        public static Entity CreateWeapon(int weaponID, float3 spawnPos, quaternion spawnRot, uint ownerID, float muzzleOffset)
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
            weaponEntity = EntityManager.CreateEntity(Archetype);

            SetComponents(weaponEntity, weaponID, spawnPos, spawnRot, Parameters.GetWeaponDataByID(weaponID), ownerID, muzzleOffset);

            return weaponEntity;
        }

        /// <summary>
        /// Set the components for the projectile based on the inputted projectile data
        /// </summary>
        /// <param name="spawnTransform"></param>
        /// <param name="data"></param>
        private static void SetComponents(Entity entity, int weaponID, float3 spawnPos, quaternion spawnRot, WeaponData data, uint ownerID, float muzzleOffset)
        {
            #region Template Weapon Entity
            #if UNITY_EDITOR
            debugIndividualID += 1;
            EntityManager.SetName(entity, data.weaponName + " Weapon " + debugIndividualID + " - Group " + debugGroupID);
            #endif

            //Generic Components
            EntityManager.SetComponentData(entity, new Translation { Value = spawnPos });
            EntityManager.SetComponentData(entity, new Rotation { Value = spawnRot });

            // specific components
            EntityManager.SetComponentData(entity, new OwnerID { Value = ownerID });
            EntityManager.SetComponentData(entity, new Weapon { ID = weaponID });
            EntityManager.SetComponentData(entity, new TimeBetweenShots { Value = 0, ResetValue = data.timeBetweenShots });
            EntityManager.SetComponentData(entity, new WeaponState { IsReloading = false, IsShooting = false });
            EntityManager.SetComponentData(entity, new Muzzle //TODO: Fix this offset stuff
            {
                Position = spawnPos + muzzleOffset * math.forward(spawnRot),
                Rotation = spawnRot,
                Offset = muzzleOffset
            });

            EntityManager.SetComponentData(entity, new ShootFrom { Position = spawnPos + muzzleOffset * math.forward(spawnRot), Rotation = spawnRot });
            #endregion
        }
    }
}