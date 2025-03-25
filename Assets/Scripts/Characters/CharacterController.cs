using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Characters
{
    public class CharacterController : IDamagable
    {
        public ScriptableCharacter characterDefinition;
        public int CurrentHealth { get; private set; }

        private void OnEnable()
        {
            CurrentHealth = characterDefinition.maxHealth;
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            
        }
    }
}
