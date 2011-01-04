using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Fap.Foundation
{
    public abstract class SimpleCrypt
    {
        protected abstract string key { get; }
        protected abstract string key2 { get; }
        protected abstract string initVector { get; }

        public byte[] ToByteArray(String HexString)
        {
            int NumberChars = HexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
            }
            return bytes;
        }


        public string Encrypt(string input)
        {
            Stream m = Encrypt(new MemoryStream(Encoding.Unicode.GetBytes(input)));
            StreamReader sr = new StreamReader(m);
            return sr.ReadToEnd();
        }

        public string Decrypt(string input)
        {
            Stream m = new MemoryStream(Encoding.Unicode.GetBytes(input));
            StreamReader sr = new StreamReader(Decrypt(new StreamReader(m)));
            return sr.ReadToEnd();
        }



        public Stream Encrypt(Stream inputStream)
        {
            RijndaelManaged engine = new RijndaelManaged();
            engine.Mode = CipherMode.CBC;
            engine.Padding = PaddingMode.Zeros;
            StreamReader sr = new StreamReader(inputStream);
            string input = sr.ReadToEnd();
            SHA256 prov = SHA256.Create();
            byte[] bytes = prov.ComputeHash(Encoding.Unicode.GetBytes((key + key2)));
            for (int z = 0; z < 10; z++)
                bytes = prov.ComputeHash(bytes);
            engine.BlockSize = 256;
            ICryptoTransform encryptor = engine.CreateEncryptor(
                                                           bytes,
                                                           Encoding.Unicode.GetBytes(initVector));
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                        encryptor,
                                                        CryptoStreamMode.Write);
            byte[] inputBytes = Encoding.Unicode.GetBytes(input);
            cryptoStream.Write(inputBytes, 0, input.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            StringBuilder sb = new StringBuilder();
            for (int z = 0; z < cipherTextBytes.Length; z++)
            {
                sb.Append(cipherTextBytes[z].ToString("x2"));
            }
            return new MemoryStream(Encoding.Unicode.GetBytes(sb.ToString()));
        }

        public Stream Decrypt(TextReader sr)
        {
            RijndaelManaged engine = new RijndaelManaged();
            engine.Mode = CipherMode.CBC;
            engine.Padding = PaddingMode.Zeros;
            engine.BlockSize = 256;
            string input = sr.ReadToEnd();
            SHA256 prov = SHA256.Create();
            byte[] bytes = prov.ComputeHash(Encoding.Unicode.GetBytes((key + key2)));
            for (int z = 0; z < 10; z++)
                bytes = prov.ComputeHash(bytes);
            ICryptoTransform decryptor = engine.CreateDecryptor(
                                                        bytes,
                                                        Encoding.Unicode.GetBytes(initVector));
            byte[] inputBytes = ToByteArray(input);
            MemoryStream memoryStream = new MemoryStream(inputBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                     decryptor,
                                                     CryptoStreamMode.Read);
            byte[] cipherTextBytes = new byte[input.Length];
            int decryptedByteCount = cryptoStream.Read(inputBytes, 0, inputBytes.Length);
            cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string decyrpted = Encoding.Unicode.GetString(cipherTextBytes,
                                                   0,
                                                   decryptedByteCount);
            StringBuilder sb = new StringBuilder();
            for (int z = 0; z < decyrpted.Length; z++)
            {
                if (!decyrpted[z].Equals('\0'))
                    sb.Append(decyrpted[z]);
            }
            return new MemoryStream(Encoding.Unicode.GetBytes(sb.ToString()));
        }
    }
}
