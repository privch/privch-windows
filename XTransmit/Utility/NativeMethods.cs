using System;
using System.Net;
using System.Runtime.InteropServices;

/**
 * Updated: 2019-08-02
 */

namespace XTransmit.Utility
{
    public static class NativeMethods
    {
        public static readonly string Bypass;
        static NativeMethods()
        {
            Bypass = string.Join(";", arrayBypass);
        }

        public static Uri GetCurrentConfig(string url)
        {
            IWebProxy proxy = WebRequest.GetSystemWebProxy();
            Uri uriProxy = proxy.GetProxy(new Uri(url));
            return uriProxy;
        }

        [DllImport("proxyctrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern int EnableProxy(string server, string bypass);

        [DllImport("proxyctrl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern int DisableProxy();

        private static readonly string[] arrayBypass = {
            "<local>", // for enable the "skip LAN" option
            "localhost",
            "127.*",
            "10.*",
            "172.16.*",
            "172.17.*",
            "172.18.*",
            "172.19.*",
            "172.20.*",
            "172.21.*",
            "172.22.*",
            "172.23.*",
            "172.24.*",
            "172.25.*",
            "172.26.*",
            "172.27.*",
            "172.28.*",
            "172.29.*",
            "172.30.*",
            "172.31.*",
            "192.168.*"
        };
    }
}
