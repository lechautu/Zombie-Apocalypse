using System.Collections.Generic;
using Characters.Animation;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapon;

public class SwapWeapon : MonoBehaviour
{
    [Header("Weapons")]
    public List<WeaponBase> weapons;
    private int _currentWeaponIndex = 0;

    private WeaponBase CurrentWeapon => weapons != null && weapons.Count > 0 ? weapons[_currentWeaponIndex] : null;
    private AnimatorIKHandler _animIK;

    void Start()
    {
        _animIK = GetComponent<AnimatorIKHandler>();
        Swap();
    }

    public void Swap()
    {
        if (weapons == null || weapons.Count == 0) return;

        if (CurrentWeapon != null) CurrentWeapon.gameObject.SetActive(false);
        _currentWeaponIndex = (_currentWeaponIndex + 1) % weapons.Count;
        CurrentWeapon.gameObject.SetActive(true);

        if (_animIK != null)
        {
            _animIK.SetIKPosition(CurrentWeapon.leftHandIKTarget, CurrentWeapon.leftElbowIKTarget);
        }
    }
}
