using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnComplete : MonoBehaviour
{
    private ParticleSystem particle;
    private float timeAlive;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();

        Destroy(gameObject, particle.main.duration);
    }
}
