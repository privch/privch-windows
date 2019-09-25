using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace XTransmit.Model.UserAgent
{
    /**
     * Updated: 2019-09-24
     */
    public static class UserAgentManager
    {
        public static List<UserAgentProfile> ListUserAgent;
        private static string PathUserAgent;
        private static readonly Random RandGen = new Random();

        // Init ua data by deserialize xml file
        public static void Load(string fileUserAgent)
        {
            List<UserAgentProfile> listUserAgent = null;
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(fileUserAgent, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<UserAgentProfile>));
                listUserAgent = (List<UserAgentProfile>)xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception) { }
            finally
            {
                fileStream?.Dispose();
            }

            ListUserAgent = listUserAgent;
            PathUserAgent = fileUserAgent;
        }
        public static void Reload()
        {
            if (!string.IsNullOrWhiteSpace(PathUserAgent))
            {
                Load(PathUserAgent);
            }
        }

        public static void Save(string fileUserAgent = null)
        {
            if (string.IsNullOrWhiteSpace(fileUserAgent))
            {
                fileUserAgent = PathUserAgent;
            }

            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(fileUserAgent, false, new System.Text.UTF8Encoding(false));
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<UserAgentProfile>));
                xmlSerializer.Serialize(streamWriter, ListUserAgent);
                streamWriter.Close();
            }
            catch (Exception) { }
            finally
            {
                streamWriter?.Dispose();
            }
        }

        public static bool HasChanges()
        {
            // current data
            byte[] md5Current;
            using (MemoryStream memStream = new MemoryStream())
            using (StreamWriter streamWriter = new StreamWriter(memStream, new System.Text.UTF8Encoding(false)))
            using (MD5 md5 = MD5.Create())
            {
                new XmlSerializer(typeof(List<UserAgentProfile>)).Serialize(streamWriter, ListUserAgent);
                streamWriter.Close();

                memStream.Flush();
                memStream.Position = 0;

                md5Current = md5.ComputeHash(memStream);                
                memStream.Close();
            }

            // original data
            byte[] md5Original;
            using (MD5 md5 = MD5.Create())
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(PathUserAgent, FileMode.Open);
                    md5Original = md5.ComputeHash(fileStream);
                    fileStream.Close();
                }
                catch (Exception)
                {
                    return true;
                }
                finally
                {
                    fileStream?.Dispose();
                }
            }

            return !md5Original.SequenceEqual(md5Current);
        }

        public static UserAgentProfile GetRandom()
        {
            if (ListUserAgent != null)
            {
                int i = RandGen.Next(0, ListUserAgent.Count - 1);
                return ListUserAgent[i];
            }
            else
            {
                return null;
            }
        }
    }
}
