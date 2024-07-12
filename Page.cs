using System.Security.Cryptography;
using NotebookEncrypt.Helpers;

namespace NotebookEncrypt
{
    public class Page
    {
        public readonly int Index;
        public string K;//key
        public string IV;//Nonce
        public Page(int index)
        {
            index = index;
            var aesKey = Helpers.AesGcmSiv.GenerateKey();
            K = JWK.Base64UrlEncode(aesKey.Key);
            IV = JWK.Base64UrlEncode(aesKey.Nonce);
        }
        public Page(int index, string K, string IV)//encrypted
        {
            index = index;
            K = K;
            IV = IV;
        }
        public string[] Lines = new string[100];//List<string>?

        public void Encrypt(RSAParameters rsaParameters)
        {
            var key = JWK.Base64UrlDecode(K);
            var Nonce = JWK.Base64UrlDecode(IV);

            K = RsaOaepEncryptor.EncryptWithRSA(K, rsaParameters);
            IV = RsaOaepEncryptor.EncryptWithRSA(IV, rsaParameters);

            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = Helpers.AesGcmSiv.Encrypt(Lines[i], key, Nonce);
            }
        }

        public void Decrypt(RSAParameters rsaParameters)
        {

            K = RsaOaepEncryptor.DecryptWithRSA(K, rsaParameters);
            IV = RsaOaepEncryptor.DecryptWithRSA(IV, rsaParameters);

            var key = JWK.Base64UrlDecode(K);
            var Nonce = JWK.Base64UrlDecode(IV);

            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = Helpers.AesGcmSiv.Decrypt(Lines[i], key, Nonce);
            }
        }

        //private void Sign(RSAParameters rsaParameters)
        //{
        //    //RSA-PSS?
        //}
    }

}
