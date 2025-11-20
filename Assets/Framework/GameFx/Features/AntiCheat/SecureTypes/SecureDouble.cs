namespace GameFx.Features.AntiCheat.SecureTypes
{
    public struct SecureDouble
    {
        const long _xorKey = 0x5A5A5A5A5A5A5A5A;

        private Struct8Bytes _data;

        public SecureDouble(double value)
        {
            _data = new Struct8Bytes();
            _data.SetDouble(value);
            long longValue = _data.GetLong();
            long encryptedValue = longValue ^ _xorKey;
            _data.SetLong(encryptedValue);
        }

        public double GetValue()
        {
            long encryptedValue = _data.GetLong();
            long longValue = encryptedValue ^ _xorKey;

            Struct8Bytes temp = new();
            temp.SetLong(longValue);
            return temp.GetDouble();
        }

        public static implicit operator SecureDouble(double value)
        {
            return new SecureDouble(value);
        }

        public static implicit operator double(SecureDouble secureDouble)
        {
            return secureDouble.GetValue();
        }
    }
}