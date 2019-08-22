using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// Apply damage to the object
    /// </summary>
    /// <param name="value"> How much damage to apply </param>
    /// <param name="owner"> The ID of the damage inflictor </param>
    void ApplyDamage(float value, int owner);

    /// <summary>
    /// Apply healing to the object
    /// </summary>
    /// <param name="value"> How much healing to apply </param>
    /// <param name="owner"> The ID of the healer </param>
    void ApplyHealing(float value, int owner);
}
