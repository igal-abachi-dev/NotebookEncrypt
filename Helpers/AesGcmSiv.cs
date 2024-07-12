using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace NotebookEncrypt.Helpers
{
    public static class AesGcmSiv //now it's aes 256 gcm , will add siv using bouncy castle / libsodium
    {
        private const int NonceSize = 12; // 96 bits
        private const int TagSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits

        public static (byte[] Key, byte[] Nonce) GenerateKey()
        {
            return (GenerateKey(KeySize), GenerateNonce());
        }
        private static byte[] GenerateNonce()
        {
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);
            return nonce;
        }

        private static byte[] GenerateKey(int size = KeySize)
        {
            byte[] key = new byte[size];
            RandomNumberGenerator.Fill(key);
            return key;
            //return Convert.ToBase64String(key);
        }

        public static string Encrypt(string plaintext, byte[] key, byte[] nonce, byte[] associatedData = null)
        {
            if (plaintext == null)
                throw new ArgumentNullException(nameof(plaintext));
            ValidateKey(key);
            if (nonce == null || nonce.Length != NonceSize) throw new ArgumentException("Invalid nonce length");

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag, associatedData);
            }

            // base64(nonce + encryptedBytes + tag)
            byte[] encryptedData = new byte[NonceSize + ciphertext.Length + TagSize];
            Buffer.BlockCopy(nonce, 0, encryptedData, 0, NonceSize);
            Buffer.BlockCopy(ciphertext, 0, encryptedData, NonceSize, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, encryptedData, NonceSize + ciphertext.Length, TagSize);

            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(plaintextBytes);
            CryptographicOperations.ZeroMemory(nonce);

            return Convert.ToBase64String(encryptedData);
        }

        private static void ValidateKey(byte[] key)
        {
            if (key == null || key.Length != KeySize)
            {
                throw new ArgumentException("Invalid key. Key length should be 32 bytes (256 bits).", nameof(key));
            }
        }

        private static void ValidateEncryptedData(byte[] encryptedDataWithNonceAndTag)
        {
            if (encryptedDataWithNonceAndTag.Length < NonceSize + TagSize)
            {
                throw new ArgumentException("Invalid encrypted text.", nameof(encryptedDataWithNonceAndTag));
            }
        }

        public static string Decrypt(string encryptedText, byte[] key, byte[] associatedData = null)
        {
            if (encryptedText == null)
                throw new ArgumentNullException(nameof(encryptedText));

            byte[] encryptedData = Convert.FromBase64String(encryptedText);
            ValidateEncryptedData(encryptedData);

            byte[] nonce = new byte[NonceSize];// iv
            byte[] ciphertext = new byte[encryptedData.Length - NonceSize - TagSize];
            byte[] tag = new byte[TagSize];// ADD

            Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedData, NonceSize, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedData, NonceSize + ciphertext.Length, tag, 0, TagSize);

            using (var aesGcm = new AesGcm(key))
            {
                byte[] plaintextBytes = new byte[ciphertext.Length];
                try
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes, associatedData);
                }
                catch (CryptographicException)
                {
                    throw new ArgumentException("Invalid encrypted data.", nameof(encryptedText));
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(key);
                    CryptographicOperations.ZeroMemory(nonce);
                }

                return Encoding.UTF8.GetString(plaintextBytes);
            }
        }
    }
}
