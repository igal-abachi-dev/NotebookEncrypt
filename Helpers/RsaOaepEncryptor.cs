using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NotebookEncrypt.Helpers
{
    internal class RsaOaepEncryptor
    {
        public static string EncryptWithRSA(string plaintext, RSAParameters publicKey)
        {
            using (var rsa = new RSACng(3072))
            {
                rsa.ImportParameters(publicKey);
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] encryptedBytes = rsa.Encrypt(plaintextBytes, RSAEncryptionPadding.OaepSHA512);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public static string DecryptWithRSA(string encryptedText, RSAParameters privateKey)
        {
            using (var rsa = new RSACng(3072))
            {
                rsa.ImportParameters(privateKey);
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA512);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        public static void GenerateKeyPair(out RSAParameters publicKey, out RSAParameters privateKey)
        {
            using (var rsa = new RSACng(3072))
            {
                publicKey = rsa.ExportParameters(false);
                privateKey = rsa.ExportParameters(true);
            }
        }
    }
}
