// ARPG/Combat/Runtime/HitboxRuntime.cs
using UnityEngine;
using System.Collections.Generic;
using ARPG.Core;

namespace ARPG.Combat
{
    public class HitboxRuntime
    {
        readonly Transform _socket;
        readonly MeleeData _data;
        readonly Team _team;
        readonly Transform _attacker;
        readonly HashSet<Collider> _hit = new();
        float _endTime;

        public bool IsActive => Time.time < _endTime;

        public HitboxRuntime(Transform socket, MeleeData data, Team team, Transform attacker)
        {
            _socket = socket; _data = data; _team = team; _attacker = attacker;
            _endTime = Time.time + _data.shape.duration;
        }

        public void Tick()
        {
            if (!IsActive) return;

            Vector3 center = _socket.TransformPoint(_data.shape.localOffset);
            Vector3 half = _data.shape.boxSize * 0.5f;
            Quaternion rot = _socket.rotation;

            var cols = Physics.OverlapBox(center, half, rot, _data.shape.hitMask, QueryTriggerInteraction.Collide);
            foreach (var c in cols)
            {
                if (!_data.shape.multiHit && _hit.Contains(c)) continue;

                var hb = c.GetComponent<Hurtbox>();
                if (!hb || !hb.owner || hb.owner.team == _team) continue;

                var motor = hb.owner.GetComponent<TopDownCharacterMotor>();
                if (motor && motor.IsInvulnerable()) continue;

                var info = BuildHitInfo(hb.owner.transform);
                if (hb.owner.ApplyHit(info))
                {
                    _hit.Add(c);
                    var rb = hb.owner.GetComponent<Rigidbody>();
                    if (rb) rb.AddForce(info.knockback, ForceMode.VelocityChange);
                    if (info.hitlag > 0f) Hitstop.Do(info.hitlag);
                }
            }
        }

        HitInfo BuildHitInfo(Transform target)
        {
            Vector3 dir = (target.position - _attacker.position);
            dir.y = 0f; if (dir.sqrMagnitude < 0.0001f) dir = _attacker.forward;
            dir.Normalize();

            return new HitInfo
            {
                damage = _data.damage.damage,
                hitstun = _data.damage.hitstun,
                hitlag = _data.damage.hitlag,
                knockback = dir * _data.damage.knockbackForce,
                attacker = _attacker,
                hitPoint = _socket,
                attackId = _data.damage.attackId
            };
        }

        public void DrawGizmo(Color c)
        {
            Vector3 center = _socket.TransformPoint(_data.shape.localOffset);
            Vector3 half = _data.shape.boxSize * 0.5f;
            Quaternion rot = _socket.rotation;

            Gizmos.color = new Color(c.r, c.g, c.b, 0.2f);
            Gizmos.matrix = Matrix4x4.TRS(center, rot, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, half * 2f);
            Gizmos.color = c;
            Gizmos.DrawWireCube(Vector3.zero, half * 2f);
        }
    }
}
