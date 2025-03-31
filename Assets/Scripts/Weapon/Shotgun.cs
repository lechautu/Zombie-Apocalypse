using System.Collections;
using UnityEngine;

namespace Weapon
{
    public class Shotgun : WeaponBase
    {
        public override void Shoot()
        {
            if (!canShoot) return;

            StartCoroutine(FireRateCooldown());
            muzzleFlash.Play();

            for (int i = 0; i < weaponData.pelletsPerShot; i++)
            {
                FirePellet();
            }
        }

        void FirePellet()
        {
            float spreadY = Random.Range(-weaponData.spreadAngle, weaponData.spreadAngle);

            Bullet bullet = BulletPool.Instance.GetBullet();
            Quaternion rotation = Quaternion.Euler(muzzlePoint.rotation.eulerAngles.x, muzzlePoint.rotation.eulerAngles.y + spreadY, 0);
            bullet.transform.SetPositionAndRotation(muzzlePoint.position, rotation);
            bullet.SetDamage(weaponData.damage / weaponData.pelletsPerShot);
        }
    }
}