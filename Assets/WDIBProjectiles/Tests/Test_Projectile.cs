using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WDIB.Factory;

public class Test_Projectile : MonoBehaviour
{
    [SerializeField]
    [Range(0, 10f)]
    public float timeBetweenSpawns = 1f;
    private float timeTillNextSpawn = 0;

    [SerializeField]
    [Range(0, 10)]
    public int ProjectileID = 0;

    // Start is called before the first frame update
    void Start()
    {
        timeTillNextSpawn = timeBetweenSpawns;
    }

    // Update is called once per frame
    void Update()
    {
        timeTillNextSpawn -= Time.deltaTime;

        if (timeTillNextSpawn < 0)
        {
            timeTillNextSpawn = timeBetweenSpawns;

            ProjectileFactory.CreateProjectiles(ProjectileID, transform.position, transform.rotation, 0);
        }
    }
}
