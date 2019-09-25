using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace XTransmit.Model.Network
{
    /**
     * Updated: 2019-09-26
     */
    public static class IPAddressManager
    {
        public static List<string> IPList { get; private set; }
        private static string IPPath;
        private static readonly Random RandGen = new Random();

        public static void Load(string fileIpXml)
        {
            List<string> ipList = null;
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(fileIpXml, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<string>));
                ipList = (List<string>)xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception) { ipList = new List<string>(); }
            finally
            {
                fileStream?.Dispose();
            }

            IPList = ipList;
            IPPath = fileIpXml;
        }
        public static void Reload()
        {
            if (!string.IsNullOrWhiteSpace(IPPath))
            {
                Load(IPPath);
            }
        }

        public static void Save(string fileIpXml = null)
        {
            if (fileIpXml == null)
                fileIpXml = IPPath;

            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(fileIpXml, false, new System.Text.UTF8Encoding(false));
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<string>));
                xmlSerializer.Serialize(streamWriter, IPList);
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
                new XmlSerializer(typeof(List<string>)).Serialize(streamWriter, IPList);
                streamWriter.Flush();
                memStream.Flush();

                memStream.Position = 0;
                md5Current = md5.ComputeHash(memStream);
            }

            // original data
            byte[] md5Original;
            using (MD5 md5 = MD5.Create())
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(IPPath, FileMode.Open);
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

        /**<summary>
         * Read ip data from file.
         * </summary>
         */
        public static HashSet<string> Import(string fileImportUtf8)
        {
            char[] separatorIPByte = new char[] { '.' };
            char[] separatorByte4 = new char[] { '/', '-' };
            HashSet<string> ipList = new HashSet<string>();

            // read file
            string[] fileLines;
            try { fileLines = File.ReadAllLines(fileImportUtf8, Encoding.UTF8); }
            catch { return ipList; }

            // load ip address
            foreach (string fileLine in fileLines)
            {
                string ipLine = fileLine.Trim();

                // skip comment line
                if (ipLine.StartsWith(@"#") || ipLine.StartsWith(@"//"))
                    continue;

                // parse ip
                string[] ipBytes = ipLine.Split(separatorIPByte, StringSplitOptions.RemoveEmptyEntries);
                if (ipBytes == null || ipBytes.Length != 4)
                    continue;

                if (ipBytes[3].Contains(@"/") || ipBytes[3].Contains(@"-"))
                {
                    // more ip
                    string[] ipByte4Range = ipBytes[3].Split(separatorByte4, StringSplitOptions.RemoveEmptyEntries);
                    if (ipByte4Range == null || ipByte4Range.Length != 2)
                        continue;

                    int ipByte4From, ipByte4To;
                    try
                    {
                        ipByte4From = int.Parse(ipByte4Range[0]);
                        ipByte4To = int.Parse(ipByte4Range[1]);
                    }
                    catch { continue; }
                    if (ipByte4From < 0 || ipByte4To < ipByte4From)
                        continue;

                    string ipByte123 = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}.";
                    for (int ipByte4 = ipByte4From; ipByte4 <= ipByte4To; ipByte4++)
                    {
                        try
                        {
                            IPAddress ip = IPAddress.Parse(ipByte123 + ipByte4.ToString());
                            ipList.Add(ip.ToString());
                        }
                        catch { continue; }
                    }
                }
                else // one ip
                {
                    try
                    {
                        IPAddress ip = IPAddress.Parse(ipLine);
                        ipList.Add(ip.ToString());
                    }
                    catch { continue; }
                }
            }

            return ipList;
        }

        public static string GetRandom()
        {
            if (IPList != null)
            {
                int i = RandGen.Next(0, IPList.Count - 1);
                return IPList[i];
            }
            else
            {
                return null;
            }
        }

        public static string GetGenerate()
        {
            return $"{RandGen.Next(1, 255)}.{RandGen.Next(0, 255)}.{RandGen.Next(0, 255)}.{RandGen.Next(0, 255)}";
        }
    }
}
