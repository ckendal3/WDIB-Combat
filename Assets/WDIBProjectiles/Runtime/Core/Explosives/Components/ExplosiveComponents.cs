using Unity.Entities;

namespace WDIB.Explosives
{
    public struct Explosive : IComponentData
    {
        public int ID;
    }

    public struct EMP : IComponentData
    {
        public float Damage;
    }
}