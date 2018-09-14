﻿using System.IO;
using System.IO.Compression;

namespace GeometryDashAPI
{
    public class Crypt
    {
        public static byte[] XOR(byte[] data, int key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key);
            }
            return result;
        }

        public static string GZipDecompress(byte[] data)
        {
            string resultString = string.Empty;
            if (data != null && data.Length > 0)
            {
                using (MemoryStream stream = new MemoryStream(data))
                using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress))
                using (StreamReader reader = new StreamReader(zip))
                    resultString = reader.ReadToEnd();
            }
            return resultString;
        }

        public static byte[] GZipCompress(byte[] data)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(outStream, CompressionMode.Compress))
                using (MemoryStream srcStream = new MemoryStream(data))
                    srcStream.CopyTo(gzipStream);
                return outStream.ToArray();
            }
        }
    }
}
