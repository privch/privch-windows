using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using XTransmit.Model.IPAddress;
using XTransmit.Utility;

namespace XTransmit.Model.Server
{
    [Serializable]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public class ServerProfile
    {
        // encrypt method
        public static readonly string[] Ciphers =
        {
            "rc4-md5",
            "aes-128-gcm", "aes-192-gcm", "aes-256-gcm",
            "aes-128-cfb", "aes-192-cfb", "aes-256-cfb",
            "aes-128-ctr", "aes-192-ctr", "aes-256-ctr",
            "bf-cfb",
            "camellia-128-cfb", "camellia-192-cfb", "camellia-256-cfb",
            "chacha20", "chacha20-ietf", "chacha20-ietf-poly1305",
            "xchacha20-ietf-poly1305",
            "salsa20",
        };

        // server arguments
        public string HostIP { get; set; }
        public int HostPort { get; set; }
        public string Encrypt { get; set; }
        public string Password { get; set; }
        public string Remarks { get; set; }

        public bool PluginEnabled { get; set; }
        public string PluginName { get; set; }
        public string PluginOption { get; set; }

        // preference and info
        public string FriendlyName { get; set; }
        public string TimeCreated { get; set; }
        public IPInfo IPData { get; set; }

        // status
        public int ListenPort { get; set; }
        public string ResponseTime { get; set; } //seconds
        public long Ping { get; set; } // less then 0 means timeout or unreachable

        public ServerProfile Copy()
        {
            return (ServerProfile)TextUtil.CopyBySerializer(this);
        }

        /**<summary>
         * Must be called after the ConfigManager.Global loaded
         * </summary> 
         */
        public ServerProfile()
        {
            HostIP = "";
            HostPort = 0;
            Encrypt = "chacha20-ietf-poly1305";
            Password = "";
            Remarks = "";

            PluginEnabled = false;
            PluginName = "";
            PluginOption = "";

            FriendlyName = "";
            TimeCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            IPData = null;

            ListenPort = -1;
            ResponseTime = "";
            Ping = 0;
        }

        public void SetFriendlyNameDefault()
        {
            FriendlyName = string.IsNullOrWhiteSpace(Remarks) ? $"{HostIP} - {HostPort}" : Remarks;
        }

        public void SetFriendNameByIPData()
        {
            if (IPData == null)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(IPData.Country))
            {
                stringBuilder.Append(IPData.Country);
            }

            if (!string.IsNullOrWhiteSpace(IPData.Region))
            {
                stringBuilder.Append(" - " + IPData.Region);
            }

            if (!string.IsNullOrWhiteSpace(IPData.City))
            {
                stringBuilder.Append(" - " + IPData.City);
            }

            string friendlyName = stringBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(friendlyName))
            {
                if (friendlyName.StartsWith(" - ", StringComparison.Ordinal))
                {
                    friendlyName = friendlyName.Substring(3);
                }

                FriendlyName = friendlyName;
            }
        }

        public void FetchIPData(bool focus)
        {
            if (IPData == null || focus)
            {
                IPData = IPInfo.Fetch(HostIP);
            }
        }

        // TODO - UA.
        // return seconds
        public void CheckResponseTime()
        {
            if (ListenPort <= 0)
            {
                return;
            }

            // curl process
            Process process = null;
            int timeout = ConfigManager.Global.SSTimeout;
            try
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = CurlManager.CurlExePath,
                        Arguments = $"--silent --connect-timeout {timeout} --proxy \"socks5://127.0.0.1:{ListenPort}\" " + "-w \"%{time_total}\" -o NUL -s \"https://google.com\"",
                        WorkingDirectory = App.PathCurl,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    });

                ResponseTime = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch
            {
                ResponseTime = "";
            }
            finally
            {
                process?.Dispose();
            }
        }

        // OO Programming
        public void CheckPingDelay(Ping ping)
        {
            if (ping == null)
            {
                return;
            }

            try
            {
                PingReply reply = ping.Send(HostIP, ConfigManager.Global.PingTimeout);
                Ping = (reply.Status == IPStatus.Success) ? reply.RoundtripTime : -1;
            }
            catch (Exception)
            {
                Ping = -1;
            }
        }

        /** Serializable ==================================================
         */
        public override int GetHashCode() => HostIP.GetHashCode() ^ HostPort;

        public override bool Equals(object serverNew)
        {
            if (serverNew is ServerProfile server)
            {
                return HostIP == server.HostIP && HostPort == server.HostPort;
            }
            else
            {
                return false;
            }
        }
    }
}
