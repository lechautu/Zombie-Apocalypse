using UnityEngine;

namespace Characters
{
    public interface IAimFacing
    {
        void SetModelYawOffset(float degrees);      // body→barrel fixed offset (per-weapon)
        void SetWeaponSocket(Transform socket);     // socket gắn vũ khí (bên hông hoặc trung tâm)
        void SetParallaxDistances(float near, float far); // dải khoảng cách để blend parallax (m)
    }

}