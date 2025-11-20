using UnityEngine;
using System.Collections.Generic;
using ARPG.Core;
using GameFx.Core;
using GameFx.Core.PoolSystem; // uses Damageable; CharacterMotor for grounded checks

namespace ARPG.Combat
{
    [RequireComponent(typeof(Animator))]
    public class SkillController : MonoBehaviour
    {
        public enum GcdMode { LockUntilSkillExit, FixedDuration }

        [Header("Refs")]
        public Animator animator;
        public TopDownCharacterMotor motor;
        public Damageable selfDamageable;
        public Transform defaultWeaponSocket;
        public SkillLoadout loadout;
        public BuffController buffController;

        [Header("Input")]
        public bool allowSkillInput = true;

        [Header("Global Cooldown / Lock")]
        public GcdMode gcdMode = GcdMode.LockUntilSkillExit;
        [Tooltip("Used only when GcdMode = FixedDuration.")]
        public float globalCooldown = 0.35f;

        [Header("Root Motion")]
        [Tooltip("Animator.applyRootMotion to use when NOT casting a skill.")]
        public bool defaultApplyRootMotion = false;

        [Header("Animation clip names")]
        public string skillPlaceholder = "SKILL_PLACEHOLDER";
        public string skillPlaceholderUpper = "SKILL_PLACEHOLDER_UPPER";

        [Header("Debug")]
        public bool drawActiveHitbox = true;

        // --- Runtime ---
        readonly Dictionary<string, SkillData> _byId = new();
        readonly Dictionary<SkillData, float> _cooldownsUntil = new();

        HitboxRuntime _activeBox;
        SkillData _currentSkill;
        bool _inSkill;
        bool _skillLockActive;
        float _gcdUntil = -1f;

        int _baseLayer = 0;
        bool _savedDefaultRM;
        float _savedAnimatorSpeed = 1f;
        AnimationClip _upperPlaceholderClip; // SKILL_PLACEHOLDER_UPPER
        bool _usingUpperBody;

        AnimatorOverrideController _aoc;
        AnimationClip _placeholderClip; // SKILL_PLACEHOLDER

        void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
            if (!motor) motor = GetComponent<TopDownCharacterMotor>();
            if (!selfDamageable) selfDamageable = GetComponent<Damageable>();
            if (!loadout) loadout = GetComponent<SkillLoadout>();

            // Per-instance override controller
            _aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = _aoc;

            _placeholderClip = FindPlaceholderClip(_aoc, skillPlaceholder);
            _upperPlaceholderClip = FindPlaceholderClip(_aoc, skillPlaceholderUpper);

            Index(loadout?.slot1);
            Index(loadout?.slot2);
            Index(loadout?.slot3);
            Index(loadout?.slot4);

            _savedDefaultRM = defaultApplyRootMotion;
            animator.applyRootMotion = _savedDefaultRM;
        }

        void Update()
        {
            if (_activeBox != null && _activeBox.IsActive) _activeBox.Tick();
            if (gcdMode == GcdMode.FixedDuration && _skillLockActive && Time.time >= _gcdUntil)
                _skillLockActive = false; // unlock when fixed GCD expires
        }

        void OnDrawGizmosSelected()
        {
            if (!drawActiveHitbox) return;
            if (_activeBox != null && _activeBox.IsActive) _activeBox.DrawGizmo(Color.yellow);
        }

        AnimationClip FindPlaceholderClip(AnimatorOverrideController aoc, string keyName)
        {
            var list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            aoc.GetOverrides(list);
            foreach (var pair in list)
                if (pair.Key && pair.Key.name == keyName) return pair.Key;
            Debug.LogWarning($"SkillController: Placeholder clip '{keyName}' not found. Ensure Skill_Generic uses it.");
            return null;
        }

        void Index(SkillData s)
        {
            if (!s || !s.clip) return;
            if (!_byId.ContainsKey(s.skillId)) _byId.Add(s.skillId, s);
        }

        // --------- INPUT API (called by SkillInput) ----------
        public void CastSlotFire() => TryCast(loadout?.slotFire);
        public void CastSlot1() => TryCast(loadout?.slot1);
        public void CastSlot2() => TryCast(loadout?.slot2);
        public void CastSlot3() => TryCast(loadout?.slot3);
        public void CastSlot4() => TryCast(loadout?.slot4);
        public void CastById(string skillId) { if (_byId.TryGetValue(skillId, out var s)) TryCast(s); }

        bool CanStartSkill(SkillData s)
        {
            if (!allowSkillInput || s == null || s.clip == null) return false;

            // Global lock / GCD
            if (_skillLockActive) return false;
            if (gcdMode == GcdMode.FixedDuration && Time.time < _gcdUntil) return false;

            // Per-skill cooldown
            _cooldownsUntil.TryGetValue(s, out float cdUntil);
            if (Time.time < cdUntil) return false;

            // Ground/Air rules
            switch (s.canCastWhen)
            {
                case SkillCastRule.GroundOnly: if (!motor || !motor.IsGrounded) return false; break;
                case SkillCastRule.AirOnly: if (!motor || motor.IsGrounded) return false; break;
            }

            // No self-interrupt during current skill
            if (_inSkill) return false;
            return true;
        }

        void TryCast(SkillData s)
        {
            if (!CanStartSkill(s)) return;

            // Movement lock policy: we *don't* freeze motor if canMoveDuringCast is true.
            // (Your motor already moves based on input; we just avoid changing root motion)
            if (!s.useUpperBodyLayer)
                animator.applyRootMotion = s.applyRootMotion;
            else
                animator.applyRootMotion = _savedDefaultRM; // keep locomotion root control

            // Inject clip into the proper placeholder
            if (_placeholderClip == null)
            {
                Debug.LogWarning("SkillController: Missing SKILL_PLACEHOLDER; abort cast.");
                animator.applyRootMotion = _savedDefaultRM;
                return;
            }

            // FULL-BODY path (same as before)
            if (!s.useUpperBodyLayer)
            {
                _usingUpperBody = false;
                _aoc[_placeholderClip] = s.clip;

                string targetState = (!motor || motor.IsGrounded || string.IsNullOrEmpty(s.animatorStateAir))
                    ? s.animatorState
                    : s.animatorStateAir;

                // Scale whole animator (full-body) by castSpeed
                _savedAnimatorSpeed = animator.speed;
                animator.speed = Mathf.Max(0.01f, s.castSpeed);

                animator.CrossFadeInFixedTime(targetState, 0.05f, _baseLayer);
            }
            else
            {
                // UPPER-BODY path
                _usingUpperBody = true;

                if (_upperPlaceholderClip == null)
                {
                    Debug.LogWarning("SkillController: Missing SKILL_PLACEHOLDER_UPPER; abort upper-body cast.");
                    return;
                }

                _aoc[_upperPlaceholderClip] = s.clip;

                // Raise the upper layer weight (instantly for prototype)
                int L = Mathf.Max(1, s.upperBodyLayerIndex);
                animator.SetLayerWeight(L, 1f);

                // Set per-state speed via parameter (Animator state's Speed->Multiplier)
                if (!string.IsNullOrEmpty(s.upperBodySpeedParam) &&
                    animator.HasParameterOfType(s.upperBodySpeedParam, AnimatorControllerParameterType.Float))
                {
                    animator.SetFloat(s.upperBodySpeedParam, Mathf.Max(0.01f, s.castSpeed));
                }

                // Crossfade only that layer to the upper-body state
                animator.CrossFadeInFixedTime(s.upperBodyState, 0.05f, L);
            }

            // Lock/GCD & bookkeeping (unchanged)
            _currentSkill = s;
            _inSkill = true;
            _skillLockActive = true;
            if (gcdMode == GcdMode.FixedDuration && globalCooldown > 0f)
                _gcdUntil = Time.time + globalCooldown;

            if (s.cooldown > 0f) _cooldownsUntil[s] = Time.time + s.cooldown;

            if (s == loadout.slotFire)
            {
                ServiceLocator.Get<EventDispatcher>().Dispatch(EventConstants.OnPlayerFire, s.skillId);
            }
        }

        // --------- Animation Events on the skill clips ----------
        public void AE_SkillSpawnHitbox()
        {
            var s = _currentSkill;
            if (s == null || s.melee == null) return;

            Transform socket = s.defaultSocketHint ? s.defaultSocketHint
                             : (defaultWeaponSocket ? defaultWeaponSocket : transform);

            _activeBox = new HitboxRuntime(
                socket,
                s.melee,
                selfDamageable ? selfDamageable.team : Team.Player,
                transform
            );
        }

        public void AE_SkillEndHitbox()
        {
            _activeBox = null;
        }

        public void AE_SkillSpawnProjectile()
        {
            var s = _currentSkill;
            if (s == null || s.projectile == null || s.projectile.prefab == null) return;

            Transform socket = s.defaultSocketHint ? s.defaultSocketHint
                             : (defaultWeaponSocket ? defaultWeaponSocket : transform);

            var proj = ServiceLocator.Get<PoolManager>().GetPool(s.projectile.prefab.gameObject).GetObject();
            proj.transform.SetPositionAndRotation(socket.TransformPoint(s.projectile.localMuzzleOffset), socket.rotation);

            Vector3 dir = (s.projectile.inheritCasterForward ? socket.forward : transform.forward);
            proj.GetComponent<ProjectileController>().Init(s.projectile, transform, selfDamageable ? selfDamageable.team : Team.Player, dir);
        }

        public void AE_SkillSpawnAoe()
        {
            var s = _currentSkill;
            if (s == null || s.aoe == null || s.aoe.prefab == null) return;

            Vector3 pos; Quaternion rot = Quaternion.identity; Transform follow = null;

            switch (s.aoe.anchor)
            {
                case AoeAnchor.CasterSelf:
                    pos = transform.position;
                    follow = transform;
                    break;
                case AoeAnchor.LockOnTarget:
                    // TODO: plug in your lock-on target position here
                    pos = transform.position;
                    break;
                default: // GroundPointInFront
                    pos = transform.position + transform.forward * s.aoe.forwardDistance;
                    break;
            }

            var zone = Instantiate(s.aoe.prefab);
            zone.Init(s.aoe, transform, selfDamageable ? selfDamageable.team : Team.Player, pos, rot, follow);
        }

        public void AE_SkillApplySelfBuff()
        {
            var s = _currentSkill;
            if (s == null || s.selfBuff == null || !buffController) return;
            buffController.Apply(s.selfBuff);
        }

        // --------- State hooks (from SkillStateFlag SMB) ----------
        public void NotifySkillEnter() { _inSkill = true; /* lock already set */ }

        public void NotifySkillExit()
        {
            _inSkill = false;
            var s = _currentSkill;
            _currentSkill = null;

            if (_usingUpperBody && s != null)
            {
                int L = Mathf.Max(1, s.upperBodyLayerIndex);
                animator.SetLayerWeight(L, 0f);

                if (!string.IsNullOrEmpty(s.upperBodySpeedParam) &&
                    animator.HasParameterOfType(s.upperBodySpeedParam, AnimatorControllerParameterType.Float))
                {
                    animator.SetFloat(s.upperBodySpeedParam, 1f);
                }
                // Do NOT touch animator.speed for upper-body path
            }
            else
            {
                // Full-body: restore animator speed
                animator.speed = _savedAnimatorSpeed;
            }

            if (gcdMode == GcdMode.LockUntilSkillExit) _skillLockActive = false;
            else if (Time.time >= _gcdUntil) _skillLockActive = false;

            animator.applyRootMotion = _savedDefaultRM;
        }

        public void ForceInterruptSkills()
        {
            var s = _currentSkill;

            _inSkill = false; _skillLockActive = false; _gcdUntil = -1f;
            _currentSkill = null; _activeBox = null;

            if (_usingUpperBody && s != null)
            {
                int L = Mathf.Max(1, s.upperBodyLayerIndex);
                animator.SetLayerWeight(L, 0f);

                if (!string.IsNullOrEmpty(s.upperBodySpeedParam) &&
                    animator.HasParameterOfType(s.upperBodySpeedParam, AnimatorControllerParameterType.Float))
                {
                    animator.SetFloat(s.upperBodySpeedParam, 1f);
                }
            }
            else
            {
                animator.speed = _savedAnimatorSpeed;
            }

            animator.applyRootMotion = _savedDefaultRM;
        }
    }
}
