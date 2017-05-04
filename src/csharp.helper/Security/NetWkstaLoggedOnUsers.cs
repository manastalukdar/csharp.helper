using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace csharp.helper.Security
{
    public class NetWkstaLoggedOnUsers
    {
        #region Private Fields

        private const int ErrorMoreData = 234;

        private const int NerrSuccess = 0;

        #endregion Private Fields

        #region Public Methods

        public static List<String> ListLoggedOnUsers(string hostName)
        {
            var ret = new List<string>();

            var bufptr = IntPtr.Zero;
            int dwEntriesread;
            var dwTotalentries = 0;
            var dwResumehandle = 0;
            int nStatus;
            var tWui1 = typeof(WkstaUserInfo1);
            var nStructSize = Marshal.SizeOf(tWui1);
            WkstaUserInfo1 wui1;

            //this.listView1.Items.Clear();

            do
            {
                nStatus = NetWkstaUserEnum(
                  hostName,
                  1,
                  out bufptr,
                  32768,
                  out dwEntriesread,
                  out dwTotalentries,
                  ref dwResumehandle);

                //
                // If the call succeeds,
                //
                if ((nStatus == NerrSuccess) | (nStatus == ErrorMoreData))
                {
                    if (dwEntriesread > 0)
                    {
                        var pstruct = bufptr;

                        //
                        // Loop through the entries.
                        //
                        for (var i = 0; i < dwEntriesread; i++)
                        {
                            wui1 = (WkstaUserInfo1)Marshal.PtrToStructure(pstruct, tWui1);
                            ret.Add(wui1.wkui1_logon_domain + "\\" + wui1.wkui1_username);
                            pstruct = (IntPtr)((int)pstruct + nStructSize);
                        }
                    }
                    else
                    {
                        Console.WriteLine("A system error has occurred : " + nStatus);
                    }
                }

                if (bufptr != IntPtr.Zero)
                {
                    NetApiBufferFree(bufptr);
                }
            }

            while (nStatus == ErrorMoreData);

            return ret;
        }

        #endregion Public Methods

        #region Private Methods

        [DllImport("netapi32.dll")]
        private static extern int NetApiBufferFree(
           IntPtr buffer);

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int NetWkstaUserEnum(
           string servername,
           int level,
           out IntPtr bufptr,
           int prefmaxlen,
           out int entriesread,
           out int totalentries,
           ref int resumeHandle);

        #endregion Private Methods

        #region Public Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WkstaUserInfo0
        {
            public string wkui0_username;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WkstaUserInfo1
        {
            public string wkui1_username;
            public string wkui1_logon_domain;
            public string wkui1_oth_domains;
            public string wkui1_logon_server;
        }

        #endregion Public Structs
    }
}