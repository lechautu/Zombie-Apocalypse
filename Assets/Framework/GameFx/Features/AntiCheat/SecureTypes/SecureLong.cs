namespace GameFx.Features.AntiCheat.SecureTypes
{
    public struct SecureLong
    {
        const long _xorKey = 0x5A5A5A5A5A5A5A5A;

        private Struct8Bytes _data;

        public SecureLong(long value)
        {
            long encryptedValue = value ^ _xorKey;
            _data = new Struct8Bytes();
            _data.SetLong(encryptedValue);
        }

        public long GetValue()
        {
            long encryptedValue = _data.GetLong();
            return encryptedValue ^ _xorKey;
        }

        public static implicit operator SecureLong(long value)
        {
            return new SecureLong(value);
        }

        public static implicit operator long(SecureLong secureLong)
        {
            return secureLong.GetValue();
        }
    }
}