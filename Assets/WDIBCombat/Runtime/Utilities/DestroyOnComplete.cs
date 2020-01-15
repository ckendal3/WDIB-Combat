using UnityEngine;

namespace WDIB.Utilities
{
    [RequireComponent(typeof(ParticleSystem))]
    public class DestroyOnComplete : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
        }

    }
}