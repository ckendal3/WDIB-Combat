using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;
using WDIB.Parameters;
using WDIB.Utilities;

public class ExplosiveSystem : JobComponentSystem
{
    private EntityManager eManager;
    private Collider[] colliders;

    private WeaponParameters wParameters;

    private EntityQuery m_explosiveGroup;

    public delegate void ExplosionHitSystemEvent(ECSExplosiveData[] hitData);
    public static ExplosionHitSystemEvent onExplosionHitSystemFinish;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int count = m_explosiveGroup.CalculateLength();
        if(count == 0)
        {
            return inputDeps;
        }

        // get the explosive entities
        var entities = m_explosiveGroup.ToEntityArray(Allocator.TempJob, out JobHandle entHandle);
        entHandle.Complete();

        // Get all our ECS components we will need - this are minified jobs
        var explosives = m_explosiveGroup.ToComponentDataArray<Explosive>(Allocator.TempJob, out JobHandle expHandle);
        var positions = m_explosiveGroup.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle posHandle);
        var owners = m_explosiveGroup.ToComponentDataArray<Owner>(Allocator.TempJob, out JobHandle ownerHandle);

        // make sure we have all our component data (minified jobs are done)
        expHandle.Complete();
        posHandle.Complete();
        ownerHandle.Complete();

        int explosiveId;
        ExplosiveData data;

        // --------------------------
        // THIS IS GENERATING GARBAGE
        // --------------------------
        List<ECSExplosiveData> tmpHitData = new List<ECSExplosiveData>();
        List<Collider> explosiveHits;

        // for every explosion
        for (int i = 0; i < count; i++)
        {
            explosiveId = (int)explosives[i].ID;
            data = wParameters.GetExplosiveDataByID(explosiveId);

            // if we have hits
            ECSExplosiveData ecsData;
            int hitCount = Physics.OverlapSphereNonAlloc(positions[i].Value, data.radius, colliders, wParameters.GetExplosiveHitLayer());
            if (hitCount > 0)
            {
                explosiveHits = new List<Collider>();
                ecsData = new ECSExplosiveData { explosiveID = explosiveId };

                // for every hit we have
                for (int j = 0; j < colliders.Length; j++)
                {
                    if(colliders[j] != null)
                    {
                        explosiveHits.Add(colliders[j]);
                    }
                }

                // may be unneeded
                // if we have hits, add it to the main array
                if(explosiveHits.Count > 0)
                {
                    ecsData.colliders = explosiveHits.ToArray();
                    tmpHitData.Add(ecsData);
                }
            }
            
        }

        // if we have explosion hits
        if(tmpHitData.Count > 0)
        {
            onExplosionHitSystemFinish?.Invoke(tmpHitData.ToArray());
        }

        // batch destroy explosive entities
        eManager.DestroyEntity(entities);

        positions.Dispose();
        entities.Dispose();
        owners.Dispose();
        explosives.Dispose();

        return inputDeps;
    }

    protected override void OnCreate()
    {
        wParameters = WeaponParameters.Instance;

        eManager = World.Active.EntityManager;

        colliders = new Collider[32];

        m_explosiveGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<Explosive>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Owner>() },
            None = new ComponentType[] { ComponentType.ReadOnly<ProjectileID>() }
        });
    }
}
