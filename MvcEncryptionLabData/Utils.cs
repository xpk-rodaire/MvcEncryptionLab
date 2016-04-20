using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MvcEncryptionLabData
{
    public class Utils
    {
        private static Random random;
        private static RNGCryptoServiceProvider crypto;
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string NUMBERS = "0123456789";

        static Utils()
        {
            random = new Random();
            crypto = new RNGCryptoServiceProvider();
        }

        /// <summary>
        /// http://www.webcodeexpert.com/2013/08/how-to-encrypt-and-decrypt.html
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        //public static string Encrypt(string plainText)
        //{
        //    string EncrptKey = "2013;[pnuLIT)WebCodeExpert";
        //    byte[] byKey = { };
        //    byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
        //    byKey = System.Text.Encoding.UTF8.GetBytes(EncrptKey.Substring(0, 8));
        //    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        //    byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);
        //    MemoryStream ms = new MemoryStream();
        //    CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
        //    cs.Write(inputByteArray, 0, inputByteArray.Length);
        //    cs.FlushFinalBlock(); 
        //    return Convert.ToBase64String(ms.ToArray());
        //}

        /// <summary>
        /// http://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="plainText"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        //public static string Encrypt2(string privateKey, string plainText, ref string iv)
        //{
        //    using (var aes = new AesCryptoServiceProvider()
        //    {
        //        Key = Convert.FromBase64String(privateKey),
        //        Mode = CipherMode.CBC,
        //        Padding = PaddingMode.PKCS7
        //    })
        //    {
        //        var input = Encoding.UTF8.GetBytes(plainText);
        //        aes.GenerateIV();
        //        var ivAsBytes = aes.IV;

        //        using (var encrypter = aes.CreateEncryptor(aes.Key, ivAsBytes))
        //        {
        //            using (var cipherStream = new MemoryStream())
        //            {
        //                using (var tCryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
        //                {
        //                    using (var tBinaryWriter = new BinaryWriter(tCryptoStream))
        //                    {
        //                        // Prepend IV to data
        //                        cipherStream.Write(ivAsBytes, 0, ivAsBytes.Length);  // Write iv to the plain stream (not tested though)
        //                        tBinaryWriter.Write(input);
        //                        tCryptoStream.FlushFinalBlock();
        //                    }

        //                    iv = Convert.ToBase64String(ivAsBytes);
        //                    return Convert.ToBase64String(cipherStream.ToArray());
        //                }
        //            }
        //        }
        //    }
        //}

        public static string Encrypt(string privateKey, string plainText, ref string iv)
        {
            AesManaged aes = new AesManaged();
            aes.GenerateIV();
            var ivAsBytes = aes.IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(Convert.FromBase64String(privateKey), aes.IV);

            // Create the streams used for encryption.
            MemoryStream memoryStream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                byte[] plainTextAsBytes = new UTF8Encoding(false).GetBytes(plainText);
                cryptoStream.Write(plainTextAsBytes, 0, plainTextAsBytes.Length);
            }

            aes.Clear();

            byte[] encryptedData = memoryStream.ToArray();

            iv = Convert.ToBase64String(ivAsBytes);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        /// 

        //public static string Decrypt2(string privateKey, string cipherText)
        ////public static string DecryptString(byte[] encryptedString, byte[] encryptionKey)
        //{
        //    var input = Encoding.UTF8.GetBytes(cipherText);

        //    using (var provider = new AesCryptoServiceProvider())
        //    {
        //        provider.Key = Convert.FromBase64String(privateKey);
        //        provider.Mode = CipherMode.CBC;
        //        provider.Padding = PaddingMode.PKCS7;

        //        using (var ms = new MemoryStream(input))
        //        {

        //            //var iv = new byte[provider.IV.Length];
        //            //memoryStream.Read(iv, 0, provider.IV.Length);
        //            //using (var decryptor = provider.CreateDecryptor(key, iv);


        //            // Read the first 16 bytes which is the IV.
        //            byte[] iv = new byte[provider.IV.Length];
        //            ms.Read(iv, 0, provider.IV.Length);
        //            provider.IV = iv;

        //            using (var decryptor = provider.CreateDecryptor())
        //            {
        //                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        //                {
        //                    using (var sr = new StreamReader(cs))
        //                    {
        //                        return sr.ReadToEnd();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public static string Decrypt2(string privateKey, string cipherText)
        //{
        //    using (var aes = new AesCryptoServiceProvider()
        //    {
        //        Key = Convert.FromBase64String(privateKey),
        //        Mode = CipherMode.CBC,
        //        Padding = PaddingMode.PKCS7
        //    })
        //    {
        //        var input = Encoding.UTF8.GetBytes(cipherText);

        //        // get first 16 bytes of IV and use it to decrypt
        //        var iv = new byte[16];
        //        Array.Copy(input, 0, iv, 0, iv.Length);

        //        using (var ms = new MemoryStream())
        //        {
        //            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, iv), CryptoStreamMode.Write))
        //            {
        //                using (var binaryWriter = new BinaryWriter(cs))
        //                {
        //                    // Decrypt Cipher Text from Message
        //                    binaryWriter.Write(
        //                        input,
        //                        iv.Length,
        //                        input.Length - iv.Length
        //                    );
        //                }
        //            }

        //            return Encoding.Default.GetString(ms.ToArray());
        //        }
        //    }
        //}

        public static string Decrypt(string privateKey, string cipherText, string iv)
        {
            AesManaged aes = new AesManaged();

            ICryptoTransform decryptor = aes.CreateDecryptor(Convert.FromBase64String(privateKey), Convert.FromBase64String(iv));

            // Create the streams used for encryption.
            MemoryStream memoryStream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
            {
                byte[] encryptedDataAsBytes = Convert.FromBase64String(cipherText);
                cryptoStream.Write(encryptedDataAsBytes, 0, encryptedDataAsBytes.Length);
            }

            aes.Clear();

            byte[] decryptedData = memoryStream.ToArray();
            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// http://www.webcodeexpert.com/2013/08/how-to-encrypt-and-decrypt.html
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        //public static string Decrypt(string cipherText)
        //{
        //    cipherText = cipherText.Replace(" ", "+");
        //    string DecryptKey = "2013;[pnuLIT)WebCodeExpert";
        //    byte[] byKey = { };
        //    byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
        //    byte[] inputByteArray = new byte[cipherText.Length];

        //    byKey = System.Text.Encoding.UTF8.GetBytes(DecryptKey.Substring(0, 8));
        //    DESCryptoServiceProvider des = new DESCryptoServiceProvider();


        //    inputByteArray = Convert.FromBase64String(cipherText.Replace(" ", "+"));
        //    MemoryStream ms = new MemoryStream();
        //    CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
        //    cs.Write(inputByteArray, 0, inputByteArray.Length);
        //    cs.FlushFinalBlock();
        //    System.Text.Encoding encoding = System.Text.Encoding.UTF8;
        //    return encoding.GetString(ms.ToArray());
        //}

        public static string GetSalt()
        {
            // Maximum length of salt
            int max_length = 32;

            // Empty salt array
            byte[] salt = new byte[max_length];

            // Build the random bytes
            crypto.GetNonZeroBytes(salt);

            // Return the string encoded salt
            return Convert.ToBase64String(salt);
        }

        public static string Hash(string value, string salt)
        {
            return Hash(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(salt));
        }

        public static string Hash(byte[] value, byte[] salt)
        {
            byte[] saltedValue = value.Concat(salt).ToArray();
            return Convert.ToBase64String((new SHA256Managed().ComputeHash(saltedValue)));
        }

        private static string RandomString(int length, string chars)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomString(int length)
        {
            return RandomString(length, CHARS);
        }

        public static string RandomSSN()
        {
            return RandomString(9, NUMBERS);
        }
    }
}
