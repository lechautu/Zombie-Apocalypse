using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.Animation;
using UnityEngine;
using Weapon;

namespace Characters
{
    public class CharacterController : MonoBehaviour, IDamageable
    {
        public ScriptableCharacter characterDefinition;

        public List<WeaponBase> weapons;
        private int _currentWeaponIndex = 0;
        
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;        

        private AnimatorIKHandler _animatorIKHandler;

        private void OnEnable()
        {
            CurrentHealth = characterDefinition.maxHealth;
        }

        private void Start()
        {
            _animatorIKHandler = GetComponent<AnimatorIKHandler>();
            SwapWeapon();
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return; // Prevent taking damage if already dead

            CurrentHealth -= damage;
            Debug.Log($"Current Health: {CurrentHealth}");
            if (IsDead)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        public void SwapWeapon()
        {
            if (weapons.Count == 0) return;

            weapons[_currentWeaponIndex].gameObject.SetActive(false);
            _currentWeaponIndex = (_currentWeaponIndex + 1) % weapons.Count;
            weapons[_currentWeaponIndex].gameObject.SetActive(true);

            if (_animatorIKHandler != null)
            {
                _animatorIKHandler.rightHandTarget = weapons[_currentWeaponIndex].rightHandIKTarget;
                _animatorIKHandler.rightElbowHint = weapons[_currentWeaponIndex].rightElbowIKTarget;
                _animatorIKHandler.leftHandTarget = weapons[_currentWeaponIndex].leftHandIKTarget;
                _animatorIKHandler.leftElbowHint = weapons[_currentWeaponIndex].leftElbowIKTarget;
            }
        }

        private void Die()
        {
            
        }
    }
}
