using System;
using System.ComponentModel;
using System.IO.Compression;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
//using QRCoder;

public static class JWK
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    public static string ToJwk(RSAParameters rsaParameters, bool includePrivateParameters)
    {
        string kid = GenerateKid(rsaParameters);
        var jwk = new RsaJwk
        {
            kid = kid,
            n = Base64UrlEncode(rsaParameters.Modulus),
            e = Base64UrlEncode(rsaParameters.Exponent),
            //
            d = includePrivateParameters ? Base64UrlEncode(rsaParameters.D) : null,
            p = includePrivateParameters ? Base64UrlEncode(rsaParameters.P) : null,
            q = includePrivateParameters ? Base64UrlEncode(rsaParameters.Q) : null,
            dp = includePrivateParameters ? Base64UrlEncode(rsaParameters.DP) : null,
            dq = includePrivateParameters ? Base64UrlEncode(rsaParameters.DQ) : null,
            qi = includePrivateParameters ? Base64UrlEncode(rsaParameters.InverseQ) : null,
        };

        return JsonSerializer.Serialize(jwk, _options);
    }

    private static string GenerateKid(RSAParameters rsaParameters)
    {
        if (rsaParameters.Modulus == null || rsaParameters.Exponent == null)
            throw new ArgumentException("RSA parameters are incomplete");

        using (SHA512 sha512 = SHA512.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(Base64UrlEncode(rsaParameters.Modulus) + Base64UrlEncode(rsaParameters.Exponent));
            byte[] hashBytes = sha512.ComputeHash(inputBytes);
            return Base64UrlEncode(hashBytes);
        }
    }

    public static RSAParameters ToRsaParameters(string jwk)
    {
        var rsaParameters = JsonSerializer.Deserialize<RsaJwk>(jwk, _options);

        return new RSAParameters
        {
            Modulus = Base64UrlDecode(rsaParameters.n),
            Exponent = Base64UrlDecode(rsaParameters.e),
            D = rsaParameters.d != null ? Base64UrlDecode(rsaParameters.d) : null,
            //
            P = rsaParameters.p != null ? Base64UrlDecode(rsaParameters.p) : null,
            Q = rsaParameters.q != null ? Base64UrlDecode(rsaParameters.q) : null,
            DP = rsaParameters.dp != null ? Base64UrlDecode(rsaParameters.dp) : null,
            DQ = rsaParameters.dq != null ? Base64UrlDecode(rsaParameters.dq) : null,
            InverseQ = rsaParameters.qi != null ? Base64UrlDecode(rsaParameters.qi) : null
        };
    }

    public static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static byte[] Base64UrlDecode(string input)
    {
        string output = input
            .Replace('-', '+')
            .Replace('_', '/');
        switch (output.Length % 4)
        {
            case 0: break;
            case 2: output += "=="; break;
            case 3: output += "="; break;
            default: throw new FormatException("Invalid Base64Url string.");
        }
        return Convert.FromBase64String(output);
    }

    /*
    public static MemoryStream GenQrCode(string data)
    {
        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.H);
            using (var qrCode = new QRCode(qrCodeData))
            {
                using (var qrCodeImage = qrCode.GetGraphic(30))
                {
                    var memoryStream = new MemoryStream();
                    qrCodeImage.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0; // Reset the stream position to the beginning
                    return memoryStream;
                }
            }
        }
    }
    */

    /*
using Aspose.Words;
using Aspose.Words.Drawing;

using System.Drawing;
using System.Drawing.Imaging;
    
      MemoryStream qrCodeStream = GenQrCode(data);

        // Load the DOCX template
        Document doc = new Document("template.docx");

        // Find the placeholder and replace it with the QR code image
        foreach (Run run in doc.GetChildNodes(NodeType.Run, true))
        {
            if (run.Text.Contains("${name}"))
            {
                run.Text = run.Text.Replace("${name}", ""); // Remove the placeholder text

                // Create a builder to insert the image
                DocumentBuilder builder = new DocumentBuilder(doc);
                builder.MoveTo(run);
                builder.InsertImage(qrCodeStream, RelativeHorizontalPosition.Margin, 0,
                                    RelativeVerticalPosition.Margin, 0,
                                    150, 150, // Set the size of the QR code image
                                    WrapType.Inline);
            }
        }

        // Save the document as a PDF
        doc.Save("output.pdf");
     */
    public static string CompressString(string input) //compress x2
    {
        //string base64Url = Base64UrlEncode(Encoding.UTF8.GetBytes(input)); //only on byte[]
        byte[] compressedData = Compress(Encoding.UTF8.GetBytes(input));
        return Base64UrlEncode(compressedData);//then to qr code 2005+ , use qrcoder & aspose pdf from docx template with placeholders
    }

    public static string DecompressString(string input)
    {
        byte[] decompressedData = Decompress(Base64UrlDecode(input));
        return Encoding.UTF8.GetString(decompressedData);
    }

    private static byte[] Compress(byte[] data)
    {//Zstandard (Zstd) using ZstdNet;?
//        removing unnecessary white spaces and newlines.?
        using (var outputStream = new MemoryStream())
        {
            using (var brotliStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
            {
                brotliStream.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }
    }

    private static byte[] Decompress(byte[] compressedData)
    {
        using (var inputStream = new MemoryStream(compressedData))
        using (var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new MemoryStream())
        {
            brotliStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }
    private class RsaJwk
    {
        // JWK fields:
        public string kty { get; set; } = "RSA";
        public string kid { get; set; } = Guid.NewGuid().ToString();
        public string use { get; set; } = "enc";
        public string alg { get; set; } = "RS256";
        // RSA fields:
        public string n { get; set; }
        public string e { get; set; }
        public string d { get; set; }
        // CRT fields:
        public string p { get; set; }
        public string q { get; set; }
        public string dp { get; set; }
        public string dq { get; set; }
        public string qi { get; set; }
    }
}
