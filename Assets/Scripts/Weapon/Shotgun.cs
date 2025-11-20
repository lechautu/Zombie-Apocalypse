using System.Collections;
using GameFx.Core;
using GameFx.Core.PoolSystem;
using UnityEngine;

namespace Weapon
{
    public class Shotgun : WeaponBase
    {
        public override void Shoot(EventDispatcher.EventArgs args)
        {
            StartCoroutine(FireRateCooldown());
            muzzleFlash.Play();
            shootSound.Play();
        }
    }
}