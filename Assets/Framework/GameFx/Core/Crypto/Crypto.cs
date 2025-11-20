using System;
using System.Security.Cryptography;
using System.Text;

namespace GameFx.Core.Crypto
{
    public sealed class Crypto : ICrypto
    {
        private readonly Aes _aes;

        public Crypto(string key)
        {
            _aes = Aes.Create();
            using var sha256 = SHA256.Create();
            _aes.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.PKCS7;
        }

        public string Encrypt(string plainText)
        {
            _aes.GenerateIV();
            var iv = _aes.IV;

            using var encryptor = _aes.CreateEncryptor(_aes.Key, iv);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[iv.Length + cipherBytes.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, iv.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[_aes.BlockSize / 8];
            var cipherBytes = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var decryptor = _aes.CreateDecryptor(_aes.Key, iv);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}