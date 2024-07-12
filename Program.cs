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

            //read from notebook.N;
            var N = notebook.N;
            var n = BigIntegerExtensions.FromUintArray(new uint[0] { }, 1).ToByteArray();

            //read from SecretsNotebook.e;
            var E = SecretsNotebook.e;
            var e = BigIntegerExtensions.FromUintArray(new uint[0] { }, 1).ToByteArray();

            //read from SecretsNotebook.D[notebook.Id];
            var D = SecretsNotebook.D[notebook.Id];
            var d = BigIntegerExtensions.FromUintArray(new uint[0] { }, 1).ToByteArray();

            notebook.Encrypt(e, n);
            notebook.Decrypt(e, n, d);
        }
    }
}