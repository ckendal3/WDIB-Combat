using UnityEngine;

namespace WDIB.Monobehaviours
{
    public class DestroyParticleOnComplete : MonoBehaviour
    {
        private ParticleSystem particle;
        private float timeAlive;

        private void Awake()
        {
            particle = GetComponent<ParticleSystem>();

            Destroy(gameObject, particle.main.duration);
        }
    }
}