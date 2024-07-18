using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
                rsa.ImportParameters(privateKey);//must have all Crt params with the private key
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA512);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            /*Padding oracle:
             The Decrypt method throws different exceptions 
            for invalid padding and authentication failures.
             This could potentially be exploited in a padding oracle attack.*/
        }

        public static void GenerateKeyPair(out RSAParameters keyPair)
        {
            //SafeBCryptKeyHandle newKey = Interop.BCrypt.BCryptGenerateKeyPair(s_algHandle, keySize);
            //Interop.BCrypt.BCryptFinalizeKeyPair(newKey);

            //     BCRYPT_RSAKEY_BLOB   header
            //     byte[cbPublicExp]    publicExponent      - Exponent
            //     byte[cbModulus]      modulus             - Modulus
            //     -- Private only --
            //     byte[cbPrime1]       prime1              - P
            //     byte[cbPrime2]       prime2              - Q
            //     byte[cbPrime1]       exponent1           - DP
            //     byte[cbPrime2]       exponent2           - DQ
            //     byte[cbPrime1]       coefficient         - InverseQ
            //     byte[cbModulus]      privateExponent     - D

            using (var rsa = new RSACng(3072))
            {
                keyPair = rsa.ExportParameters(true);//includes also the public key
            }
        }

        /*
        public static RSAParameters GetFullPrivateParameters(BigInteger p, BigInteger q, BigInteger d)
        {
            var dp = d % (p - 1);
            var dq = d % (q - 1);

            var qInv = ModInverse(q, p);
            //Assert.Equal(1, (qInv * q) % p);

            return new RSAParameters
            {
                DP =dp.ToByteArray().Reverse().ToArray(),
                DQ =dq.ToByteArray().Reverse().ToArray(),
                InverseQ = qInv.ToByteArray().Reverse().ToArray(),
            };
        }
        public static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger t = 0, newT = 1;
            BigInteger r = m, newR = a;

            // Ensure the modulus is positive
            if (m < 0)
            {
                m = -m;
            }

            // Handle negative a by bringing it within the range of the modulus
            if (a < 0)
            {
                //a = m - (-a % m);//incorrect
                a = (a % m + m) % m;//correct
            }
            //Extended Euclidean Algorithm for computing the modular inverse
            while (newR != 0)
            {
                BigInteger quotient = r / newR;

                BigInteger tmp = newT; 
                newT = t - quotient * newT; 
                t = tmp;

                tmp = newR; 
                newR = r - quotient * newR; 
                r = tmp;
            }

            // If r != 1, then a and m are not coprime, and the inverse does not exist
            if (r > 1)
            {
                throw new ArgumentException($"{nameof(a)} is not invertible modulo .");
            }

            // Ensure the result is positive, in the range [0, m-1]
            if (t < 0)
            {
                t += m;
            }
            return t;
        }
        */
    }
}
