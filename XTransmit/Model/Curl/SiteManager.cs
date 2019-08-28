using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-08-02
     */
    public class SiteManager
    {
        // see also Preference.LoadFileOrDefault()
        public static List<SiteProfile> LoadFileOrDefault(string fileXhttpXml)
        {
            List<SiteProfile> siteList;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SiteProfile>));
                FileStream fileStream = new FileStream(fileXhttpXml, FileMode.Open);
                siteList = (List<SiteProfile>)xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception)
            {
                siteList = new List<SiteProfile>();
            }

            return siteList;
        }

        public static void WriteFile(string fileXhttpXml, List<SiteProfile> siteList)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SiteProfile>));
                StreamWriter writer = new StreamWriter(fileXhttpXml);
                xmlSerializer.Serialize(writer, siteList);
                writer.Close();
            }
            catch (Exception) { }
        }
    }
}
