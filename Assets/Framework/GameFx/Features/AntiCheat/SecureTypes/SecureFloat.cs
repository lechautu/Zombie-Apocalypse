namespace GameFx.Features.AntiCheat.SecureTypes
{
    public struct SecureFloat
    {
        const int _xorKey = 0x5A5A5A5A;

        private Struct4Bytes _data;

        public SecureFloat(float value)
        {
            _data = new Struct4Bytes();
            _data.SetFloat(value);
            int intValue = _data.GetInt();
            int encryptedValue = intValue ^ _xorKey;
            _data.SetInt(encryptedValue);
        }

        public float GetValue()
        {
            int encryptedValue = _data.GetInt();
            int intValue = encryptedValue ^ _xorKey;

            Struct4Bytes temp = new();
            temp.SetInt(intValue);
            return temp.GetFloat();
        }

        public static implicit operator SecureFloat(float value)
        {
            return new SecureFloat(value);
        }

        public static implicit operator float(SecureFloat secureFloat)
        {
            return secureFloat.GetValue();
        }
    }
}