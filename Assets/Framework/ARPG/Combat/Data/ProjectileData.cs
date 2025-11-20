// ARPG/Combat/Data/ProjectileData.cs  (refined)
using UnityEngine;

namespace ARPG.Combat
{
    [CreateAssetMenu(fileName = "ProjectileData", menuName = "ARPG/Combat/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Visual / Prefab")]
        public ProjectileController prefab;

        [Header("Motion")]
        public float speed = 16f;
        public float gravity = 0f;
        public float lifetime = 3f;
        public bool faceVelocity = true;
        public bool homing = false;
        public float homingTurnRateDeg = 360f;
        public float homingMaxRange = 18f;

        [Header("Spawn")]
        public Vector3 localMuzzleOffset = new(0, 1.0f, 0.6f);
        public bool inheritCasterForward = true;

        [Header("Collision")]
        public LayerMask hitMask = ~0;
        public int maxPenetration = 1;
        public Vector3 offset;
        public float sweepRadius;

        [Header("On Hit")]
        public DamageSpec damage = new();
    }
}
