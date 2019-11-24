using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using WDIB.Components;
using WDIB.Explosives;
using WDIB.Interfaces;
using WDIB.Parameters;
using WDIB.Projectiles;
using WDIB.Systems;

//TODO: Implement Hit Visuals
//TODO: Implment physics hits/forces
//TODO: Implement Impulse force and damage based on distance for explosions
//TODO: Destroy/Stop multi-hit projectiles on environmental hit, continue on soft target hits

namespace WDIB.Utilities
{
    public static class HitHandlerSystem
    {
        private static EntityManager EntityManager;
        private static WeaponParameters Parameters;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            EntityManager = World.Active.EntityManager;

            HitSystem.onHitSystemFinish += OnHitSystemEvent;
            MultiHitSystem.onMultiHitSystemFinish += OnMultiHitSystemEvent;
            ExplosiveSystem.onExplosionHitSystemFinish += OnExplosiveSystemEvent;

            Parameters = WeaponParameters.Instance;
        }

        /// <summary>
        /// The delegate only returns hits with non-null colliders and deals with one projectile's hit at a time.
        /// </summary>
        /// <param name="hitData"></param>
        public static void OnHitSystemEvent(HitHandlerData handlerData, RaycastHit hit)
        {
            ProjectileData data = Parameters.GetProjectileDataByID(EntityManager.GetComponentData<Projectile>(handlerData.Entity).ID);

            float damage = EntityManager.GetComponentData<Damage>(handlerData.Entity).Value;
            uint ownerID = EntityManager.GetComponentData<OwnerID>(handlerData.Entity).Value;

            TryToApplyDamage(damage, ownerID, hit.collider);

            //TryToApplyForce();
            CreateHitVisual();

            DestroyEntity(handlerData.Entity);

        }

        /// <summary>
        /// The delegate only returns hits with non-null colliders and deals with one projectile's sets of hits at a time.
        /// </summary>
        /// <param name="hitData"></param>
        public static void OnMultiHitSystemEvent(HitHandlerData ecsData, NativeArray<RaycastHit> hitData)
        {
            RaycastHit hit;
            Entity entity = ecsData.Entity;
            bool canExplode = EntityManager.HasComponent<Explosive>(entity);
            MultiHit multiComponent = EntityManager.GetComponentData<MultiHit>(entity);
            uint ownerID = EntityManager.GetComponentData<OwnerID>(entity).Value;
            float damage = EntityManager.GetComponentData<Damage>(entity).Value;

            // for every hit
            for (int i = 0; i < hitData.Length; i++)
            {
                hit = hitData[i];

                if (canExplode)
                {
                    AddExplosion(entity, hit.point, ownerID);
                }

                EntityManager.SetComponentData(entity, new MultiHit { Hits = multiComponent.Hits + 1, MaxHits = multiComponent.MaxHits });

                TryToApplyDamage(damage, ownerID, hit.collider);
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
                for (int j = 0; j < data.Colliders.Length; j++)
                {
                    collider = data.Colliders[j];

                    ExplosiveData eData = WeaponParameters.Instance.GetExplosiveDataByID(hitData[i].ExplosiveID);

                    TryToApplyDamage(eData.damage, data.OwnerID, collider);
                    //TryToApplyForce();
                    CreateHitVisual();
                }

            }
        }

        private static void TryToApplyDamage(float damage, uint ownerID, Collider collider) //ref Entity entity, Collider collider
        {
            // if there is no damageable interface - don't do anything else
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            damageable.ApplyDamage(damage, ownerID);
        }

        private static void TryToApplyForce(Entity entity, Collider collider) //ref Entity entity, Collider collider
        {
            // if there is no physicsable interface - don't do anything else
            IPhysicsable physicsable = collider.GetComponent<IPhysicsable>();
            if (physicsable == null)
            {
                return;
            }

            //TODO: Change owners to uints or ints?
            float force = EntityManager.GetComponentData<Damage>(entity).Value; // use damage as force for now
            int ownerID = (int)EntityManager.GetComponentData<OwnerID>(entity).Value;

            physicsable.AddForce(force);
        }

        private static void TryToApplyImpulseForce(Entity entity, Collider collider) //ref Entity entity, Collider collider
        {
            // if there is no physicsable interface - don't do anything else
            IPhysicsable physicsable = collider.GetComponent<IPhysicsable>();
            if (physicsable == null)
            {
                return;
            }

            float force = EntityManager.GetComponentData<Damage>(entity).Value;
            int ownerID = (int)EntityManager.GetComponentData<OwnerID>(entity).Value;

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

        private static void AddExplosion(Entity entity, Vector3 hitPoint, uint ownerID)
        {
            Explosive explosive = EntityManager.GetComponentData<Explosive>(entity);
            ExplosiveFactory.CreateExplosive((int)explosive.ID, hitPoint, ownerID);
        }

        private static void DestroyEntity(Entity entity)
        {
            EntityManager.SetComponentData(entity, new Distance { Value = 1000, MaxDistance = 0 });
        }

    }

    public struct HitHandlerData
    {
        public Entity Entity;
        public int ProjectileID;
        public uint OwnerID;
    }

    public struct ECSExplosiveData
    {
        public int ExplosiveID; // the explosion data
        public uint OwnerID; // the player(?) that caused the explosion
        public Collider[] Colliders; // all the things this explosion hit
    }
}