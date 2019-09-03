using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WDIB.Components;

public class ShootFromSystem : ComponentSystem
{
    EntityManager entityManager;

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref ShootFrom shootFrom) =>
        {
            float3 newPos = entityManager.GetComponentData<Translation>(shootFrom.entity).Value;
            quaternion newRot = entityManager.GetComponentData<Rotation>(shootFrom.entity).Value;

            shootFrom.position = newPos;
            shootFrom.rotation = newRot;
        });
    }

    protected override void OnCreate()
    {
        entityManager = World.Active.EntityManager;
    }
}
