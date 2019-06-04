using UnityEngine;
using WDIB.Utilities;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "Weapon Parameters", menuName = "WDIB/Parameters/WeaponParameters")]
    public class WeaponParameters : SingletonScriptableObject<WeaponParameters>
    {
        [Header("Global")]
        [Tooltip("This is the maximum distance any projectile should last.")]
        [Range(1, 10000f)]
        [SerializeField]
        private float GlobalMaximumProjectileRange = 1000f;

        [Header("Projectiles")]
        [SerializeField]
        private ProjectileData[] projectileSet = null;

        [SerializeField]
        private LayerMask projectileHitLayers = 1 << 0;

        [Header("Explosives")]
        [SerializeField]
        private ExplosiveData[] explosiveSet = null;

        [SerializeField]
        private LayerMask explosiveHitLayers = 1 << 0;


#if UNITY_EDITOR

        [Header("Debug Settings")]
        [Tooltip("This is the explosive used when there is an explosive related error.")]
        [SerializeField]
        private ExplosiveData defaultExplosive = null;

        [SerializeField]
        [Tooltip("This is the projectile used when there is a projectile related error.")]
        private ProjectileData defaultProjectile = null;
#endif

        /// <summary>
        /// Get the explosive data by the specified ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ExplosiveData GetExplosiveDataByID(int ID)
        {
#if UNITY_EDITOR
            if (explosiveSet == null || explosiveSet.Length == 0 || ID >= explosiveSet.Length || explosiveSet[ID] == null)
            {
                Debug.LogWarning($"Explosive ID {ID} does not exist. Returning default explosive.");
                return defaultExplosive;
            }
#endif

            return explosiveSet[ID];
        }

        /// <summary>
        /// Get the projectile data by the specified ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ProjectileData GetProjectileDataByID(int ID)
        {
#if UNITY_EDITOR
            if (projectileSet == null || projectileSet.Length == 0 || ID >= projectileSet.Length || projectileSet[ID] == null) //projectileSet == null || 
            {
                Debug.LogWarning($"Projectile ID {ID} does not exist. Returning default projectile.");
                return defaultProjectile;
            }
#endif

            return projectileSet[ID];
        }

        /// <summary>
        /// Get the maximum distance a projectile should go before being destroyed.
        /// </summary>
        /// <returns></returns>
        public float GetProjectileMaximumDistance()
        {
            return GlobalMaximumProjectileRange;
        }

        /// <summary>
        /// Get the explosive hit layer
        /// </summary>
        /// <returns></returns>
        public LayerMask GetExplosiveHitLayer()
        {
            return explosiveHitLayers;
        }

        /// <summary>
        /// Get the projectile hit layer
        /// </summary>
        /// <returns></returns>
        public LayerMask GetProjectileHitLayer()
        {
            return projectileHitLayers;
        }
    }
}