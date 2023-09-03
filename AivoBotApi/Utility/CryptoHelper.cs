using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BasketApi.Components.Helpers
{
    public class CryptoHelper
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        private static string Key = ConfigurationManager.AppSettings["AISEEncryptionKey"];
        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Hash(string value)
        {
            try
            {
                StringBuilder Sb = new StringBuilder();

                using (SHA256 hash = SHA256Managed.Create())
                {
                    Encoding enc = Encoding.UTF8;
                    Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                    foreach (Byte b in result)
                        Sb.Append(b.ToString("x2"));
                }

                return Sb.ToString();
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                return string.Empty;
            }

        }

        public static string CreatePasswordResetToken(DateTime passwordResetTokenExpiryDate, string modelEmail)
        {
            try
            {
                return Hash(modelEmail + passwordResetTokenExpiryDate.ToFileTime());
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                return string.Empty;
            }
          
        }


        //public static string Encrypt(string plainText)
        //{
        //    try
        //    {
        //        string passPhrase = Key;
        //        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        //        // so that the same Salt and IV values can be used when decrypting.  
        //        var saltStringBytes = Generate256BitsOfRandomEntropy();
        //        var ivStringBytes = Generate256BitsOfRandomEntropy();
        //        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        //        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        //        {
        //            var keyBytes = password.GetBytes(Keysize / 8);
        //            using (var symmetricKey = new RijndaelManaged())
        //            {
        //                symmetricKey.BlockSize = 256;
        //                symmetricKey.Mode = CipherMode.CBC;
        //                symmetricKey.Padding = PaddingMode.PKCS7;
        //                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
        //                {
        //                    using (var memoryStream = new MemoryStream())
        //                    {
        //                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        //                        {
        //                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        //                            cryptoStream.FlushFinalBlock();
        //                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
        //                            var cipherTextBytes = saltStringBytes;
        //                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
        //                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
        //                            memoryStream.Close();
        //                            cryptoStream.Close();
        //                            return Convert.ToBase64String(cipherTextBytes);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.LogError(ex);
        //        return string.Empty;
        //    }

        //}

        //public static string Decrypt(string cipherText)
        //{
        //    try
        //    {
        //        string passPhrase = Key;
        //        // Get the complete stream of bytes that represent:
        //        // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        //        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        //        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        //        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        //        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        //        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        //        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        //        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        //        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        //        {
        //            var keyBytes = password.GetBytes(Keysize / 8);
        //            using (var symmetricKey = new RijndaelManaged())
        //            {
        //                symmetricKey.BlockSize = 256;
        //                symmetricKey.Mode = CipherMode.CBC;
        //                symmetricKey.Padding = PaddingMode.PKCS7;
        //                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
        //                {
        //                    using (var memoryStream = new MemoryStream(cipherTextBytes))
        //                    {
        //                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        //                        {
        //                            var plainTextBytes = new byte[cipherTextBytes.Length];
        //                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        //                            memoryStream.Close();
        //                            cryptoStream.Close();
        //                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.LogError(ex);
        //        return string.Empty;
        //    }

        //}

        //private static byte[] Generate256BitsOfRandomEntropy()
        //{
        //    try
        //    {
        //        var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        //        using (var rngCsp = new RNGCryptoServiceProvider())
        //        {
        //            // Fill the array with cryptographically secure random bytes.
        //            rngCsp.GetBytes(randomBytes);
        //        }
        //        return randomBytes;
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.LogError(ex);
        //        return null;
        //    }

        //}

        public static string EncryptStringToBase64(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encrypted;

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }

                return Convert.ToBase64String(encrypted); // Convert the byte array to a base64-encoded string
            }
        }

        public static string DecryptStringFromBase64(string base64CipherText, byte[] key, byte[] iv)
        {
            byte[] cipherText = Convert.FromBase64String(base64CipherText); // Convert the base64-encoded string back to byte array

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                string plaintext = null;

                using (var msDecrypt = new System.IO.MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

                return plaintext;
            }
        }



    }
}
