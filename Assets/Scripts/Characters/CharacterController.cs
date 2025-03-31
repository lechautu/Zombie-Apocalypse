using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Characters
{
    public class CharacterController : MonoBehaviour, IDamageable
    {
        public ScriptableCharacter characterDefinition;
        
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;        

        private void OnEnable()
        {
            CurrentHealth = characterDefinition.maxHealth;
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

        private void Die()
        {
            
        }
    }
}
