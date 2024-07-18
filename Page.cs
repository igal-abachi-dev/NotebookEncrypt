using System.Security.Cryptography;
using System.Text;
using NotebookEncrypt.Helpers;

namespace NotebookEncrypt
{
    public class Page
    {
        private readonly byte[] NotebookId;
        public readonly int Index;
        //
        private string K;//key
        private string IV;//Nonce


        private int _linesPerPage = 100; //todo: make configurable per notebook

        public Page(int index,string notebookId, int lines = 100)
        {
            index = index;
            _linesPerPage = lines;
            Lines = new string[_linesPerPage];

            NotebookId = Encoding.UTF8.GetBytes(notebookId);//of guid

            var aesKey = Helpers.AesGcmSiv.GenerateKey();
            K = JWK.Base64UrlEncode(aesKey.Key);
            IV = JWK.Base64UrlEncode(aesKey.Nonce);
        }
        public Page(int index, string notebookId ,string K, string IV, int lines= 100)//encrypted
        {
            index = index;
            NotebookId = Encoding.UTF8.GetBytes(notebookId);//of guid
            K = K;
            IV = IV;
            _linesPerPage = lines;
            Lines = new string[_linesPerPage];
        }
        public readonly string[] Lines;//List<string>?

        public void SetPageKey(string k, string iv)
        {
            K = k;
            IV = iv;
        }

        public void Encrypt(RSAParameters rsaParameters)
        {
            var key = JWK.Base64UrlDecode(K);
            var Nonce = JWK.Base64UrlDecode(IV);

            K = RsaOaepEncryptor.EncryptWithRSA(K, rsaParameters);
            IV = RsaOaepEncryptor.EncryptWithRSA(IV, rsaParameters);

            K = JWK.CompressString(K);
            IV = JWK.CompressString(IV);

            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = Helpers.AesGcmSiv.Encrypt(Lines[i], key, Nonce, NotebookId);
            }
        }

        public void Decrypt(RSAParameters rsaParameters)
        {
            K = JWK.DecompressString(K);
            IV = JWK.DecompressString(IV);

            K = RsaOaepEncryptor.DecryptWithRSA((K), rsaParameters);
            IV = RsaOaepEncryptor.DecryptWithRSA(IV, rsaParameters);

            var key = JWK.Base64UrlDecode(K);
            var Nonce = JWK.Base64UrlDecode(IV);

            for (int i = 0; i < Lines.Length; i++)
            {
                // Implement constant-time comparison for authentication tags.
                Lines[i] = Helpers.AesGcmSiv.Decrypt(Lines[i], key, Nonce, NotebookId);
            }
        }

           // Add digital signatures for data integrity and authenticity.
        //private void Sign(RSAParameters rsaParameters)
        //{//no need in written notebook
        //    //RSA-PSS?
        //}
    }

}
