using UnityEngine;
using Unity.Mathematics;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "WDIB/Weapon")]
    public class WeaponData : ScriptableObject
    {
        [Tooltip("Name of the Weapon")]
        public string weaponName = "Default";

        [Tooltip("The projectile ID to use")]
        [Range(0, 50)]
        public int projectileId = 0;

        [Tooltip("The type of firing mode")]
        public EFiringType firingType = EFiringType.Automatic;

        [Tooltip("The time between each shot.")]
        [Range(0f, 10f)]
        public float timeBetweenShots = .5f;

        // TODO: Abstract burst to its own component
        [Tooltip("How long between each round in a burst.")]
        [Range(0f, 5f)]
        public float timeBetweenBurstRounds = .2f;

        [Tooltip("The amount of bullets in each burst.")]
        [Range(1, 50)]
        public int burstAmount = 2;

        [Tooltip("The position of the muzzle.")]
        [SerializeField]
        public float3 muzzleOffset = float3.zero;
    }

    public enum EFiringType
    {
        Single,
        Burst,
        Automatic
    }

}