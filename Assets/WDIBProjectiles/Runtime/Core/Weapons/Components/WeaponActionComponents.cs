using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Reload : IComponentData
{
    public float value; // this is how long is left
    public float durtationValue; // this is where the countdown timer starts
}

public struct ReloadTag : IComponentData { }

public struct TimeBetweenShot : IComponentData
{
    public float value; // this is how long is left
    public float durtationValue; // this is where the countdown timer starts
}

public struct MeleeComponent : IComponentData
{
    public float damage; // how much damage to do

    // TODO: Read the comments below
    // these should be pairable with other actions - it could just use an action tag
    public float elapsed; // how long till its done
    public float durationValue; // this is where the countdown timer starts
}

public struct LungeComponent : IComponentData
{
    public float distance; // max lunge distance
    public float rate; // how many m/s to move
}

//TODO: It should be a buffer to determine how many levels
public struct Zoom : IComponentData
{

}

/// <summary>
/// A tag to determine if the weapon is equipped
/// </summary>
public struct EquippedTag : IComponentData { }

//This should be used as a way to determine if we can do anything
public struct Actioning : IComponentData
{
    public float value;
    public float duration;
}
