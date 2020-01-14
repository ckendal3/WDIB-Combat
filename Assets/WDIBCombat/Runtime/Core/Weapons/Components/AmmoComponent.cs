using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct AmmoComponent : IComponentData
{
    public int value;
    public int maxValue;

    public int reserveValue;
    public int maxReserve;
}

public struct ReduceAmmoTag : IComponentData { }