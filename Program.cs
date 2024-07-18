using System.Security.Cryptography;

namespace NotebookEncrypt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var notebook = new Notebook();
            var pageContent = notebook.Pages[0].Lines;
            pageContent[0] = "hello world";


            //var n = BigIntegerExtensions.FromUintArray(new uint[96] {//size 384
            //    0x777952be,     0x5393fdad,     0x35ecddfe,     0xfcf5b834,     0x6e967699,
            //    0x76551772,     0xb0535562,     0xb9e3ae70,     0xd02b36e3,     0x0ea47975,
            //    0x8c0a9997,     0xb08ea35f,     0xb35ee4ac,     0xfa28c456,     0xf4f3c869,
            //    0x0d098846,     0x726c749d,     0xe728074f,     0xc0008d21,     0x8623bbf7,
            //    0xca55bddc,     0x94dab552,     0x988a4d40,     0x29cb5934,     0x81283c60,
            //    0xadc5ddea,     0x7066886d,     0x10f9d407,     0xc25f1f49,     0xeb24785f,
            //    0xdd499d9a,     0xd3d1a005,     0xe8d4df89,     0x4df66ad6,     0x42ad5ef7,
            //    0x2936e53b,     0x7d4038a5,     0x4e336c46,     0xe52c03a1,     0x56686d7f,
            //    0x9014ac54,     0xc5465aa0,     0x711b7c96,     0x69d2e3c6,     0x3d2a6bdc,
            //    0x2be6d1e7,     0xb3226664,     0xd268cdb8,     0x459136b1,     0x6a84910c,
            //    0xc45664eb,     0x39f2aeab,     0x981c02d3,     0x3f65a182,     0x3fff7046,
            //    0xe05f6f49,     0xe73193aa,     0x464a7fbc,     0x151472b7,     0xcdfbc966,
            //    0x27dad481,     0x2abacdab,     0xfa69516e,     0xba248257,     0xa85edabb,
            //    0x10908e99,     0x6fd2a0a9,     0xe352f99d,     0x77e8b42d,     0x3281fa67,
            //    0x6f7d2fa4,     0x854b6b71,     0xd723672c,     0x038830c2,     0x2ba50ae0,
            //    0x757f7973,     0x6e34824b,     0xe535bedd,     0x59b738fa,     0x7a01635e,
            //    0x081e3fac,     0x478f26d9,     0xb931a3f0,     0x08370a47,     0xc86a1e4f,
            //    0x818dde15,     0x08dc156a,     0x6ec6f826,     0x37b2fffc,     0x048a8f4d,
            //    0xf1c2f5c1,     0xb076bdb5,     0x344b3615,     0x73ef8513,     0x7c6e0e94,
            //    0x25835880}, 1).ToByteArray();

            var N = notebook.N;
            var n = N.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();

            var E = SecretsNotebook.e;//65537
            var e = E.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();//BigIntegerExtensions.FromUintArray(new uint[1] { 0x00010001 }, 1).ToByteArray();

            var D = SecretsNotebook.DPQ_DP_DQ_InvQ[notebook.Id][0];
            var d = D.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();

            var P = SecretsNotebook.DPQ_DP_DQ_InvQ[notebook.Id][1];
            var p = P.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();

            var Q = SecretsNotebook.DPQ_DP_DQ_InvQ[notebook.Id][2];
            var q = Q.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();

            var DP = SecretsNotebook.DPQ_DP_DQ_InvQ[notebook.Id][3];
            var dp = DP.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();

            var DQ = SecretsNotebook.DPQ_DP_DQ_InvQ[notebook.Id][4];
            var dq = DQ.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();

            var InvQ = SecretsNotebook.DPQ_DP_DQ_InvQ[notebook.Id][5];
            var invQ = InvQ.ToBigIntegerFromFormattedString().ToByteArray().Reverse().ToArray();

            notebook.Encrypt(e, n);
            notebook.Decrypt(e, n, d, p, q, dp, dq,invQ);
        }
    }
}