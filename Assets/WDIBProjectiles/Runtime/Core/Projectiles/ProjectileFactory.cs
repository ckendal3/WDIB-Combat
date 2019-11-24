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
using WDIB.Explosives;

namespace WDIB.Projectiles
{
    public static class ProjectileFactory
    {
        static EntityManager EntityManager;
        static EntityArchetype Archetype;

        static WeaponParameters Parameters;

        private static Random random;

        // use for debugging
        #if UNITY_EDITOR
        static int debugGroupID = 0;
        static int debugIndividualID = 0;
        #endif

        static ProjectileFactory()
        {
            EntityManager = World.Active.EntityManager;

            // create our archetype
            Archetype = EntityManager.CreateArchetype(                                                               // probably could just get away with Scale component
                                        ComponentType.ReadWrite<Translation>(), ComponentType.ReadWrite<Rotation>(), ComponentType.ReadWrite<NonUniformScale>(), 
                                        ComponentType.ReadWrite<PreviousTranslation>(), ComponentType.ReadWrite<Speed>(), ComponentType.ReadWrite<Damage>(), 
                                        ComponentType.ReadWrite<Projectile>(), ComponentType.ReadWrite<OwnerID>(), ComponentType.ReadWrite<Distance>(),
                                        ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadWrite<RenderMesh>()
                                        );

            Parameters = WeaponParameters.Instance;
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

            SetComponents(projectileID, spawnPos, spawnRot, Parameters.GetProjectileDataByID(projectileID), ownerID);
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
            templateEnt = EntityManager.CreateEntity(Archetype);


            #if UNITY_EDITOR
            debugIndividualID += 1;
            EntityManager.SetName(templateEnt, data.projectileName + " Projectile " + debugIndividualID + " - Group " + debugGroupID);
            #endif

            //Generic Components
            EntityManager.SetComponentData(templateEnt, new Translation { Value = spawnPos });
            EntityManager.SetComponentData(templateEnt, new Rotation { Value = spawnRot });
            EntityManager.SetComponentData(templateEnt, new NonUniformScale { Value = new float3(1, 1, data.length) });
            EntityManager.SetSharedComponentData(templateEnt, new RenderMesh { mesh = data.mesh, material = data.material });

            // specific components
            EntityManager.SetComponentData(templateEnt, new PreviousTranslation { Value = spawnPos });
            EntityManager.SetComponentData(templateEnt, new OwnerID { Value = ownerID });
            EntityManager.SetComponentData(templateEnt, new Distance { Value = 0, MaxDistance = data.maxDistance });
            EntityManager.SetComponentData(templateEnt, new Damage { Value = data.damage });
            EntityManager.SetComponentData(templateEnt, new Speed { Value = data.speed });
            EntityManager.SetComponentData(templateEnt, new Projectile { ID = projectileID });

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
                EntityManager.Instantiate(templateEnt, cloneEnts);

                // get random spreads
                GetConeOfFire(data, spawnRot, projectileCount, out NativeArray<quaternion> rSpreads);

                // We need to add random spread to the first projectile
                EntityManager.SetComponentData(templateEnt, new Rotation { Value = rSpreads[0] });

                // we start at one because set our templateEnt to the first spread
                for (int i = 1; i < projectileCount; i++)
                {
                    #region Entity Debugging
#if UNITY_EDITOR
                    debugIndividualID += 1;
                    EntityManager.SetName(cloneEnts[i - 1], data.projectileName + " Projectile " + debugIndividualID + " - Group " + debugGroupID);
#endif
                    #endregion

                    // we subtract one because our cloned projectiles need to iterate starting at zero
                    EntityManager.SetComponentData(cloneEnts[i - 1], new Rotation { Value = rSpreads[i] });
                }

                //we have set all our data, dispose our native container to avoid memory leak
                cloneEnts.Dispose();
                rSpreads.Dispose();
            }
        }

        [BurstCompile]
        private static void GetConeOfFire(ProjectileData data, quaternion rotation, int count, out NativeArray<quaternion> rSpreads)
        {
            rSpreads = new NativeArray<quaternion>(count, Allocator.TempJob);

            var coneOfFireJob = new CreateConeOfFireJob
            {
                spreads = rSpreads,
                baseRot = rotation,
                random = new Random(34377 * (uint)System.Environment.TickCount),
                spreadAmounts = new float2(data.spread.minimumSpread, data.spread.maximumSpread)
            }.Schedule(count, 32);

            coneOfFireJob.Complete();
        }

        [BurstCompile]
        private struct CreateConeOfFireJob : IJobParallelFor
        {
            [WriteOnly]
            public NativeArray<quaternion> spreads;

            [ReadOnly] public quaternion baseRot;
            [ReadOnly] public Random random;

            [ReadOnly] public float2 spreadAmounts;

            public void Execute(int index)
            {
                float3 deviation = new float3(random.NextFloat(spreadAmounts.x, spreadAmounts.y),
                                   random.NextFloat(spreadAmounts.x, spreadAmounts.y),
                                   random.NextFloat(spreadAmounts.x, spreadAmounts.y));

                spreads[index] = math.mul(baseRot, quaternion.Euler(deviation));
            }
        }


        // TODO: DYNAMIC component data adding for all factories
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
                    EntityManager.AddComponentData(entity, (HeadShotMultiplier)data.GetComponentData());
                    return;
                case EComponentType.MultiHit:
                    EntityManager.AddComponentData(entity, (MultiHit)data.GetComponentData());
                    return;
                case EComponentType.EMP:
                    Debug.LogWarning("EMP is not yet implemented.");
                    return;
                case EComponentType.SuperCombine:
                    EntityManager.AddComponentData(entity, (SuperCombine)data.GetComponentData());
                    return;
                case EComponentType.Tracking:
                    EntityManager.AddComponentData(entity, (TrackPlayer)data.GetComponentData());
                    return;
                case EComponentType.Explosive:
                    EntityManager.AddComponentData(entity, (Explosive)data.GetComponentData());
                    return;
                case EComponentType.NotImplemented:
                    Debug.Log("Not yet immplemented.");
                    return;
            }
        }
    }
}