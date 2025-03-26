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
            float spreadX = Random.Range(-weaponData.spreadAngle, weaponData.spreadAngle);
            float spreadY = Random.Range(-weaponData.spreadAngle, weaponData.spreadAngle);

            Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);

            Bullet bullet = BulletPool.Instance.GetBullet();
            bullet.transform.SetPositionAndRotation(muzzlePoint.position, muzzlePoint.rotation * spreadRotation);
            bullet.SetDamage(weaponData.damage / weaponData.pelletsPerShot);
        }

    }
}