using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NotebookEncrypt
{
    //private keys notebook
    public static class SecretsNotebook
    {
        //s:65537
        //e = null;
        public static readonly string e = new BigInteger(new byte[3]{
             0x01, 0x00, 0x01
        }).ToFormattedString("e");

        //private BigInteger e = BigIntegerExtensions.FromUintArray(null, 0x10001);//65537)

        //var e = BigIntegerExtensions.FromUintArray(new uint[1] { 0x00010001 }, 1).ToByteArray();
        //same public exponent in all rsa 0x10001=65537

        // save with a proper key management system? azure keyvault,aws kms,hashicorp vault....
        public static readonly Dictionary<string, string[]> DPQ_DP_DQ_InvQ = new Dictionary<string, string[]>();//<guid , 6* uint[limbs]+sign>

        public static string CreateCompressedPrivateKey(string Id)
        {
            var d = DPQ_DP_DQ_InvQ[Id][0];
            var p = DPQ_DP_DQ_InvQ[Id][1];
            var q = DPQ_DP_DQ_InvQ[Id][2];
            var dp = DPQ_DP_DQ_InvQ[Id][3];
            var dq = DPQ_DP_DQ_InvQ[Id][4];
            var invQ= DPQ_DP_DQ_InvQ[Id][5];

            var compressed = JWK.CompressString(string.Join('|', d, p, q, dp, dq, invQ));
            return compressed;
        }

        public static string[] DecompressPrivateKey(string compressedKey)
        {
            // Decompress the compressed key
            string decompressedString = JWK.DecompressString(compressedKey);

            // Split the decompressed string into its components
            string[] components = decompressedString.Split('|');

            // Check if the number of components matches the expected count (6 components)
            if (components.Length != 6)
            {
            }

            return components;
        }

        //private readonly string _d; //private key;
        //private readonly string _p; //prime1;
        //private readonly string _q; //prime2;
    }

}
