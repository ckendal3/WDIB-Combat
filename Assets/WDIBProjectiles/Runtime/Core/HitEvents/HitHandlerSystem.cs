using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using WDIB.Components;
using WDIB.Factory;
using WDIB.Parameters;
using WDIB.Systems;

//TODO: Implement Hit Visuals
//TODO: Implment physics hits/forces
//TODO: Change owner IDs from uints to ints?
//TODO: Implement Impulse force and damage based on distance for explosions
//TODO: Destroy/Stop multi-hit projectiles on environmental hit, continue on soft target hits

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

            float damage = eManager.GetComponentData<Damage>(handlerData.entity).Value;
            int ownerID = (int)eManager.GetComponentData<Owner>(handlerData.entity).ID;

            TryToApplyDamage(damage, ownerID, hit.collider);

            //TryToApplyForce();
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
            uint ownerID = eManager.GetComponentData<Owner>(entity).ID;
            float damage = eManager.GetComponentData<Damage>(entity).Value;

            // for every hit
            for (int i = 0; i < hitData.Length; i++)
            {
                hit = hitData[i];

                if (canExplode)
                {
                    AddExplosion(ref entity, hit.point, ownerID);
                }

                eManager.SetComponentData(entity, new MultiHit { hits = multiComponent.hits + 1, maxHits = multiComponent.maxHits });

                TryToApplyDamage(damage, (int)ownerID, hit.collider);
                //TryToApplyForce();
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

                    ExplosiveData eData = WeaponParameters.Instance.GetExplosiveDataByID(hitData[i].explosiveID);

                    TryToApplyDamage(eData.damage, data.ownerID, collider);
                    //TryToApplyForce();
                    CreateHitVisual();
                }

            }
        }

        private static void TryToApplyDamage(float damage, int ownerID, Collider collider) //ref Entity entity, Collider collider
        {
            // if there is no damageable interface - don't do anything else
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            damageable.ApplyDamage(damage, ownerID);
        }

        private static void TryToApplyForce(ref Entity entity, Collider collider) //ref Entity entity, Collider collider
        {
            // if there is no physicsable interface - don't do anything else
            IPhysicsable physicsable = collider.GetComponent<IPhysicsable>();
            if (physicsable == null)
            {
                return;
            }

            //TODO: Change owners to uints or ints?
            float force = eManager.GetComponentData<Damage>(entity).Value; // use damage as force for now
            int ownerID = (int)eManager.GetComponentData<Owner>(entity).ID;

            physicsable.AddForce(force);
        }

        private static void TryToApplyImpulseForce(ref Entity entity, Collider collider) //ref Entity entity, Collider collider
        {
            // if there is no physicsable interface - don't do anything else
            IPhysicsable physicsable = collider.GetComponent<IPhysicsable>();
            if (physicsable == null)
            {
                return;
            }

            float force = eManager.GetComponentData<Damage>(entity).Value;
            int ownerID = (int)eManager.GetComponentData<Owner>(entity).ID;

            physicsable.AddImpulseForce(force, ownerID);
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

        private static void AddExplosion(ref Entity entity, Vector3 hitPoint, uint ownerID)
        {
            Explosive explosive = eManager.GetComponentData<Explosive>(entity);
            ExplosiveFactory.CreateExplosive((int)explosive.ID, hitPoint, ownerID);
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
        public int ownerID; // the player(?) that caused the explosion
        public int explosiveID; // the explosion data
        public Collider[] colliders; // all the things this explosion hit
    }
}