namespace GameFx.Core.Crypto
{
    public interface ICrypto
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}