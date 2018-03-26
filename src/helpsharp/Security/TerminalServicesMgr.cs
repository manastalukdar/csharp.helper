using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace helpsharp.Security
{
    //http://www.pinvoke.net/default.aspx/wtsapi32/WTSEnumerateSessions.html
    public static class TerminalServicesMgr
    {
        #region Public Enums

        public enum WtsConnectstateClass
        {
            WtsActive = 0,
            WtsConnected = 1,
            WtsConnectQuery = 2,
            WtsShadow = 3,
            WtsDisconnected = 4,
            WtsIdle = 5,
            WtsListen = 6,
            WtsReset = 7,
            WtsDown = 8,
            WtsInit = 9
        }

        #endregion Public Enums

        #region Public Methods

        public static void CloseServer(IntPtr serverHandle)
        {
            WTSCloseServer(serverHandle);
        }

        public static List<string> ListSessions(string serverName)
        {
            var ret = new List<string>();
            var server = OpenServer(serverName);

            try
            {
                var ppSessionInfo = IntPtr.Zero;

                var count = 0;
                var retval = WTSEnumerateSessions(server, 0, 1, ref ppSessionInfo, ref count);
                var dataSize = Marshal.SizeOf(typeof(WtsSessionInfo));

                long current = (int)ppSessionInfo;

                if (retval != 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var si = (WtsSessionInfo)Marshal.PtrToStructure((IntPtr)current, typeof(WtsSessionInfo));
                        current += dataSize;

                        ret.Add(si.SessionID + " " + si.State + " " + si.pWinStationName);
                    }

                    WTSFreeMemory(ppSessionInfo);
                }
            }
            finally
            {
                CloseServer(server);
            }

            return ret;
        }

        public static IntPtr OpenServer(string name)
        {
            return WTSOpenServer(name);
        }

        #endregion Public Methods

        #region Private Methods

        [DllImport("wtsapi32.dll")]
        private static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern int WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] int reserved,
            [MarshalAs(UnmanagedType.U4)] int version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

        #endregion Private Methods

        #region Private Structs

        [StructLayout(LayoutKind.Sequential)]
        private struct WtsSessionInfo
        {
            public readonly int SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string pWinStationName;

            public readonly WtsConnectstateClass State;
        }

        #endregion Private Structs
    }
}