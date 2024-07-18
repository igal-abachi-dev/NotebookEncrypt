using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;

public static class BigIntegerExtensions
{
    private delegate BigInteger BigIntegerCtorDelegate(int n, uint[]? rgu);
    private static readonly BigIntegerCtorDelegate _bigIntegerCtor;

    private static readonly FieldInfo _signField;
    private static readonly FieldInfo _bitsField;
    static BigIntegerExtensions()
    {
        // Use expression trees to create a delegate for the internal constructor of bigint
        var constructorInfo = typeof(BigInteger).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(int), typeof(uint[]) },
            null);

        var nParam = Expression.Parameter(typeof(int), "n");
        var rguParam = Expression.Parameter(typeof(uint[]), "rgu");

        var newExpression = Expression.New(constructorInfo, nParam, rguParam);
        _bigIntegerCtor = Expression.Lambda<BigIntegerCtorDelegate>(newExpression, nParam, rguParam).Compile();

        // Get the internal fields _sign and _bits
        _signField = typeof(BigInteger).GetField("_sign", BindingFlags.Instance | BindingFlags.NonPublic);
        _bitsField = typeof(BigInteger).GetField("_bits", BindingFlags.Instance | BindingFlags.NonPublic);
    }
    private static int GetSign(this BigInteger bigInt)
    {
        return (int)_signField.GetValue(bigInt);
    }

    private static uint[] GetBits(this BigInteger bigInt)
    {
        return (uint[])_bitsField.GetValue(bigInt);
    }

    public static string ToFormattedString(this BigInteger bigInt, string variableName)
    {
        // Determine the sign
        //int sign = bigInt.Sign;
        var sign = bigInt.GetSign();//65537

        var uints = bigInt.GetBits();//can be null
        /*
        // Convert to byte array
        byte[] bytes = bigInt.ToByteArray();
        if (bytes.Length % 4 != 0)
        {
            Array.Resize(ref bytes, bytes.Length + (4 - (bytes.Length % 4)));
        }

        // Convert to uint array
        uint[] uints = new uint[bytes.Length / 4];
        for (int i = 0; i < uints.Length; i++)
        {
            uints[i] = BitConverter.ToUInt32(bytes, i * 4);
        }

        var bits = bigInt.GetBits();//can be null
        //if (bits == null)
        //{
        //    bits = new uint[1] { (uint)sign2 };
        //    sign2 = sign2 == 0?0: sign2>0?1:-1;
        //}
        if (bits.Length != uints.Length)
        {

        }
        */

        // Create the formatted string
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"//s:{sign}");
        sb.AppendLine($"{variableName} = " + ((uints == null) ? "null;" : $"new uint[{uints.Length}]"));
        if (uints != null)
        {
            sb.AppendLine("{");

            for (int i = 0; i < uints.Length; i++)
            {
                sb.Append($"    0x{uints[i]:x8}");
                if (i < uints.Length - 1)
                {
                    sb.Append(", ");
                }

                if ((i + 1) % 5 == 0)
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            sb.AppendLine("};");
        }

        return sb.ToString();
    }

    public static BigInteger ToBigIntegerFromFormattedString(this string input)
    {
        // Extract the sign after the comment prefix
        int signStartIndex = input.IndexOf("//s:") + 4;
        int signEndIndex = input.IndexOfAny(new char[] { '\n', '\r', ';' }, signStartIndex);
        string signString = input.Substring(signStartIndex, signEndIndex - signStartIndex).Trim();
        int signVal = (signString == "65537" || signString == "0x10001") ? 65537 : int.Parse(signString);

        // Extract the array data
        int startIndex = input.IndexOf('{');
        int endIndex = input.IndexOf('}');
        uint[] bits = null;

        if (startIndex != -1 && endIndex != -1)
        {
            startIndex++;
            string extractedArrayString = input.Substring(startIndex , endIndex - startIndex).Trim();

            // Convert to uint[]
            bits = extractedArrayString
                .Split(new[] { ',', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => Convert.ToUInt32(value, 16))
                .ToArray();
        }

        BigInteger bigInt = FromUintArray(bits, signVal);
        return bigInt;
    }

    public static BigInteger FromUintArray(uint[] uintArray/*can be bits or null*/, int sign/*0 / 1 / -1 / 65537*/)
    {
        return _bigIntegerCtor(sign, uintArray);

        // Convert uint[] to byte[]
        byte[] byteArray = new byte[uintArray.Length * 4];
        for (int i = 0; i < uintArray.Length; i++)
        {
            byte[] temp = BitConverter.GetBytes(uintArray[i]);
            Array.Copy(temp, 0, byteArray, i * 4, 4);
        }
        // Create the BigInteger
        BigInteger bigInt = new BigInteger(byteArray);//.Reverse().ToArray()?

        // Adjust the sign
        if (sign < 0)
        {
            bigInt = BigInteger.Negate(bigInt);// new BigInteger(-value._sign, value._bits); calls internal BigInteger(int n, uint[]? rgu)
        }

        return bigInt;
    }
}