using UnityEngine;
using System.Collections;

namespace ARPG.Core
{
    public class TopDownCharacterMotor : CharacterMotor
    {
        [Header("Landing Variants")]
        public float landRunThreshold = 3.5f;
        public float gravityMultiplier;

        // ===== GROUNDING =====
        [Header("Grounding Probe")]
        public float groundProbeExtra = 0.15f;
        public float groundProbeRadiusScale = 0.9f;
        public LayerMask groundMask = ~0;

        [Header("Grounding Grace")]
        float _groundingIgnoreUntil = -1f;
        bool _rawGrounded = false;

        // ===== REFERENCES =====
        [Header("References")]
        public Camera mainCamera;
        public Animator animator;

        // ===== MOVEMENT =====
        [Header("Movement")]
        public float moveSpeed = 5.5f;
        public float runSpeed = 7.5f;
        public float acceleration = 25f;
        public float deceleration = 30f;
        public float rotationSpeed = 12f;

        // ===== ANIM LAND DETECTION =====
        [Header("Animator Land Detection")]
        public float landMinSpeed = 1.5f;
        float _prevYVel;

        // ===== INTERNALS =====
        CharacterController _capsule;
        private float _modelYawOffsetDeg = 0f;
        float _verticalVelocity;
        Vector3 _relativeVelocity;

        // animation IDs
        private int _animIDSpeedX;
        private int _animIDSpeedY;

        public bool IsGrounded { get; private set; }

        private void AssignAnimationIDs()
        {
            _animIDSpeedX = Animator.StringToHash("SpeedX");
            _animIDSpeedY = Animator.StringToHash("SpeedY");
        }

        void Awake()
        {
            _capsule = GetComponent<CharacterController>();
            mainCamera ??= Camera.main;
            AssignAnimationIDs();
        }

        void Update()
        {
            // Animator params
            if (animator)
            {
                animator.SetFloat(_animIDSpeedX, _relativeVelocity.x);
                animator.SetFloat(_animIDSpeedY, _relativeVelocity.z);
                animator.SetBool("Grounded", IsGrounded);

                bool animAirborne = (Time.time < _groundingIgnoreUntil) || !IsGrounded;
                animator.SetBool("Airborne", animAirborne);
                animator.SetFloat("YVel", _relativeVelocity.y);
            }
        }

        void FixedUpdate()
        {
            UpdateGrounded();

            // Stronger gravity feel
            GravityUpdate();

            // No dash branch — only movement + jump FSM
            HandleMovement();            
            RotateBody();
        }

        // ===== GROUND PROBE =====
        void UpdateGrounded()
        {
            float yVel = _verticalVelocity;

            float radius = Mathf.Max(0.01f, _capsule.radius * groundProbeRadiusScale);
            Vector3 feet = transform.position + Vector3.up * radius;
            float castDist = radius + groundProbeExtra;
            bool hit = Physics.SphereCast(feet + Vector3.up * 0.05f, radius, Vector3.down, out RaycastHit h, castDist, groundMask, QueryTriggerInteraction.Ignore);

            bool ignoring = Time.time < _groundingIgnoreUntil;
            _rawGrounded = hit;
            bool prevGrounded = IsGrounded;
            IsGrounded = ignoring ? false : _rawGrounded;

            bool justLanded = !prevGrounded && IsGrounded;
            if (justLanded)
            {
                if (h.distance > 0f)
                    transform.position = new Vector3(transform.position.x, transform.position.y - (h.distance - 0.01f), transform.position.z);

                if (animator)
                {
                    float impact = Mathf.Abs(_prevYVel);
                    animator.SetFloat("FallSpeed", impact);

                    Vector3 v = _capsule.velocity;
                    float hSpeed = new Vector2(v.x, v.z).magnitude;
                    animator.SetFloat("LandHSpeed", hSpeed);
                    animator.SetInteger("LandStyle", hSpeed >= landRunThreshold ? 1 : 0);

                    if (impact >= landMinSpeed)
                        animator.SetTrigger("Land");
                }
            }
            _prevYVel = yVel;
        }

        // ===== MOVEMENT =====
        void HandleMovement()
        {
            float targetSpeed = runHeld ? runSpeed : moveSpeed;

            // moveInput is already normalized (x = horizontal, y = vertical)
            Vector3 desiredDir = new Vector3(moveInput.x, 0f, moveInput.y);
            if (desiredDir.sqrMagnitude > 1f)
                desiredDir.Normalize();

            Vector3 vXZ = _capsule.velocity;

            Vector3 desiredVel = desiredDir * targetSpeed;
            float accel = (desiredVel.sqrMagnitude > vXZ.sqrMagnitude) ? acceleration : deceleration;
            Vector3 newXZ = Vector3.MoveTowards(vXZ, desiredVel, accel * Time.deltaTime) * Time.deltaTime +
                            new Vector3(0, _verticalVelocity * Time.deltaTime, 0);
            _capsule.Move(newXZ);

            _relativeVelocity = transform.InverseTransformVector(_capsule.velocity);
        }

        private void RotateBody()
        {
            Vector3 flatDir;

            if (lookInput.sqrMagnitude > 0.01f)
            {
                // Joystick look (camera-relative → world XZ)
                Vector2 look = lookInput.normalized;

                Vector3 camForward = mainCamera.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                Vector3 camRight = mainCamera.transform.right;
                camRight.y = 0f;
                camRight.Normalize();

                // Up on stick = camera forward, right on stick = camera right
                flatDir = camForward * look.y + camRight * look.x;
            }
            else
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(ray, out var hit, 2000f, groundMask)) return;

                flatDir = hit.point - transform.position;
                flatDir.y = 0f;
            }

            if (flatDir.sqrMagnitude < 1e-6f) return;

            // Final rotation: base look + parallax + model offset
            Quaternion lookRot =
                Quaternion.LookRotation(flatDir.normalized, Vector3.up) *
                Quaternion.AngleAxis(_modelYawOffsetDeg, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                rotationSpeed * Time.deltaTime
            );
        }

        void GravityUpdate()
        {
            if (IsGrounded)
            {
                if (_verticalVelocity < 0)
                    _verticalVelocity = -2;
            }

            _verticalVelocity -= gravityMultiplier * Time.deltaTime;
        }

        // ===== INVULN =====
        public bool IsInvulnerable() => false;
    }

    static class AnimatorExtensions
    {
        public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
        {
            if (!self || string.IsNullOrEmpty(name)) return false;
            foreach (var p in self.parameters)
                if (p.type == type && p.name == name) return true;
            return false;
        }
    }
}
