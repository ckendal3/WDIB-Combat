using Unity.Entities;

public struct BatteryComponent : IComponentData
{
    public uint value;
    public uint maxValue;
}

public struct ReduceBatteryTag : IComponentData { }