using Unity.Entities;

namespace WDIB.Utilities
{
    public class MovementSystemGroup : ComponentSystemGroup { } // Update rotations system then update movement system

    [UpdateAfter(typeof(MovementSystemGroup))]
    public class HitSystemGroup : ComponentSystemGroup { } // Raycasts, projectiles, etc and then do logic for hits

    [UpdateAfter(typeof(HitSystemGroup))]
    public class LifeSystemGroup : ComponentSystemGroup { } // lifetime reduction system and then destroy system
}