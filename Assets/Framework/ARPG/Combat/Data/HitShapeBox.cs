// ARPG/Combat/Data/HitShapeBox.cs
using UnityEngine;

namespace ARPG.Combat
{
    [System.Serializable]
    public class HitShapeBox
    {
        public Vector3 localOffset = new(0f, 1.0f, 0.9f);
        public Vector3 boxSize = new(0.7f, 0.6f, 1.4f);
        public float duration = 0.12f;
        public LayerMask hitMask = ~0;
        public bool multiHit = false;
    }
}
