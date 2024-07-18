using System;
using System.Security.Cryptography;
using System.Text;

namespace NotebookEncrypt.Helpers
{
    public static class AesGcmSiv // Now it's AES 256 GCM, will add SIV using Bouncy Castle / libsodium
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
        }

        public static string Encrypt(string plaintext, byte[] key, byte[] nonce, byte[] associatedData = null)
        {
            if (string.IsNullOrEmpty(plaintext))
                return "";

            ValidateKey(key);
            if (nonce == null || nonce.Length != NonceSize)
                throw new ArgumentException("Invalid nonce length");

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag, associatedData);
            }

            // Debug logging
            //Console.WriteLine($"Nonce (Encrypt): {BitConverter.ToString(nonce)}");
            //Console.WriteLine($"Ciphertext (Encrypt): {BitConverter.ToString(ciphertext)}");
            //Console.WriteLine($"Tag (Encrypt): {BitConverter.ToString(tag)}");

            byte[] encryptedData = new byte[ciphertext.Length + TagSize];
            Buffer.BlockCopy(ciphertext, 0, encryptedData, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, encryptedData, ciphertext.Length, TagSize);

            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(plaintextBytes);
            CryptographicOperations.ZeroMemory(nonce);
            CryptographicOperations.ZeroMemory(tag);

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
            if (encryptedDataWithNonceAndTag.Length < TagSize)
            {
                throw new ArgumentException("Invalid encrypted text.", nameof(encryptedDataWithNonceAndTag));
            }
        }

        public static string Decrypt(string encryptedText, byte[] key, byte[] nonce, byte[] associatedData = null)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return "";

            byte[] encryptedData = Convert.FromBase64String(encryptedText);
            ValidateEncryptedData(encryptedData);
            byte[] ciphertext = new byte[encryptedData.Length - TagSize];
            byte[] tag = new byte[TagSize];

            Buffer.BlockCopy(encryptedData, 0, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(encryptedData, ciphertext.Length, tag, 0, TagSize);
            
            // Debug logging
            //Console.WriteLine($"Nonce (Decrypt): {BitConverter.ToString(nonce)}");
            //Console.WriteLine($"Ciphertext (Decrypt): {BitConverter.ToString(ciphertext)}");
            //Console.WriteLine($"Tag (Decrypt): {BitConverter.ToString(tag)}");

            using (var aesGcm = new AesGcm(key))
            {
                byte[] plaintextBytes = new byte[ciphertext.Length];
                try
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes, associatedData);
                    //Console.WriteLine($"Decryption successful. Plaintext: {BitConverter.ToString(plaintextBytes)}");
                }
                catch (CryptographicException ex)
                {
                    //Console.WriteLine($"Exception: {ex.Message}");
                    throw new ArgumentException("Invalid encrypted data or authentication tag mismatch.", nameof(encryptedText), ex);
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(key);
                    CryptographicOperations.ZeroMemory(nonce);
                    CryptographicOperations.ZeroMemory(tag);
                    CryptographicOperations.ZeroMemory(ciphertext);
                }

                return Encoding.UTF8.GetString(plaintextBytes);
            }
        }
    }
}
