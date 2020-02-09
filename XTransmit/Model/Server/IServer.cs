namespace XTransmit.Model.Server
{
    internal interface IServer
    {
        string HostAddress { get; set; }
        int HostPort { get; set; }

        string ResponseTime { get; set; }
        long PingDelay { get; set; }

        int ListenPort { get; set; }

        string GetID();
        bool IsServerEqual(object server);

        void UpdateIPInfo(bool force);
        void UpdateResponse();
        void UpdatePing();
    }
}
