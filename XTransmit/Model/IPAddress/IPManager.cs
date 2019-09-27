using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XTransmit.Utility;

namespace XTransmit.Model.IPAddress
{
    /**
     * Updated: 2019-09-28
     */
    public static class IPManager
    {
        public static List<IPProfile> IPList;
        private static string IPXmlPath;
        private static readonly Random RandGen = new Random();

        public static void Load(string pathIpXml)
        {
            if (FileUtil.XmlDeserialize(pathIpXml, typeof(List<IPProfile>)) is List<IPProfile> listIp)
            {
                IPList = listIp;
            }
            else
            {
                IPList = new List<IPProfile>();
            }
            
            IPXmlPath = pathIpXml;
        }
        public static void Reload()
        {
            if (!string.IsNullOrWhiteSpace(IPXmlPath))
            {
                Load(IPXmlPath);
            }
        }

        // save, save as
        public static void Save(string pathIpXml = null)
        {
            if (string.IsNullOrWhiteSpace(pathIpXml))
            {
                pathIpXml = IPXmlPath;
            }

            FileUtil.XmlSerialize(pathIpXml, IPList);
        }

        // determin wether the ip list has been changed
        public static bool HasChangesToFile(List<IPProfile> ipList = null)
        {
            if (ipList == null)
            {
                ipList = IPList;
            }

            byte[] md5Data = TextUtil.GetXmlMD5(ipList);
            byte[] md5File = FileUtil.GetMD5(IPXmlPath);

            return (md5Data != null && md5File != null) ? !md5File.SequenceEqual(md5Data) : true;
        }

        /**<summary>
         * Read ip data from file.
         * TODO - Test
         * </summary>
         */
        public static HashSet<string> Import(string pathTxtUtf8)
        {
            char[] separatorIPByte = new char[] { '.' };
            char[] separatorByte4 = new char[] { '/', '-' };
            HashSet<string> ipList = new HashSet<string>();

            // read file
            string[] fileLines;
            try { fileLines = File.ReadAllLines(pathTxtUtf8, Encoding.UTF8); }
            catch
            {
                return ipList;
            }

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
                            System.Net.IPAddress ip = System.Net.IPAddress.Parse(ipByte123 + ipByte4.ToString());
                            ipList.Add(ip.ToString());
                        }
                        catch { continue; }
                    }
                }
                else // one ip
                {
                    try
                    {
                        System.Net.IPAddress ip = System.Net.IPAddress.Parse(ipLine);
                        ipList.Add(ip.ToString());
                    }
                    catch { continue; }
                }
            }

            return ipList;
        }

        public static IPProfile GetRandom()
        {
            if (IPList != null && IPList.Count > 0)
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
