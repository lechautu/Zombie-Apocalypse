using System;
using UnityEngine;

namespace Weapon
{
    [Serializable, CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/New Weapon")]
    public class ScriptableWeapon : ScriptableObject
    {
        public string weaponName;
        public float damage;
        public float fireRate;

        [Header("Shotgun Settings")]
        public int pelletsPerShot; // 1 for single-shot weapons, more for shotguns
        public float spreadAngle;  // Only relevant for shotguns
    }
}