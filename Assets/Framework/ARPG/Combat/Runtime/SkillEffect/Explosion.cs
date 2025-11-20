using ARPG.Core;
using GameFx.Core;
using GameFx.Core.PoolSystem;
using UnityEngine;

namespace ARPG.Combat
{
    public class Explosion : MonoBehaviour, ISkillPostEffect
    {
        [SerializeField] private AoeData data;

        public void PostEffect(HitInfo hitInfo)
        {
            var go = ServiceLocator.Get<PoolManager>().GetPool(data.prefab.gameObject).GetObject();
            go.GetComponent<AoeZone>().Init(data, hitInfo.attacker, hitInfo.attacker.GetComponent<Damageable>().team, hitInfo.hitPoint.position, Quaternion.identity, null);
        }
    }
}