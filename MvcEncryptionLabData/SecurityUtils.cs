using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MvcEncryptionLabData
{
    // TODO: Validate encryption key
    //       > Choose phrase to store as check in database
    //       > When key used for first time, encrypt phrase with key, store IV and encrypted value in database
    //       > On all subsequent key entries, decrypt phrase and display to user
    //            When entering security key, 
    //       > Prompt user if decrypted phrase is accurate, proceed with encrypt/decrypt

    public class SecurityUtils
    {
        #region Encryption Key

        private static Dictionary<string, ExpirableSecureValue> _encryptionKeys = new Dictionary<string, ExpirableSecureValue>();

        public static string GetUserEncryptionKey(string user)
        {
            ExpirableSecureValue secureValue;
            _encryptionKeys.TryGetValue(user, out secureValue);

            if (secureValue == null || !secureValue.HasValue)
            {
                _encryptionKeys.Remove(user);
                return null;
                //throw new ApplicationException("Security key not available.");
            }
            else
            {
                return secureValue.Value;
            }
        }

        public static void SetUserEncryptionKey(string user, string value)
        {
            SetUserEncryptionKey(user, value, 300);
        }

        public static void SetUserEncryptionKey(string user, string value, int expireInSeconds)
        {
            ExpirableSecureValue secureValue;
            _encryptionKeys.TryGetValue(user, out secureValue);

            if (secureValue != null)
            {
                _encryptionKeys.Remove(user);
            }

            ExpirableSecureValue newSecureValue = new ExpirableSecureValue(expireInSeconds);
            newSecureValue.Value = value;
            _encryptionKeys.Add(user, newSecureValue);
        }

        public static bool UserHasEncryptionKey(string user)
        {
            ExpirableSecureValue secureValue;
            _encryptionKeys.TryGetValue(user, out secureValue);

            if (secureValue == null || !secureValue.HasValue)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void ExpireUserEncryptionKey(string user)
        {
            ExpirableSecureValue secureValue;
            _encryptionKeys.TryGetValue(user, out secureValue);

            if (secureValue != null && secureValue.HasValue)
            {
                secureValue.Expire();
                _encryptionKeys.Remove(user);
            }
        }

        public static void LockUserEncryptionKey(string user, bool isLocked)
        {
            ExpirableSecureValue secureValue;
            _encryptionKeys.TryGetValue(user, out secureValue);

            if (secureValue != null)
            {
                secureValue.IsLocked = isLocked;
            }
        }

        public static bool ValidateEncryptionKeyFormat(string user, string key)
        {
            // TODO: ValidateEncryptionKey() - validate encryption key format
            // No white spaces
            // RegEx for key value?
            return true;
        }

        public static string EncryptionKeyTestingOnly { get; set; }

        #endregion 

        private static RNGCryptoServiceProvider crypto;

        static SecurityUtils()
        {
            crypto = new RNGCryptoServiceProvider();
        }

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

            SHA256CryptoServiceProvider sha = new SHA256CryptoServiceProvider();
            Byte[] hashedBytes = sha.ComputeHash(saltedValue);

            return Convert.ToBase64String(hashedBytes);
        }

        #region Encrypt

        public static string Encrypt(string plainText, ref string iv, string userName)
        {
            string key = GetUserEncryptionKey(userName);
            if (string.IsNullOrEmpty(key))
            {
                throw new ApplicationException(String.Format("No key found for user '{0}'.", userName));
            }

            return _Encrypt(key, plainText, ref iv);
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/system.security.cryptography.aescryptoserviceprovider.aspx
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="plainText"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        private static string _Encrypt(string privateKey, string plainText, ref string iv)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (privateKey == null || privateKey.Length <= 0)
                throw new ArgumentNullException("Key");

            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Convert.FromBase64String(privateKey);
                iv = Convert.ToBase64String(aesAlg.IV);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return Convert.ToBase64String(encrypted);
        }

        #endregion

        #region Decrypt

        public static string Decrypt(string cipherText, string iv, string userName)
        {
            string key = GetUserEncryptionKey(userName);
            if (string.IsNullOrEmpty(key))
            {
                throw new ApplicationException(String.Format("No key found for user '{0}'.", userName));
            }

            return _Decrypt(key, cipherText, iv);
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/system.security.cryptography.aescryptoserviceprovider.aspx
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="cipherText"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        private static string _Decrypt(string privateKey, string cipherText, string iv)
        {
            byte[] cipherTextAsBytes = Convert.FromBase64String(cipherText);

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (privateKey == null || privateKey.Length <= 0)
                throw new ArgumentNullException("Key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Convert.FromBase64String(privateKey);
                aesAlg.IV = Convert.FromBase64String(iv);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherTextAsBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            return plaintext;
        }

        #endregion
    }

    public static class SecurityExtensions
    {
        /// <summary>
        /// https://blogs.msdn.microsoft.com/fpintos/2009/06/12/how-to-properly-convert-securestring-to-string/
        /// http://www.codeproject.com/Tips/549109/Working-with-SecureString
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertToUnsecureString(this SecureString value)
        {
            if (value == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static SecureString ConvertToSecureString(this string value)
        {
            var securePassword = new SecureString();
            foreach (var c in value.ToCharArray())
            {
                securePassword.AppendChar(c);
            }
            securePassword.MakeReadOnly();

            return securePassword;
        }
    }

    public class ExpirableSecureValue
    {
        private SecureString _value = null;
        private int _expireInSeconds = 0;
        private DateTime _expireTime;
        private bool _isLocked = false;

        public ExpirableSecureValue(int expireInSeconds)
        {
            if (expireInSeconds <= 0 || expireInSeconds > (60 * 30))
            {
                throw new ArgumentException(String.Format("Invalid expireInSeconds value: {0}. Must be >= 1 and <= 1800.", expireInSeconds));
            }

            this._expireInSeconds = expireInSeconds;
        }

        public void Expire()
        {
            this._value.Dispose();
            this._value = null;
            this.IsLocked = false;
        }

        public bool IsLocked
        {
            get
            {
                return this._isLocked;
            }
            
            set
            {
                if (this._value != null)
                {
                    this._isLocked = value;
                }
            }
        }

        public string Value
        {
            set
            {
                this._value = value.ConvertToSecureString();
                this._expireTime = DateTime.Now.AddSeconds(this._expireInSeconds);
            }

            get
            {
                if (this._value != null)
                {
                    if (IsLocked)
                    {
                        return this._value.ConvertToUnsecureString();
                    }
                    else if (DateTime.Compare(DateTime.Now, this._expireTime) < 0)
                    {
                        return this._value.ConvertToUnsecureString();
                    }
                    else
                    {
                        this._value.Dispose();
                        this._value = null;
                        this.IsLocked = false;
                        return null;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// By calling the Value property this is also checking to see if key has expired!
        /// </summary>
        public Boolean HasValue
        {
            get
            {
                return this.Value != null;
            }
        }
    }
}
