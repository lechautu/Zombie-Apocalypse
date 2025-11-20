// ARPG/Combat/Data/AoeData.cs  (refined)
using UnityEngine;

namespace ARPG.Combat
{
    public enum AoeAnchor { CasterSelf, LockOnTarget, GroundPointInFront }

    [CreateAssetMenu(fileName = "AoeData", menuName = "ARPG/Combat/AoE Data")]
    public class AoeData : ScriptableObject
    {
        public AoeZone prefab;
        public float radius = 2.5f;
        public float duration = 2f;
        public float tickInterval = 0.25f;
        public AoeAnchor anchor = AoeAnchor.CasterSelf;
        public float forwardDistance = 2.5f;
        public bool followAnchor = false;
        public LayerMask hitMask = ~0;

        [Header("On Hit")]
        public DamageSpec damage = new();
    }
}
