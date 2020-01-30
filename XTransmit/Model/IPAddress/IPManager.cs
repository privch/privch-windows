using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using XTransmit.Utility;

namespace XTransmit.Model.IPAddress
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    internal static class IPManager
    {
        /**
         * TODO - Remove unnecessary lockers
         * 
         * NOTE
         * If use List<IPProfile> it will cause an exception when the App exit 
         * when the number of IPProfile item is big (such as 60029 or more).
         */
        private static IPProfile[] IPArray;
        private static string IPXmlPath;

        private static readonly Random RandGen = new Random();
        private static readonly object locker = new object();

        public static IPProfile[] GetIPArray()
        {
            return IPArray;
        }

        public static void Load(string pathIpXml)
        {
            if (FileUtil.XmlDeserialize(pathIpXml, typeof(IPProfile[])) is IPProfile[] ipArray)
            {
                IPArray = ipArray;
            }
            else
            {
                IPArray = Array.Empty<IPProfile>();
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
        public static void Save(IPProfile[] ipArray)
        {
            FileUtil.XmlSerialize(IPXmlPath, ipArray);
            IPArray = ipArray;
        }

        // determin wether the ip list has been changed
        public static bool HasChangesToFile(IPProfile[] ipArray = null)
        {
            if (ipArray == null)
            {
                ipArray = IPArray;
            }

            byte[] md5Data = TextUtil.GetMD5(ipArray);
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
                if (ipLine.StartsWith(@"#", StringComparison.Ordinal) || ipLine.StartsWith(@"//", StringComparison.Ordinal))
                {
                    continue;
                }

                // parse ip
                string[] ipBytes = ipLine.Split(separatorIPByte, StringSplitOptions.RemoveEmptyEntries);
                if (ipBytes == null || ipBytes.Length != 4)
                {
                    continue;
                }

                if (ipBytes[3].Contains(@"/") || ipBytes[3].Contains(@"-"))
                {
                    // more ip
                    string[] ipByte4Range = ipBytes[3].Split(separatorByte4, StringSplitOptions.RemoveEmptyEntries);
                    if (ipByte4Range == null || ipByte4Range.Length != 2)
                    {
                        continue;
                    }

                    int ipByte4From, ipByte4To;
                    try
                    {
                        ipByte4From = int.Parse(ipByte4Range[0], CultureInfo.InvariantCulture);
                        ipByte4To = int.Parse(ipByte4Range[1], CultureInfo.InvariantCulture);
                    }
                    catch { continue; }
                    if (ipByte4From < 0 || ipByte4To < ipByte4From)
                    {
                        continue;
                    }

                    string ipByte123 = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}.";
                    for (int ipByte4 = ipByte4From; ipByte4 <= ipByte4To; ipByte4++)
                    {
                        try
                        {
                            System.Net.IPAddress ip = System.Net.IPAddress.Parse(
                                ipByte123 + ipByte4.ToString(CultureInfo.InvariantCulture));
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
            lock (locker)
            {
                if (IPArray != null && IPArray.Length > 0)
                {
                    int i = RandGen.Next(0, IPArray.Length - 1);
                    return IPArray[i];
                }
                else
                {
                    return null;
                }
            }
        }

        public static string GetGenerate()
        {
            lock (locker)
            {
                return $"{RandGen.Next(1, 255)}.{RandGen.Next(0, 255)}.{RandGen.Next(0, 255)}.{RandGen.Next(0, 255)}";
            }
        }
    }
}
