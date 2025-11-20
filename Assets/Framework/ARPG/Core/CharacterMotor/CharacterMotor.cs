using UnityEngine;

namespace ARPG.Core
{
    public class CharacterMotor : MonoBehaviour
    {
        [HideInInspector] public Vector2 moveInput;
        [HideInInspector] public Vector2 lookInput;
        [HideInInspector] public bool runHeld;
        [HideInInspector] public bool dashPressed;

        protected static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}