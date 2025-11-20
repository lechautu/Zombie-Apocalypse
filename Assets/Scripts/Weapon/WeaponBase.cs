using System.Collections;
using ARPG.Combat;
using ARPG.Core;
using GameFx.Core;
using GameFx.Core.PoolSystem;
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
        public SkillData shootingSkill;
        public bool isHandledByAI;

        protected ARPGPlayerInput _input;
        protected bool isOnCooldown;

        //IK target left/right hands and elbows
        public Transform leftHandIKTarget;
        public Transform rightHandIKTarget;
        public Transform leftElbowIKTarget;
        public Transform rightElbowIKTarget;

        SkillLoadout _skillLoadout;

        void OnEnable()
        {
            _skillLoadout = GetComponentInParent<SkillLoadout>();
            _skillLoadout.slotFire = shootingSkill;

            ServiceLocator.Get<EventDispatcher>().Subscribe(EventConstants.OnPlayerFire, Shoot);
        }

        void OnDisable()
        {
            ServiceLocator.Get<EventDispatcher>().Unsubscribe(EventConstants.OnPlayerFire, Shoot);
        }

        void Start()
        {
            // Find StarterAssetsInputs on the player
            _input = GetComponentInParent<ARPGPlayerInput>();
        }

        public abstract void Shoot(EventDispatcher.EventArgs args);

        protected IEnumerator FireRateCooldown()
        {
            isOnCooldown = true;
            yield return new WaitForSeconds(weaponData.fireRate);
            isOnCooldown = false;
        }
    }
}