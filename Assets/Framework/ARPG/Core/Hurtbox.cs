// ARPG/Core/Hurtbox.cs
using UnityEngine;

namespace ARPG.Core
{
    // Put this on any collider you want to be hittable (enemy body parts, etc.)
    [RequireComponent(typeof(Collider))]
    public class Hurtbox : MonoBehaviour
    {
        public Damageable owner; // auto-filled if missing
        void Reset() => owner = GetComponentInParent<Damageable>();
        void OnValidate() { if (!owner) owner = GetComponentInParent<Damageable>(); }
    }
}
