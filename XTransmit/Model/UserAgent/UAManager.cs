using System;
using System.Collections.Generic;
using System.Linq;
using XTransmit.Utility;

namespace XTransmit.Model.UserAgent
{
    public static class UAManager
    {
        public static List<UAProfile> UAList { get; private set; }
        private static string UAXmlPath;

        private static readonly Random RandGen = new Random();
        private static readonly object locker = new object();

        // Init ua data by deserialize xml file
        public static void Load(string pathUaXml)
        {
            if (FileUtil.XmlDeserialize(pathUaXml, typeof(List<UAProfile>)) is List<UAProfile> listUa)
            {
                UAList = listUa;
            }
            else
            {
                UAList = new List<UAProfile>();
            }

            UAXmlPath = pathUaXml;
        }
        public static void Reload()
        {
            if (!string.IsNullOrWhiteSpace(UAXmlPath))
            {
                Load(UAXmlPath);
            }
        }

        public static void Save(List<UAProfile> uaList)
        {
            FileUtil.XmlSerialize(UAXmlPath, uaList);
            UAList = uaList;
        }

        // determin wether the ip list has been changed
        public static bool HasChangesToFile(List<UAProfile> uaList = null)
        {
            if (uaList == null)
            {
                uaList = UAList;
            }

            byte[] md5Data = TextUtil.GetMD5(uaList);
            byte[] md5File = FileUtil.GetMD5(UAXmlPath);

            return (md5Data != null && md5File != null) ? !md5File.SequenceEqual(md5Data) : true;
        }

        public static UAProfile GetRandom()
        {
            lock (locker)
            {
                if (UAList != null && UAList.Count > 0)
                {
                    int i = RandGen.Next(0, UAList.Count - 1);
                    return UAList[i];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
