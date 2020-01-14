using Unity.Entities;
using UnityEngine;
using WDIB.Player;

public class PlayerStateSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref PlayerState player) =>
        {
            player.CameraPos = Camera.main.transform.position;
            player.CameraRot = Camera.main.transform.rotation;
        });
    }
}
