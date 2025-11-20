// ARPG/Combat/Runtime/ProjectileController.cs
using UnityEngine;
using System.Collections.Generic;
using ARPG.Core;
using GameFx.Core;
using GameFx.Core.PoolSystem;
using Cysharp.Threading.Tasks;

namespace ARPG.Combat
{
    public class ProjectileController : MonoBehaviour
    {
        private ProjectileData data;
        [SerializeField] GameObject hitEffect;

        Transform _attacker;
        Team _team;
        Vector3 _vel;
        float _dieAt;
        int _hits;
        readonly HashSet<Collider> _hitSet = new();
        GameObject _hitInstance;

        public void Init(ProjectileData d, Transform attacker, Team team, Vector3 dirWorld)
        {
            dirWorld.y = 0;
            data = d; _attacker = attacker; _team = team;
            _vel = dirWorld.normalized * data.speed;
            _dieAt = Time.time + data.lifetime;
            if (data.faceVelocity) transform.rotation = Quaternion.LookRotation(_vel);
        }

        void Update()
        {
            if (Time.time >= _dieAt) { ReturnToPool(); return; }

            if (data.gravity > 0f) _vel += Vector3.down * data.gravity * Time.deltaTime;

            transform.position += _vel * Time.deltaTime;
            if (data.faceVelocity && _vel.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(_vel);
        }

        void FixedUpdate()
        {
            var newPos = transform.position + _vel * Time.fixedDeltaTime;
            var delta = newPos - transform.position;
            var dist = delta.magnitude;

            if (dist > 0.01f)
            {
                if (Physics.SphereCast(transform.position, data.sweepRadius, delta.normalized,
                                       out RaycastHit hit, dist, data.hitMask, QueryTriggerInteraction.Collide))
                {
                    var other = hit.collider;
                    var hb = other.GetComponent<Hurtbox>();
                    if (!hb || !hb.owner || hb.owner.team == _team) return;
                    if (_hitSet.Contains(other)) return;

                    var info = BuildHitInfo(hb.owner.transform);
                    if (hb.owner.ApplyHit(info))
                    {
                        _hitInstance = ServiceLocator.Get<PoolManager>().GetPool(hitEffect).GetObject();
                        _hitInstance.transform.SetPositionAndRotation(hit.point, Quaternion.identity);
                        ReturnHitEffect(_hitInstance).Forget();
                        _hitSet.Add(other);
                        var rb = hb.owner.GetComponent<Rigidbody>();
                        if (rb) rb.AddForce(info.knockback, ForceMode.VelocityChange);
                        if (info.hitlag > 0f) Hitstop.Do(info.hitlag);
                        _hits++;
                        if (TryGetComponent<ISkillPostEffect>(out var postEffect))
                        {
                            postEffect.PostEffect(info);
                        }
                        if (_hits >= data.maxPenetration) ReturnToPool();
                    }
                }
            }
        }

        async UniTask ReturnHitEffect(GameObject instance)
        {
            await UniTask.WaitForSeconds(2);
            ServiceLocator.Get<PoolManager>().ReturnToPool(instance);
        }

        HitInfo BuildHitInfo(Transform target)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0f; if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
            dir.Normalize();

            return new HitInfo
            {
                damage = data.damage.damage,
                hitstun = data.damage.hitstun,
                hitlag = data.damage.hitlag,
                knockback = dir * data.damage.knockbackForce,
                attacker = _attacker,
                hitPoint = transform,
                attackId = data.damage.attackId
            };
        }

        void ReturnToPool()
        {
            _hitSet.Clear();
            _hits = 0;
            ServiceLocator.Get<PoolManager>().ReturnToPool(gameObject);
        }
    }
}
