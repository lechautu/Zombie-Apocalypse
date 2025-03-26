using System.Collections;
using StarterAssets;
using UnityEngine;

namespace Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        public ScriptableWeapon weaponData;
        public Transform muzzlePoint;
        public ParticleSystem muzzleFlash;

        protected StarterAssetsInputs _input;
        protected bool canShoot = true;

        void Start()
        {
            // Find StarterAssetsInputs on the player
            _input = GetComponentInParent<StarterAssetsInputs>();
        }

        void Update()
        {
            if (_input != null && _input.fire && canShoot)
            {
                Shoot();
            }
        }

        public abstract void Shoot();

        protected IEnumerator FireRateCooldown()
        {
            canShoot = false;
            yield return new WaitForSeconds(weaponData.fireRate);
            canShoot = true;
        }
    }
}