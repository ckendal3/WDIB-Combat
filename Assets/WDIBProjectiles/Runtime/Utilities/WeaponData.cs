using UnityEngine;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "WDIB/Weapon")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName = "Default";

        [Range(0, 50)]
        public int projectileId = 0;

        public EFiringType firingType = EFiringType.Automatic;

        // used for single shot and automatic - trigger pull
        [Range(0f, 10f)]
        public float timeBetweenShots = .5f;

        // time between rounds in a burst to spawn
        [Range(0f, 5f)]
        public float timeBetweenBurstRounds = .2f;

        // Projectiles to spawn per burst
        [Range(1, 50)]
        public int burstAmount = 2;
    }

    public enum EFiringType
    {
        Single,
        Burst,
        Automatic
    }

}