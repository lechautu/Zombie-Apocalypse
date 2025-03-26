using UnityEngine;
using System.Collections;
using Characters;

namespace Weapon
{
    public class Pistol : WeaponBase
    {
        public override void Shoot()
        {
            if (!canShoot) return;

            StartCoroutine(FireRateCooldown());
            muzzleFlash.Play();

            Bullet bullet = BulletPool.Instance.GetBullet();
            bullet.transform.SetPositionAndRotation(muzzlePoint.position, muzzlePoint.rotation);
            bullet.SetDamage(weaponData.damage);
        }

    }
}