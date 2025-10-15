using UnityEngine;
using System.Collections;
using Characters;
using System;
using Characters.AI;
using Enemy.Behaviors;

namespace Weapon
{
    public class Drone : WeaponBase, IHasTarget
    {
        public Light muzzleLight;

        private ZombieAI _currentTarget;

        public override void Shoot()
        {
            StartCoroutine(FireRateCooldown());
            muzzleFlash.Play();
            muzzleLight.enabled = true;
            shootSound.Play();
            StartCoroutine(MuzzleLightCooldown());

            Bullet bullet = BulletPool.Instance.GetBullet();
            bullet.transform.SetPositionAndRotation(muzzlePoint.position, muzzlePoint.rotation);
            bullet.SetDamage(weaponData.damage);
        }

        private IEnumerator MuzzleLightCooldown()
        {
            yield return new WaitForSeconds(0.1f);
            muzzleLight.enabled = false;
        }

        public override bool CanShoot()
        {
            return !isOnCooldown && _currentTarget != null && !_currentTarget.IsDead;
        }

        public void SetTarget(ZombieAI target)
        {
            _currentTarget = target;
        }

        public ZombieAI GetTarget()
        {
            return _currentTarget;
        }
    }
}