using System.Collections.Generic;
using UnityEngine;

namespace ARPG.Core
{
    [DisallowMultipleComponent]
    public class LockOnTarget : MonoBehaviour
    {
        [Header("Refs")]
        public Transform playerRoot;                 // optional; used for distances/backups
        public Camera mainCamera;                    // REQUIRED for camera-based targeting
        public MonoBehaviour cameraRig;              // optional: has SetLockTarget/ ClearLockTarget

        [Header("Search (Camera-Based)")]
        public LayerMask targetLayers = ~0;
        public LayerMask obstructionLayers = 0;      // walls/geometry for LoS
        public float maxDistanceFromCamera = 30f;    // world distance cam->target
        [Range(0f, 1f)] public float onScreenPadding = 0.06f; // shrink viewport rect to avoid edges
        public bool requireLineOfSight = true;

        [Header("Cycle")]
        public float cycleSearchRadius = 35f;        // world distance cap (secondary filter)

        [Header("Debug")]
        public bool drawDebug;

        // input flags (set by ARPGPlayerInput)
        [HideInInspector] public bool toggleLockPressed;
        [HideInInspector] public bool cycleLeftPressed;
        [HideInInspector] public bool cycleRightPressed;

        public Transform currentTarget { get; private set; }
        public Targetable currentTargetable { get; private set; }

        readonly Collider[] _overlapTmp = new Collider[96];

        Transform _player => playerRoot ? playerRoot : transform;

        void Awake()
        {
            if (!mainCamera) mainCamera = Camera.main;
        }

        void Update()
        {
            HandleInput();
            if (currentTarget) ValidateCurrentTarget();
        }

        void HandleInput()
        {
            if (toggleLockPressed)
            {
                toggleLockPressed = false;
                if (currentTarget) ClearLock();
                else AcquireBestTargetFromCamera();
            }

            if (currentTarget)
            {
                if (cycleLeftPressed) { cycleLeftPressed = false; Cycle(-1); }
                if (cycleRightPressed) { cycleRightPressed = false; Cycle(+1); }
            }
            else
            {
                cycleLeftPressed = false;
                cycleRightPressed = false;
            }
        }

        // ===== Acquire / Clear =====
        public void AcquireBestTargetFromCamera()
        {
            if (!mainCamera) { Debug.LogWarning("LockOnTarget: No Camera assigned."); return; }

            var cand = GetCameraVisibleCandidates();
            if (cand.Count == 0) return;

            // Score: smallest distance to screen center, then nearest in world
            float bestScore = float.NegativeInfinity;
            Transform best = null;

            foreach (var t in cand)
            {
                var vp = mainCamera.WorldToViewportPoint(t.position);
                float dx = (vp.x - 0.5f);
                float dy = (vp.y - 0.5f);
                float screenCenterDist = Mathf.Sqrt(dx * dx + dy * dy); // 0 = center
                float worldDist = Vector3.Distance(mainCamera.transform.position, t.position);

                float score = -screenCenterDist * 10f - worldDist * 0.05f; // tune weights
                if (score > bestScore) { bestScore = score; best = t; }
            }

            if (best) SetLock(best);
        }

        public void ClearLock() => SetLockInternal(null, null);

        // ===== Cycling (screen-space left/right) =====
        void Cycle(int dir) // -1 left, +1 right
        {
            if (!mainCamera || !currentTarget) return;

            var cand = GetCameraVisibleCandidates();
            if (cand.Count == 0) return;

            // Find targets left/right of current target's screen X
            Vector3 curVP = mainCamera.WorldToViewportPoint(currentTarget.position);
            float curX = curVP.x;
            Transform best = null;
            float bestDx = float.MaxValue;

            foreach (var t in cand)
            {
                if (t == currentTarget) continue;
                var vp = mainCamera.WorldToViewportPoint(t.position);
                float dx = vp.x - curX;
                if (dir > 0 && dx <= 0f) continue; // want to the right
                if (dir < 0 && dx >= 0f) continue; // want to the left

                float abs = Mathf.Abs(dx);
                // prefer closest by X; if tie, choose closer to center
                if (abs < bestDx)
                {
                    bestDx = abs;
                    best = t;
                }
            }

            // fallback: pick absolute nearest by X even if not strictly on that side
            if (!best)
            {
                float bestAbs = float.MaxValue;
                foreach (var t in cand)
                {
                    if (t == currentTarget) continue;
                    var vp = mainCamera.WorldToViewportPoint(t.position);
                    float abs = Mathf.Abs(vp.x - curX);
                    if (abs < bestAbs) { bestAbs = abs; best = t; }
                }
            }

            if (best) SetLock(best);
        }

        // ===== Internals =====
        void SetLock(Transform t)
        {
            var targetable = t ? t.GetComponentInParent<Targetable>() : null;
            if (!targetable || !targetable.canBeTargeted) return;

            // prefer lockPoint
            var focus = targetable.lockPoint ? targetable.lockPoint : t;
            SetLockInternal(focus, targetable);
        }

        void SetLockInternal(Transform focus, Targetable targetable)
        {
            currentTarget = focus;
            currentTargetable = targetable;

            if (cameraRig)
            {
                var t = cameraRig.GetType();
                var set = t.GetMethod("SetLockTarget");
                var clr = t.GetMethod("ClearLockTarget");
                if (focus != null && set != null) set.Invoke(cameraRig, new object[] { focus });
                if (focus == null && clr != null) clr.Invoke(cameraRig, null);
            }
        }

        void ValidateCurrentTarget()
        {
            if (!mainCamera || !currentTargetable || !currentTargetable.canBeTargeted)
            {
                ClearLock(); return;
            }

            // must remain within distance cap (with hysteresis)
            float dist = Vector3.Distance(mainCamera.transform.position, currentTarget.position);
            if (dist > maxDistanceFromCamera * 1.25f) { ClearLock(); return; }

            // must remain at least roughly on screen (looser rect)
            var vp = mainCamera.WorldToViewportPoint(currentTarget.position);
            if (vp.z <= 0f) { ClearLock(); return; }
            if (!InsideViewport(vp, onScreenPadding * 1.5f)) { /* optional sticky: keep lock */ }

            if (requireLineOfSight && !HasLineOfSightFromCamera(currentTarget))
            {
                // optional sticky lock: comment the next line to keep lock through brief occlusion
                // ClearLock(); return;
            }
        }

        // ===== Candidate gathering (from camera view) =====
        List<Transform> GetCameraVisibleCandidates()
        {
            var list = new List<Transform>(32);
            if (!mainCamera) return list;

            // World-space broad-phase: overlap around the CAMERA
            int count = Physics.OverlapSphereNonAlloc(mainCamera.transform.position, maxDistanceFromCamera, _overlapTmp, targetLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                var col = _overlapTmp[i];
                if (!col) continue;

                var targ = col.GetComponentInParent<Targetable>();
                if (!targ || !targ.canBeTargeted) continue;

                var t = targ.lockPoint ? targ.lockPoint : targ.transform;

                // Distance cap from camera
                float dist = Vector3.Distance(mainCamera.transform.position, t.position);
                if (dist > maxDistanceFromCamera) continue;

                // Must be in front of camera and inside viewport (with padding)
                var vp = mainCamera.WorldToViewportPoint(t.position);
                if (vp.z <= 0f) continue;
                if (!InsideViewport(vp, onScreenPadding)) continue;

                // Optional second cap using player distance (prevents grabbing way-off things in view)
                if (Vector3.Distance(_player.position, t.position) > cycleSearchRadius) continue;

                if (requireLineOfSight && !HasLineOfSightFromCamera(t)) continue;

                list.Add(t);
            }
            return list;
        }

        bool InsideViewport(Vector3 vp, float pad)
        {
            return vp.x >= pad && vp.x <= 1f - pad &&
                   vp.y >= pad && vp.y <= 1f - pad;
        }

        bool HasLineOfSightFromCamera(Transform t)
        {
            if (obstructionLayers == 0) return true;
            Vector3 origin = mainCamera.transform.position;
            Vector3 target = t.position;
            Vector3 dir = target - origin;
            float dist = dir.magnitude;
            if (dist < 0.001f) return true;
            dir /= dist;
            return !Physics.Raycast(origin, dir, dist, obstructionLayers, QueryTriggerInteraction.Ignore);
        }

        void OnDrawGizmosSelected()
        {
            if (!drawDebug || !mainCamera) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mainCamera.transform.position, maxDistanceFromCamera);

            if (currentTarget)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(mainCamera.transform.position, currentTarget.position);
                Gizmos.DrawWireSphere(currentTarget.position, 0.2f);
            }
        }
    }
}
