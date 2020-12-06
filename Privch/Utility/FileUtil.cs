using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PrivCh.Utility
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class FileUtil
    {
        public static bool CheckMD5(string filePath, string md5Hex)
        {
            byte[] md5File = GetMD5(filePath);
            if (md5File == null)
            {
                return false;
            }

            // convert input md5 hex string to the "0x" format
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < md5File.Length; i++)
            {
                sBuilder.Append(md5File[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return sBuilder.ToString().Equals(md5Hex, StringComparison.OrdinalIgnoreCase);
        }

        /*
         * TODO - Use cryptographically stronger options
         * https://docs.microsoft.com/en-us/visualstudio/code-quality/ca5351?view=vs-2019
         */
        [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "<Pending>")]
        public static byte[] GetMD5(string filePath)
        {
            // original data
            byte[] md5Value;
            using (MD5 md5 = MD5.Create())
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = File.OpenRead(filePath);
                    md5Value = md5.ComputeHash(fileStream);
                    fileStream.Close();
                }
                catch
                {
                    return null;
                }
                finally
                {
                    fileStream?.Dispose();
                }
            }

            return md5Value;
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
            catch
            {
                return false;
            }
            finally
            {
                fileStream?.Dispose();
            }

            return true;
        }


        /** XML ------------------------------------------------------------------------
         */
        public static void XmlSerialize(string pathXml, object objInput)
        {
            StreamWriter swXml = null;
            try
            {
                swXml = new StreamWriter(pathXml, false, new UTF8Encoding(false));
                new XmlSerializer(objInput.GetType()).Serialize(swXml, objInput);

                swXml.Close();
            }
            catch { }
            finally
            {
                swXml?.Dispose();
            }
        }

        public static object XmlDeserialize(string pathXml, Type type)
        {
            object result = null;
            FileStream fsXml = null;
            XmlReader xmlReader = null;

            try
            {
                fsXml = new FileStream(pathXml, FileMode.Open);
                xmlReader = XmlReader.Create(fsXml);
                result = new XmlSerializer(type).Deserialize(xmlReader);

                xmlReader.Close();
                fsXml.Close();
            }
            catch { }
            finally
            {
                xmlReader?.Dispose();
                fsXml?.Dispose();
            }

            return result;
        }


        public static bool UncompressGZ(string filePath, byte[] content)
        {
            MemoryStream memContent = null;
            GZipStream gzStream = null;
            FileStream fileStream = null;

            try
            {
                memContent = new MemoryStream(content);
                gzStream = new GZipStream(memContent, CompressionMode.Decompress, false);
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                // Because the uncompressed size of the file is unknown, we are using an arbitrary buffer size.
                byte[] buffer = new byte[4096];
                int count;
                while ((count = gzStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, count);
                }

                memContent.Close();
                gzStream.Close();
                fileStream.Close();
            }
            catch
            {
                return false;
            }
            finally
            {
                memContent?.Dispose();
                gzStream?.Dispose();
                fileStream?.Dispose();
            }

            return true;
        }
    }
}
