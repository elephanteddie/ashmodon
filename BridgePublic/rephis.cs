using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace BridgePublic
{
    public static class rehpis
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 10000;

        public static string Encrypt(string plainText, string uphash)
        {            
            try
            {
                // Allocate a byte array to copy the string into
                byte[] bytes = Encoding.ASCII.GetBytes(gethash(uphash));

                // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
                // so that the same Salt and IV values can be used when decrypting.  

                var saltStringBytes = Generate256BitsOfRandomEntropy();
                var ivStringBytes = Generate256BitsOfRandomEntropy();
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                using (var password = new Rfc2898DeriveBytes(bytes, saltStringBytes, 10000))
                {
                    for (int i = 0; i < bytes.Count(); i++) { bytes[i] = 0; }

                    var keyBytes = password.GetBytes(256 / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;

                        using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                        {
                            for (int i = 0; i < keyBytes.Length; i++) { keyBytes[i] = 0; }

                            using (var memoryStream = new MemoryStream())
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                                {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    for (int i = 0; i < plainTextBytes.Length; i++) { plainTextBytes[i] = 0; }

                                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                    var cipherTextBytes = saltStringBytes;

                                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                    for (int i = 0; i < ivStringBytes.Length; i++) { ivStringBytes[i] = 0; }
                                    for (int i = 0; i < saltStringBytes.Length; i++) { saltStringBytes[i] = 0; }

                                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                    memoryStream.Close();
                                    cryptoStream.Close();

                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            finally
            {
            }
        }

        public static string Decrypt(string cipherText, string uphash)
        {
            try
            {
                // Allocate a byte array to copy the string into
                byte[] bytes = Encoding.ASCII.GetBytes(gethash(uphash));

                // Get the complete stream of bytes that represent:
                // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
                var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
                // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
                var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(256 / 8).ToArray();
                // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
                var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(256 / 8).Take(256 / 8).ToArray();
                // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
                var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((256 / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((256 / 8) * 2)).ToArray();

                for (int i = 0; i < cipherTextBytesWithSaltAndIv.Length; i++) { cipherTextBytesWithSaltAndIv[i] = 0; }

                using (var password = new Rfc2898DeriveBytes(bytes, saltStringBytes, 10000))
                {
                    for (int i = 0; i < bytes.Count(); i++) { bytes[i] = 0; }
                    for (int i = 0; i < saltStringBytes.Length; i++) { saltStringBytes[i] = 0; }

                    var keyBytes = password.GetBytes(256 / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                        {
                            for (int i = 0; i < keyBytes.Length; i++) { keyBytes[i] = 0; }
                            for (int i = 0; i < ivStringBytes.Length; i++) { ivStringBytes[i] = 0; }

                            using (var memoryStream = new MemoryStream(cipherTextBytes))
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                {
                                    var plainTextBytes = new byte[cipherTextBytes.Length];
                                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();

                                    for (int i = 0; i < cipherTextBytes.Length; i++) { cipherTextBytes[i] = 0; }

                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            finally
            {

            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        private static string gethash(string unam)
        {
            var hash0 = new MD5CryptoServiceProvider();
            var hash0data = hash0.ComputeHash(Encoding.ASCII.GetBytes("ent" + unam + "ropy"));

            var sha1 = new SHA1CryptoServiceProvider();
            var sha11data = sha1.ComputeHash(Encoding.ASCII.GetBytes("wa" + Convert.ToBase64String(hash0data) + "cky"));

            string hash = Convert.ToBase64String(sha11data);

            for (int i = 0; i < hash0data.Length; i++) { hash0data[i] = 0; }
            for (int i = 0; i < sha11data.Length; i++) { sha11data[i] = 0; }

            return hash;
        }
    }
}
