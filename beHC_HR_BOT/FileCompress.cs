using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class FileCompress
    {
        //   private static string ModuleName = "Data Compression";

        public static byte[] opDataZip(string oData)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(oData);
                using (MemoryStream byteStream = new MemoryStream(bytes))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                        {
                            byteStream.CopyTo(gzipStream);
                        }
                        return memoryStream.ToArray();
                    }
                }

            }
            catch (Exception ex)
            {
                Common.Logs(ex.ToString());
                return null;
            }

        }

        public static byte[] opDataZip(byte[] oData)
        {
            try
            {
                byte[] oReturn = null;
                if (oData.Length > 0)
                {
                    using (MemoryStream byteStream = new MemoryStream(oData))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                            {
                                byteStream.CopyTo(gzipStream);
                            }
                            oReturn = memoryStream.ToArray();
                        }
                    }

                }
                return oReturn;



            }
            catch (Exception ex)
            {
                Common.Logs(ex.ToString());
                return null;
            }

        }

        public static string opDataUnzip(byte[] bytes)
        {
            try
            {
                //if (bytes == null)
                //    return "";
                using (MemoryStream byteStream = new MemoryStream(bytes))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (GZipStream gzipStream = new GZipStream(byteStream, CompressionMode.Decompress))
                        {
                            gzipStream.CopyTo(memoryStream);
                        }
                        return Encoding.UTF8.GetString(memoryStream.ToArray());
                        //return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }

            }
            catch (Exception ex)
            {
                Common.Logs(ex.ToString());
                return Convert.ToBase64String(bytes);
                //return Encoding.UTF8.GetString(bytes);
            }

        }
    }
}
