using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace reddit_connect
{
    public class EncryptionProvider
    {
        #region Public Methods

        public void GetHashAndSaltString(string data, out string hash, out string salt)
        {
            byte[] hashOut, saltOut;
            GetHashAndSalt(Encoding.UTF8.GetBytes(data), out hashOut, out saltOut);

            hash = Convert.ToBase64String(hashOut);
            salt = Convert.ToBase64String(saltOut);
        }

        public bool VerifyHashString(string data, string hash, string salt)
        {
            byte[] dataToVerify = Encoding.UTF8.GetBytes(data);
            byte[] hashToVerify = Convert.FromBase64String(hash);
            byte[] saltToVerify = Convert.FromBase64String(salt);

            return VerifyHash(dataToVerify, hashToVerify, saltToVerify);
        }

        #endregion

        #region Private Methods

        private void GetHashAndSalt(byte[] data, out byte[] hash, out byte[] salt)
        {
            var random = new RNGCryptoServiceProvider();

            salt = new byte[128];
            random.GetNonZeroBytes(salt);

            hash = ComputeHash(data, salt);
        }

        private bool VerifyHash(byte[] data, byte[] hash, byte[] salt)
        {
            byte[] newHash = ComputeHash(data, salt);

            if (newHash.Length != hash.Length)
                return false;

            for (int i = 0; i < hash.Length; i++)
            {
                if (!hash[i].Equals(newHash[i]))
                    return false;
            }

            return true;
        }

        private byte[] ComputeHash(byte[] data, byte[] salt)
        {
            var dataAndSalt = new byte[data.Length + salt.Length];

            Array.Copy(data, dataAndSalt, data.Length);
            Array.Copy(salt, 0, dataAndSalt, data.Length, salt.Length);

            var hashAlgorithm = SHA512.Create();

            return hashAlgorithm.ComputeHash(dataAndSalt);
        }

        #endregion
    }
}