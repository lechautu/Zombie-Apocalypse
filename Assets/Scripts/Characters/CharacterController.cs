using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Animation;
using StarterAssets;
using UnityEngine;
using Weapon;

namespace Characters
{
    /// <summary>
    /// Character root controller (health, weapon swap, IK wiring).
    /// Now also performs per-weapon yaw auto-calibration so the gun barrel
    /// aims exactly at the cursor when the facing system reads modelYawOffsetDeg.
    /// </summary>
    public class CharacterController : MonoBehaviour, IDamageable
    {
        [Header("Definition")]
        public ScriptableCharacter characterDefinition;

        [Header("Weapons")]
        public List<WeaponBase> weapons;
        private int _currentWeaponIndex = 0;

        [Header("Aim / Facing Integration")]
        [SerializeField] private Transform yawRoot;               // model root
        [SerializeField] private ThirdPersonController facingController;  // assign ThirdPersonController
        [SerializeField] private bool autoCalibrateYawOffset = true;

        [Header("Sockets")]
        [SerializeField] private Transform weaponSocket;

        [Header("Parallax Distances (m)")]
        [SerializeField] private float parallaxNear = 2.0f;
        [SerializeField] private float parallaxFar = 8.0f;

        private IAimFacing _aimFacing;

        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        private WeaponBase CurrentWeapon => weapons != null && weapons.Count > 0 ? weapons[_currentWeaponIndex] : null;

        private AnimatorIKHandler _animIK;

        private void OnEnable()
        {
            CurrentHealth = characterDefinition.maxHealth;
        }

        void Start()
        {
            _animIK = GetComponent<AnimatorIKHandler>();
            if (yawRoot == null) yawRoot = transform;
            _aimFacing = facingController;
            SwapWeapon();
        }

        public void SwapWeapon()
        {
            if (weapons == null || weapons.Count == 0) return;

            if (CurrentWeapon != null) CurrentWeapon.gameObject.SetActive(false);
            _currentWeaponIndex = (_currentWeaponIndex + 1) % weapons.Count;
            CurrentWeapon.gameObject.SetActive(true);

            // Push parallax config & socket to facing
            if (_aimFacing != null)
            {
                _aimFacing.SetWeaponSocket(weaponSocket);
                _aimFacing.SetParallaxDistances(parallaxNear, parallaxFar);
            }

            // Compute model yaw offset between body forward and barrel axis (world)
            if (_aimFacing != null && CurrentWeapon.weaponForwardRef != null)
            {
                float offsetDeg = Vector3.SignedAngle(
                    transform.forward,
                    CurrentWeapon.weaponForwardRef.forward,
                    Vector3.up
                );
                _aimFacing.SetModelYawOffset(offsetDeg);
            }

            if (_animIK != null)
            {
                _animIK.rightHandTarget = CurrentWeapon.rightHandIKTarget;
                _animIK.leftHandTarget = CurrentWeapon.leftHandIKTarget;
                _animIK.rightElbowHint = CurrentWeapon.rightElbowIKTarget;
                _animIK.leftElbowHint = CurrentWeapon.leftElbowIKTarget;
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;

            CurrentHealth -= damage;
            Debug.Log($"Current Health: {CurrentHealth}");
            if (IsDead)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            // TODO: death flow
        }
    }
}
