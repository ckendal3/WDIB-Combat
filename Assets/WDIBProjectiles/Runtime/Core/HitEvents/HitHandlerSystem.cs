using Unity.Entities;
using UnityEngine;
using WDIB.Components;
using WDIB.Factory;
using WDIB.Parameters;
using WDIB.Systems;

namespace WDIB.Utilities
{
    public static class HitHandlerSystem
    {
        private static EntityManager eManager;
        private static WeaponParameters wParameters;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            eManager = World.Active.EntityManager;

            HitSystem.onHitSystemFinish += OnHitSystemEvent;
            MultiHitSystem.onMultiHitSystemFinish += OnMultiHitSystemEvent;
            ExplosiveSystem.onExplosionHitSystemFinish += OnExplosiveSystemEvent;

            wParameters = WeaponParameters.Instance;
        }

        /// <summary>
        /// The delegate only returns hits with non-null colliders
        /// </summary>
        /// <param name="hitData"></param>
        public static void OnHitSystemEvent(ECSHitData[] hitData)
        {
            RaycastHit hit;
            Entity entity;
            ProjectileData data;
            for (int i = 0; i < hitData.Length; i++)
            {
                entity = hitData[i].entity;
                hit = hitData[i].hit;
                data = wParameters.GetProjectileDataByID((int)eManager.GetComponentData<ProjectileID>(entity).ID);

                //-------------------------------------------------
                // Hit Logic Goes Here ----------------------------
                // ------------------------------------------------

                Debug.Log("HIT");
            }
        }

        /// <summary>
        /// The delegate only returns hits with non-null colliders
        /// </summary>
        /// <param name="hitData"></param>
        public static void OnMultiHitSystemEvent(ECSMultiHitData[] hitData)
        {
            uint attackerID;
            RaycastHit hit;
            Entity entity;
            // for every projectile 
            for (int i = 0; i < hitData.Length; i++)
            {
                entity = hitData[i].entity;
                attackerID = eManager.GetComponentData<Owner>(entity).ID;

                bool canExplode = eManager.HasComponent<Explosive>(entity);

                // for every hit from this projectile
                MultiHit mHit;
                for (int j = 0; j < hitData[i].hits.Length; j++)
                {
                    hit = hitData[i].hits[j];

                    //-------------------------------------------------
                    // Hit Logic Goes Here ----------------------------
                    // ------------------------------------------------

                    Debug.Log("MULTI-HIT");

                    if (canExplode)
                    {
                        AddExplosion(ref entity, hit.point, attackerID);
                    }

                    #region Multi-Hit
                    mHit = eManager.GetComponentData<MultiHit>(entity);
                    eManager.SetComponentData(entity, new MultiHit { hits = mHit.hits + 1, maxHits = mHit.maxHits });

                    // need to check what type of hit it is and check if it needs to be destroyed
                    // aka Player hit it continues - but environmental hit it stops

                    #endregion

                    TryToApplyDamage();
                    TryToApplyForce();
                }
            }
        }

        public static void OnExplosiveSystemEvent(ECSExplosiveData[] hitData)
        {
            ECSExplosiveData data;
            // for every explosion
            for (int i = 0; i < hitData.Length; i++)
            {
                data = hitData[i];
                Collider collider;

                // for every hit from the explosion
                for (int j = 0; j < data.colliders.Length; j++)
                {
                    collider = data.colliders[j];

                    //-------------------------------------------------
                    // Hit Logic Goes Here ----------------------------
                    // ------------------------------------------------

                    Debug.Log("EXPLOSION-HIT");

                    TryToApplyDamage();
                    TryToApplyForce();
                }

            }
        }

        private static void TryToApplyDamage() //ref Entity entity, Collider collider
        {
            //float damage = eManager.GetComponentData<Damage>(entity).Value;

            Debug.LogWarning("Applying damage is not implemented yet.");
        }

        private static void TryToApplyForce() //ref Entity entity, Collider collider
        {
            Debug.LogWarning("Applying force is not implemented yet.");
        }

        private static void AddExplosion(ref Entity entity, Vector3 hitPoint, uint attackerID)
        {
            Explosive explosive = eManager.GetComponentData<Explosive>(entity);
            ExplosiveFactory.CreateExplosive((int)explosive.ID, hitPoint, attackerID);
        }

        private static void DestroyEntity(ref Entity entity)
        {
            eManager.SetComponentData(entity, new Distance { Value = 1000, MaxDistance = 0 });
        }

    }

    public struct ECSHitData
    {
        public Entity entity;
        public uint projectileID;

        public RaycastHit hit;
    }

    public struct ECSExplosiveData
    {
        public int explosiveID; // the explosion data
        public Collider[] colliders; // all the things this explosion hit
    }

    public struct ECSMultiHitData
    {
        public Entity entity;
        public uint projectileID;

        public RaycastHit[] hits;
    }
}