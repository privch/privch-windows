using System;
using System.Data;

namespace XTransmit.Model.Network
{
    /**
     * Updated: 2019-08-02
     */
    public static class UserAgentManager
    {
        public static DataSet DataSetUA { get; private set; }

        public static void Load(string fileUserAgentXml)
        {
            DataSetUA = new DataSet("DataSet-UA");
            try { DataSetUA.ReadXml(fileUserAgentXml); }
            catch (Exception) { }

            DataSetUA.AcceptChanges();
        }

        public static void Save(string fileUserAgentXml)
        {
            DataSetUA.AcceptChanges();
            try { DataSetUA.WriteXml(fileUserAgentXml); }
            catch (Exception) { }
        }
    }
}
