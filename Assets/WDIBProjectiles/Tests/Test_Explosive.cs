using UnityEngine;
using WDIB.Factory;

public class Test_Explosive : MonoBehaviour
{
    [SerializeField]
    public float timeBetweenSpawns = 1f;
    private float timeTillNextSpawn = 0;

    [SerializeField]
    public int ExplosiveID = 0;

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

            ExplosiveFactory.CreateExplosive(ExplosiveID, transform.position, 0);
        }
    }
}