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
        public AudioSource shootSound;
        public Transform weaponForwardRef;
        public bool isHandledByAI;

        protected StarterAssetsInputs _input;
        protected bool isOnCooldown;

        //IK target left/right hands and elbows
        public Transform leftHandIKTarget;
        public Transform rightHandIKTarget;
        public Transform leftElbowIKTarget;
        public Transform rightElbowIKTarget;

        void Start()
        {
            // Find StarterAssetsInputs on the player
            _input = GetComponentInParent<StarterAssetsInputs>();
        }

        void Update()
        {
            if (CanShoot() && !isHandledByAI)
            {
                Shoot();
            }
        }

        public abstract void Shoot();
        public abstract bool CanShoot();

        protected IEnumerator FireRateCooldown()
        {
            isOnCooldown = true;
            yield return new WaitForSeconds(weaponData.fireRate);
            isOnCooldown = false;
        }
    }
}