using System.Numerics;
using System.Security.Cryptography;
using NotebookEncrypt.Helpers;

namespace NotebookEncrypt
{
    public class Notebook
    {
        private string _n; //modulus;
        private readonly string _id = Guid.NewGuid().ToString();
        public List<Page> Pages = new List<Page>(100);


        public string N => _n;
        public string Id => _id;

        public Notebook()
        {
            RsaOaepEncryptor.GenerateKeyPair(out var _publicKey, out var _privateKey);
            _n = new BigInteger(_publicKey.Modulus).ToFormattedString("n");
            //store in private book
            SecretsNotebook.D[_id] = new BigInteger(_privateKey.D).ToFormattedString("d");

            for (int i = 0; i < 100; i++)
            {
                Pages.Add(new Page(i));
            }
        }


        public void Encrypt(byte[] E, byte[] N)
        {
            var rsaParameters = new RSAParameters { Modulus = N, Exponent = E };

            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].Encrypt(rsaParameters);
            }
        }


        public void Decrypt(byte[] E, byte[] N, byte[] D)
        {
            var rsaParameters = new RSAParameters { D = D, Modulus = N, Exponent = E };

            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].Decrypt(rsaParameters);
            }
        }
    }

}
