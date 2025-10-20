using System.Security.Cryptography;
using System.Text;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration configuration)
        {
            // Use a hard-coded key for now (you can change this later)
            _key = new byte[]
            {
                0xB3, 0x74, 0xA2, 0x6A, 0x71, 0x49, 0x04, 0x37,
                0xAA, 0x02, 0x4E, 0x4F, 0xAD, 0xD5, 0xB4, 0x97,
                0xFD, 0xFF, 0x1A, 0x8E, 0xA6, 0xFF, 0x12, 0xF6,
                0xFB, 0x65, 0xAF, 0x27, 0x20, 0xB5, 0x9C, 0xCF
            };

            _iv = new byte[16]; // Initialization vector
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
    }
}