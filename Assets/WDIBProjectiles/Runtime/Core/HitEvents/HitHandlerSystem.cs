using Unity.Collections;
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
        /// The delegate only returns hits with non-null colliders and deals with one projectile's hit at a time.
        /// </summary>
        /// <param name="hitData"></param>
        public static void OnHitSystemEvent(HitHandlerData handlerData, RaycastHit hit)
        {
            ProjectileData data = wParameters.GetProjectileDataByID((int)eManager.GetComponentData<ProjectileID>(handlerData.entity).ID);

            //-------------------------------------------------
            // Hit Logic Goes Here ----------------------------
            // ------------------------------------------------

            //Debug.Log("HIT");

            
            TryToApplyDamage();
            TryToApplyForce();
            CreateHitVisual();

            DestroyEntity(ref handlerData.entity);

        }

        /// <summary>
        /// The delegate only returns hits with non-null colliders and deals with one projectile's sets of hits at a time.
        /// </summary>
        /// <param name="hitData"></param>
        public static void OnMultiHitSystemEvent(HitHandlerData ecsData, NativeArray<RaycastHit> hitData)
        {
            RaycastHit hit;
            Entity entity = ecsData.entity;
            bool canExplode = eManager.HasComponent<Explosive>(entity);
            MultiHit multiComponent = eManager.GetComponentData<MultiHit>(entity);
            uint attackerID = eManager.GetComponentData<Owner>(entity).ID;

            // for every hit
            for (int i = 0; i < hitData.Length; i++)
            {
                hit = hitData[i];

                //-------------------------------------------------
                // Hit Logic Goes Here ----------------------------
                // ------------------------------------------------

                //Debug.Log("MULTI-HIT");

                if (canExplode)
                {
                    AddExplosion(ref entity, hit.point, attackerID);
                }
                
                eManager.SetComponentData(entity, new MultiHit { hits = multiComponent.hits + 1, maxHits = multiComponent.maxHits });
                // need to check what type of hit it is and check if it needs to be destroyed
                // aka Player hit it continues - but environmental hit it stops

                
                TryToApplyDamage();
                TryToApplyForce();
                CreateHitVisual();
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

                    //Debug.Log("EXPLOSION-HIT");

                    
                    TryToApplyDamage();
                    TryToApplyForce();
                    CreateHitVisual();
                }

            }
        }

        private static void TryToApplyDamage() //ref Entity entity, Collider collider
        {
            //float damage = eManager.GetComponentData<Damage>(entity).Value;

            //Debug.LogWarning("Applying damage is not implemented yet.");
        }

        private static void TryToApplyForce() //ref Entity entity, Collider collider
        {
            //Debug.LogWarning("Applying force is not implemented yet.");
        }

        private static void CreateHitVisual()
        {
            // Implement visual hit logic here
        }

        private static void CreateHitVisual(int visualsID, RaycastHit hit)
        {
            // Implement visual hit logic here
            VisualHitParameters.Instance.GetHitVFXDataByType(visualsID, MaterialType.Dirt); // MaterialType.Dirt should be gathered from the hit
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

    public struct HitHandlerData
    {
        public Entity entity;
        public uint projectileID;
        public uint ownerID;
    }

    public struct ECSExplosiveData
    {
        public int explosiveID; // the explosion data
        public Collider[] colliders; // all the things this explosion hit
    }
}