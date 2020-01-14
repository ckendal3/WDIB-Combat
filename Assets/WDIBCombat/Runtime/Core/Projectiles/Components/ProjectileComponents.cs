using Unity.Entities;
using Unity.Mathematics;

namespace WDIB.Projectiles
{
    public struct Projectile : IComponentData
    {
        public int ID;
    }

    public struct HeadShotMultiplier : IComponentData
    {
        public float Value;
    }

    public struct MultiHit : IComponentData
    {
        public int Hits;
        public int MaxHits;
    }

    public struct SuperCombine : IComponentData
    {
        public int Hits;
        public int HitsToCombine;

        public float Damage;
    }
}
