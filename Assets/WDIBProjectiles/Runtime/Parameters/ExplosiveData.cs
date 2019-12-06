using UnityEngine;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "New Explosive", menuName = "WDIB/Data/Explosive")]
    public class ExplosiveData : ScriptableObject
    {
        public string explosiveName = "Explosive";

        [Tooltip("The effective range of the explosive.")]
        [Range(1f, 20f)]
        public float radius = 5f;

        [Tooltip("The damage of the explosion at the epicenter.")]
        [Range(1f, 1000f)]
        public float damage = 100f;

        [Tooltip("The force of the explosion at the epicenter.")]
        [Range(1, 10000)]
        public float force = 500f;

        [Tooltip("The falloff curve for damage based on distance from the epicenter.")]
        public AnimationCurve damageCurve;

        [Tooltip("The visual particle effect to display on hit.")]
        public GameObject particleEffect; // don't want to use gameobject

        //add components like ExplodeOnImpact, DelayedExplosion?
    }
}
