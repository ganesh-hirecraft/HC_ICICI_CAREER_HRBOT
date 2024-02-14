using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace beHC_HR_BOT
{
    public class Cryptographer
    {
        #region "Members"
        const string DefaultKey = "74@8j9{MT_#L3xU3";
        const string DefaultIV = "4\0#w28/";

        public byte[] EncryptionKey;
        public byte[] IV;
        private System.Security.Cryptography.RC2CryptoServiceProvider rc2;
        #endregion

        public Cryptographer()
        {


            this.rc2 = new RC2CryptoServiceProvider();
            this.rc2.KeySize = 128;

            EncryptionKey = GetDefaultKey();
            IV = GetDefaultVector();

            rc2.GenerateKey();
            rc2.Key = EncryptionKey;
            rc2.GenerateIV();
            rc2.IV = IV;
        }

        public static string getconnectionString()
        {
            string connection = "";
            if (Convert.ToInt64(System.Configuration.ConfigurationManager.ConnectionStrings["IsEncrypted"]) == 1)
            {
                Cryptographer crypto = new Cryptographer();
                connection = crypto.opDecryptPasswordBase64(System.Configuration.ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString);
            }
            else
            {
                connection = System.Configuration.ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;
            }
            return connection;

        }



        public static byte[] GetDefaultKey()
        {
            return UTF8Encoding.UTF8.GetBytes(DefaultKey);
        }

        public static byte[] GetDefaultVector()
        {
            return UTF8Encoding.UTF8.GetBytes(DefaultIV);
        }

        public byte[] GenerateKey()
        {
            while (true)
            {
                rc2.GenerateKey();
                if (UTF8Encoding.UTF8.GetString(rc2.Key).IndexOf("'") == -1 && UTF8Encoding.UTF8.GetString(rc2.Key).IndexOf((char)34) == -1)
                    return rc2.Key;
            }
        }

        public byte[] GenerateVector()
        {
            while (true)
            {
                rc2.GenerateIV();
                if (UTF8Encoding.UTF8.GetString(rc2.IV).IndexOf("'") == -1 && UTF8Encoding.UTF8.GetString(rc2.IV).IndexOf((char)34) == -1)
                    return rc2.IV;
            }
        }

        public string mEncryptURLEncode(string text)
        {
            byte[] plainBytes = Encoding.Unicode.GetBytes(text);
            byte[] cipherBytes;

            using (ICryptoTransform sse = rc2.CreateEncryptor())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, sse, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                        cs.Dispose();
                    }
                    cipherBytes = ms.ToArray();
                    ms.Close();
                    ms.Dispose();
                }
                sse.Dispose();

            }

            string sEncrypt = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
            return HttpUtility.UrlEncode(sEncrypt);

        }

        public string mDecryptURLEncode(string EncryptedText)
        {
            string DecryptedText = "";
            System.Security.Cryptography.ICryptoTransform ssd = rc2.CreateDecryptor();

            string sEncrypt = HttpUtility.UrlDecode(EncryptedText);

            byte[] cipherBytes = Convert.FromBase64String(sEncrypt);
            byte[] initialText = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(cipherBytes))
            {
                using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, ssd, System.Security.Cryptography.CryptoStreamMode.Read))
                {
                    int iCount = opGetArrLength(cipherBytes);
                    initialText = new Byte[cipherBytes.Length];
                    cs.Read(initialText, 0, initialText.Length);
                    cs.Close();
                    cs.Dispose();
                }
                ms.Close();
                ms.Dispose();

            }
            DecryptedText = System.Text.UTF8Encoding.UTF8.GetString(initialText);
            return DecryptedText = DecryptedText.Replace("\0", "");
        }

        public string mEncryptUTF(string text)
        {
            byte[] plainBytes = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            byte[] cipherBytes;

            using (System.Security.Cryptography.ICryptoTransform sse = rc2.CreateEncryptor())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, sse, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                        cs.Dispose();
                    }
                    cipherBytes = ms.ToArray();
                    ms.Close();
                    ms.Dispose();
                }
                sse.Dispose();

            }

            return Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

        }

        public string mDecryptUTF(string EncryptedText)
        {
            string DecryptedText = "";
            ICryptoTransform ssd = rc2.CreateDecryptor();
            byte[] cipherBytes = Convert.FromBase64String(EncryptedText);
            byte[] initialText = null;
            using (MemoryStream ms = new MemoryStream(cipherBytes))
            {
                using (CryptoStream cs = new CryptoStream(ms, ssd, CryptoStreamMode.Read))
                {
                    int iCount = opGetArrLength(cipherBytes);
                    initialText = new Byte[cipherBytes.Length];
                    cs.Read(initialText, 0, initialText.Length);
                    cs.Close();
                    cs.Dispose();
                }
                ms.Close();
                ms.Dispose();

            }
            DecryptedText = UTF8Encoding.UTF8.GetString(initialText);
            return DecryptedText = DecryptedText.Replace("\0", "");
        }

        public string mEncryptPassword(string text)
        {
            byte[] plainBytes = Encoding.ASCII.GetBytes(text.ToCharArray());
            byte[] cipherBytes;

            using (ICryptoTransform sse = rc2.CreateEncryptor())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, sse, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                        cs.Dispose();
                    }
                    cipherBytes = ms.ToArray();
                    ms.Close();
                    ms.Dispose();
                }
                sse.Dispose();
            }
            return Encoding.ASCII.GetString(cipherBytes);
            // return Convert.ToBase64String (cipherBytes);

        }

        public string opDecryptPassword(string text)
        {
            string DecryptedText = "";
            ICryptoTransform ssd = rc2.CreateDecryptor();
            byte[] cipherBytes = Encoding.ASCII.GetBytes(text);
            byte[] initialText = null;
            using (MemoryStream ms = new MemoryStream(cipherBytes))
            {
                using (CryptoStream cs = new CryptoStream(ms, ssd, CryptoStreamMode.Read))
                {
                    initialText = new Byte[cipherBytes.Length];
                    cs.Read(initialText, 0, initialText.Length);
                }
            }


            DecryptedText = UTF8Encoding.UTF8.GetString(initialText);
            return DecryptedText = DecryptedText.Replace("\0", "");

        }

        private int opGetArrLength(byte[] Array)
        {
            try
            {

                int icount = 0;
                for (int i = Array.Length - 1; i >= 0; i--)
                {
                    if (Array[i] == 0)
                        icount++;
                    if (Array[i] > 0)
                        break;
                }

                return icount;
            }
            catch
            {
                return 0;
            }
        }

        #region [Base64 encrypt and decrypt]

        public string opEncryptPasswordBase64(string text)
        {
            byte[] plainBytes = Encoding.ASCII.GetBytes(text.ToCharArray());
            byte[] cipherBytes;

            using (ICryptoTransform sse = rc2.CreateEncryptor())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, sse, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                        cs.Dispose();
                    }
                    cipherBytes = ms.ToArray();
                    ms.Close();
                    ms.Dispose();
                }
                sse.Dispose();
            }

            return Convert.ToBase64String(cipherBytes);

        }

        public string opDecryptPasswordBase64(string encryptedText)
        {
            string DecryptedText = "";
            ICryptoTransform ssd = rc2.CreateDecryptor();
            byte[] cipherBytes = Convert.FromBase64String(encryptedText);
            byte[] initialText = null;
            using (MemoryStream ms = new MemoryStream(cipherBytes))
            {
                using (CryptoStream cs = new CryptoStream(ms, ssd, CryptoStreamMode.Read))
                {
                    initialText = new Byte[cipherBytes.Length];
                    cs.Read(initialText, 0, initialText.Length);
                }
            }


            DecryptedText = UTF8Encoding.UTF8.GetString(initialText);
            return DecryptedText = DecryptedText.Replace("\0", "");

        }

        #endregion


        #region [Triple DES Algorithm  Password Encryption  and Decryption]

        public string EncryptTextToMemory(string Data)
        {
            byte[] Key = Encoding.ASCII.GetBytes("74@8j9{MT_#L3xU3");
            byte[] IV = Encoding.ASCII.GetBytes("4\0#w28/_*9)$#@");
            try
            {

                // Create a MemoryStream.
                MemoryStream mStream = new MemoryStream();

                // Create a new TripleDES object.
                TripleDES tripleDESalg = TripleDES.Create();

                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                CryptoStream cStream = new CryptoStream(mStream,
                    tripleDESalg.CreateEncryptor(Key, IV),
                    CryptoStreamMode.Write);

                // Convert the passed string to a byte array.
                byte[] toEncrypt = new ASCIIEncoding().GetBytes(Data);

                // Write the byte array to the crypto stream and flush it.
                cStream.Write(toEncrypt, 0, toEncrypt.Length);
                cStream.FlushFinalBlock();

                // Get an array of bytes from the 
                // MemoryStream that holds the 
                // encrypted data.
                byte[] ret = mStream.ToArray();

                // Close the streams.
                cStream.Close();
                mStream.Close();

                // Return the encrypted buffer.
                return Convert.ToBase64String(ret.ToArray());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }

        }

        public string DecryptTextFromMemory(string encryptedText)
        {
            //Athavan 07April 2016:
            // Check  If  encryptedText  is  a  valid  Triple Des  encryption string  .
            //When  the  TripleDES  encrypted string  contains  '=' character  and  when it  is  sent from  the browser  url  , padding character  ==  will escape .  
            //The solution  is  Triple DES encryption string length  will  always  be  divided  by 4 .  If the  length % 4 != 0 , then  add  padding characters '==' till  the  string length %4 == 0
            if (encryptedText.Length % 4 != 0)
            {
                //Todo : Check  with various  encryption string to decide  how  many padding  characters  need to  be added. Now two padding characters are added
                encryptedText = encryptedText + "==";
            }
            byte[] Key = Encoding.ASCII.GetBytes("74@8j9{MT_#L3xU3");
            byte[] IV = Encoding.ASCII.GetBytes("4\0#w28/_*9)$#@");
            try
            {
                byte[] Data = Convert.FromBase64String(encryptedText);

                // Create a new MemoryStream using the passed 
                // array of encrypted data.
                MemoryStream msDecrypt = new MemoryStream(Data);

                // Create a new TripleDES object.
                TripleDES tripleDESalg = TripleDES.Create();

                // Create a CryptoStream using the MemoryStream 
                // and the passed key and initialization vector (IV).
                CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                    tripleDESalg.CreateDecryptor(Key, IV),
                    CryptoStreamMode.Read);

                // Create buffer to hold the decrypted data.
                byte[] fromEncrypt = new byte[Data.Length];

                // Read the decrypted data out of the crypto stream
                // and place it into the temporary buffer.
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                //Convert the buffer into a string and return it.
                return new ASCIIEncoding().GetString(fromEncrypt);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }

        #endregion
    }
}
