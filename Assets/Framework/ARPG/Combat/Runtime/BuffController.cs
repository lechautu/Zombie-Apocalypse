// ARPG/Combat/Runtime/BuffController.cs
using UnityEngine;
using System.Collections.Generic;
using ARPG.Core;

namespace ARPG.Combat
{
    [DisallowMultipleComponent]
    public class BuffController : MonoBehaviour
    {
        class Active
        {
            public BuffData data;
            public int stacks = 1;
            public float until;
        }

        public TopDownCharacterMotor motor;
        public Damageable damageable;

        readonly Dictionary<string, Active> _actives = new();

        void Awake()
        {
            if (!motor) motor = GetComponent<TopDownCharacterMotor>();
            if (!damageable) damageable = GetComponent<Damageable>();
        }

        public void Apply(BuffData buff)
        {
            if (buff == null) return;

            if (_actives.TryGetValue(buff.buffId, out var a))
            {
                switch (buff.stackPolicy)
                {
                    case BuffStackPolicy.RefreshDuration:
                        a.until = Time.time + buff.duration;
                        break;
                    case BuffStackPolicy.StackIntensity:
                        a.stacks = Mathf.Min(buff.maxStacks, a.stacks + 1);
                        a.until = Time.time + buff.duration;
                        break;
                    case BuffStackPolicy.IgnoreIfActive:
                        return;
                }
            }
            else
            {
                a = new Active { data = buff, stacks = 1, until = Time.time + buff.duration };
                _actives.Add(buff.buffId, a);
            }
            RecomputeStats();
        }

        void Update()
        {
            bool changed = false;
            var keys = new List<string>(_actives.Keys);
            foreach (var k in keys)
            {
                if (Time.time >= _actives[k].until) { _actives.Remove(k); changed = true; }
            }
            if (changed) RecomputeStats();
        }

        void RecomputeStats()
        {
            // Reset to base (you can store base values elsewhere if you allow runtime changes)
            if (motor)
            {
                // Suppose your CharacterMotor exposes base values via public fields you won't edit elsewhere.
                float runBase = motor.runSpeed;
                float moveBase = motor.moveSpeed;

                float moveMult = 1f;
                foreach (var a in _actives.Values)
                {
                    float m = Mathf.Pow(a.data.moveSpeedMultiplier, a.stacks);
                    moveMult *= m;
                }
                motor.runSpeed = runBase * moveMult;
                motor.moveSpeed = moveBase * moveMult;
            }

            if (damageable)
            {
                bool invul = false;
                float dmgMult = 1f;
                foreach (var a in _actives.Values)
                {
                    if (a.data.grantInvulnerability) invul = true;
                    dmgMult *= Mathf.Pow(a.data.damageTakenMultiplier, a.stacks);
                }
                damageable.invulnerable = invul;
                // For damageTakenMultiplier you can either multiply inside Damageable.ApplyHit
                // or store it here and expose a getter. Minimal route below:
                _damageMultiplier = dmgMult;
            }
        }

        // Minimal hook for Damageable to query damage multiplier
        float _damageMultiplier = 1f;
        public float GetDamageTakenMultiplier() => _damageMultiplier;
    }
}
