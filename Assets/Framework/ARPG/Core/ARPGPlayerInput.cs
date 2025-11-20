using ARPG.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPG.Core
{
    public class ARPGPlayerInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private LockOnTarget lockOnSystem;
        [SerializeField] private SkillController skillController;
        [SerializeField] private SwapWeapon swapWeapon;

        [Header("Cursor Lock")]
        [SerializeField] private bool lockCursor;

        public bool fire;

        void Start()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = true;
            }
        }

        void OnMove(InputValue value) => MoveInput(value.Get<Vector2>());
        void OnLook(InputValue value) => LookInput(value.Get<Vector2>());
        void OnRun(InputValue value) => motor.runHeld = value.isPressed;
        void OnSwap(InputValue value) { if (value.isPressed) SwapWeapon(); }
        
        void OnLockToggle(InputValue value) { if (value.isPressed) lockOnSystem.toggleLockPressed = true; }
        void OnLockCycleLeft(InputValue value) { if (value.isPressed) lockOnSystem.cycleLeftPressed = true; }
        void OnLockCycleRight(InputValue value) { if (value.isPressed) lockOnSystem.cycleRightPressed = true; }

        void OnFire(InputValue v) { if (v.isPressed) skillController.CastSlotFire(); }
        void OnSkillSlot1(InputValue v) { if (v.isPressed) skillController.CastSlot1(); }
        void OnSkillSlot2(InputValue v) { if (v.isPressed) skillController.CastSlot2(); }
        void OnSkillSlot3(InputValue v) { if (v.isPressed) skillController.CastSlot3(); }
        void OnSkillSlot4(InputValue v) { if (v.isPressed) skillController.CastSlot4(); }

        public void MoveInput(Vector2 input)
        {
            motor.moveInput = input;
        }

        public void LookInput(Vector2 input)
        {
            motor.lookInput = input;
        }

        public void SwapWeapon()
        {
            swapWeapon.Swap();
        }

#if !UNITY_EDITOR
        void Update()
        {
            // Auto-fire while look stick is held
            // (SkillController should handle cooldown / attack rate)
            if (motor.lookInput.sqrMagnitude >= 0.5f)
            {
                skillController.CastSlotFire();
            }
        }
#endif
    }
}
