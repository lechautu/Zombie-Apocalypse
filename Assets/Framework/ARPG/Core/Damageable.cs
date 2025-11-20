using UnityEngine;
using System;
using ARPG.Combat;

namespace ARPG.Core
{
    [DisallowMultipleComponent]
    public class Damageable : MonoBehaviour
    {
        [Header("Team")]
        public Team team = Team.Enemy;

        [Header("Health")]
        public float maxHealth = 100f;
        public float health = 100f;

        [Header("Poise / Hyper Armor")]
        [Tooltip("Maximum poise. Reaches 0 → stagger.")]
        public float maxPoise = 100f;
        [Tooltip("Current poise. Depletes on hits, regenerates after delay.")]
        public float poise = 100f;
        [Tooltip("How much poise damage a hit must cause (after multipliers) to ‘count’. 0 = all count.")]
        public float poiseFloor = 0f;
        [Tooltip("Poise regen per second.")]
        public float poiseRegenRate = 35f;
        [Tooltip("Delay after taking damage before regen starts.")]
        public float poiseRegenDelay = 0.8f;
        float _poiseRegenResumeAt = -1f;

        [Header("Damage Multipliers")]
        public float damageTakenMul = 1f;  // 0..1 for resistance, >1 for vulnerability
        public float poiseTakenMul = 1f;

        [Header("Invulnerability / Super Armor")]
        [Tooltip("If true, health won’t change (used by cutscenes, etc.).")]
        public bool godMode = false;
        [Tooltip("While active, hits apply health damage but won’t break poise (no staggers).")]
        public bool superArmor = false;
        [Tooltip("Global i-frames – ignore *all* hits while active.")]
        public bool invulnerable = false;

        [Header("Runtime Refs")]
        public ReactionController reactions;   // optional; auto-grab if missing

        // Signals
        public event Action<HitInfo> OnDamaged;
        public event Action<HitInfo> OnKilled;
        public event Action<float> OnHealthChanged; // new health
        public event Action<float> OnPoiseChanged;  // new poise (0..max)

        void Awake()
        {
            if (!reactions) reactions = GetComponent<ReactionController>();
            health = Mathf.Clamp(health <= 0f ? maxHealth : health, 0f, maxHealth);
            poise = Mathf.Clamp(poise <= 0f ? maxPoise : poise, 0f, maxPoise);
        }

        void Update()
        {
            // Poise regen
            if (Time.time >= _poiseRegenResumeAt && poise < maxPoise)
            {
                poise = Mathf.Min(maxPoise, poise + poiseRegenRate * Time.deltaTime);
                OnPoiseChanged?.Invoke(poise);
            }
        }

        /// <summary> Entry point used by hitboxes/projectiles. Returns true if the hit was applied. </summary>
        public bool ApplyHit(in HitInfo hit)
        {
            if (invulnerable || godMode) return false;
            if (hit.attacker && hit.attacker == transform) return false; // ignore self
            if (hit.team == team) return false; // friendly fire off (tune as needed)

            // Health/poise deltas
            float dmg = Mathf.Max(0f, hit.damage) * damageTakenMul;
            float pDmg = Mathf.Max(0f, hit.poiseDamage > 0f ? hit.poiseDamage : hit.damage) * poiseTakenMul;

            // Health
            float prevHP = health;
            if (!godMode) health = Mathf.Max(0f, health - dmg);
            OnHealthChanged?.Invoke(health);

            bool killed = health <= 0f;

            // Poise (unless super armor)
            bool poiseBroken = false;
            if (!superArmor && pDmg >= poiseFloor)
            {
                poise = Mathf.Max(0f, poise - pDmg);
                OnPoiseChanged?.Invoke(poise);
                _poiseRegenResumeAt = Time.time + poiseRegenDelay;
                poiseBroken = poise <= 0f;
            }

            // Notify listeners
            OnDamaged?.Invoke(hit);

            // Reactions
            if (reactions)
            {
                if (killed) reactions.ReactDeath(hit);
                else if (poiseBroken) reactions.ReactStagger(hit); // strong reaction
                else reactions.ReactHit(hit);                      // light reaction
            }

            if (killed) OnKilled?.Invoke(hit);

            // Reset poise after a successful stagger so next chain must build again (Souls-like)
            if (poiseBroken) poise = maxPoise;

            // Optional: global hitlag
            if (hit.hitlag > 0f) Hitstop.Do(hit.hitlag);

            return true;
        }

        // Utility API (for skills/buffs)
        public void GrantIFrames(float seconds) { if (seconds > 0f) StartCoroutine(CoIFrames(seconds)); }
        System.Collections.IEnumerator CoIFrames(float s)
        {
            invulnerable = true;
            yield return new WaitForSeconds(s);
            invulnerable = false;
        }

        public void GrantSuperArmor(float seconds) { if (seconds > 0f) StartCoroutine(CoSA(seconds)); }
        System.Collections.IEnumerator CoSA(float s)
        {
            superArmor = true;
            yield return new WaitForSeconds(s);
            superArmor = false;
        }
    }

    // Keep this aligned with the rest of your combat code
    [System.Serializable]
    public struct HitInfo
    {
        public string skillId;
        public string attackId;
        public float damage;
        public float poiseDamage; // optional override; if 0, falls back to damage
        public float hitstun;     // used by reactions for brief input lock or anim length
        public float hitlag;      // global time slow (Hitstop)
        public Vector3 knockback; // world-space impulse (m/s via VelocityChange)
        public Transform attacker;
        public Transform hitPoint;
        public Team team;
    }
}
