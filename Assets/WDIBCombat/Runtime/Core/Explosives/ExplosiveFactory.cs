using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Utilities;

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
            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

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

            CreateVisualComponents(spawnPos, Parameters.GetExplosiveDataByID(explosiveID)); // temporary, see method

            SetComponents(explosiveID, spawnPos, Parameters.GetExplosiveDataByID(explosiveID), ownerID);
        }

        private static void CreateVisualComponents(Vector3 spawnPos, ExplosiveData data)
        {
            // TODO: Write Generic Pooling for VFX and eventually pure ECS when more stable
            #if UNITY_EDITOR
            if(data.particleEffect == null)
            {
                Debug.LogWarning($"{data.explosiveName}'s particle effect does not have a particle effect.", data);
                return;
            }
            #endif
               
            GameObject tmpGO = GameObject.Instantiate(data.particleEffect, spawnPos, Quaternion.identity);
        }

        private static void SetComponents(int explosiveID, float3 spawnPos, ExplosiveData data, uint ownerID)
        {
            Entity entity;

            entity = EntityManager.CreateEntity(Archetype);

            #if UNITY_EDITOR
            if (debugGroupID > 65000)
            {
                debugGroupID = 0;
            }
            EntityManager.SetName(entity, data.explosiveName + " Explosive " + debugGroupID);
            #endif

            //Generic Components
            EntityManager.SetComponentData(entity, new Translation { Value = spawnPos });
            EntityManager.SetComponentData(entity, new OwnerID { Value = ownerID });
            EntityManager.SetComponentData(entity, new Explosive { ID = explosiveID });
        }
    }
}