using ARPG.Core;
using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        [Header("Output")]
        public ARPGPlayerInput starterAssetsInputs;

        [Header("Refs")]
        [SerializeField] GameObject explosiveButton;
        [SerializeField] GameObject regularBulletButton;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualSwapInput()
        {
            starterAssetsInputs.SwapWeapon();
            regularBulletButton.SetActive(!regularBulletButton.activeSelf);
            explosiveButton.SetActive(!explosiveButton.activeSelf);
        }        
    }
}
