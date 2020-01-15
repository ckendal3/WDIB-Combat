using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using WDIB.Components;

namespace WDIB.Parameters
{
    [CreateAssetMenu(fileName = "Projectile Data", menuName = "WDIB/Data/Projectile")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Main Data")]
        [Tooltip("Name of the Projectile.")]
        public new string name = "Nameless Projectile";

        [Tooltip("The amount of projectiles to spawn per a shot.")]
        [Range(1, 25)]
        public int projectilesPerAShot = 1;

        [Tooltip("The distance the projectile will be alive.")]
        [Range(1f, 1000f)]
        public float maxDistance = 1000f;

        [Tooltip("The base amount of damage.")]
        [Range(0f, 500f)]
        public float damage = 10f;

        [Tooltip("Rate of speed of a projectile (m/s)")]
        [Range(1f, 1000f)]
        public float speed = 10f;

        [Tooltip("The amount of spread per a projectile per a shot.")]
        public PelletSpread spread = default;

        // Visual settings
        [Header("Visual Components")]
        public Mesh mesh;
        public Material material;

        [Tooltip("The length of the projectile visual.")]
        [Range(.01f, 10)]
        public float length = 1.0f;

        [Header("System Components")]
        [Tooltip("The components that this projectile has.")]
        [SerializeReference] public List<IComponentData> components;

        private void OnEnable()
        {
            if(components == null || components.Count < 0)
            {
                components = new List<IComponentData>();
                components.Add(new LifeTime { Value = 15f });
                components.Add(new Health { Value = 15f, MaxValue = 15f });
                components.Add(new Damage { Value = 15f});
            }
        }
    }
}

//TODO: Write a property drawer for pelletspread <- Should probably be a component
[System.Serializable]
public struct PelletSpread
{
    [Range(0, -.35f)]
    public float minimumSpread;

    [Range(0, .35f)]
    public float maximumSpread;
}