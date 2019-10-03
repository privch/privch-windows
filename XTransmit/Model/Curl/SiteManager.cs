using System.Collections.Generic;
using System.Linq;
using XTransmit.Utility;

namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-09-28
     */
    public class SiteManager
    {
        public static List<SiteProfile> SiteList;
        private static string CurlXmlPath;

        // see also Preference.LoadFileOrDefault()
        public static void Load(string pathCurlXml)
        {
            if (FileUtil.XmlDeserialize(pathCurlXml, typeof(List<SiteProfile>)) is List<SiteProfile> listSite)
            {
                SiteList = listSite;
            }
            else
            {
                SiteList = new List<SiteProfile>();
            }

            CurlXmlPath = pathCurlXml;
        }
        public static void Reload()
        {
            if (!string.IsNullOrWhiteSpace(CurlXmlPath))
            {
                Load(CurlXmlPath);
            }
        }

        public static void Save(List<SiteProfile> listSite)
        {
            FileUtil.XmlSerialize(CurlXmlPath, listSite);
        }

        // determin wether the ip list has been changed
        public static bool HasChangesToFile()
        {
            byte[] md5Data = TextUtil.GetMD5(SiteList);
            byte[] md5File = FileUtil.GetMD5(CurlXmlPath);

            return (md5Data != null && md5File != null) ? !md5File.SequenceEqual(md5Data) : true;
        }
    }
}
