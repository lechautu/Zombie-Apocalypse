using UnityEngine;
using System.Collections;
using ARPG.Core;

namespace ARPG.Combat
{
    [DisallowMultipleComponent]
    public class ReactionController : MonoBehaviour
    {
        [Header("Refs")]
        public Animator animator;
        public Rigidbody rb;
        public TopDownCharacterMotor motor;     // optional: to face/lock or zero inputs

        [Header("Knockback")]
        public float upBoostOnKnockdown = 0.6f; // small lift on heavy hits
        public float maxKBPerHit = 8f;          // cap to avoid yeets

        [Header("Tuning")]
        [Tooltip("Damage threshold for a ‘heavy’ flinch when poise not broken.")]
        public float heavyDamageThreshold = 25f;
        [Tooltip("Knockback magnitude that causes auto-knockdown.")]
        public float knockdownKBThreshold = 6.0f;

        [Header("Animator Params")]
        public string pHitSmall = "HitSmall";
        public string pHitLarge = "HitLarge";
        public string pStagger = "Stagger";
        public string pKnockdown = "Knockdown";
        public string pDead = "Dead";
        public string pHitDir = "HitDir";   // -1 left, +1 right (optional)

        bool _dead;

        void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
            if (!rb) rb = GetComponent<Rigidbody>();
            if (!motor) motor = GetComponent<TopDownCharacterMotor>();
        }

        void OnEnable()
        {
            _dead = false;
        }

        // ----- Public reaction entry points -----
        public void ReactHit(in HitInfo hit)
        {
            if (_dead) return;

            // Direction for hit anim blend (optional)
            if (animator && !string.IsNullOrEmpty(pHitDir))
            {
                float side = SignedSideToAttacker(hit);
                animator.SetFloat(pHitDir, side);
            }

            bool heavy = hit.damage >= heavyDamageThreshold;
            if (animator)
            {
                if (heavy && !string.IsNullOrEmpty(pHitLarge)) animator.SetTrigger(pHitLarge);
                else if (!string.IsNullOrEmpty(pHitSmall)) animator.SetTrigger(pHitSmall);
            }

            ApplyKnockback(hit.knockback, false);

            // Brief stun window from hitstun (optional)
            if (hit.hitstun > 0.01f) StartCoroutine(CoBriefStun(hit.hitstun));
        }

        public void ReactStagger(in HitInfo hit)
        {
            if (_dead) return;

            if (animator && !string.IsNullOrEmpty(pStagger)) animator.SetTrigger(pStagger);

            // If big knockback → escalate to knockdown
            bool toKD = hit.knockback.magnitude >= knockdownKBThreshold;
            ApplyKnockback(hit.knockback, toKD);

            float stunT = Mathf.Max(hit.hitstun, 0.25f);
            StartCoroutine(CoBriefStun(stunT));
        }

        public void ReactDeath(in HitInfo hit)
        {
            if (_dead) return;
            _dead = true;

            // Drop locomotion control
            StartCoroutine(CoBriefStun(1.5f));

            if (animator && !string.IsNullOrEmpty(pDead)) animator.SetTrigger(pDead);

            // A little impulse for drama if provided
            if (hit.knockback.sqrMagnitude > 0.0001f)
                ApplyKnockback(hit.knockback * 0.7f, true);
        }

        // ----- Internals -----
        void ApplyKnockback(Vector3 kb, bool knockdown)
        {
            if (!rb || kb.sqrMagnitude < 0.0001f) return;

            Vector3 impulse = Vector3.ClampMagnitude(kb, maxKBPerHit);
            if (knockdown) impulse += Vector3.up * upBoostOnKnockdown;

            // do not overwrite vertical velocity abruptly; use VelocityChange
            rb.AddForce(impulse, ForceMode.VelocityChange);
        }

        float SignedSideToAttacker(in HitInfo hit)
        {
            if (!hit.attacker) return 0f;
            Vector3 to = hit.attacker.position - transform.position; to.y = 0f;
            if (to.sqrMagnitude < 1e-4f) return 0f;
            to.Normalize();
            float side = Vector3.SignedAngle(transform.forward, to, Vector3.up);
            // map to [-1,1] by 90° scale
            return Mathf.Clamp(side / 90f, -1f, 1f);
        }

        IEnumerator CoBriefStun(float seconds)
        {
            float until = Time.time + seconds;
            // Minimal “stun”: just kill inputs for the motor (if present)
            if (motor)
            {
                Vector2 prev = motor.moveInput;
                while (Time.time < until)
                {
                    motor.moveInput = Vector2.zero;
                    yield return null;
                }
                motor.moveInput = prev;
            }
            else
            {
                // No motor? just wait (anim handles motion lock)
                yield return new WaitForSeconds(seconds);
            }
        }
    }
}
