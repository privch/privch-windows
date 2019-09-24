using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

/**
 * TODO - Memory leak test
 * Updated: 2019-09-24
 */

namespace XTransmit.Utility
{
    public static class FileUtil
    {
        public static bool CheckMD5(string filePath, string md5Code)
        {
            byte[] md5Data;
            using (MD5 md5 = MD5.Create())
            {
                try
                {
                    FileStream fileStream = File.OpenRead(filePath);
                    md5Data = md5.ComputeHash(fileStream);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            // Create a new Stringbuilder to collect the bytes and create a string.
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < md5Data.Length; i++)
            {
                sBuilder.Append(md5Data[i].ToString("x2"));
            }

            return sBuilder.ToString().Equals(md5Code);
        }

        public static bool WriteUTF8(string filePath, string utf8Content)
        {
            FileStream fileStream = null;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(utf8Content);
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                fileStream.Write(buffer, 0, buffer.Length);
                fileStream.Close();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                fileStream?.Dispose();
            }

            return true;
        }

        public static bool UncompressGZ(string filePath, byte[] content)
        {
            FileStream fileStream = null;
            GZipStream gzStream = null;

            try
            {
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                gzStream = new GZipStream(new MemoryStream(content), CompressionMode.Decompress, false);

                // Because the uncompressed size of the file is unknown, we are using an arbitrary buffer size.
                byte[] buffer = new byte[4096];
                int count;
                while ((count = gzStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, count);
                }

                gzStream.Close();
                fileStream.Close();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                gzStream?.Dispose();
                fileStream?.Dispose();
            }

            return true;
        }
    }
}
