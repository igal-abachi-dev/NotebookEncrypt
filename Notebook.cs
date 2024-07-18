using System.Numerics;
using System.Security.Cryptography;
using NotebookEncrypt.Helpers;

namespace NotebookEncrypt
{
    public class Notebook
    {
        private readonly string _n; //modulus;
        private readonly string _id = Guid.NewGuid().ToString("d", null).ToUpperInvariant();
        public List<Page> Pages = new List<Page>(100);

        private const int PagePerNotebook = 100; //todo: make configurable per notebook
        private const int LinesPerPage = 100; //todo: make configurable per notebook

        public string N => _n;
        public string Id => _id;

        //private RSAParameters _key; //for debug

//todo:        Key rotation: Implement a key rotation mechanism to periodically(90 days?) update encryption keys.

        public Notebook()
        {
            RsaOaepEncryptor.GenerateKeyPair(out var keyPair);
            //_key = keyPair;

            _n = new BigInteger(keyPair.Modulus.Reverse().ToArray()).ToFormattedString("n");
            //store in private book
            SecretsNotebook.DPQ_DP_DQ_InvQ[_id] = new string[6]
            {
                new BigInteger(keyPair.D.Reverse().ToArray()).ToFormattedString("d"),
                new BigInteger(keyPair.P.Reverse().ToArray()).ToFormattedString("p"),
                new BigInteger(keyPair.Q.Reverse().ToArray()).ToFormattedString("q"),
                new BigInteger(keyPair.DP.Reverse().ToArray()).ToFormattedString("dp"),
                new BigInteger(keyPair.DQ.Reverse().ToArray()).ToFormattedString("dq"),
                new BigInteger(keyPair.InverseQ.Reverse().ToArray()).ToFormattedString("invQ"),
            };

            //2554 len vs 5712
            //var cKey = SecretsNotebook.CreateCompressedPrivateKey(_id);

            for (int i = 0; i < PagePerNotebook; i++)
            {
                Pages.Add(new Page(i, _id));
            }
        }

        public Notebook(string id, byte[] n, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] invQ)
        {
            _id = id;
            _n = new BigInteger(n.Reverse().ToArray()).ToFormattedString("n");
            //store in private book
            SecretsNotebook.DPQ_DP_DQ_InvQ[_id] = new string[6]
            {
                new BigInteger(d.Reverse().ToArray()).ToFormattedString("d"),
                new BigInteger(p.Reverse().ToArray()).ToFormattedString("p"),
                new BigInteger(q.Reverse().ToArray()).ToFormattedString("q"),
                new BigInteger(dp.Reverse().ToArray()).ToFormattedString("dp"),
                new BigInteger(dq.Reverse().ToArray()).ToFormattedString("dq"),
                new BigInteger(invQ.Reverse().ToArray()).ToFormattedString("invQ"),
            };

            for (int i = 0; i < 100; i++)
            {
                Pages.Add(new Page(i, _id));
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


        public void Decrypt(byte[] E, byte[] N, byte[] D, byte[] P, byte[] Q, byte[] DP, byte[] DQ, byte[] InvQ)
        {
            var rsaParameters = new RSAParameters
            {
                D = D,
                Modulus = N,
                Exponent = E,//65537
                P = P,
                Q = Q,
                DP = DP,
                DQ = DQ,
                InverseQ = InvQ
            };


            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].Decrypt(rsaParameters);
            }
        }
    }

}
