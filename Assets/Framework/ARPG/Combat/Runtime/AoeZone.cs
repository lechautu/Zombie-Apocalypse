using UnityEngine;
using System.Collections.Generic;
using ARPG.Core;
using GameFx.Core;
using GameFx.Core.PoolSystem;

namespace ARPG.Combat
{
    public class AoeZone : MonoBehaviour
    {
        AoeData data;
        Transform attacker;
        Team team;

        Transform _follow;
        float _endAt;
        float _nextTick;
        readonly HashSet<Damageable> _seenThisTick = new();

        public void Init(AoeData d, Transform atk, Team t, Vector3 pos, Quaternion rot, Transform follow)
        {
            data = d; attacker = atk; team = t;
            _follow = data.followAnchor ? follow : null;
            transform.SetPositionAndRotation(pos, rot);
            _endAt = Time.time + data.duration;
            _nextTick = Time.time; // first tick immediately
        }

        void Update()
        {
            if (_follow) transform.position = _follow.position;
            if (Time.time >= _endAt) { ServiceLocator.Get<PoolManager>().ReturnToPool(gameObject); return; }
            if (Time.time >= _nextTick) { _nextTick += data.tickInterval; Tick(); }
        }

        void Tick()
        {
            _seenThisTick.Clear();
            var cols = Physics.OverlapSphere(transform.position, data.radius, data.hitMask, QueryTriggerInteraction.Collide);
            foreach (var c in cols)
            {
                var hb = c.GetComponent<Hurtbox>();
                if (!hb || !hb.owner || hb.owner.team == team) continue;
                if (_seenThisTick.Contains(hb.owner)) continue;
                _seenThisTick.Add(hb.owner);

                var info = BuildHitInfo(hb.owner.transform);
                if (hb.owner.ApplyHit(info) && info.hitlag > 0f)
                    Hitstop.Do(info.hitlag * 0.5f); // AoE ticks feel better with lighter stop
            }
        }

        HitInfo BuildHitInfo(Transform target)
        {
            Vector3 dir = (target.position - transform.position);
            dir.y = 0f; if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

            return new HitInfo
            {
                attackId = data.damage.attackId,
                damage = data.damage.damage,
                hitstun = data.damage.hitstun,
                hitlag = data.damage.hitlag,
                knockback = dir * data.damage.knockbackForce,
                attacker = attacker,
                hitPoint = transform
            };
        }

        void OnDrawGizmosSelected()
        {
            if (!data) return;
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.2f);
            Gizmos.DrawSphere(transform.position, data.radius);
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 1f);
            Gizmos.DrawWireSphere(transform.position, data.radius);
        }
    }
}
