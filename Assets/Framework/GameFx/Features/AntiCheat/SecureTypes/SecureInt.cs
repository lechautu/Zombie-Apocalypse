namespace GameFx.Features.AntiCheat.SecureTypes
{
    public struct SecureInt
    {
        const int _xorKey = 0x5A5A5A5A;

        private Struct4Bytes _data;

        public SecureInt(int value)
        {
            int encryptedValue = value ^ _xorKey;
            _data = new Struct4Bytes();
            _data.SetInt(encryptedValue);
        }

        public int GetValue()
        {
            int encryptedValue = _data.GetInt();
            return encryptedValue ^ _xorKey;
        }

        public static implicit operator SecureInt(int value)
        {
            return new SecureInt(value);
        }

        public static implicit operator int(SecureInt secureInt)
        {
            return secureInt.GetValue();
        }
    }
}