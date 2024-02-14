using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Security.Cryptography;

namespace beHC_HR_BOT
{
    public class Common
    {
        #region Connection
        public class Connection
        {
            private static string Con;
            public static string ConStr
            {
                get { return Con; }
                set { Con = getconnectionString(value); }
            }
        }
        #endregion

        public static string getconnectionString(string connection)
        {
            string con = "";
            if (Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["IsEncrypted"]) == 1)
            {
                Cryptographer crypto = new Cryptographer();
                con = crypto.opDecryptPasswordBase64(connection);
            }
            else
            {
                con = connection;
            }
            return con;

        }

        public string opValidationSplChars(string tString)
        {
            try
            {
                if (tString.ToLower().IndexOf("drop ") == 0 || tString.ToLower().IndexOf(" drop ") > 0)
                    tString = tString.ToLower().Replace("drop ", "");

                if (tString.ToLower().IndexOf("shutdown ") == 0 || tString.ToLower().IndexOf(" shutdown ") > 0)
                    tString = tString.ToLower().Replace("shutdown ", "");

                if (tString.ToLower().IndexOf("select ") == 0 || tString.ToLower().IndexOf(" select ") > 0)
                    tString = tString.ToLower().Replace("select ", "");

                return tString.Replace(';', ',').Replace(Convert.ToChar(39), ' ').Replace("--", " ").Replace('*', ' ').Replace("<", "").Replace(">", "").Replace("&", "");
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string CreateXMLData(Object objCandidate)
        {
            XmlDocument xmlDoc = new XmlDocument();   //Represents an XML document, 
            // Initializes a new instance of the XmlDocument class.          
            XmlSerializer xmlSerializer = new XmlSerializer(objCandidate.GetType());
            // Creates a stream whose backing store is memory. 
            using (MemoryStream xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, objCandidate);
                xmlStream.Position = 0;
                //Loads the XML document from the specified string.
                xmlDoc.Load(xmlStream);
                return xmlDoc.InnerXml;
            }
        }

        public static void Logs(string msg)
        {
            try
            {
                string LogPath = System.Configuration.ConfigurationManager.AppSettings["DirectryPath"];
                if (!Directory.Exists(LogPath + @"\Log"))
                    Directory.CreateDirectory(LogPath + @"\Log");
                string repeatChar = new string('*', 50);
                File.AppendAllText(
                    LogPath + @"\Log\" + DateTime.Now.ToString("dd_MMM_yyyy") + ".txt",
                    repeatChar +
                    Environment.NewLine + "Error Date and time: " + DateTime.Now.ToString() + Environment.NewLine +
                    msg +
                    Environment.NewLine +
                    repeatChar +
                    Environment.NewLine);
            }
            catch (Exception ex)
            {
                Logs(ex.ToString());
            }
        }

        public static void ActivityLogs(string msg)
        {
            try
            {
                string ALogPath = System.Configuration.ConfigurationManager.AppSettings["ActivityLog"];
                if (!Directory.Exists(ALogPath + @"\ActivityLog"))
                    Directory.CreateDirectory(ALogPath + @"\ActivityLog");
                string repeatChar = new string('*', 50);
                File.AppendAllText(
                    ALogPath + @"\ActivityLog\" + DateTime.Now.ToString("dd_MMM_yyyy") + ".txt",
                    repeatChar +
                    Environment.NewLine + "Error Date and time: " + DateTime.Now.ToString() + Environment.NewLine +
                    msg +
                    Environment.NewLine +
                    repeatChar +
                    Environment.NewLine);
            }
            catch (Exception ex)
            {
                Logs(ex.ToString());
            }
        }

        /// <summary>
        /// Bind String value | Method | Accept Object and return string value
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string bStr(Object obj)
        {
            return Convert.ToString(obj);
        }
        /// <summary>
        /// Bind Intiger value| Method | Accept Object and return Integer 64 value
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int64 bInt64(Object obj)
        {
            return Convert.ToInt64(obj);
        }
        /// <summary>
        /// Bind Intiger value| Method | Accept Object and return Integer 32 value
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int32 bInt32(Object obj)
        {
            return Convert.ToInt32(obj);
        }

        public static bool bBoolean(Object obj)
        {
            return Convert.ToBoolean(obj);
        }

        public static int GetFileType(string FileExt)
        {
            Int32 tFileType;
            switch (FileExt)
            {
                #region New Changes
                case "xls":
                case "xlsx":
                    tFileType = 1;
                    break;
                case "pdf":
                    tFileType = 2;
                    break;
                case "rtf":
                    tFileType = 3;
                    break;
                case "txt":
                    tFileType = 4;
                    break;
                case "doc":
                case "docx":
                    tFileType = 5;
                    break;
                case "png":
                    tFileType = 6;
                    break;
                case "jpeg":
                case "jpg":
                    tFileType = 7;
                    break;
                case "html":
                    tFileType = 8;
                    break;
                case "htm":
                    tFileType = 9;
                    break;
                default:
                    tFileType = 0;
                    break;
                    #endregion
            }
            return tFileType;
        }

        //public static string GetUserName(long UserID, SqlConnection ObjCon)
        //{
        //    string tUserName = "";
        //    using (SqlCommand oSqlCmd = new SqlCommand("Select FirstName+' '+ISNULL(LastName,'') as UserName from HC_User_Main With(NOLOCK) where RID =@UserID ", ObjCon))
        //    {
        //        oSqlCmd.Parameters.AddWithValue("@UserID", UserID);
        //        tUserName = Common.bStr(oSqlCmd.ExecuteScalar());

        //    }
        //    return tUserName;
        //}


        public static apiResponse DuplicateResponse(apiResponse res, object data, Int64 tNoOfRecord = 0)
        {
            res = new apiResponse();
            res.ResponseCode = 101;
            res.ResponseStatus = "success";
            res.ResponseMessage = "Duplicate Data Found";
            res.NoOfRecord = tNoOfRecord;
            res.Data = data;
            return res;
        }
        public static apiResponse SuccessResponse(apiResponse res, object data, Int64 tNoOfRecord = 0)
        {
            res = new apiResponse();
            res.ResponseCode = 100;
            res.ResponseStatus = "success";
            res.ResponseMessage = "Request completed successfully.";
            res.NoOfRecord = tNoOfRecord;
            res.Data = data;
            return res;
        }
        public static apiResponse ErrorResponse(apiResponse res, Int64 tNoOfRecord = 0, string msg = "")
        {
            if (msg == "")
                msg = "Request failed with error.";
            res = new apiResponse();
            res.ResponseCode = 101;
            res.ResponseStatus = "error";
            res.ResponseMessage = msg;
            res.NoOfRecord = tNoOfRecord;
            res.Data = null;
            return res;
        }
        public static apiResponse ErrorRes(apiResponse res, Exception ex)
        {
            try
            {
                Logs(ex.ToString());
                res = new apiResponse();
                res.ResponseCode = 101;
                res.ResponseStatus = "error";
                res.ResponseMessage = "Request failed with error.";
                res.NoOfRecord = 0;

                Error objError = new Error();
                objError.ErrorMessage = ex.Message;
                objError.ErrorDetails = ex.StackTrace;

                res.Data = objError;
            }
            catch (Exception err)
            {
                Logs(err.ToString());
            }
            return res;
        }
        public static apiResponse ErrorRes(apiResponse res, string str)
        {
            try
            {
                Logs(str);

                res = new apiResponse();
                res.ResponseCode = 101;
                res.ResponseStatus = "error";
                res.ResponseMessage = "Request failed with error.";
                res.NoOfRecord = 0;

                Error objError = new Error();
                objError.ErrorMessage = str;
                objError.ErrorDetails = str;

                res.Data = objError;
            }
            catch (Exception err)
            {
                Logs(err.ToString());
            }
            return res;
        }
        public static apiResponse NotFoundResponse(apiResponse res, Int64 tNoOfRecord = 0)
        {
            res = new apiResponse();
            res.ResponseCode = 103;
            res.ResponseStatus = "no content";
            res.ResponseMessage = "No Data Found.";
            res.NoOfRecord = tNoOfRecord;
            res.Data = null;
            return res;
        }
        public static apiResponse InvalidResponse(apiResponse res, Int64 tNoOfRecord = 0)
        {
            res = new apiResponse();
            res.ResponseCode = 104;
            res.ResponseStatus = "invalid";
            res.ResponseMessage = "invalid request";
            res.NoOfRecord = tNoOfRecord;
            res.Data = null;
            return res;
        }
        public static apiResponse UnauthorizedResponse(apiResponse res, string msg = "")
        {
            if (msg == "")
                msg = "Authorization has been denied for this request.";
            res = new apiResponse();
            res.ResponseCode = 401;
            res.ResponseStatus = "Failed";
            res.ResponseMessage = msg;
            res.NoOfRecord = 0;
            res.Data = null;
            return res;
        }
        public static apiResponse SomethingWentWrongResponse(apiResponse res, string msg = "")
        {
            if (msg == "")
                msg = "Something went wrong.";
            res = new apiResponse();
            res.ResponseCode = 101;
            res.ResponseStatus = "Failed";
            res.ResponseMessage = msg;
            res.NoOfRecord = 0;
            res.Data = null;
            return res;
        }

        public DataSet beSPDataSet(string tdata)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(Connection.ConStr))
                {
                    conn.Open();

                    SqlCommand Comm = new SqlCommand(tdata, conn);
                    //Comm.Parameters.AddWithValue("@Query", tdata);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = Comm;
                    da.Fill(ds);
                    Comm.ExecuteNonQuery();

                    return ds;
                }
            }
            catch (Exception ex)
            {
                Logs(ex.ToString());
                return null;

            }
        }

        #region 
        public DataSet beSPDataSet(string spName, string tdata, ref string returnValue)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(Connection.ConStr))
                {
                    conn.Open();

                    SqlCommand Comm = new SqlCommand(spName, conn);
                    Comm.CommandType = CommandType.StoredProcedure;
                    Comm.Parameters.AddWithValue("@tData", tdata);

                    Comm.Parameters.Add("@Return", SqlDbType.Char, 500);
                    Comm.Parameters["@Return"].Direction = ParameterDirection.Output;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = Comm;

                    da.Fill(ds);
                    Comm.ExecuteNonQuery();
                    returnValue = Comm.Parameters["@Return"].Value.ToString();
                    return ds;
                }
            }
            catch (Exception ex)
            {
                Logs(ex.ToString());
                return null;

            }
        }
        public string beSPString(string spName, string tdata, ref string returnValue)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(Connection.ConStr))
                {
                    conn.Open();
                    SqlCommand Comm = new SqlCommand(spName, conn);
                    Comm.CommandType = CommandType.StoredProcedure;
                    Comm.Parameters.AddWithValue("@tData", tdata);

                    Comm.Parameters.Add("@Return", SqlDbType.Char, 500);
                    Comm.Parameters["@Return"].Direction = ParameterDirection.Output;
                    Comm.ExecuteNonQuery();
                    returnValue = Comm.Parameters["@Return"].Value.ToString();
                    return returnValue;
                }
            }
            catch (Exception ex)
            {
                Logs(ex.ToString());
                return returnValue;

            }
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public static List<Dictionary<string, object>> DataTableToJSONObject(DataTable table)
        {
            try
            {
                List<Dictionary<string, object>> pRow = new List<Dictionary<string, object>>();
                Dictionary<string, object> childRow;
                foreach (DataRow row in table.Rows)
                {
                    childRow = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        childRow.Add(col.ColumnName, row[col]);
                    }
                    pRow.Add(childRow);
                }
                return pRow;
            }
            catch (Exception ex)
            {
                Logs("DataTableToJSONObject(): " + Environment.NewLine + "Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
                return null;
            }
        }

        public static string beApiLogs(string MethodName, string Request, string Response)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlDataAdapter objDataAdapter = new SqlDataAdapter("select top 0 RID,MethodName,Request,Response from HC_IBankAPI_log", conn))
                    {
                        using (SqlCommandBuilder objCB = new SqlCommandBuilder(objDataAdapter))
                        {
                            DataSet objDataSet = new DataSet();
                            DataRow dr = null;
                            objDataAdapter.Fill(objDataSet, "HC_IBankAPI_log");

                            dr = objDataSet.Tables["HC_IBankAPI_log"].NewRow();

                            dr["MethodName"] = MethodName;
                            dr["Request"] = Request;
                            dr["Response"] = Response;

                            objDataSet.Tables["HC_IBankAPI_log"].Rows.Add(dr);

                            objDataAdapter.Update(objDataSet, "HC_IBankAPI_log");
                            return "1";


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs(ex.ToString());
                return "-9:" + ex.ToString();

            }
        }

        #endregion       

        #region Xml Serializer
        public static void SerializeXml<T>(T xml, ref string tdata)
        {
            MemoryStream ms = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(xml.GetType());
            xs.Serialize(ms, xml);
            tdata = Encoding.UTF8.GetString(ms.ToArray());
        }
        #endregion

        #region[File Extension]

        public static bool beFileExenetionCheck(string fileName)
        {
            bool fileExt = false;

            if (fileName.Contains(".exe") || fileName.Contains(".bat") || fileName.Contains(".js") || fileName.Contains(".htm") || fileName.Contains(".dll")
                || fileName.Contains(".rar") || fileName.Contains(".html") || fileName.Contains(".css") || fileName.Contains(".sh")
                || fileName.Contains(".xls") || fileName.Contains(".xlsx") || fileName.Contains(".jse")
                || fileName.Contains(".msg") || fileName.Contains(".dll") || fileName.Contains(".php") || fileName.Contains(".py") || fileName.Contains(".vb")
                || fileName.Contains(".scr") || fileName.Contains(".com") || fileName.Contains(".jar") || fileName.Contains(".bat") || fileName.Contains(".cmd")
                || fileName.Contains(".vbe") || fileName.Contains(".ws") || fileName.Contains(".wsf") || fileName.Contains(".lnk") || fileName.Contains(".vbs")
                )
            {
                fileExt = true;
            }

            return fileExt;
        }

        #endregion

        #region[]
        public static string getRegExPattern()
        {
            string regexPattern = @"(<script>\w.*</script>)|(alert\()|(<script src=)|(javascript:alert)|(<span>\w.*</span>)|(<p>\w.*</p>)|(<iframe src=)|(&lt;)|(a href=)|(<img src=)|(<section>)|(<!doctype>)|(<!doctype)|(<a)|(<abbr)|(<acronym)|(<address)|(<applet)|(<area)|(<article)|(<aside)|(<audio)|(<b)|(<base)|(<basefont)|(<bb)|(<bdo)|(<big)|(<blockquote)|(<body)|(<button)|(<canvas)|(<caption)|(<center)|(<cite)|(<code)|(<col)|(<colgroup)|(<command)|(<datagrid)|(<datalist)|(<dd)|(<del)|(<details)|(<dfn)|(<dialog)|(<dir)|(<div)|(<dl)|(<dt)|(<em)|(<embed)|(<eventsource)|(<fieldset)|(<figcaption)|(<figure)|(<font)|(<footer)|(<form)|(<frame)|(<frameset)|(<head)|(<header)|(<hgroup)|(<hr/)|(<html)|(<i)|(<iframe)|(<img)|(<input)|(<ins)|(<isindex)|(<kbd)|(<keygen)|(<label)|(<legend)|(<li)|(<link)|(<map)|(<mark)|(<menu)|(<meta)|(<meter)|(<nav)|(<noframes)|(<noscript)|(<object)|(<ol)|(<optgroup)|(<option)|(<output)|(<param)|(<pre)|(<progress)|(<q)|(<rp)|(<rt)|(<ruby)|(<s)|(<samp)|(<section)|(<select)|(<small)|(<source)|(<span)|(<strike)|(<strong)|(<style)|(<sub)|(<sup)|(<table)|(<tbody)|(<td)|(<textarea)|(<tfoot)|(<th)|(<thead)|(<time)|(<title)|(<tr)|(<track)|(<tt)|(<u)|(<ul)|(<var)|(<video)|(<wbr)";

            return regexPattern;
        }


        #endregion

    }

    #region Settings
    public class APIUsers
    {
        public string Type { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public int AccessRights { get; set; }
    }
    public class APISettings
    {
        public string Type { get; set; }
        public string SqlQuery { get; set; }
    }
    #endregion

    #region Master
    public class HCMaster
    {
        public Int64 Value { get; set; }
        public string Title { get; set; }
    }

    public class MasterNameSearch
    {
        public Int64 UserId { get; set; }
        public string type { get; set; }
        public string searchByTitle { get; set; }
    }

    public class GetPanelList
    {
        public Int64 PanelID { get; set; }
        public string PanelTitle { get; set; }
        public string VanueTitle { get; set; }
        public string InterviewDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

    }
    #endregion

    #region API Response
    public class apiResponse
    {
        public object Data { get; set; }
        public Int64 ResponseCode { get; set; }
        public string ResponseStatus { get; set; }
        public string ResponseMessage { get; set; }
        public Int64 NoOfRecord { get; set; }

    }
    #endregion

    #region Confirmation
    internal class Confirmation
    {
        public string Message { get; set; }
        public string ReturnData { get; set; }
        public string candidateNo { get; set; }
    }
    #endregion

    #region Error
    internal class Error
    {
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }

    }
    #endregion

    //public class ParamIstudio
    //{
    //    public long UserID { get; set; }
    //    public long InterviewID { get; set; }
    //}
    public class IstudioData
    {
        public string CandidateUrl { get; set; }
        public string CandidateToken { get; set; }
        public string UserUrl { get; set; }
    }
    public class CryptLib
    {

        UTF8Encoding _enc;

        RijndaelManaged _rcipher;

        byte[] _key, _pwd, _ivBytes, _iv;




        /***

         * Encryption mode enumeration

         */

        private enum EncryptMode { ENCRYPT, DECRYPT };




        static readonly char[] CharacterMatrixForRandomIVStringGeneration = {

                     'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',

                     'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',

                     'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',

                     'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

                     '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'

              };




        /**

         * This function generates random string of the given input length.

         *

         * @param _plainText

         *            Plain text to be encrypted

         * @param _key

         *            Encryption Key. You'll have to use the same key for decryption

         * @return returns encrypted (cipher) text

         */

        internal static string GenerateRandomIV(int length)
        {

            char[] _iv = new char[length];

            byte[] randomBytes = new byte[length];




            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {

                rng.GetBytes(randomBytes); //Fills an array of bytes with a cryptographically strong sequence of random values.

            }




            for (int i = 0; i < _iv.Length; i++)
            {

                int ptr = randomBytes[i] % CharacterMatrixForRandomIVStringGeneration.Length;

                _iv[i] = CharacterMatrixForRandomIVStringGeneration[ptr];

            }




            return new string(_iv);

        }










        public CryptLib()
        {

            _enc = new UTF8Encoding();

            _rcipher = new RijndaelManaged();

            _rcipher.Mode = CipherMode.CBC;

            _rcipher.Padding = PaddingMode.PKCS7;

            _rcipher.KeySize = 256;

            _rcipher.BlockSize = 128;

            _key = new byte[32];

            _iv = new byte[_rcipher.BlockSize / 8]; //128 bit / 8 = 16 bytes

            _ivBytes = new byte[16];

        }




        /**

         *

         * @param _inputText

         *            Text to be encrypted or decrypted

         * @param _encryptionKey

         *            Encryption key to used for encryption / decryption

         * @param _mode

         *            specify the mode encryption / decryption

         * @param _initVector

         *                      initialization vector

         * @return encrypted or decrypted string based on the mode

        */

        private String encryptDecrypt(string _inputText, string _encryptionKey, EncryptMode _mode, string _initVector)
        {




            string _out = "";// output string

            //_encryptionKey = MD5Hash (_encryptionKey);

            _pwd = Encoding.UTF8.GetBytes(_encryptionKey);

            _ivBytes = Encoding.UTF8.GetBytes(_initVector);




            int len = _pwd.Length;

            if (len > _key.Length)
            {

                len = _key.Length;

            }

            int ivLenth = _ivBytes.Length;

            if (ivLenth > _iv.Length)
            {

                ivLenth = _iv.Length;

            }




            Array.Copy(_pwd, _key, len);

            Array.Copy(_ivBytes, _iv, ivLenth);

            _rcipher.Key = _key;

            _rcipher.IV = _iv;




            if (_mode.Equals(EncryptMode.ENCRYPT))
            {

                //encrypt

                byte[] plainText = _rcipher.CreateEncryptor().TransformFinalBlock(_enc.GetBytes(_inputText), 0, _inputText.Length);

                _out = Convert.ToBase64String(plainText);

            }

            if (_mode.Equals(EncryptMode.DECRYPT))
            {

                //decrypt

                byte[] plainText = _rcipher.CreateDecryptor().TransformFinalBlock(Convert.FromBase64String(_inputText), 0, Convert.FromBase64String(_inputText).Length);

                _out = _enc.GetString(plainText);

            }

            _rcipher.Dispose();

            return _out;// return encrypted/decrypted string

        }




        /**

         * This function encrypts the plain text to cipher text using the key

         * provided. You'll have to use the same key for decryption

         *

         * @param _plainText

         *            Plain text to be encrypted

         * @param _key

         *            Encryption Key. You'll have to use the same key for decryption

         * @return returns encrypted (cipher) text

         */

        public string encrypt(string _plainText, string _key, string _initVector)
        {

            return encryptDecrypt(_plainText, _key, EncryptMode.ENCRYPT, _initVector);

        }




        /***

         * This funtion decrypts the encrypted text to plain text using the key

         * provided. You'll have to use the same key which you used during

         * encryprtion

         *

         * @param _encryptedText

         *            Encrypted/Cipher text to be decrypted

         * @param _key

         *            Encryption key which you used during encryption

         * @return encrypted value

         */




        public string decrypt(string _encryptedText, string _key, string _initVector)
        {

            return encryptDecrypt(_encryptedText, _key, EncryptMode.DECRYPT, _initVector);

        }




        /***

         * This function decrypts the encrypted text to plain text using the key

         * provided. You'll have to use the same key which you used during

         * encryption

         *

         * @param _encryptedText

         *            Encrypted/Cipher text to be decrypted

         * @param _key

         *            Encryption key which you used during encryption

         */

        public static string getHashSha256(string text, int length)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();

            byte[] hash = hashstring.ComputeHash(bytes);

            string hashString = string.Empty;

            foreach (byte x in hash)
            {

                hashString += String.Format("{0:x2}", x); //covert to hex string

            }

            if (length > hashString.Length)

                return hashString;

            else

                return hashString.Substring(0, length);

        }




        //this function is no longer used.

        private static string MD5Hash(string text)
        {

            MD5 md5 = new MD5CryptoServiceProvider();




            //compute hash from the bytes of text

            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));




            //get hash result after compute it

            byte[] result = md5.Hash;




            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < result.Length; i++)
            {

                //change it into 2 hexadecimal digits

                //for each byte

                strBuilder.Append(result[i].ToString("x2"));

            }

            Console.WriteLine("md5 hash of they key=" + strBuilder.ToString());

            return strBuilder.ToString();

        }




    }
}
