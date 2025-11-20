using UnityEngine;

namespace ARPG.Core
{
    [DisallowMultipleComponent]
    public class Targetable : MonoBehaviour
    {
        [Header("Lock On")]
        [Tooltip("Point the camera/reticle should focus at (defaults to this transform).")]
        public Transform lockPoint;

        [Tooltip("If false, this target will be ignored (e.g., dead).")]
        public bool canBeTargeted = true;

        void Reset()
        {
            if (!lockPoint) lockPoint = transform;
        }
    }
}
