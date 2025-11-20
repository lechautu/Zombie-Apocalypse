using System.Runtime.InteropServices;

namespace GameFx.Features.AntiCheat.SecureTypes
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Struct8Bytes
    {
        // [FieldOffset(0)]
        // private byte byte0;
        // [FieldOffset(1)]
        // private byte byte1;
        // [FieldOffset(2)]
        // private byte byte2;
        // [FieldOffset(3)]
        // private byte byte3;
        // [FieldOffset(4)]
        // private byte byte4;
        // [FieldOffset(5)]
        // private byte byte5;
        // [FieldOffset(6)]
        // private byte byte6;
        // [FieldOffset(7)]
        // private byte byte7;

        [FieldOffset(0)]
        private long long0;

        [FieldOffset(0)]
        private double double0;

        public void SetLong(long value)
        {
            long0 = value;
        }

        public long GetLong()
        {
            return long0;
        }

        public void SetDouble(double value)
        {
            double0 = value;
        }

        public double GetDouble()
        {
            return double0;
        }
    }
}