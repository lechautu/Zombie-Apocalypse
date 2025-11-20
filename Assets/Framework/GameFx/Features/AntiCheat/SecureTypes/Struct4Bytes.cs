
using System.Runtime.InteropServices;

namespace GameFx.Features.AntiCheat.SecureTypes
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Struct4Bytes
    {
        // [FieldOffset(0)]
        // private byte byte0;
        // [FieldOffset(1)]
        // private byte byte1;
        // [FieldOffset(2)]
        // private byte byte2;
        // [FieldOffset(3)]
        // private byte byte3;

        [FieldOffset(0)]
        private int int0;

        [FieldOffset(0)]
        private float float0;

        public void SetInt(int value)
        {
            int0 = value;
        }

        public int GetInt()
        {
            return int0;
        }

        public void SetFloat(float value)
        {
            float0 = value;
        }

        public float GetFloat()
        {
            return float0;
        }
    }
}