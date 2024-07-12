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
        //s:1
        public static readonly string e = new BigInteger(new byte[3]{
             0x01, 0x00, 0x01
        }).ToFormattedString("e");
        //same public exponent in all rsa 0x10001=65537


        public static readonly Dictionary<string, string> D = new Dictionary<string, string>();//<guid ,uint[limbs]+sign>
    }

}
