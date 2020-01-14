using System.Collections.Generic;
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
    [UpdateInGroup(typeof(HitSystemGroup))]
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
            // TODO: FIX GC: Adding to list is creating 21.9 KB a frame
            // THIS IS GENERATING GARBAGE
            // --------------------------
            List<ECSExplosiveData> tmpHitData = new List<ECSExplosiveData>();
            List<Collider> explosiveHits;

            // for every explosion
            for (int i = 0; i < count; i++)
            {
                explosiveId = explosives[i].ID;
                data = Parameters.GetExplosiveDataByID(explosiveId);

                // if we have hits
                ECSExplosiveData ecsData;
                // TODO: Check, is this going to apply to child colliders too? We don't want that
                int hitCount = Physics.OverlapSphereNonAlloc(positions[i].Value, data.radius, Colliders, HitMask);
                if (hitCount > 0)
                {
                    explosiveHits = new List<Collider>();
                    ecsData = new ECSExplosiveData { ExplosiveID = explosiveId, OwnerID = owners[i].Value, Position = positions[i].Value };

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