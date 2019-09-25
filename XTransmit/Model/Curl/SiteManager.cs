using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace XTransmit.Model.Curl
{
    /**
     * Updated: 2019-09-24
     */
    public class SiteManager
    {
        // see also Preference.LoadFileOrDefault()
        public static List<SiteProfile> LoadFileOrDefault(string fileXhttpXml)
        {
            List<SiteProfile> siteList;
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(fileXhttpXml, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SiteProfile>));
                siteList = (List<SiteProfile>)xmlSerializer.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception)
            {
                siteList = new List<SiteProfile>();
            }
            finally
            {
                fileStream?.Dispose();
            }

            return siteList;
        }

        public static void WriteFile(string fileXhttpXml, List<SiteProfile> siteList)
        {
            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(fileXhttpXml);
                new XmlSerializer(typeof(List<SiteProfile>)).Serialize(streamWriter, siteList);
                streamWriter.Close();
            }
            catch (Exception) { }
            finally
            {
                streamWriter?.Dispose();
            }
        }
    }
}
