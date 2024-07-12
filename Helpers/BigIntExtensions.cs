using System;
using System.Numerics;
using System.Text;

public static class BigIntegerExtensions
{
    public static string ToFormattedString(this BigInteger bigInt, string variableName)
    {
        // Determine the sign
        int sign = bigInt.Sign;

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

        // Create the formatted string
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"//s:{sign}");
        sb.AppendLine($"{variableName} = new uint[{uints.Length}]");
        sb.AppendLine("{");

        for (int i = 0; i < uints.Length; i++)
        {
            sb.Append($"    0x{uints[i]:x8}");
            if (i < uints.Length - 1)
            {
                sb.Append(", ");
            }

            if ((i + 1) % 8 == 0)
            {
                sb.AppendLine();
            }
        }

        sb.AppendLine();
        sb.AppendLine("};");

        return sb.ToString();
    }

    public static BigInteger FromUintArray(uint[] uintArray, int sign)
    {
        // Convert uint[] to byte[]
        byte[] byteArray = new byte[uintArray.Length * 4];
        Buffer.BlockCopy(uintArray, 0, byteArray, 0, byteArray.Length);
        return new BigInteger(byteArray) * sign;
    }
}