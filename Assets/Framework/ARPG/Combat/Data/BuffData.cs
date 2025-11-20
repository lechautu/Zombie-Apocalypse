// ARPG/Combat/Data/BuffData.cs
using UnityEngine;

namespace ARPG.Combat
{
    public enum BuffStackPolicy { RefreshDuration, StackIntensity, IgnoreIfActive }

    [CreateAssetMenu(fileName = "BuffData", menuName = "ARPG/Combat/Buff Data")]
    public class BuffData : ScriptableObject
    {
        [Header("Meta")]
        public string buffId = "";

        [Header("Duration / Stacking")]
        public float duration = 6f;
        public BuffStackPolicy stackPolicy = BuffStackPolicy.RefreshDuration;
        public int maxStacks = 3;

        [Header("Effects (prototype)")]
        [Range(0.1f, 3f)] public float moveSpeedMultiplier = 1.2f; // multiply CharacterMotor.runSpeed & moveSpeed
        [Range(0f, 1f)] public float damageTakenMultiplier = 1.0f; // 0.8 = 20% DR
        public bool grantInvulnerability = false;
    }
}
