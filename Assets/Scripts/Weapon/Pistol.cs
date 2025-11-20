using UnityEngine;
using System.Collections;
using Characters;
using System;
using GameFx.Core.PoolSystem;
using GameFx.Core;

namespace Weapon
{
    public class Pistol : WeaponBase
    {
        [Header("VFX Effects")]
        public Light muzzleLight;

        public override void Shoot(EventDispatcher.EventArgs args)
        {
            StartCoroutine(FireRateCooldown());
            muzzleFlash.Play();
            muzzleLight.enabled = true;
            shootSound.Play();
            StartCoroutine(MuzzleLightCooldown());
        }

        private IEnumerator MuzzleLightCooldown()
        {
            yield return new WaitForSeconds(0.1f);
            muzzleLight.enabled = false;
        }
    }
}