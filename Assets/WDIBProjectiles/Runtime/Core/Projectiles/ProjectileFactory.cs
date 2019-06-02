using WDIB.Parameters;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using WDIB.Components;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace WDIB.Factory
{
    public static class ProjectileFactory
    {
        static EntityManager eManager;
        static EntityArchetype projectileArch;

        static WeaponParameters wParameters;

        private static Random random;

        // use for debugging
        #if UNITY_EDITOR
        static int debugGroupID = 0;
        static int debugIndividualID = 0;
        #endif

        static ProjectileFactory()
        {
            eManager = World.Active.EntityManager;

            // create our archetype
            projectileArch = eManager.CreateArchetype(
                                        ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Rotation>(), ComponentType.ReadWrite<PreviousTranslation>(),
                                        ComponentType.ReadWrite<Speed>(), ComponentType.ReadWrite<Damage>(), ComponentType.ReadWrite<ProjectileID>(),
                                        ComponentType.ReadWrite<Owner>(), ComponentType.ReadWrite<Distance>(),
                                        ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadWrite<RenderMesh>()
                                        );

            wParameters = WeaponParameters.Instance;
        }

        /// <summary>
        /// Spawn a certain amount of projectiles based on the approprate projectile ID
        /// </summary>
        /// <param name="projectileID"></param>
        /// <param name="spawnAmount"></param>
        public static void CreateProjectiles(int projectileID, float3 spawnPos, quaternion spawnRot, uint ownerID)
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

            SetComponents(projectileID, spawnPos, spawnRot, wParameters.GetProjectileDataByID(projectileID), ownerID);
        }

        /// <summary>
        /// Set the components for the projectile based on the inputted projectile data
        /// </summary>
        /// <param name="spawnTransform"></param>
        /// <param name="data"></param>
        private static void SetComponents(int projectileID, float3 spawnPos, quaternion spawnRot, ProjectileData data, uint ownerID)
        {
            int projectileCount = data.projectilesPerAShot;

            #region Template Projectile Entity
            Entity templateEnt;

            // create the entity to clone from
            templateEnt = eManager.CreateEntity(projectileArch);


            #if UNITY_EDITOR
            debugIndividualID += 1;
            eManager.SetName(templateEnt, data.projectileName + " Projectile " + debugIndividualID + " - Group " + debugGroupID);
            #endif

            //Generic Components
            eManager.SetComponentData(templateEnt, new Translation { Value = spawnPos });
            eManager.SetComponentData(templateEnt, new Rotation { Value = spawnRot });
            eManager.SetSharedComponentData(templateEnt, new RenderMesh { mesh = data.mesh, material = data.material });

            // specific components
            eManager.SetComponentData(templateEnt, new PreviousTranslation { Value = spawnPos });
            eManager.SetComponentData(templateEnt, new Owner { ID = ownerID });
            eManager.SetComponentData(templateEnt, new Distance { Value = 0, MaxDistance = data.maxDistance });
            eManager.SetComponentData(templateEnt, new Damage { Value = data.damage });
            eManager.SetComponentData(templateEnt, new Speed { Value = data.speed });
            eManager.SetComponentData(templateEnt, new ProjectileID { ID = (uint)projectileID });

            // Add extra components specified in the projectile data
            if (data.componentData != null && data.componentData.Length > 0)
            {
                int componentCount = data.componentData.Length;
                for (int i = 0; i < componentCount; i++)
                {
                    AddExtraComponentData(ref templateEnt, data.componentData[i]);
                }
            }
            #endregion

            // if we have more than one projectile
            if (projectileCount > 1)
            {
                // taking into account the template entity - we subtract one
                NativeArray<Entity> cloneEnts = new NativeArray<Entity>(projectileCount - 1, Allocator.TempJob);

                // creating and destroying entities is batched
                // we are cloning the entity so all components have the same value
                eManager.Instantiate(templateEnt, cloneEnts);

                // get random spreads
                GetConeOfFire(data, spawnRot, projectileCount, out NativeArray<quaternion> rSpreads);

                // We need to add random spread to the first projectile
                eManager.SetComponentData(templateEnt, new Rotation { Value = rSpreads[0] });

                // we start at one because set our templateEnt to the first spread
                for (int i = 1; i < projectileCount; i++)
                {
                    #region Entity Debugging
#if UNITY_EDITOR
                    debugIndividualID += 1;
                    eManager.SetName(cloneEnts[i - 1], data.projectileName + " Projectile " + debugIndividualID + " - Group " + debugGroupID);
#endif
                    #endregion

                    // we subtract one because our cloned projectiles need to iterate starting at zero
                    eManager.SetComponentData(cloneEnts[i - 1], new Rotation { Value = rSpreads[i] });
                }

                //we have set all our data, dispose our native container to avoid memory leak
                cloneEnts.Dispose();
                rSpreads.Dispose();
            }
        }

        private static void GetConeOfFire(ProjectileData data, quaternion rotation, int count, out NativeArray<quaternion> rSpreads)
        {
            rSpreads = new NativeArray<quaternion>(count, Allocator.TempJob);

            random = new Unity.Mathematics.Random(34377 * (uint)System.Environment.TickCount);

            float3 deviation;

            for (int i = 0; i < count; i++)
            {
                deviation = new float3(random.NextFloat(data.spread.minimumSpread, data.spread.maximumSpread),
                               random.NextFloat(data.spread.minimumSpread, data.spread.maximumSpread),
                               random.NextFloat(data.spread.minimumSpread, data.spread.maximumSpread));

                rSpreads[i] = math.mul(rotation, quaternion.Euler(deviation));
            }
        }

        /// <summary>
        /// Retrieve the components specified in the data and add them to the projectiles
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        private static void AddExtraComponentData(ref Entity entity, ComponentDataStruct data)
        {
            switch (data.componentType)
            {
                case EComponentType.HeadShot:
                    eManager.AddComponentData(entity, (HeadShotMultiplier)data.GetComponentData());
                    return;
                case EComponentType.MultiHit:
                    eManager.AddComponentData(entity, (MultiHit)data.GetComponentData());
                    return;
                case EComponentType.EMP:
                    Debug.LogWarning("EMP is not yet implemented.");
                    return;
                case EComponentType.SuperCombine:
                    eManager.AddComponentData(entity, (SuperCombine)data.GetComponentData());
                    return;
                case EComponentType.Tracking:
                    eManager.AddComponentData(entity, (TrackPlayer)data.GetComponentData());
                    return;
                case EComponentType.Explosive:
                    eManager.AddComponentData(entity, (Explosive)data.GetComponentData());
                    return;
                case EComponentType.NotImplemented:
                    Debug.Log("Not yet immplemented/");
                    return;
            }
        }
    }
}