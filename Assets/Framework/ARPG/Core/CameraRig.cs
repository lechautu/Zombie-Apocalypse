using UnityEngine;

namespace ARPG.Core
{
    /// <summary>
    /// Simple third-person orbit camera with soft lock-on support and camera collision.
    /// Attach this to a pivot GameObject. Assign the player as followTarget and a Camera.
    /// ARPGPlayerInput should feed lookInput (Vector2).
    /// LockOnTarget will call SetLockTarget/ClearLockTarget when locking.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraRig : MonoBehaviour
    {
        [Header("Follow")]
        public Transform followTarget;              // player root / pelvis etc.
        public Vector3 followOffset = new Vector3(0f, 1.6f, 0f);
        [Range(0f, 20f)] public float followSmoothTime = 0.06f;

        [Header("Orbit")]
        public float distance = 4.2f;
        public float minDistance = 1.2f;
        public float maxDistance = 6.0f;
        public float yawSpeed = 180f;               // deg/sec per input unit
        public float pitchSpeed = 140f;             // deg/sec per input unit
        public float minPitch = -30f;               // degrees (look down)
        public float maxPitch = 70f;                // degrees (look up)
        public bool invertY = false;
        public float zoomSpeed = 6f;                // use SetZoomInput(...) if you like

        [Header("Collision")]
        public LayerMask collisionMask = ~0;        // environment layers
        public float probeRadius = 0.2f;            // spherecast radius
        public float probePadding = 0.2f;           // keep away from wall

        [Header("Lock-On")]
        public bool lockHoldsDistance = true;       // true: keep user-set distance in lock
        public float lockYawLerp = 12f;             // how fast yaw aligns to target
        public float lockPitchLerp = 10f;
        public float lockSideOffset = 0.8f;         // horizontal camera offset to show both
        public float lockUpBias = 0.2f;             // aim slightly above target center

        [Header("Refs")]
        public Camera cam;                          // assign, or auto-grab child/main

        // --- Input (fed by ARPGPlayerInput) ---
        [HideInInspector] public Vector2 lookInput; // delta per frame (scaled by sensitivity)

        // --- Runtime ---
        Transform _lockTarget;
        bool _locked;
        float _yaw, _pitch;
        float _desiredDistance;
        Vector3 _followVel;
        float _zoomVel;

        void Awake()
        {
            if (!cam) cam = Camera.main;
            if (followTarget == null)
                Debug.LogWarning("CameraRig: followTarget is not assigned.");

            // Initialize yaw/pitch from current transform
            Vector3 e = transform.rotation.eulerAngles;
            _yaw = e.y;
            _pitch = ClampPitch(e.x);
            _desiredDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        void LateUpdate()
        {
            if (!followTarget || !cam) return;

            // 1) Smooth follow pivot
            Vector3 targetPos = followTarget.position + followOffset;
            Vector3 pivot = Vector3.SmoothDamp(transform.position, targetPos, ref _followVel, followSmoothTime);
            transform.position = pivot;

            // 2) Update yaw/pitch (orbit or lock aim)
            if (_locked && _lockTarget)
                AimAtLockTarget();
            else
                IntegrateLookInput();

            // 3) Compute desired camera position (with optional lock side offset)
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);

            Vector3 side = Vector3.zero;
            if (_locked && _lockTarget && Mathf.Abs(lockSideOffset) > 0.001f)
                side = rot * Vector3.right * lockSideOffset;

            float dist = Mathf.SmoothDamp(distance, _desiredDistance, ref _zoomVel, 0.08f);
            distance = Mathf.Clamp(dist, minDistance, maxDistance);

            Vector3 desiredCamPos = pivot + side - rot * Vector3.forward * distance;

            // 4) Collision resolve (sphere cast from pivot to desired)
            Vector3 camPos = ResolveCollision(pivot, desiredCamPos);
            cam.transform.SetPositionAndRotation(camPos, rot);
        }

        // ---------- INPUT HELPERS ----------
        public void SetZoomInput(float scroll) // positive to zoom in/out depending on your binding
        {
            _desiredDistance = Mathf.Clamp(_desiredDistance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        void IntegrateLookInput()
        {
            float dt = Time.deltaTime;
            float lx = lookInput.x;
            float ly = invertY ? lookInput.y : -lookInput.y;

            _yaw += lx * yawSpeed * dt;
            _pitch += ly * pitchSpeed * dt;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        void AimAtLockTarget()
        {
            // Aim at target point (slight up-bias to focus chest/head)
            Vector3 aimPoint = _lockTarget.position + Vector3.up * lockUpBias;

            // Use the rig/pivot as the aiming origin (more stable than camera when colliding)
            Vector3 from = transform.position;
            Vector3 to = aimPoint;
            Vector3 dir = to - from;
            if (dir.sqrMagnitude < 0.0001f) return;

            // --- Yaw (XZ only) ---
            Vector3 xz = new Vector3(dir.x, 0f, dir.z);
            if (xz.sqrMagnitude > 0.0001f)
            {
                float targetYaw = Mathf.Atan2(xz.x, xz.z) * Mathf.Rad2Deg;
                _yaw = Mathf.LerpAngle(_yaw, targetYaw, 1f - Mathf.Exp(-lockYawLerp * Time.deltaTime));
            }

            // --- Pitch (Unity convention: positive = look DOWN) ---
            // use -atan2(y, horizontalLen) so that target above -> negative (look UP)
            float horiz = Mathf.Max(0.0001f, xz.magnitude);
            float targetPitch = -Mathf.Atan2(dir.y, horiz) * Mathf.Rad2Deg;

            // clamp to your ranges (e.g., minPitch = -60, maxPitch = 70)
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            _pitch = Mathf.Lerp(_pitch, targetPitch, 1f - Mathf.Exp(-lockPitchLerp * Time.deltaTime));
        }


        // ---------- COLLISION ----------
        Vector3 ResolveCollision(Vector3 pivot, Vector3 desired)
        {
            Vector3 dir = desired - pivot;
            float dist = dir.magnitude;
            if (dist < 0.001f) return desired;
            dir /= dist;

            if (Physics.SphereCast(pivot, probeRadius, dir, out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                float hitDist = Mathf.Max(0f, hit.distance - probePadding);
                return pivot + dir * hitDist;
            }
            return desired;
        }

        float ClampPitch(float x)
        {
            // Normalize 0..360 → clamp to [-180,180] then apply pitch clamp
            x = (x > 180f) ? x - 360f : x;
            return Mathf.Clamp(x, minPitch, maxPitch);
        }

        // ---------- Lock-On API (called by LockOnTarget) ----------
        public void SetLockTarget(Transform t)
        {
            _lockTarget = t;
            _locked = t != null;
            // Keep current user distance, or snap if preferred
            if (!lockHoldsDistance)
                _desiredDistance = Mathf.Clamp(Vector3.Distance(cam.transform.position, transform.position), minDistance, maxDistance);
        }

        public void ClearLockTarget()
        {
            _lockTarget = null;
            _locked = false;
        }

        // ---------- Utilities ----------
        /// <summary> Returns a flattened camera forward (y=0), normalized. Useful for player movement. </summary>
        public Vector3 FlatForward()
        {
            Vector3 f = cam.transform.forward;
            f.y = 0f;
            return f.sqrMagnitude > 0.0001f ? f.normalized : transform.forward;
        }

        /// <summary> Returns a flattened camera right (y=0), normalized. </summary>
        public Vector3 FlatRight()
        {
            Vector3 r = cam.transform.right;
            r.y = 0f;
            return r.sqrMagnitude > 0.0001f ? r.normalized : transform.right;
        }
    }
}
