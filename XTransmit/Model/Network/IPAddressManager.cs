using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace XTransmit.Model.Network
{
    /**
     * Updated: 2019-08-02
     */
    public static class IPAddressManager
    {
        public static DataSet DataSetIP { get; private set; }

        public static void Load(string fileIpXml)
        {
            DataSetIP = new DataSet("DataSet-IP");
            try { DataSetIP.ReadXml(fileIpXml); }
            catch (Exception) { }

            DataSetIP.AcceptChanges();
        }

        public static void Save(string fileIpXml)
        {
            DataSetIP.AcceptChanges();
            try { DataSetIP.WriteXml(fileIpXml); }
            catch (Exception) { }
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
    }
}
