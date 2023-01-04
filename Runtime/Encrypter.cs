using System;
using System.Text;
using System.Security.Cryptography;

namespace Mixin.Save
{
    public static class Encrypter
    {
        public static string Encrypt(string stringToEncrypt, string salt)
        {
            byte[] data = UTF8Encoding.UTF8.GetBytes(stringToEncrypt);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(salt));
                using (TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider()
                {
                    Key = key,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                }
                    )
                {
                    ICryptoTransform cryptoTransform = cryptoServiceProvider.CreateEncryptor();
                    byte[] result = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                    return Convert.ToBase64String(result, 0, result.Length);
                }
            }
        }

        public static string Decrypt(string stringToDecrypt, string salt)
        {
            byte[] data = Convert.FromBase64String(stringToDecrypt);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(salt));
                using (TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider()
                {
                    Key = key,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                }
                    )
                {
                    ICryptoTransform cryptoTransform = cryptoServiceProvider.CreateDecryptor();
                    byte[] result = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                    return UTF8Encoding.UTF8.GetString(result);
                }
            }
        }
    }
}