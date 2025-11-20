using UnityEngine;

namespace ARPG.Combat
{
    public enum SkillCastRule { Any, GroundOnly, AirOnly }

    [CreateAssetMenu(fileName = "SkillData", menuName = "ARPG/Combat/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Identity")]
        public string skillId = "SkillId";                // unique, used by events & lookups

        [Header("Animation")]
        [Tooltip("Clip injected at runtime into the generic skill state (AnimatorOverrideController).")]
        public AnimationClip clip;
        [Tooltip("Generic state to play; must use SKILL_PLACEHOLDER as its motion.")]
        public string animatorState = "Skill_Generic";
        [Tooltip("Optional alternate state for aerial casting.")]
        public string animatorStateAir = "Skill_Generic_Air";
        public bool applyRootMotion = true;

        [Header("Cast Rules")]
        public SkillCastRule canCastWhen = SkillCastRule.Any;
        public float cooldown = 0.6f;

        [Header("Hitbox")]
        public MeleeData melee;        // for close-range skills
        public ProjectileData projectile;
        public AoeData aoe;
        public BuffData selfBuff;           // if set, skill applies a self-buff
        public Transform defaultSocketHint;  // optional, controller falls back to default socket

        [Header("Timing")]
        [Tooltip("1.0 = normal. >1 faster cast, <1 slower.")]
        public float castSpeed = 1f;
        
        [Header("Movement During Cast")]
        [Tooltip("If true, player motor keeps moving normally during the cast.")]
        public bool canMoveDuringCast = true;

        [Header("Upper-Body Casting")]
        [Tooltip("If true, play the clip on an upper-body layer while locomotion keeps running on base.")]
        public bool useUpperBodyLayer = false;

        [Tooltip("Animator layer index used for upper-body override (mask should be upper body).")]
        public int upperBodyLayerIndex = 1;

        [Tooltip("Animator state name on the upper-body layer (e.g., Skill_UpperBody).")]
        public string upperBodyState = "Skill_UpperBody";

        [Tooltip("Float parameter used as speed multiplier by the upper-body state (Animator state Speed->Multiplier).")]
        public string upperBodySpeedParam = "UpperBodySpeedMul";
    }
}
