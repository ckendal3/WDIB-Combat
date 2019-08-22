﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// TODO: Implement timed reload
// TODO: Implement ammo reduction on reload
// TODO: Implement Battery Cooldown
public class ReloadSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<ReloadTag, Weapon>().ForEach((Entity entity, ref AmmoComponent ammo) =>
        {
            ammo.value = ammo.maxValue;

            World.Active.EntityManager.RemoveComponent<ReloadTag>(entity);
        });
    }
}