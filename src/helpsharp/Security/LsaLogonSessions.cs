using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace helpsharp.Security
{
    public class LsaLogonSessions
    {
        #region Private Enums

        private enum SecurityLogonType : uint
        {
            Interactive = 2,    //The security principal is logging on interactively.
            Network,            //The security principal is logging using a network.
            Batch,              //The logon is for a batch process.
            Service,            //The logon is for a service account.
            Proxy,              //Not supported.
            Unlock,             //The logon is an attempt to unlock a workstation.
            NetworkCleartext,   //The logon is a network logon with cleartext credentials.
            NewCredentials,     // Allows the caller to clone its current token and specify new credentials for outbound connections. The new logon session has the same local identity but uses different credentials for other network connections.
            RemoteInteractive,  // A terminal server session that is both remote and interactive.
            CachedInteractive, // Attempt to use the cached credentials without going out across the network.
            CachedRemoteInteractive, // Same as RemoteInteractive, except used internally for auditing purposes.
            CachedUnlock          // The logon is an attempt to unlock a workstation.
        }

        #endregion Private Enums

        #region Public Methods

        public static List<String> ListSessions()
        {
            var ret = new List<string>();

            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent();

            var systime = new DateTime(1601, 1, 1, 0, 0, 0, 0); //win32 systemdate

            UInt64 count;
            var luidPtr = IntPtr.Zero;
            LsaEnumerateLogonSessions(out count, out luidPtr);  //gets an array of pointers to LUIDs

            var iter = luidPtr;      //set the pointer to the start of the array

            for (ulong i = 0; i < count; i++)   //for each pointer in the array
            {
                IntPtr sessionData;

                LsaGetLogonSessionData(iter, out sessionData);
                if (sessionData != IntPtr.Zero)
                {
                    var data = (SecurityLogonSessionData)Marshal.PtrToStructure(sessionData, typeof(SecurityLogonSessionData));

                    //if we have a valid logon
                    if (data.PSiD != IntPtr.Zero)
                    {
                        //get the security identifier for further use
                        var sid = new System.Security.Principal.SecurityIdentifier(data.PSiD);

                        //extract some useful information from the session data struct
                        var username = Marshal.PtrToStringUni(data.Username.buffer).Trim();          //get the account username
                        var domain = Marshal.PtrToStringUni(data.LoginDomain.buffer).Trim();        //domain for this account
                        var authpackage = Marshal.PtrToStringUni(data.AuthenticationPackage.buffer).Trim();    //authentication package

                        var secType = (SecurityLogonType)data.LogonType;
                        var loginTime = systime.AddTicks((long)data.LoginTime);                              //get the datetime the session was logged in

                        ret.Add("User: " + username + " *** Domain: " + domain + " *** Login Type: (" + data.LogonType + ") " + secType.ToString() + " *** Login Time: " + loginTime.ToLocalTime().ToString());
                    }
                }

                iter = (IntPtr)((double)iter + Marshal.SizeOf(typeof(Luid)));  //move the pointer forward
                LsaFreeReturnBuffer(sessionData);   //free the SECURITY_LOGON_SESSION_DATA memory in the struct
            }

            LsaFreeReturnBuffer(luidPtr);   //free the array of LUIDs

            return ret;
        }

        #endregion Public Methods

        /************************************************************************/
        /* The following Interop code should be placed in a sealed internal NativeMethod class
         * but has been left here to simplify the example.
        /************************************************************************/

        #region Private Methods

        [DllImport("Secur32.dll", SetLastError = false)]
        private static extern uint LsaEnumerateLogonSessions(out UInt64 logonSessionCount, out IntPtr logonSessionList);

        [DllImport("secur32.dll", SetLastError = false)]
        private static extern uint LsaFreeReturnBuffer(IntPtr buffer);

        [DllImport("Secur32.dll", SetLastError = false)]
        private static extern uint LsaGetLogonSessionData(IntPtr luid, out IntPtr ppLogonSessionData);

        #endregion Private Methods

        #region Private Structs

        [StructLayout(LayoutKind.Sequential)]
        private struct LsaUnicodeString
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            public IntPtr buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Luid
        {
            public UInt32 LowPart;
            public UInt32 HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityLogonSessionData
        {
            public UInt32 Size;
            public Luid LoginID;
            public LsaUnicodeString Username;
            public LsaUnicodeString LoginDomain;
            public LsaUnicodeString AuthenticationPackage;
            public UInt32 LogonType;
            public UInt32 Session;
            public IntPtr PSiD;
            public UInt64 LoginTime;
            public LsaUnicodeString LogonServer;
            public LsaUnicodeString DnsDomainName;
            public LsaUnicodeString Upn;
        }

        #endregion Private Structs
    }
}