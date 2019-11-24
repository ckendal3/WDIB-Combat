﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Parameters;

namespace WDIB.Explosives
{
    public static class ExplosiveFactory
    {
        static EntityManager EntityManager;
        static EntityArchetype Archetype;
        static WeaponParameters Parameters;

        // use for debugging
        #if UNITY_EDITOR
        static int debugGroupID = 0;
        #endif

        static ExplosiveFactory()
        {
            EntityManager = World.Active.EntityManager;

            // create our archetype
            Archetype = EntityManager.CreateArchetype(
                                        ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Explosive>(),
                                        ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadWrite<OwnerID>()
                                        );

            Parameters = WeaponParameters.Instance;
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
            CreateVisualComponents(spawnPos, Parameters.GetExplosiveDataByID(explosiveID)); // temporary, see method

            SetComponents(explosiveID, spawnPos, Parameters.GetExplosiveDataByID(explosiveID), ownerID);
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
            templateEnt = EntityManager.CreateEntity(Archetype);

            #if UNITY_EDITOR
            if (debugGroupID > 65000)
            {
                debugGroupID = 0;
            }
            EntityManager.SetName(templateEnt, data.explosiveName + " Explosive " + debugGroupID);
            #endif

            //Generic Components
            EntityManager.SetComponentData(templateEnt, new Translation { Value = spawnPos });
            EntityManager.SetComponentData(templateEnt, new OwnerID { Value = ownerID });
            EntityManager.SetComponentData(templateEnt, new Explosive { ID = explosiveID });
        }
    }
}