using UnityEngine;
using System.Collections;
using Characters;
using System;

namespace Weapon
{
    public class Pistol : WeaponBase
    {
        public Light muzzleLight;

        public override void Shoot()
        {
            if (!canShoot) return;

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
    }
}