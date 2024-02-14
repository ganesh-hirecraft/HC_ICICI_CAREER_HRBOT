using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class ClsCrypto
    {
        public static string DecryptUsingAES(string DataToEncrypt)
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {

                myRijndael.Mode = CipherMode.CBC;
                myRijndael.Padding = PaddingMode.PKCS7;
                myRijndael.FeedbackSize = 128;

                byte[] Key = Encoding.UTF8.GetBytes("$P@mOu$0172@0r!P");
                byte[] IV = Encoding.UTF8.GetBytes("@1O2j3D4e5F6g7P8");


                //DECRYPT FROM CRIPTOJS
                byte[] encrypted = Convert.FromBase64String(DataToEncrypt);

                // Decrypt the bytes to a string.
                string roundtrip = DecryptStringFromBytes(encrypted, Key, IV);

                return roundtrip;
            }
        }

        public static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
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

        public static string EncryptUsingAES(string plainText)
        {
            byte[] Key = Encoding.UTF8.GetBytes("$P@mOu$0172@0r!P");
            byte[] IV = Encoding.UTF8.GetBytes("@1O2j3D4e5F6g7P8");

            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

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

        public static string DecryptUserIDUsingAES(string DataToEncrypt, string encryptIV)
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {

                myRijndael.Mode = CipherMode.CBC;
                myRijndael.Padding = PaddingMode.PKCS7;
                myRijndael.FeedbackSize = 128;

                byte[] Key = Encoding.UTF8.GetBytes("$P@mOu$0172@0r!P");
                byte[] IV = Encoding.UTF8.GetBytes(encryptIV);


                //DECRYPT FROM CRIPTOJS
                byte[] encrypted = Convert.FromBase64String(DataToEncrypt);

                // Decrypt the bytes to a string.
                string roundtrip = DecryptStringFromBytes(encrypted, Key, IV);

                return roundtrip;
            }

            //try
            //{
            //    UTF8Encoding uTF8 = new UTF8Encoding();
            //    string Key_data = "$P@mOu$0172@0r!P";
            //    //string Key_data = ConfigurationManager.AppSettings["EncryptionKey"];
            //    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            //    aes.BlockSize = 128;
            //    aes.KeySize = 128;
            //    aes.IV = Encoding.UTF8.GetBytes(encryptIV);
            //    byte[] iv = aes.IV;
            //    aes.Key = Encoding.UTF8.GetBytes(Key_data);
            //    aes.Mode = CipherMode.CBC;
            //    aes.Padding = PaddingMode.PKCS7;
            //    using (AesCryptoServiceProvider aes_data = new AesCryptoServiceProvider())
            //    {
            //        byte[] iv_str = Convert.FromBase64String(Convert.ToBase64String(aes.IV));
            //        byte[] keyBytes = Convert.FromBase64String(Convert.ToBase64String(aes.Key));
            //        var decryptor = aes_data.CreateDecryptor(keyBytes, iv_str);
            //        byte[] decryptedBytes = decryptor.TransformFinalBlock(uTF8.GetBytes(DataToEncrypt), 0, DataToEncrypt.Length);

            //        // Console.WriteLine("Decrypted: {0}", Encoding.UTF8.GetString(decryptedBytes));
            //        return Encoding.UTF8.GetString(decryptedBytes);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return "-1: " + ex.Message;
            //    //  return "";
            //}
        }

        public static string DecryptConfig(string cipherText, string Vector)
        {
            string plaintext = "";
            try
            {

                string Key_data = "$P@mOu$0172@0r!P"; // "6Le0DgMTAAAAANokdEEial1";
                //byte[] IVarray = Encoding.UTF8.GetBytes(IV);
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                //aes.BlockSize = 128;
                //aes.KeySize = 256;
                aes.IV = Encoding.UTF8.GetBytes(Vector);
                byte[] iv = aes.IV;
                aes.Key = Encoding.UTF8.GetBytes(Key_data);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                byte[] cipherTextKey = Encoding.UTF8.GetBytes(cipherText);
                using (AesCryptoServiceProvider aes_data = new AesCryptoServiceProvider())
                {
                    // Decrypt the text
                    byte[] iv_str = Convert.FromBase64String(Convert.ToBase64String(aes.IV));
                    byte[] keyBytes = Convert.FromBase64String(Convert.ToBase64String(aes.Key));
                    byte[] fromBase64ToBytes = Convert.FromBase64String(cipherText);
                    var decryptor = aes_data.CreateDecryptor(keyBytes, iv_str);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(fromBase64ToBytes, 0, fromBase64ToBytes.Length);
                    plaintext = Encoding.UTF8.GetString(decryptedBytes);
                }

            }

            catch (Exception ex)
            {
                plaintext = "-1: " + ex.Message;
                //  return "";
            }
            return plaintext;
        }

        public static string DecryptUsingAESCareer(string DataToEncrypt)
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {

                myRijndael.Mode = CipherMode.CBC;
                myRijndael.Padding = PaddingMode.PKCS7;
                myRijndael.FeedbackSize = 128;

                byte[] Key = Encoding.UTF8.GetBytes("AKsks749JWHHsu43");
                byte[] IV = Encoding.UTF8.GetBytes("As8730Jwimak39IU");


                //DECRYPT FROM CRIPTOJS
                byte[] encrypted = Convert.FromBase64String(DataToEncrypt);

                // Decrypt the bytes to a string.
                string roundtrip = DecryptStringFromBytes(encrypted, Key, IV);

                return roundtrip;
            }
        }
    }
}
