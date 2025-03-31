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

        //IK target left/right hands and elbows
        public Transform leftHandIKTarget;
        public Transform rightHandIKTarget;
        public Transform leftElbowIKTarget;
        public Transform rightElbowIKTarget;

        void OnEnable()
        {
            // Reset the canShoot flag when the weapon is enabled
            canShoot = true;
        }

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