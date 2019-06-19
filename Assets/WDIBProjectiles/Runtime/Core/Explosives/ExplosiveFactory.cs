using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Parameters;

namespace WDIB.Factory
{
    public static class ExplosiveFactory
    {
        static EntityManager eManager;
        static EntityArchetype explosiveArch;
        static WeaponParameters weaponParameters;

        // use for debugging
        #if UNITY_EDITOR
        static int debugGroupID = 0;
        #endif

        static ExplosiveFactory()
        {
            eManager = World.Active.EntityManager;

            // create our archetype
            explosiveArch = eManager.CreateArchetype(
                                        ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Explosive>(),
                                        ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadWrite<Owner>()
                                        );

            weaponParameters = WeaponParameters.Instance;
        }

        public static void CreateExplosive(int explosiveID, float3 spawnPos, uint ownerID)
        {
            #if UNITY_EDITOR
            debugGroupID += 1;
            #endif

            // -------------------
            // create visuals here
            // Make new method ---
            // -------------------
            CreateVisualComponents(spawnPos, weaponParameters.GetExplosiveDataByID(explosiveID)); // temporary, see method

            SetComponents(explosiveID, spawnPos, weaponParameters.GetExplosiveDataByID(explosiveID), ownerID);
        }

        private static void CreateVisualComponents(Vector3 spawnPos, ExplosiveData data)
        {
            // TODO: Write Generic Pooling for VFX and eventually pure ECS when more stable (multi-threaded)
            // ---------------------------------------------------------------------------------------------
            GameObject tmpGO = GameObject.Instantiate(data.particleEffect, spawnPos, Quaternion.identity);
            tmpGO.AddComponent<DestroyOnComplete>();
            // ---------------------------------------------------------------------------------------------
        }

        private static void SetComponents(int explosiveID, float3 spawnPos, ExplosiveData data, uint ownerID)
        {
            Entity templateEnt;

            // create the entity to clone from
            templateEnt = eManager.CreateEntity(explosiveArch);

            #if UNITY_EDITOR
            if (debugGroupID > 65000)
            {
                debugGroupID = 0;
            }
            eManager.SetName(templateEnt, data.explosiveName + " Explosive " + debugGroupID);
            #endif

            //Generic Components
            eManager.SetComponentData(templateEnt, new Translation { Value = spawnPos });
            eManager.SetComponentData(templateEnt, new Owner { ID = ownerID });
            eManager.SetComponentData(templateEnt, new Explosive { ID = (uint)explosiveID });
        }
    }
}