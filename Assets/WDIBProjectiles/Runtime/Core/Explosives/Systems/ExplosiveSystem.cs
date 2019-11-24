﻿using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Projectiles;
using WDIB.Utilities;

namespace WDIB.Explosives
{
    // TODO: EXPLOSIVES::Reduce main thread waiting
    public class ExplosiveSystem : JobComponentSystem
    {
        private Collider[] Colliders;

        private WeaponParameters Parameters;
        private LayerMask HitMask;

        private EntityQuery ExplosiveQuery;

        public delegate void ExplosionHitSystemEvent(ECSExplosiveData[] hitData);
        public static ExplosionHitSystemEvent onExplosionHitSystemFinish;

        public struct ValidHitCheckJob : IJobParallelFor
        {
            [ReadOnly]
            public float3 fromPosition;
            [ReadOnly]
            public LayerMask hitMask;
            [WriteOnly]
            NativeArray<RaycastCommand> commands;
            [ReadOnly]
            NativeArray<float3> toPositions;

            public void Execute(int index)
            {
                commands[index] = new RaycastCommand
                {
                    from = fromPosition,
                    distance = math.distance(toPositions[index], fromPosition),
                    direction = math.normalize(toPositions[index] - fromPosition),
                    layerMask = hitMask,
                    maxHits = 1
                };
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            int count = ExplosiveQuery.CalculateEntityCount();
            if (count == 0)
            {
                return inputDeps;
            }

            // get the explosive entities
            var entities = ExplosiveQuery.ToEntityArray(Allocator.TempJob, out JobHandle entHandle);
            entHandle.Complete();

            // Get all our ECS components we will need - this are minified jobs
            var explosives = ExplosiveQuery.ToComponentDataArray<Explosive>(Allocator.TempJob, out JobHandle expHandle);
            var positions = ExplosiveQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle posHandle);
            var owners = ExplosiveQuery.ToComponentDataArray<OwnerID>(Allocator.TempJob, out JobHandle ownerHandle);

            // make sure we have all our component data (minified jobs are done)
            expHandle.Complete();
            posHandle.Complete();
            ownerHandle.Complete();

            int explosiveId;
            ExplosiveData data;

            // --------------------------
            // THIS IS GENERATING GARBAGE?
            // --------------------------
            List<ECSExplosiveData> tmpHitData = new List<ECSExplosiveData>();
            List<Collider> explosiveHits;

            // for every explosion
            for (int i = 0; i < count; i++)
            {
                explosiveId = (int)explosives[i].ID;
                data = Parameters.GetExplosiveDataByID(explosiveId);

                // if we have hits
                ECSExplosiveData ecsData;
                int hitCount = Physics.OverlapSphereNonAlloc(positions[i].Value, data.radius, Colliders, HitMask);
                if (hitCount > 0)
                {
                    explosiveHits = new List<Collider>();
                    ecsData = new ECSExplosiveData { ExplosiveID = explosiveId };

                    // Do a batch raycast to all hits and filter out all overlapping colliders that 
                    // aren't hit first - so raycast to an overlapped object and make sure there isn't
                    // a wall inbetween them and the explosion
                    // need to have an environmental check so we don't have to cycle through everything
                    //NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(hitCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    //NativeArray<RaycastHit> hitResults = new NativeArray<RaycastHit>(hitCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    //RaycastCommand.ScheduleBatch(commands, hitResults, 2, );

                    // for every hit we have
                    for (int j = 0; j < Colliders.Length; j++)
                    {
                        // if we have a null collider - every collider after should be null as well
                        if (Colliders[j] == null)
                        {
                            break;
                        }

                        explosiveHits.Add(Colliders[j]);
                    }

                    // if we have hits, add it to the main array - may be an unneeded check
                    if (explosiveHits.Count > 0)
                    {
                        ecsData.Colliders = explosiveHits.ToArray();
                        tmpHitData.Add(ecsData);
                    }

                    //commands.Dispose();
                    //hitResults.Dispose();
                }

            }

            // if we have explosion hits
            if (tmpHitData.Count > 0)
            {
                onExplosionHitSystemFinish?.Invoke(tmpHitData.ToArray());
            }

            // batch destroy explosive entities
            EntityManager.DestroyEntity(entities);

            positions.Dispose();
            entities.Dispose();
            owners.Dispose();
            explosives.Dispose();

            return inputDeps;
        }

        protected override void OnCreate()
        {
            Parameters = WeaponParameters.Instance;
            HitMask = Parameters.GetExplosiveHitLayer();

            Colliders = new Collider[32];

            ExplosiveQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<Explosive>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<OwnerID>() },
                None = new ComponentType[] { ComponentType.ReadOnly<Projectile>() }
            });
        }
    }
}